﻿// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslynator;
using Roslynator.CSharp;
using Roslynator.CSharp.Extensions;
using Roslynator.Metadata;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Roslynator.CSharp.CSharpFactory;

namespace CodeGenerator
{
    public class OptionsPagePropertiesGenerator : Generator
    {
        public StringComparer InvariantComparer { get; } = StringComparer.InvariantCulture;

        public OptionsPagePropertiesGenerator()
        {
            DefaultNamespace = "Roslynator.VisualStudio";
        }

        public CompilationUnitSyntax Generate(IEnumerable<RefactoringDescriptor> refactorings)
        {
            return CompilationUnit()
                .WithUsings(List(new UsingDirectiveSyntax[] {
                    UsingDirective(ParseName(MetadataNames.System_Collections_Generic)),
                    UsingDirective(ParseName(MetadataNames.System_ComponentModel)),
                    UsingDirective(ParseName(MetadataNames.System_Linq)),
                    UsingDirective(ParseName("Roslynator.CSharp.Refactorings")),
                    UsingDirective(ParseName("Roslynator.VisualStudio.TypeConverters"))}))
                .WithMembers(
                    NamespaceDeclaration(DefaultNamespace)
                        .WithMembers(
                            ClassDeclaration("RefactoringsOptionsPage")
                                .WithModifiers(ModifierFactory.PublicPartial())
                                .WithMembers(
                                    CreateMembers(refactorings))));
        }

        private IEnumerable<MemberDeclarationSyntax> CreateMembers(IEnumerable<RefactoringDescriptor> refactorings)
        {
            yield return ConstructorDeclaration("RefactoringsOptionsPage")
                .WithModifiers(ModifierFactory.Public())
                .WithBody(
                    Block(refactorings
                        .OrderBy(f => f.Id, InvariantComparer)
                        .Select(refactoring =>
                        {
                            return ExpressionStatement(
                                SimpleAssignmentExpression(
                                    IdentifierName(refactoring.Id),
                                    (refactoring.IsEnabledByDefault) ? TrueLiteralExpression() : FalseLiteralExpression()));
                        })));

            yield return MethodDeclaration(VoidType(), "SaveValuesToView")
                .WithModifiers(ModifierFactory.Public())
                .WithParameterList(ParameterList(Parameter(ParseTypeName("ICollection<RefactoringModel>"), Identifier("refactorings"))))
                .WithBody(
                    Block(refactorings
                        .OrderBy(f => f.Id, InvariantComparer)
                        .Select(refactoring =>
                        {
                            return ExpressionStatement(
                                ParseExpression($"refactorings.Add(new RefactoringModel(\"{refactoring.Id}\", \"{StringUtility.EscapeQuote(refactoring.Title)}\", {refactoring.Id}))"));
                        })));

            yield return MethodDeclaration(VoidType(), "LoadValuesFromView")
                .WithModifiers(ModifierFactory.Public())
                .WithParameterList(ParameterList(Parameter(ParseTypeName("ICollection<RefactoringModel>"), Identifier("refactorings"))))
                .WithBody(
                    Block(refactorings
                        .OrderBy(f => f.Id, InvariantComparer)
                        .Select(refactoring =>
                        {
                            return ExpressionStatement(
                                ParseExpression($"{refactoring.Id} = refactorings.FirstOrDefault(f => f.Id == \"{refactoring.Id}\").Enabled"));
                        })));

            yield return MethodDeclaration(VoidType(), "Apply")
                .WithModifiers(ModifierFactory.Public())
                .WithBody(
                    Block(refactorings
                        .OrderBy(f => f.Identifier, InvariantComparer)
                        .Select(refactoring =>
                        {
                            return ExpressionStatement(
                                InvocationExpression(
                                    "SetIsEnabled",
                                    ArgumentList(
                                        Argument(
                                            SimpleMemberAccessExpression(
                                                IdentifierName("RefactoringIdentifiers"),
                                                IdentifierName(refactoring.Identifier))),
                                        Argument(IdentifierName(refactoring.Id)))));
                        })));

            foreach (RefactoringDescriptor info in refactorings.OrderBy(f => f.Id, InvariantComparer))
                yield return CreateRefactoringProperty(info);
        }

        private PropertyDeclarationSyntax CreateRefactoringProperty(RefactoringDescriptor refactoring)
        {
            return PropertyDeclaration(BoolType(), refactoring.Id)
                .WithAttributeLists(
                    AttributeList(Attribute("Category", IdentifierName("RefactoringCategory"))),
                    AttributeList(Attribute("DisplayName", StringLiteralExpression(refactoring.Title))),
                    AttributeList(Attribute("Description", StringLiteralExpression(CreateDescription(refactoring)))),
                    AttributeList(Attribute("TypeConverter", TypeOfExpression(IdentifierName("EnabledDisabledConverter")))))
                .WithModifiers(ModifierFactory.Public())
                .WithAccessorList(
                    AccessorList(
                        AutoImplementedGetter(),
                        AutoImplementedSetter()));
        }

        private static string CreateDescription(RefactoringDescriptor refactoring)
        {
            string s = "";

            if (refactoring.Syntaxes.Count > 0)
                s = "Syntax: " + string.Join(", ", refactoring.Syntaxes.Select(f => f.Name));

            if (!string.IsNullOrEmpty(refactoring.Scope))
            {
                if (!string.IsNullOrEmpty(s))
                    s += "\r\n";

                s += "Scope: " + refactoring.Scope;
            }

            return s;
        }
    }
}
