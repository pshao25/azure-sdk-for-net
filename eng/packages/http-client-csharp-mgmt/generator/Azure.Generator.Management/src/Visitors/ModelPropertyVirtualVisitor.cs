// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.TypeSpec.Generator.ClientModel;
using Microsoft.TypeSpec.Generator.Primitives;
using Microsoft.TypeSpec.Generator.Providers;

namespace Azure.Generator.Management.Visitors
{
    internal class ModelPropertyVirtualVisitor : ScmLibraryVisitor
    {
        protected override PropertyProvider? VisitProperty(PropertyProvider property)
        {
            const MethodSignatureModifiers excludedModifiers = MethodSignatureModifiers.Static
                | MethodSignatureModifiers.Virtual | MethodSignatureModifiers.Override | MethodSignatureModifiers.Abstract;

            // WireInfo limits this to data properties, excluding infrastructure such as the ref-returning Patch.
            if (property.EnclosingType is ModelProvider model
                && !model.Type.IsStruct
                && !model.DeclarationModifiers.HasFlag(TypeSignatureModifiers.Sealed)
                && model.CustomCodeView?.DeclarationModifiers.HasFlag(TypeSignatureModifiers.Sealed) != true
                && property.WireInfo is not null
                && property.ExplicitInterface is null
                && property.Modifiers.HasFlag(MethodSignatureModifiers.Public)
                && (property.Modifiers & excludedModifiers) == 0)
            {
                property.Update(modifiers: property.Modifiers | MethodSignatureModifiers.Virtual);
            }

            return base.VisitProperty(property);
        }
    }
}
