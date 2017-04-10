﻿// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Roslynator
{
    internal class AsyncMethodUniqueNameProvider : UniqueNameProvider
    {
        public override string EnsureUniqueName(string baseName, HashSet<string> reservedNames)
        {
            int suffix = 1;

            string name = baseName + "Async";

            while (!NameGenerator.IsUniqueName(name, reservedNames))
            {
                suffix++;
                name = baseName + suffix.ToString() + "Async";
            }

            return name;
        }

        public override string EnsureUniqueName(string baseName, ImmutableArray<ISymbol> symbols, bool isCaseSensitive)
        {
            int suffix = 1;

            string name = baseName + "Async";

            while (!NameGenerator.IsUniqueName(name, symbols, isCaseSensitive))
            {
                suffix++;
                name = baseName + suffix.ToString() + "Async";
            }

            return name;
        }
    }
}