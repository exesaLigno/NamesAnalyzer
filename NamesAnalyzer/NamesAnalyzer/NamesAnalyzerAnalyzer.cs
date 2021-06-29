using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;



namespace NamesAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class NamesAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        int ClassNameLength = 3;
        int FunctionNameLength = 4;
        int PropertyNameLength = 5;
        int FieldNameLength = 6;
        int VariableNameLength = 7;

        public const string DiagnosticId = "NamesAnalyzer";

        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.AnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.AnalyzerDescription), Resources.ResourceManager, typeof(Resources));
        private const string Category = "Naming";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterSyntaxNodeAction(AnalyzeClassDeclaration, SyntaxKind.ClassDeclaration);
            context.RegisterSyntaxNodeAction(AnalyzeMethodDeclaration, SyntaxKind.MethodDeclaration);
            context.RegisterSyntaxNodeAction(AnalyzePropertyDeclaration, SyntaxKind.PropertyDeclaration);
            context.RegisterSyntaxNodeAction(AnalyzeVariableDeclaration, SyntaxKind.VariableDeclarator);
        }

        private void AnalyzeClassDeclaration(SyntaxNodeAnalysisContext context)
        {
            ClassDeclarationSyntax node = (ClassDeclarationSyntax) context.Node;

            int MaximumLength = ClassNameLength;
            if (node.Identifier.ValueText.Length > MaximumLength)
            {
                var diagnostic = Diagnostic.Create(Rule, node.Identifier.GetLocation(), "Class", node.Identifier.ValueText, MaximumLength);
                context.ReportDiagnostic(diagnostic);
            }
        }

        private void AnalyzeMethodDeclaration(SyntaxNodeAnalysisContext context)
        {
            MethodDeclarationSyntax node = (MethodDeclarationSyntax)context.Node;

            int MaximumLemgth = FunctionNameLength;
            if (node.Identifier.ValueText.Length > MaximumLemgth)
            {
                var diagnostic = Diagnostic.Create(Rule, node.Identifier.GetLocation(), "Method", node.Identifier.ValueText, MaximumLemgth);
                context.ReportDiagnostic(diagnostic);
            }
        }

        private void AnalyzePropertyDeclaration(SyntaxNodeAnalysisContext context)
        {
            PropertyDeclarationSyntax node = (PropertyDeclarationSyntax)context.Node;

            int MaximumLength = PropertyNameLength;
            if (node.Identifier.ValueText.Length > MaximumLength)
            {
                var diagnostic = Diagnostic.Create(Rule, node.Identifier.GetLocation(), "Property", node.Identifier.ValueText, MaximumLength);
                context.ReportDiagnostic(diagnostic);
            }
        }

        private void AnalyzeVariableDeclaration(SyntaxNodeAnalysisContext context)
        {
            VariableDeclaratorSyntax node = (VariableDeclaratorSyntax)context.Node;

            if (node.Parent.Parent is FieldDeclarationSyntax)
            {
                int MaximumLength = FieldNameLength;
                if (node.Identifier.ValueText.Length > MaximumLength)
                {
                    var diagnostic = Diagnostic.Create(Rule, node.Identifier.GetLocation(), "Field", node.Identifier.ValueText, MaximumLength);
                    context.ReportDiagnostic(diagnostic);
                }
            }

            else
            {
                int MaximumLength = VariableNameLength;
                if (node.Identifier.ValueText.Length > MaximumLength)
                {
                    var diagnostic = Diagnostic.Create(Rule, node.Identifier.GetLocation(), "Variable", node.Identifier.ValueText, MaximumLength);
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }
    }
}
