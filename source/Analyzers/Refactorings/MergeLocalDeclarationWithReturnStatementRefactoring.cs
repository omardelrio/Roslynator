﻿// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using Roslynator.CSharp;

namespace Roslynator.CSharp.Refactorings
{
    internal static class MergeLocalDeclarationWithReturnStatementRefactoring
    {
        private static DiagnosticDescriptor FadeOutDescriptor
        {
            get { return DiagnosticDescriptors.MergeLocalDeclarationWithReturnStatementFadeOut; }
        }

        public static void Analyze(SyntaxNodeAnalysisContext context)
        {
            var returnStatement = (ReturnStatementSyntax)context.Node;
            ExpressionSyntax expression = returnStatement.Expression;

            if (expression?.IsKind(SyntaxKind.IdentifierName) != true)
                return;

            LocalDeclarationStatementSyntax localDeclaration = GetLocalDeclaration(returnStatement);

            if (localDeclaration == null)
                return;

            VariableDeclaratorSyntax declarator = GetVariableDeclarator(localDeclaration);

            if (declarator == null)
                return;

            ISymbol symbol = context.SemanticModel.GetSymbol(expression, context.CancellationToken);

            if (symbol?.IsLocal() != true)
                return;

            ISymbol symbol2 = context.SemanticModel.GetDeclaredSymbol(declarator, context.CancellationToken);

            if (symbol.Equals(symbol2))
            {
                TextSpan span = TextSpan.FromBounds(localDeclaration.Span.Start, returnStatement.Span.End);

                if (returnStatement.Parent
                    .DescendantTrivia(span, descendIntoTrivia: false)
                    .All(f => f.IsWhitespaceOrEndOfLineTrivia()))
                {
                    context.ReportDiagnostic(
                        DiagnosticDescriptors.MergeLocalDeclarationWithReturnStatement,
                        Location.Create(context.Node.SyntaxTree, span));
                }

                context.ReportNode(FadeOutDescriptor, localDeclaration.Declaration.Type);
                context.ReportToken(FadeOutDescriptor, declarator.Identifier);
                context.ReportToken(FadeOutDescriptor, declarator.Initializer.EqualsToken);
                context.ReportToken(FadeOutDescriptor, localDeclaration.SemicolonToken);
                context.ReportNode(FadeOutDescriptor, expression);
            }
        }

        private static LocalDeclarationStatementSyntax GetLocalDeclaration(ReturnStatementSyntax returnStatement)
        {
            if (returnStatement.IsParentKind(SyntaxKind.Block))
            {
                var block = (BlockSyntax)returnStatement.Parent;
                SyntaxList<StatementSyntax> statements = block.Statements;

                if (statements.Count > 1)
                {
                    int index = statements.IndexOf(returnStatement);

                    if (index > 0)
                    {
                        StatementSyntax statement = statements[index - 1];

                        if (statement.IsKind(SyntaxKind.LocalDeclarationStatement))
                            return (LocalDeclarationStatementSyntax)statement;
                    }
                }
            }

            return null;
        }

        private static VariableDeclaratorSyntax GetVariableDeclarator(LocalDeclarationStatementSyntax localDeclaration)
        {
            VariableDeclaratorSyntax declarator = localDeclaration.Declaration?.SingleVariableOrDefault();

            if (declarator?.Initializer?.Value != null)
            {
                return declarator;
            }
            else
            {
                return null;
            }
        }

        public static async Task<Document> RefactorAsync(
            Document document,
            LocalDeclarationStatementSyntax localDeclaration,
            CancellationToken cancellationToken)
        {
            var block = (BlockSyntax)localDeclaration.Parent;

            SyntaxList<StatementSyntax> statements = block.Statements;

            int index = statements.IndexOf(localDeclaration);

            var returnStatement = (ReturnStatementSyntax)statements[index + 1];

            SemanticModel semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);

            ExpressionSyntax expression = GetExpressionToInline(localDeclaration, semanticModel, cancellationToken);

            ReturnStatementSyntax newReturnStatement = returnStatement
                .WithExpression(expression)
                .WithLeadingTrivia(localDeclaration.GetLeadingTrivia())
                .WithTrailingTrivia(returnStatement.GetTrailingTrivia())
                .WithFormatterAnnotation();

            SyntaxList<StatementSyntax> newStatements = statements
                .Replace(returnStatement, newReturnStatement)
                .RemoveAt(index);

            BlockSyntax newBlock = block.WithStatements(newStatements);

            return await document.ReplaceNodeAsync(block, newBlock, cancellationToken).ConfigureAwait(false);
        }

        private static ExpressionSyntax GetExpressionToInline(LocalDeclarationStatementSyntax localDeclaration, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            VariableDeclarationSyntax variableDeclaration = localDeclaration.Declaration;

            ExpressionSyntax value = variableDeclaration
                .Variables
                .First()
                .Initializer
                .Value;

            ITypeSymbol typeSymbol = semanticModel.GetTypeSymbol(variableDeclaration.Type, cancellationToken);

            if (typeSymbol.SupportsExplicitDeclaration())
            {
                value = value.Parenthesize(moveTrivia: true).WithSimplifierAnnotation();

                TypeSyntax type = typeSymbol.ToMinimalTypeSyntax(semanticModel, localDeclaration.SpanStart);

                return SyntaxFactory.CastExpression(type, value).WithSimplifierAnnotation();
            }
            else
            {
                return value;
            }
        }
    }
}
