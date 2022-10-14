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
    public class SolveBenchmark
    {
        private Random rng;
        private byte[] board;
        private SquareSolver ss;
        private LineSolver ls;
        private LegalSolver es;

        public SolveBenchmark()
        {
            rng = new Random();
            board = Board.Generate(rng.Next()).ToArray();

            ss = new SquareSolver();
            ls = new LineSolver();
            es = new LegalSolver();
        }

        [Benchmark]
        public int SolveSquare() 
        {
            var pos = rng.Next(81);

            board[pos] = 0;

            return ss.Solve(board, pos);
        }

        [Benchmark]
        public int SolveLine()
        {
            var pos = rng.Next(81);

            board[pos] = 0;

            return ls.Solve(board, pos);
        }

        [Benchmark]
        public int SolveLegal()
        {
            var pos = rng.Next(81);

            board[pos] = 0;

            return ls.Solve(board, pos);
        }
    }

}
