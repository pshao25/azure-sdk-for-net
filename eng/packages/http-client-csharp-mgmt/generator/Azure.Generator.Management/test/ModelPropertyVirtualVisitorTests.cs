// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Azure.Generator.Management.Tests.Common;
using Azure.Generator.Management.Tests.TestHelpers;
using Azure.Generator.Management.Visitors;
using Microsoft.TypeSpec.Generator.Input;
using Microsoft.TypeSpec.Generator.Primitives;
using Microsoft.TypeSpec.Generator.Providers;
using NUnit.Framework;
using System.Linq;
using System.Reflection;

namespace Azure.Generator.Mgmt.Tests
{
    public class ModelPropertyVirtualVisitorTests
    {
        [TestCase("{ \"package-name\": \"sample-library\" }", false)]
        [TestCase("{ \"package-name\": \"sample-library\", \"model-properties-virtual\": false }", false)]
        [TestCase("{ \"package-name\": \"sample-library\", \"model-properties-virtual\": true }", true)]
        public void VisitorRegistrationIsOptIn(string configurationJson, bool expectedRegistered)
        {
            var plugin = ManagementMockHelpers.LoadMockPlugin(configurationJson: configurationJson);

            Assert.That(plugin.Object.Visitors.OfType<ModelPropertyVirtualVisitor>().Count(),
                Is.EqualTo(expectedRegistered ? 1 : 0));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void PublicWirePropertyBecomesVirtualWithoutChangingSetterAndIsIdempotent(bool isReadOnly)
        {
            var inputModel = InputFactory.Model(
                "TestModel",
                properties: [InputFactory.Property("value", InputPrimitiveType.String, isRequired: true, isReadOnly: isReadOnly)]);
            var plugin = ManagementMockHelpers.LoadMockPlugin(
                inputModels: () => [inputModel],
                configurationJson: "{ \"package-name\": \"sample-library\", \"model-properties-virtual\": true }");
            var model = plugin.Object.TypeFactory.CreateModel(inputModel)!;
            var property = model.Properties.Single(p => p.Name == "Value");
            var originalModifiers = property.Modifiers;
            var originalBody = property.Body;
            var originalWireInfo = property.WireInfo;

            Assert.That(originalModifiers.HasFlag(MethodSignatureModifiers.Public), Is.True);
            Assert.That(originalModifiers.HasFlag(MethodSignatureModifiers.Virtual), Is.False);
            Assert.That(originalWireInfo, Is.Not.Null);
            Assert.That(originalBody.HasSetter, Is.EqualTo(!isReadOnly));

            var visitor = plugin.Object.Visitors.OfType<ModelPropertyVirtualVisitor>().Single();
            Assert.That(VisitProperty(visitor, property), Is.SameAs(property));
            Assert.That(property.Modifiers, Is.EqualTo(originalModifiers | MethodSignatureModifiers.Virtual));
            Assert.That(property.Body, Is.SameAs(originalBody));
            Assert.That(property.Body.HasSetter, Is.EqualTo(!isReadOnly));
            Assert.That(property.WireInfo, Is.EqualTo(originalWireInfo));

            Assert.That(VisitProperty(visitor, property), Is.SameAs(property));
            Assert.That(property.Modifiers, Is.EqualTo(originalModifiers | MethodSignatureModifiers.Virtual));
            Assert.That(property.Body, Is.SameAs(originalBody));
            Assert.That(property.Body.HasSetter, Is.EqualTo(!isReadOnly));
        }

        [TestCase(MethodSignatureModifiers.Public | MethodSignatureModifiers.Static)]
        [TestCase(MethodSignatureModifiers.Public | MethodSignatureModifiers.Override)]
        [TestCase(MethodSignatureModifiers.Public | MethodSignatureModifiers.Abstract)]
        [TestCase(MethodSignatureModifiers.Public | MethodSignatureModifiers.Virtual)]
        [TestCase(MethodSignatureModifiers.Internal)]
        [TestCase(MethodSignatureModifiers.Private)]
        public void ExcludedPropertyModifiersArePreserved(MethodSignatureModifiers modifiers)
        {
            var inputModel = InputFactory.Model(
                "TestModel",
                properties: [InputFactory.Property("value", InputPrimitiveType.String)]);
            var plugin = ManagementMockHelpers.LoadMockPlugin(inputModels: () => [inputModel]);
            var model = plugin.Object.TypeFactory.CreateModel(inputModel)!;
            var property = model.Properties.Single(p => p.Name == "Value");
            property.Update(modifiers: modifiers);
            var originalBody = property.Body;

            Assert.That(property.WireInfo, Is.Not.Null);
            Assert.That(VisitProperty(new ModelPropertyVirtualVisitor(), property), Is.SameAs(property));
            Assert.That(property.Modifiers, Is.EqualTo(modifiers));
            Assert.That(property.Body, Is.SameAs(originalBody));
        }

        [Test]
        public void SealedCustomModelPropertyDoesNotBecomeVirtual()
        {
            var inputModel = InputFactory.Model(
                "TestModel",
                properties: [InputFactory.Property("value", InputPrimitiveType.String)]);
            var plugin = ManagementMockHelpers.LoadMockPlugin(
                inputModels: () => [inputModel],
                configurationJson: "{ \"package-name\": \"sample-library\", \"model-namespace\": true, \"model-properties-virtual\": true }",
                customizationSources: ["""
                    namespace Samples.Models
                    {
                        public sealed partial class TestModel { }
                    }
                    """]);
            var model = plugin.Object.TypeFactory.CreateModel(inputModel)!;
            var property = model.Properties.Single(p => p.Name == "Value");
            var originalModifiers = property.Modifiers;
            var originalBody = property.Body;

            Assert.That(model.CustomCodeView, Is.Not.Null);
            Assert.That(model.CustomCodeView!.DeclarationModifiers.HasFlag(TypeSignatureModifiers.Sealed), Is.True);
            Assert.That(originalModifiers.HasFlag(MethodSignatureModifiers.Public), Is.True);
            Assert.That(originalModifiers.HasFlag(MethodSignatureModifiers.Virtual), Is.False);
            Assert.That(property.WireInfo, Is.Not.Null);

            var visitor = plugin.Object.Visitors.OfType<ModelPropertyVirtualVisitor>().Single();
            Assert.That(VisitProperty(visitor, property), Is.SameAs(property));
            Assert.That(property.Modifiers, Is.EqualTo(originalModifiers));
            Assert.That(property.Body, Is.SameAs(originalBody));
        }

        private static PropertyProvider? VisitProperty(ModelPropertyVirtualVisitor visitor, PropertyProvider property)
        {
            var visitProperty = typeof(ModelPropertyVirtualVisitor).GetMethod(
                "VisitProperty", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.That(visitProperty, Is.Not.Null);
            return (PropertyProvider?)visitProperty!.Invoke(visitor, [property]);
        }
    }
}
