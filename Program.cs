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


namespace RoslynTest
{
    class Program
    {
        static void Main(String[] argv)
        {
            RoslynTest.Analyzer analyzer = new RoslynTest.Analyzer();

            analyzer.AnalyzeSolution(@"C:\Users\vskar\source\repos\Playground\Playground.sln");

            System.Console.WriteLine("\n\n-------------------------\n\n");

            analyzer.AnalyzeProject(@"C:\Users\vskar\source\repos\Playground\Playground.csproj");
        }
    }
}