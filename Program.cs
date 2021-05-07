using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static RoslynTest.configuration;


void CheckNode(SyntaxNode node)
{
    if (node.Kind().ToString() == "MethodDeclaration")
    {
        IEnumerable<SyntaxToken> tokens = node.ChildTokens();
        foreach (SyntaxToken token in tokens)
        {
            if (token.Kind().ToString() == "IdentifierToken")
            {
                System.Console.WriteLine($"Name of function is {token.Text}");
                if (token.Text.Length > Default.function_name_length)
                {
                    System.Console.WriteLine($"This name is too long! Recommended to use under {Default.function_name_length} symbols");
                }
            }
        }
    }

    else if (node.Kind().ToString() == "VariableDeclarator")
    {
        IEnumerable<SyntaxToken> tokens = node.ChildTokens();
        foreach (SyntaxToken token in tokens)
        {
            if (token.Kind().ToString() == "IdentifierToken")
                System.Console.WriteLine($"Name of variable is {token.Text}");
        }
    }

    else if (node.Kind().ToString() == "ClassDeclaration")
    {
        IEnumerable<SyntaxToken> tokens = node.ChildTokens();
        foreach (SyntaxToken token in tokens)
        {
            if (token.Kind().ToString() == "IdentifierToken")
                System.Console.WriteLine($"Name of class is {token.Text}");
        }
    }
}


void GoAround(SyntaxNode node)
{
    System.Console.WriteLine($"Checking Node {node.Kind()}");
    CheckNode(node);
    IEnumerable<SyntaxNode> childs = node.ChildNodes();
    foreach (SyntaxNode child in childs)
    {
        GoAround(child);
    }
}



var test = MSBuildLocator.QueryVisualStudioInstances().ToArray();
var inst = test[0];
MSBuildLocator.RegisterInstance(inst);


MSBuildWorkspace workspace = MSBuildWorkspace.Create();

System.Console.WriteLine("Hello!");
Solution solution = workspace.OpenSolutionAsync(@"C:\Users\vskar\source\repos\Playground\Playground.sln").Result;
Project project = solution.Projects.ToArray()[0];

System.Console.WriteLine($"Project {project.Name} is opened!");
Compilation compilation = project.GetCompilationAsync().Result;
Document[] documents = project.Documents.ToArray();

foreach (Document document in documents)
{
    SyntaxTree AST = document.GetSyntaxTreeAsync().Result;
    SemanticModel model = compilation.GetSemanticModel(AST);
    SyntaxNode root = AST.GetRoot();
    GoAround(root);
}

