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


void CheckNode(SyntaxNode node)
{
    String identifier_name = GetName(node);
    int maximum_length = GetMaximumLength(node);

    Location location = node.GetLocation();
    FileLinePositionSpan test = location.GetMappedLineSpan();


    if (identifier_name != null)
    {
        if (identifier_name.Length > maximum_length)
            System.Console.WriteLine($"\x1b[35mWarning [{test.StartLinePosition.Line + 1}]\x1b[0m: name '{identifier_name}' has too long name! Maximum {maximum_length} symbols");
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

