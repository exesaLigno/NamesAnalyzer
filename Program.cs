using System;


namespace RoslynTest
{
    class Program
    {
        static void Main(String[] argv)
        {
            RoslynTest.Analyzer analyzer = new RoslynTest.Analyzer();

            analyzer.Analyze(argv[0]);
        }
    }
}