﻿// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Composition;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Pihrtsoft.CodeAnalysis.CSharp.Refactoring;

namespace Pihrtsoft.CodeAnalysis.CSharp.CodeFixProviders
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(MemberAccessExpressionCodeFixProvider))]
    [Shared]
    public class MemberAccessExpressionCodeFixProvider : BaseCodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds
            => ImmutableArray.Create(DiagnosticIdentifiers.ReplaceStringEmptyWithEmptyStringLiteral);

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            SyntaxNode root = await context
                .Document
                .GetSyntaxRootAsync(context.CancellationToken)
                .ConfigureAwait(false);

            MemberAccessExpressionSyntax memberAccess = root
                .FindNode(context.Span, getInnermostNodeForTie: true)?
                .FirstAncestorOrSelf<MemberAccessExpressionSyntax>();

            CodeAction codeAction = CodeAction.Create(
                $"Replace '{memberAccess}' with \"\"",
                cancellationToken =>
                {
                    return ReplaceStringEmptyWithEmptyStringLiteralRefactoring.RefactorAsync(
                        context.Document,
                        memberAccess,
                        cancellationToken);
                },
                DiagnosticIdentifiers.ReplaceStringEmptyWithEmptyStringLiteral + EquivalenceKeySuffix);

            context.RegisterCodeFix(codeAction, context.Diagnostics);
        }
    }
}