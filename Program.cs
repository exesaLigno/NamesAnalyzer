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


String GetName(SyntaxNode node)
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


int GetMaximumLength(SyntaxNode node)
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


bool CheckNode(SyntaxNode node)
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


int GoAround(SyntaxNode node)
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



var test = MSBuildLocator.QueryVisualStudioInstances().ToArray();
var inst = test[0];
MSBuildLocator.RegisterInstance(inst);


MSBuildWorkspace workspace = MSBuildWorkspace.Create();

System.Console.WriteLine("Started loading of Solution!");
Solution solution = workspace.OpenSolutionAsync(@"C:\Users\vskar\Downloads\roslyn-main\roslyn-main\Roslyn.sln").Result;
System.Console.WriteLine("Solution opened!");
IEnumerable<Project> projects = solution.Projects;

foreach (Project project in projects)
{
    System.Console.WriteLine($"\nAnalyzing project '{project.Name}'");
    Document[] documents = project.Documents.ToArray();

    foreach (Document document in documents)
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
}

