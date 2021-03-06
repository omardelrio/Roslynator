﻿// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Roslynator.CSharp.Refactorings
{
    internal static class MemberDeclarationRefactoring
    {
        public static async Task ComputeRefactoringsAsync(RefactoringContext context, MemberDeclarationSyntax member)
        {
            switch (member.Kind())
            {
                case SyntaxKind.MethodDeclaration:
                case SyntaxKind.IndexerDeclaration:
                case SyntaxKind.PropertyDeclaration:
                case SyntaxKind.OperatorDeclaration:
                case SyntaxKind.ConversionOperatorDeclaration:
                case SyntaxKind.ConstructorDeclaration:
                case SyntaxKind.EventDeclaration:
                case SyntaxKind.NamespaceDeclaration:
                case SyntaxKind.ClassDeclaration:
                case SyntaxKind.StructDeclaration:
                case SyntaxKind.InterfaceDeclaration:
                case SyntaxKind.EnumDeclaration:
                    {
                        if (context.IsAnyRefactoringEnabled(
                                RefactoringIdentifiers.RemoveMember,
                                RefactoringIdentifiers.DuplicateMember,
                                RefactoringIdentifiers.CommentOutMember)
                            && BraceContainsSpan(context, member))
                        {
                            if (member.Parent?.IsKind(
                                SyntaxKind.NamespaceDeclaration,
                                SyntaxKind.ClassDeclaration,
                                SyntaxKind.StructDeclaration,
                                SyntaxKind.InterfaceDeclaration,
                                SyntaxKind.CompilationUnit) == true)
                            {
                                if (context.IsRefactoringEnabled(RefactoringIdentifiers.RemoveMember))
                                {
                                    context.RegisterRefactoring(
                                        "Remove " + member.GetTitle(),
                                        cancellationToken => context.Document.RemoveMemberAsync(member, cancellationToken));
                                }

                                if (context.IsRefactoringEnabled(RefactoringIdentifiers.DuplicateMember))
                                {
                                    context.RegisterRefactoring(
                                        "Duplicate " + member.GetTitle(),
                                        cancellationToken => DuplicateMemberDeclarationRefactoring.RefactorAsync(context.Document, member, cancellationToken));
                                }
                            }

                            if (context.IsRefactoringEnabled(RefactoringIdentifiers.CommentOutMember))
                                CommentOutRefactoring.RegisterRefactoring(context, member);
                        }

                        break;
                    }
            }

            if (context.IsRefactoringEnabled(RefactoringIdentifiers.RemoveAllStatements))
                RemoveAllStatementsRefactoring.ComputeRefactoring(context, member);

            if (context.IsRefactoringEnabled(RefactoringIdentifiers.RemoveAllMemberDeclarations))
                RemoveAllMemberDeclarationsRefactoring.ComputeRefactoring(context, member);

            if (context.IsAnyRefactoringEnabled(
                    RefactoringIdentifiers.SwapMemberDeclarations,
                    RefactoringIdentifiers.RemoveMemberDeclarations)
                && !member.Span.IntersectsWith(context.Span))
            {
                MemberDeclarationsRefactoring.ComputeRefactoring(context, member);
            }

            switch (member.Kind())
            {
                case SyntaxKind.NamespaceDeclaration:
                    {
                        NamespaceDeclarationRefactoring.ComputeRefactorings(context, (NamespaceDeclarationSyntax)member);
                        break;
                    }
                case SyntaxKind.ClassDeclaration:
                    {
                        await ClassDeclarationRefactoring.ComputeRefactorings(context, (ClassDeclarationSyntax)member).ConfigureAwait(false);
                        break;
                    }
                case SyntaxKind.StructDeclaration:
                    {
                        await StructDeclarationRefactoring.ComputeRefactoringsAsync(context, (StructDeclarationSyntax)member).ConfigureAwait(false);
                        break;
                    }
                case SyntaxKind.InterfaceDeclaration:
                    {
                        InterfaceDeclarationRefactoring.ComputeRefactorings(context, (InterfaceDeclarationSyntax)member);
                        break;
                    }
                case SyntaxKind.EnumDeclaration:
                    {
                        await EnumDeclarationRefactoring.ComputeRefactoringAsync(context, (EnumDeclarationSyntax)member).ConfigureAwait(false);
                        break;
                    }
                case SyntaxKind.EnumMemberDeclaration:
                    {
                        await EnumMemberDeclarationRefactoring.ComputeRefactoringAsync(context, (EnumMemberDeclarationSyntax)member).ConfigureAwait(false);
                        break;
                    }
                case SyntaxKind.DelegateDeclaration:
                    {
                        DelegateDeclarationRefactoring.ComputeRefactorings(context, (DelegateDeclarationSyntax)member);
                        break;
                    }
                case SyntaxKind.MethodDeclaration:
                    {
                        await MethodDeclarationRefactoring.ComputeRefactoringsAsync(context, (MethodDeclarationSyntax)member).ConfigureAwait(false);
                        break;
                    }
                case SyntaxKind.ConstructorDeclaration:
                    {
                        await ConstructorDeclarationRefactoring.ComputeRefactoringsAsync(context, (ConstructorDeclarationSyntax)member).ConfigureAwait(false);
                        break;
                    }
                case SyntaxKind.DestructorDeclaration:
                    {
                        DestructorDeclarationRefactoring.ComputeRefactorings(context, (DestructorDeclarationSyntax)member);
                        break;
                    }
                case SyntaxKind.IndexerDeclaration:
                    {
                        await IndexerDeclarationRefactoring.ComputeRefactoringsAsync(context, (IndexerDeclarationSyntax)member).ConfigureAwait(false);
                        break;
                    }
                case SyntaxKind.PropertyDeclaration:
                    {
                        await PropertyDeclarationRefactoring.ComputeRefactoringsAsync(context, (PropertyDeclarationSyntax)member).ConfigureAwait(false);
                        break;
                    }
                case SyntaxKind.OperatorDeclaration:
                    {
                        ComputeRefactorings(context, (OperatorDeclarationSyntax)member);
                        break;
                    }
                case SyntaxKind.ConversionOperatorDeclaration:
                    {
                        ComputeRefactorings(context, (ConversionOperatorDeclarationSyntax)member);
                        break;
                    }
                case SyntaxKind.FieldDeclaration:
                    {
                        await FieldDeclarationRefactoring.ComputeRefactoringsAsync(context, (FieldDeclarationSyntax)member).ConfigureAwait(false);
                        break;
                    }
                case SyntaxKind.EventDeclaration:
                    {
                        await EventDeclarationRefactoring.ComputeRefactoringsAsync(context, (EventDeclarationSyntax)member).ConfigureAwait(false);
                        break;
                    }
                case SyntaxKind.EventFieldDeclaration:
                    {
                        await EventFieldDeclarationRefactoring.ComputeRefactoringsAsync(context, (EventFieldDeclarationSyntax)member).ConfigureAwait(false);
                        break;
                    }
            }
        }

        private static void ComputeRefactorings(RefactoringContext context, OperatorDeclarationSyntax operatorDeclaration)
        {
            if (context.IsRefactoringEnabled(RefactoringIdentifiers.UseExpressionBodiedMember)
                && operatorDeclaration.Body?.Span.Contains(context.Span) == true
                && context.SupportsCSharp6
                && UseExpressionBodiedMemberRefactoring.CanRefactor(operatorDeclaration))
            {
                context.RegisterRefactoring(
                    "Use expression-bodied member",
                    cancellationToken => UseExpressionBodiedMemberRefactoring.RefactorAsync(context.Document, operatorDeclaration, cancellationToken));
            }
        }

        private static void ComputeRefactorings(RefactoringContext context, ConversionOperatorDeclarationSyntax operatorDeclaration)
        {
            if (context.IsRefactoringEnabled(RefactoringIdentifiers.UseExpressionBodiedMember)
                && operatorDeclaration.Body?.Span.Contains(context.Span) == true
                && context.SupportsCSharp6
                && UseExpressionBodiedMemberRefactoring.CanRefactor(operatorDeclaration))
            {
                context.RegisterRefactoring(
                    "Use expression-bodied member",
                    cancellationToken => UseExpressionBodiedMemberRefactoring.RefactorAsync(context.Document, operatorDeclaration, cancellationToken));
            }
        }

        private static bool BraceContainsSpan(RefactoringContext context, MemberDeclarationSyntax member)
        {
            switch (member.Kind())
            {
                case SyntaxKind.MethodDeclaration:
                    return ((MethodDeclarationSyntax)member).Body?.BraceContainsSpan(context.Span) == true;
                case SyntaxKind.IndexerDeclaration:
                    return ((IndexerDeclarationSyntax)member).AccessorList?.BraceContainsSpan(context.Span) == true;
                case SyntaxKind.OperatorDeclaration:
                    return ((OperatorDeclarationSyntax)member).Body?.BraceContainsSpan(context.Span) == true;
                case SyntaxKind.ConversionOperatorDeclaration:
                    return ((ConversionOperatorDeclarationSyntax)member).Body?.BraceContainsSpan(context.Span) == true;
                case SyntaxKind.ConstructorDeclaration:
                    return ((ConstructorDeclarationSyntax)member).Body?.BraceContainsSpan(context.Span) == true;
                case SyntaxKind.PropertyDeclaration:
                    return ((PropertyDeclarationSyntax)member).AccessorList?.BraceContainsSpan(context.Span) == true;
                case SyntaxKind.EventDeclaration:
                    return ((EventDeclarationSyntax)member).AccessorList?.BraceContainsSpan(context.Span) == true;
                case SyntaxKind.NamespaceDeclaration:
                    return ((NamespaceDeclarationSyntax)member).BraceContainsSpan(context.Span);
                case SyntaxKind.ClassDeclaration:
                    return ((ClassDeclarationSyntax)member).BraceContainsSpan(context.Span);
                case SyntaxKind.StructDeclaration:
                    return ((StructDeclarationSyntax)member).BraceContainsSpan(context.Span);
                case SyntaxKind.InterfaceDeclaration:
                    return ((InterfaceDeclarationSyntax)member).BraceContainsSpan(context.Span);
                case SyntaxKind.EnumDeclaration:
                    return ((EnumDeclarationSyntax)member).BraceContainsSpan(context.Span);
            }

            return false;
        }

        private static bool BraceContainsSpan(this NamespaceDeclarationSyntax declaration, TextSpan span)
        {
            return declaration.OpenBraceToken.Span.Contains(span)
                || declaration.CloseBraceToken.Span.Contains(span);
        }

        private static bool BraceContainsSpan(this ClassDeclarationSyntax declaration, TextSpan span)
        {
            return declaration.OpenBraceToken.Span.Contains(span)
                || declaration.CloseBraceToken.Span.Contains(span);
        }

        private static bool BraceContainsSpan(this StructDeclarationSyntax declaration, TextSpan span)
        {
            return declaration.OpenBraceToken.Span.Contains(span)
                || declaration.CloseBraceToken.Span.Contains(span);
        }

        private static bool BraceContainsSpan(this InterfaceDeclarationSyntax declaration, TextSpan span)
        {
            return declaration.OpenBraceToken.Span.Contains(span)
                || declaration.CloseBraceToken.Span.Contains(span);
        }

        private static bool BraceContainsSpan(this EnumDeclarationSyntax declaration, TextSpan span)
        {
            return declaration.OpenBraceToken.Span.Contains(span)
                || declaration.CloseBraceToken.Span.Contains(span);
        }

        private static bool BraceContainsSpan(this BlockSyntax body, TextSpan span)
        {
            return body.OpenBraceToken.Span.Contains(span)
                || body.CloseBraceToken.Span.Contains(span);
        }

        private static bool BraceContainsSpan(this AccessorListSyntax accessorList, TextSpan span)
        {
            return accessorList.OpenBraceToken.Span.Contains(span)
                || accessorList.CloseBraceToken.Span.Contains(span);
        }
    }
}