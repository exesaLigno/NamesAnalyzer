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


String GetIdentifierName(SyntaxNode node)
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


SyntaxNode GetVariableDeclarator(SyntaxNode node)
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


void CheckNode(SyntaxNode node)
{
    if (node.Kind().ToString() == "MethodDeclaration")      // Prototype for functions
    {
        String identifier_name = GetIdentifierName(node);
        System.Console.WriteLine($"Function name is {identifier_name}");
    }

    else if (node.Kind().ToString() == "LocalDeclarationStatement")
    {
        SyntaxNode declarator = GetVariableDeclarator(node);
        String identifier_name = GetIdentifierName(declarator);
        System.Console.WriteLine($"Local variable name is {identifier_name}");
    }

    else if (node.Kind().ToString() == "FieldDeclaration")
    {
        SyntaxNode declarator = GetVariableDeclarator(node);
        String identifier_name = GetIdentifierName(declarator);
        System.Console.WriteLine($"Field name is {identifier_name}");
    }

    else if (node.Kind().ToString() == "PropertyDeclaration")
    {
        String identifier_name = GetIdentifierName(node);
        System.Console.WriteLine($"Property name is {identifier_name}");
    }

    else if (node.Kind().ToString() == "ClassDeclaration")
    {
        String identifier_name = GetIdentifierName(node);
        System.Console.WriteLine($"Class name is {identifier_name}");
    }
}


void GoAround(SyntaxNode node)
{
    CheckNode(node);
    //System.Console.WriteLine($"Now checking {node.Kind()} node");
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

System.Console.WriteLine("Started loading of Solution!");
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

