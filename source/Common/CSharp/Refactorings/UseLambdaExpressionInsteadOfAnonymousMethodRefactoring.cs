﻿// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Roslynator.CSharp.CSharpFactory;

namespace Roslynator.CSharp.Refactorings
{
    internal static class UseLambdaExpressionInsteadOfAnonymousMethodRefactoring
    {
        public static bool CanRefactor(AnonymousMethodExpressionSyntax anonymousMethod)
        {
            if (anonymousMethod == null)
                throw new ArgumentNullException(nameof(anonymousMethod));

            return anonymousMethod.ParameterList?.IsMissing == false;
        }

        public static Task<Document> RefactorAsync(
            Document document,
            AnonymousMethodExpressionSyntax anonymousMethod,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document));

            if (anonymousMethod == null)
                throw new ArgumentNullException(nameof(anonymousMethod));

            ExpressionSyntax newNode = ParenthesizedLambdaExpression(
                anonymousMethod.AsyncKeyword,
                anonymousMethod.ParameterList,
                EqualsGreaterThanToken(),
                anonymousMethod.Block);

            newNode = newNode
                .Parenthesize()
                .WithTriviaFrom(anonymousMethod)
                .WithSimplifierAnnotation()
                .WithFormatterAnnotation();

            return document.ReplaceNodeAsync(anonymousMethod, newNode, cancellationToken);
        }
    }
}
