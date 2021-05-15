using System;

namespace RoslynTest
{
    class Program
    {
        static void Main(String[] argv)
        {
            RoslynTest.NamesAnalyzer analyzer = new RoslynTest.NamesAnalyzer();

            analyzer.Analyze(argv[0]);
        }
    }
}