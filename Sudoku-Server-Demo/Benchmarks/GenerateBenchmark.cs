using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Diagnostics.Windows.Configs;
using BenchmarkDotNet.Running;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Sudoku_Server_Demo.Benchmarks
{
    [MemoryDiagnoser]
    public class GenerateBenchmark
    {
        private Random rng;

        public GenerateBenchmark()
        {
            rng = new Random();
        }

        [Benchmark]
        public Span<byte> Generate() => Board.Generate(rng.Next());
    }

}
