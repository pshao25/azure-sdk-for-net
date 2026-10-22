// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Azure.Generator.MgmtTypeSpec.Tests.Models;
using Moq;
using NUnit.Framework;
using System.ClientModel.Primitives;
using System.Reflection;
using System.Text.Json;

namespace Azure.Generator.Mgmt.Tests
{
    public class ModelPropertyMockingTests
    {
        [Test]
        public void InputModelReadOnlyPropertyCanBeConfigured()
        {
            var mock = new Mock<FooPreviewAction>("preview") { CallBase = true };
            mock.SetupGet(model => model.Result).Returns("Succeeded");

            FooPreviewAction input = mock.Object;

            Assert.That(input.Result, Is.EqualTo("Succeeded"));
            Assert.That(input.Action, Is.EqualTo("preview"));
            input.Action = "update";
            Assert.That(input.Action, Is.EqualTo("update"));
            Assert.That(typeof(FooPreviewAction).GetProperty(nameof(FooPreviewAction.Result))!.CanWrite, Is.False);
            mock.VerifyGet(model => model.Result, Times.Once);
        }

        [Test]
        public void InputModelReadOnlyPropertyCanReturnAnUpdatedValue()
        {
            var result = "Pending";
            var mock = new Mock<FooPreviewAction>("preview") { CallBase = true };
            mock.SetupGet(model => model.Result).Returns(() => result);
            FooPreviewAction input = mock.Object;

            Assert.That(input.Result, Is.EqualTo("Pending"));

            result = "Succeeded";

            Assert.That(input.Result, Is.EqualTo("Succeeded"));
        }

        [Test]
        public void MockedReadOnlyPropertyPreservesSerializationVisibility()
        {
            var mock = new Mock<FooPreviewAction>("preview") { CallBase = true };
            mock.SetupGet(model => model.Result).Returns("Succeeded");
            FooPreviewAction input = mock.Object;

            using var json = JsonDocument.Parse(ModelReaderWriter.Write(input, new ModelReaderWriterOptions("J")));
            Assert.That(json.RootElement.GetProperty("result").GetString(), Is.EqualTo("Succeeded"));

            using var wire = JsonDocument.Parse(ModelReaderWriter.Write(input, new ModelReaderWriterOptions("W")));
            Assert.That(wire.RootElement.GetProperty("action").GetString(), Is.EqualTo("preview"));
            Assert.That(wire.RootElement.TryGetProperty("result", out _), Is.False);
        }

        [Test]
        public void AllDeclaredPublicModelDataPropertiesAreOverridable()
        {
            var properties = typeof(FooPreviewAction).Assembly.GetTypes()
                .Where(type => type.IsClass && !type.IsSealed)
                .SelectMany(type => type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
                .Where(property => property.CustomAttributes.Any(attribute => attribute.AttributeType.Name == "WirePathAttribute"))
                .ToList();

            Assert.That(properties, Is.Not.Empty);
            foreach (var property in properties)
            {
                foreach (var accessor in property.GetAccessors())
                {
                    Assert.That(accessor.IsVirtual && !accessor.IsFinal, Is.True,
                        $"{property.DeclaringType!.Name}.{property.Name} must be overridable.");
                }
            }
        }
    }
}
