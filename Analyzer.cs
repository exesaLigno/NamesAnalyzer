using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RoslynTest
{
    class Analyzer
    {
        private MSBuildWorkspace workspace;

        public Analyzer()
        {
            SetMSBuildInstance();
            workspace = MSBuildWorkspace.Create();
        }


        private String GetIdentifierName(SyntaxNode node)
        {
            IEnumerable<SyntaxToken> tokens = node.ChildTokens();
            foreach (SyntaxToken token in tokens)
            {
                if (token.Kind().ToString() == "IdentifierToken")
                {
                    return token.Text;
                }
            }

            return null;
        }


        private SyntaxNode GetVariableDeclarator(SyntaxNode node)
        {
            if (node.Kind().ToString() == "VariableDeclarator")
            {
                return node;
            }

            IEnumerable<SyntaxNode> childs = node.ChildNodes();
            foreach (SyntaxNode child in childs)
            {
                SyntaxNode founded = GetVariableDeclarator(child);
                if (founded != null)
                    return founded;
            }

            return null;
        }


        private String GetName(SyntaxNode node)
        {
            String name = null;

            if (node is LocalDeclarationStatementSyntax || node is FieldDeclarationSyntax)
                node = GetVariableDeclarator(node);

            if (node is LocalDeclarationStatementSyntax || node is FieldDeclarationSyntax ||
                node is MethodDeclarationSyntax || node is ClassDeclarationSyntax ||
                node is PropertyDeclarationSyntax)
                name = GetIdentifierName(node);

            return name;
        }


        private int GetMaximumLength(SyntaxNode node)
        {
            int maximum_length = 0;

            if (node is LocalDeclarationStatementSyntax)
                maximum_length = RoslynTest.Properties.Settings.Default.variable_name_length;

            else if (node is FieldDeclarationSyntax)
                maximum_length = RoslynTest.Properties.Settings.Default.field_name_length;

            else if (node is MethodDeclarationSyntax)
                maximum_length = RoslynTest.Properties.Settings.Default.function_name_length;

            else if (node is ClassDeclarationSyntax)
                maximum_length = RoslynTest.Properties.Settings.Default.type_name_length;

            else if (node is PropertyDeclarationSyntax)
                maximum_length = RoslynTest.Properties.Settings.Default.property_name_length;

            return maximum_length;
        }


        private bool CheckNode(SyntaxNode node)
        {
            bool warning_generated = false;

            String identifier_name = GetName(node);
            int maximum_length = GetMaximumLength(node);

            Location location = node.GetLocation();
            FileLinePositionSpan test = location.GetMappedLineSpan();


            if (identifier_name != null)
            {
                if (identifier_name.Length > maximum_length)
                {
                    System.Console.WriteLine($"\t\t\x1b[35mWarning [{test.StartLinePosition.Line + 1}]\x1b[0m: name '{identifier_name}' has too long name! Maximum {maximum_length} symbols");
                    warning_generated = true;
                }
            }

            return warning_generated;
        }


        private int GoAround(SyntaxNode node)
        {
            bool warning_generated = CheckNode(node);
            int warnings_count = warning_generated ? 1 : 0;

            IEnumerable<SyntaxNode> childs = node.ChildNodes();
            foreach (SyntaxNode child in childs)
            {
                warnings_count += GoAround(child);
            }

            return warnings_count;
        }


        private void SetMSBuildInstance()
        {
            var instances = MSBuildLocator.QueryVisualStudioInstances().ToArray();
            var instance = instances[0];
            MSBuildLocator.RegisterInstance(instance);
        }


        private void AnalyzeDocument(Document document)
        {
            System.Console.WriteLine($"\tAnalyzing document '{document.Name}'");
            SyntaxTree AST = document.GetSyntaxTreeAsync().Result;
            SyntaxNode root = AST.GetRoot();
            int warnings_count = GoAround(AST.GetRoot());
            if (warnings_count == 0)
            {
                System.Console.WriteLine("\t\t\x1b[32mNo warnings in this file!\x1b[0m");
            }
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
    }
}
