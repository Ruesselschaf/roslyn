﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace Microsoft.CodeAnalysis
{
    internal partial struct SymbolKey
    {
        private static class ModuleSymbolKey
        {
            public static void Create(IModuleSymbol symbol, SymbolKeyWriter visitor)
                => visitor.WriteSymbolKey(symbol.ContainingSymbol);

            public static SymbolKeyResolution Resolve(SymbolKeyReader reader)
            {
                var containingSymbolResolution = reader.ReadSymbolKey();

                using var result = PooledArrayBuilder<IModuleSymbol>.GetInstance();
                foreach (var containingSymbol in containingSymbolResolution)
                {
                    if (containingSymbol is IAssemblySymbol assembly)
                    {
                        // Don't check ModuleIds for equality because in practice, no-one uses them,
                        // and there is no way to set netmodule name programmatically using Roslyn
                        result.AddValuesIfNotNull(assembly.Modules);
                    }
                }

                return CreateSymbolInfo(result);
            }
        }
    }
}
