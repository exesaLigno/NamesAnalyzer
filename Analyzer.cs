using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Build.Locator;

namespace RoslynTest
{
    class NamesAnalyzer : CSharpSyntaxWalker
    {
        private MSBuildWorkspace workspace { get; }

        public NamesAnalyzer()
        {
            RegisterMSBuild();
            workspace = MSBuildWorkspace.Create();
        }

        private void RegisterMSBuild()
        {
            VisualStudioInstance[] instances = MSBuildLocator.QueryVisualStudioInstances().ToArray();
            VisualStudioInstance instance = instances[0];
            MSBuildLocator.RegisterInstance(instance);
        }

        private void AnalyzeDocument(Document document)
        {
            System.Console.WriteLine($"\tAnalyzing document '{document.Name}'");
            SyntaxTree AST = document.GetSyntaxTreeAsync().Result;
            SyntaxNode root = AST.GetRoot();
            Visit(AST.GetRoot());
        }

        private void AnalyzeProject(Project project)
        {
            System.Console.WriteLine($"\nAnalyzing project '{project.Name}'");
            Document[] documents = project.Documents.ToArray();

            foreach (Document document in documents)
                AnalyzeDocument(document);
        }

        public void AnalyzeProject(String project_path)
        {
            System.Console.WriteLine("Started loading of Project!");
            Project project = workspace.OpenProjectAsync(project_path).Result;
            System.Console.WriteLine("Project loaded!");

            AnalyzeProject(project);

            workspace.CloseSolution();
        }

        public void AnalyzeSolution(String solution_path)
        {
            System.Console.WriteLine("Started loading of Solution!");
            Solution solution = workspace.OpenSolutionAsync(solution_path).Result;
            System.Console.WriteLine("Solution loaded!");

            IEnumerable<Project> projects = solution.Projects;

            foreach (Project project in projects)
                AnalyzeProject(project);

            workspace.CloseSolution();
        }

        public void Analyze(String path)
        {
            if (path.EndsWith(".sln"))
                AnalyzeSolution(path);

            else if (path.EndsWith(".csproj"))
                AnalyzeProject(path);

            else
                System.Console.WriteLine("\x1b[31merror\x1b[0m: This file is not a solution or a project!");
        }

        private int GetNodeLine(SyntaxNode node)
        {
            Location location = node.GetLocation();
            FileLinePositionSpan line = location.GetMappedLineSpan();

            return line.StartLinePosition.Line + 1;
        }

        public override void VisitVariableDeclarator(VariableDeclaratorSyntax node)
        {
            if (node.Parent.Parent is FieldDeclarationSyntax)
            {
                int maximum_length = RoslynTest.Properties.Settings.Default.field_name_length;

                if (node.Identifier.Text.Length > maximum_length)
                    Console.WriteLine($"\t\t\x1b[35mWarning [{GetNodeLine(node)}]\x1b[0m: name of field '{node.Identifier.Text}' is too long! Maximum {maximum_length} symbols");
            }

            else
            {
                int maximum_length = RoslynTest.Properties.Settings.Default.variable_name_length;

                if (node.Identifier.Text.Length > maximum_length)
                    Console.WriteLine($"\t\t\x1b[35mWarning [{GetNodeLine(node)}]\x1b[0m: name of local variable '{node.Identifier.Text}' is too long! Maximum {maximum_length} symbols");
            }

            base.VisitVariableDeclarator(node);
        }

        public override void VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            int maximum_length = RoslynTest.Properties.Settings.Default.type_name_length;

            if (node.Identifier.Text.Length > maximum_length)
                Console.WriteLine($"\t\t\x1b[35mWarning [{GetNodeLine(node)}]\x1b[0m: name of type '{node.Identifier.Text}' is too long! Maximum {maximum_length} symbols");

            base.VisitClassDeclaration(node);
        }

        public override void VisitPropertyDeclaration(PropertyDeclarationSyntax node)
        {
            int maximum_length = RoslynTest.Properties.Settings.Default.property_name_length;

            if (node.Identifier.Text.Length > maximum_length)
                Console.WriteLine($"\t\t\x1b[35mWarning [{GetNodeLine(node)}]\x1b[0m: name of property '{node.Identifier.Text}' is too long! Maximum {maximum_length} symbols");

            base.VisitPropertyDeclaration(node);
        }

        public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            int maximum_length = RoslynTest.Properties.Settings.Default.function_name_length;

            if (node.Identifier.Text.Length > maximum_length)
                Console.WriteLine($"\t\t\x1b[35mWarning [{GetNodeLine(node)}]\x1b[0m: name of function '{node.Identifier.Text}' is too long! Maximum {maximum_length} symbols");

            base.VisitMethodDeclaration(node);
        }
    }
}