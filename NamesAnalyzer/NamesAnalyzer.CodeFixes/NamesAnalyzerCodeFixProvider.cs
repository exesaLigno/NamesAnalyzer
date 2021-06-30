using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Rename;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NamesAnalyzer
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(NamesAnalyzerCodeFixProvider)), Shared]
    public class NamesAnalyzerCodeFixProvider : CodeFixProvider
    {
        int ClassNameLength = 3;
        int FunctionNameLength = 4;
        int PropertyNameLength = 5;
        int FieldNameLength = 6;
        int VariableNameLength = 7;

        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(NamesAnalyzerAnalyzer.DiagnosticId); }
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            var declaration = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().First();

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: CodeFixResources.CodeFixTitle,
                    createChangedSolution: c => ShrinkNameAsync(context.Document, declaration, c),
                    equivalenceKey: nameof(CodeFixResources.CodeFixTitle)),
                diagnostic);
        }

        private async Task<Solution> ShrinkNameAsync(Document document, SyntaxNode node, CancellationToken cancellationToken)
        {
            var newName = "";

            if (node is ClassDeclarationSyntax classDeclaration)
                newName = classDeclaration.Identifier.Text.Remove(ClassNameLength);

            else if (node is MethodDeclarationSyntax methodDeclaration)
                newName = methodDeclaration.Identifier.Text.Remove(FunctionNameLength);

            else if (node is PropertyDeclarationSyntax propertyDeclaration)
                newName = propertyDeclaration.Identifier.Text.Remove(PropertyNameLength);

            else if (node is VariableDeclaratorSyntax fieldDeclarator && node.Parent.Parent is FieldDeclarationSyntax)
                newName = fieldDeclarator.Identifier.Text.Remove(FieldNameLength);

            else if (node is VariableDeclaratorSyntax variableDeclarator)
                newName = variableDeclarator.Identifier.Text.Remove(VariableNameLength);

            var semanticModel = await document.GetSemanticModelAsync(cancellationToken);
            var symbol = semanticModel.GetDeclaredSymbol(node, cancellationToken);

            var originalSolution = document.Project.Solution;
            var optionSet = originalSolution.Workspace.Options;
            var newSolution = await Renamer.RenameSymbolAsync(document.Project.Solution, symbol, newName, optionSet, cancellationToken).ConfigureAwait(false);

            return newSolution;
        }
    }
}
