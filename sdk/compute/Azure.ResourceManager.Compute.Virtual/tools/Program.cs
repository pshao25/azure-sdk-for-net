// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

internal static class Program
{
    private static readonly UTF8Encoding Utf8 = new(false, true);

    private static int Main(string[] args)
    {
        try
        {
            if (args is ["--self-test"])
            {
                SelfTest();
                return 0;
            }
            if (args.Length is < 1 or > 2 || (args.Length == 2 && args[1] != "--verify"))
                throw new ArgumentException("Usage: Virtualize <Azure.ResourceManager.Compute.Virtual directory> [--verify] | --self-test");

            string root = Path.GetFullPath(args[0]);
            string source = Path.GetFullPath(Path.Combine(root, "../Azure.ResourceManager.Compute/src"));
            string destination = Path.Combine(root, "src");
            if (Path.GetFileName(root) != "Azure.ResourceManager.Compute.Virtual" ||
                !File.Exists(Path.Combine(destination, "Azure.ResourceManager.Compute.Virtual.csproj")) ||
                !File.Exists(Path.Combine(source, "Azure.ResourceManager.Compute.csproj")))
                throw new InvalidOperationException("Expected the experimental sibling project and original Compute source project.");

            bool verify = args.Length == 2;
            var inputs = SourceFiles(source).ToDictionary(p => p, p => File.ReadAllBytes(Path.Combine(source, p)), StringComparer.Ordinal);
            var texts = inputs.Where(p => p.Key.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
                .ToDictionary(p => p.Key, p => Utf8.GetString(p.Value.AsSpan(HasBom(p.Value) ? 3 : 0)), StringComparer.Ordinal);
            var decisions = Analyze(texts);
            var outputs = new SortedDictionary<string, byte[]>(StringComparer.Ordinal);
            var files = new List<FileEntry>();
            foreach (var (path, bytes) in inputs.OrderBy(p => p.Key, StringComparer.Ordinal))
            {
                var properties = decisions.GetValueOrDefault(path) ?? [];
                var changed = properties.Where(p => p.Reason == null).OrderBy(p => p.Position).ToArray();
                byte[] output = bytes;
                if (changed.Length != 0)
                {
                    string transformed = Transform(texts[path], changed);
                    byte[] body = Utf8.GetBytes(transformed);
                    output = HasBom(bytes) ? [0xef, 0xbb, 0xbf, .. body] : body;
                }
                outputs.Add(path, output);
                files.Add(new(path, Hash(bytes), Hash(output), changed.Select(p => p.Name).ToArray(),
                    properties.Where(p => p.Reason != null).Select(p => new SkippedProperty(p.Name, p.Reason!)).ToArray()));
            }

            var skippedCounts = new SortedDictionary<string, int>(StringComparer.Ordinal);
            foreach (var property in files.SelectMany(f => f.SkippedProperties))
                skippedCounts[property.Reason] = skippedCounts.GetValueOrDefault(property.Reason) + 1;
            var manifest = new Manifest(1, "../Azure.ResourceManager.Compute/src", Hash(File.ReadAllBytes(Path.Combine(source, "Azure.ResourceManager.Compute.csproj"))),
                files.Sum(f => f.ChangedProperties.Length), skippedCounts, files.ToArray());
            byte[] manifestBytes = Utf8.GetBytes(JsonSerializer.Serialize(manifest, new JsonSerializerOptions { WriteIndented = true }) + "\n");
            string manifestPath = Path.Combine(root, "virtualization-manifest.json");
            var extras = SourceFiles(destination).Except(outputs.Keys, StringComparer.Ordinal).ToArray();

            if (verify)
            {
                var failures = outputs.Where(p => !Matches(Path.Combine(destination, p.Key), p.Value)).Select(p => p.Key).Concat(extras).ToList();
                if (!Matches(manifestPath, manifestBytes))
                    failures.Add("virtualization-manifest.json");
                if (failures.Count != 0)
                    throw new InvalidOperationException("Clone is stale (no files written):\n" + string.Join("\n", failures));
            }
            else
            {
                // Only prune obsolete outputs that still match the previous generator-owned manifest.
                var previous = File.Exists(manifestPath) ? JsonSerializer.Deserialize<Manifest>(File.ReadAllBytes(manifestPath)) : null;
                foreach (string extra in extras)
                {
                    var old = previous?.Files.SingleOrDefault(f => f.Path == extra);
                    if (old == null || Hash(File.ReadAllBytes(Path.Combine(destination, extra))) != old.OutputSha256)
                        throw new InvalidOperationException($"Unrecognized or edited output: {extra}. Refusing to remove it.");
                }
                foreach (string extra in extras)
                    File.Delete(Path.Combine(destination, extra));
                foreach (var (path, bytes) in outputs)
                {
                    string target = Path.Combine(destination, path);
                    if (Matches(target, bytes))
                        continue;
                    Directory.CreateDirectory(Path.GetDirectoryName(target)!);
                    File.WriteAllBytes(target, bytes);
                }
                if (!Matches(manifestPath, manifestBytes))
                    File.WriteAllBytes(manifestPath, manifestBytes);
            }
            Console.WriteLine($"{(verify ? "Verified" : "Generated")} {files.Count} source files; {manifest.ChangedPropertyCount} properties changed.");
            Console.WriteLine($"{files.Count(f => f.SourceSha256 != f.OutputSha256)} files changed; {files.Count(f => f.Path.EndsWith(".Serialization.cs", StringComparison.Ordinal) && f.SourceSha256 == f.OutputSha256)} serialization files byte-identical.");
            foreach (var (reason, count) in skippedCounts)
                Console.WriteLine($"Skipped {reason}: {count}");
            return 0;
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine(exception.Message);
            return 1;
        }
    }

    private static IEnumerable<string> SourceFiles(string root) => Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories)
        .Select(p => Path.GetRelativePath(root, p).Replace('\\', '/'))
        .Where(p => !p.Split('/').Any(s => s.Equals("bin", StringComparison.OrdinalIgnoreCase)
            || s.Equals("obj", StringComparison.OrdinalIgnoreCase)
            || s.Equals(".vs", StringComparison.OrdinalIgnoreCase))
            && !p.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase))
        .Order(StringComparer.Ordinal);

    private static bool HasBom(byte[] bytes) => bytes.AsSpan().StartsWith(new byte[] { 0xef, 0xbb, 0xbf });
    private static string Hash(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes));
    private static bool Matches(string path, byte[] bytes) => File.Exists(path) && File.ReadAllBytes(path).AsSpan().SequenceEqual(bytes);

    private static Dictionary<string, PropertyDecision[]> Analyze(Dictionary<string, string> texts)
    {
        var options = new CSharpParseOptions(LanguageVersion.CSharp14);
        var initial = texts.ToDictionary(p => p.Key, p => CSharpSyntaxTree.ParseText(p.Value, options, p.Key).GetRoot(), StringComparer.Ordinal);
        var symbols = initial.Values.SelectMany(r => r.DescendantTrivia(descendIntoTrivia: true))
            .Select(t => t.GetStructure()).OfType<DirectiveTriviaSyntax>()
            .SelectMany(d => d.DescendantNodes().OfType<IdentifierNameSyntax>())
            .Select(n => n.Identifier.ValueText).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToArray();
        if (symbols.Length > 8)
            throw new InvalidOperationException("Too many conditional symbols to exhaustively check safely (maximum 8).");
        var result = texts.Keys.ToDictionary(p => p, _ => new SortedDictionary<int, PropertyDecision>(), StringComparer.Ordinal);
        // Visit every conditional branch, not just the host machine's target framework.
        for (int mask = 0; mask < 1 << symbols.Length; mask++)
        {
            var configured = options.WithPreprocessorSymbols(symbols.Where((_, i) => (mask & (1 << i)) != 0));
            var roots = mask == 0 ? initial : texts.ToDictionary(p => p.Key, p => CSharpSyntaxTree.ParseText(p.Value, configured, p.Key).GetRoot(), StringComparer.Ordinal);
            foreach (var (path, syntax) in roots)
            {
                var errors = syntax.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
                if (errors.Length != 0)
                    throw new InvalidOperationException($"Cannot parse {path}: {string.Join<Diagnostic>("\n", errors)}");
            }
            var sealedTypes = roots.Values.SelectMany(r => r.DescendantNodes().OfType<TypeDeclarationSyntax>())
                .Where(t => t.Modifiers.Any(SyntaxKind.SealedKeyword) || t.Modifiers.Any(SyntaxKind.StaticKeyword))
                .Select(TypeName).ToHashSet(StringComparer.Ordinal);
            foreach (var (path, syntax) in roots)
            {
                foreach (var property in syntax.DescendantNodes().OfType<BasePropertyDeclarationSyntax>())
                {
                    if (property is not (PropertyDeclarationSyntax or IndexerDeclarationSyntax))
                        continue;
                    string name = property is PropertyDeclarationSyntax p ? p.Identifier.ValueText : "this[" + string.Join(",", ((IndexerDeclarationSyntax)property).ParameterList.Parameters.Select(p => p.Type!.ToString())) + "]";
                    var type = property.Parent as TypeDeclarationSyntax;
                    string fullName = (type == null ? "<unknown>" : TypeName(type)) + "." + name;
                    var modifiers = property.Modifiers;
                    string? reason = null;
                    if (type is not ClassDeclarationSyntax && !(type is RecordDeclarationSyntax record && !record.ClassOrStructKeyword.IsKind(SyntaxKind.StructKeyword)))
                        reason = "non-class";
                    else if (sealedTypes.Contains(TypeName(type)))
                        reason = "sealed-or-static-type";
                    else if (property.ExplicitInterfaceSpecifier != null)
                        reason = "explicit-interface";
                    else if (modifiers.Any(SyntaxKind.StaticKeyword))
                        reason = "static";
                    else if (modifiers.Any(SyntaxKind.VirtualKeyword) || modifiers.Any(SyntaxKind.OverrideKeyword) || modifiers.Any(SyntaxKind.AbstractKeyword) || modifiers.Any(SyntaxKind.SealedKeyword))
                        reason = "already-virtual-override-abstract-sealed";
                    else if (!modifiers.Any(SyntaxKind.PublicKeyword) && !modifiers.Any(SyntaxKind.ProtectedKeyword) && !modifiers.Any(SyntaxKind.InternalKeyword))
                        reason = "private";
                    else if (property.Type is RefTypeSyntax)
                        reason = "ref-return";
                    var decision = new PropertyDecision(property.Type.SpanStart, fullName, reason);
                    if (result[path].TryGetValue(decision.Position, out var previous) && previous != decision)
                        throw new InvalidOperationException($"Conditional eligibility differs for {path}: {fullName}.");
                    result[path][decision.Position] = decision;
                }
            }
        }
        return result.ToDictionary(p => p.Key, p => p.Value.Values.ToArray(), StringComparer.Ordinal);
    }

    private static string TypeName(TypeDeclarationSyntax type) => string.Join(".", type.AncestorsAndSelf().Reverse().Select(n => n switch
    {
        BaseNamespaceDeclarationSyntax ns => string.Join(".", ns.Name.DescendantTokens().Where(t => t.IsKind(SyntaxKind.IdentifierToken)).Select(t => t.ValueText)),
        TypeDeclarationSyntax t => t.Identifier.ValueText + (t.TypeParameterList == null ? "" : "`" + t.TypeParameterList.Parameters.Count),
        _ => null
    }).Where(n => n != null));

    private static string Transform(string original, PropertyDecision[] changed)
    {
        var transformed = new StringBuilder(original);
        foreach (var property in changed.Reverse())
            transformed.Insert(property.Position, "virtual ");
        string output = transformed.ToString();
        // Independently reverse the insertions to prove that all other characters/trivia are identical.
        for (int i = changed.Length - 1; i >= 0; i--)
            transformed.Remove(changed[i].Position + i * "virtual ".Length, "virtual ".Length);
        if (transformed.ToString() != original)
            throw new InvalidOperationException("Transformation changed more than property modifiers.");
        return output;
    }

    private static void SelfTest()
    {
        var texts = new Dictionary<string, string>
        {
            ["one.cs"] = """
                namespace N;
                interface I { int Explicit { get; } }
                abstract class Base { public abstract int Abstract { get; } public virtual int Override => 1; }
                partial class C : Base, I
                {
                    public /*keep*/ int Public { get; private set; }
                    protected int Protected => 0;
                    internal int Internal { get; set; }
                    protected internal int ProtectedInternal => 0;
                    private protected int PrivateProtected => 0;
                    private int Private => 0;
                    int ImplicitPrivate => 0;
                    public static int Static => 0;
                    public virtual int Virtual => 0;
                    public override int Abstract => 0;
                    public sealed override int Override => 0;
                    int I.Explicit => 0;
                    private int _value;
                    public ref int Ref => ref _value;
                    public int this[int i] => i;
                #if FEATURE
                    public
                #else
                    internal
                #endif
                    int ConditionalModifier { get; set; }
                #if FEATURE
                    public int Conditional => 1;
                #else
                    public int Conditional => 2;
                #endif
                }
                partial class S { public int Sealed => 0; }
                class S<T> { public int Generic => 0; }
                struct Value { public int ValueProperty => 0; }
                record struct RecordValue { public int ValueProperty => 0; }
                record RecordClass { public int RecordProperty => 0; }
                class Outer { public sealed class Nested { public int No => 0; } }
                class Other { public class Nested { public int Yes => 0; } }
                """.Replace("\n", "\r\n"),
            ["two.cs"] = "namespace N { sealed partial class S {} }"
        };
        var decisions = Analyze(texts);
        var changed = decisions["one.cs"].Where(p => p.Reason == null).OrderBy(p => p.Position).ToArray();
        string[] expected = ["N.C.Public", "N.C.Protected", "N.C.Internal", "N.C.ProtectedInternal", "N.C.PrivateProtected", "N.C.this[int]", "N.C.ConditionalModifier", "N.C.Conditional", "N.C.Conditional", "N.S`1.Generic", "N.RecordClass.RecordProperty", "N.Other.Nested.Yes"];
        if (!changed.Select(p => p.Name).SequenceEqual(expected))
            throw new InvalidOperationException("Self-test eligibility mismatch: " + string.Join(", ", changed.Select(p => p.Name)));
        texts["one.cs"] = Transform(texts["one.cs"], changed);
        if (!texts["one.cs"].Contains("public /*keep*/ virtual int Public", StringComparison.Ordinal) || Analyze(texts).Values.SelectMany(p => p).Any(p => p.Reason == null))
            throw new InvalidOperationException("Self-test trivia/idempotency failure.");
        var references = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!).Split(Path.PathSeparator)
            .Select(p => MetadataReference.CreateFromFile(p));
        foreach (string[] symbols in new[] { Array.Empty<string>(), new[] { "FEATURE" } })
        {
            var compilation = CSharpCompilation.Create("SelfTest", texts.Select(p => CSharpSyntaxTree.ParseText(p.Value,
                new CSharpParseOptions(LanguageVersion.CSharp14, preprocessorSymbols: symbols), p.Key)), references,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
            var errors = compilation.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
            if (errors.Length != 0)
                throw new InvalidOperationException("Self-test transformed compilation failed: " + string.Join<Diagnostic>("\n", errors));
        }
        Console.WriteLine("Self-test passed: accessibilities, modifiers, partial sealing, generic/nested types, records, indexers, ref returns, conditional branches, trivia, and idempotency.");
    }

    private sealed record PropertyDecision(int Position, string Name, string? Reason);
    private sealed record SkippedProperty(string Name, string Reason);
    private sealed record FileEntry(string Path, string SourceSha256, string OutputSha256, string[] ChangedProperties, SkippedProperty[] SkippedProperties);
    private sealed record Manifest(int FormatVersion, string SourceRoot, string SourceProjectSha256, int ChangedPropertyCount, SortedDictionary<string, int> SkippedCounts, FileEntry[] Files);
}
