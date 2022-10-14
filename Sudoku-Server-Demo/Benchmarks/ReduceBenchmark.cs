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
    public class ReduceBenchmark
    {
        private Random rng;
        private byte[] board;
        private int[] gap_positions;

        public ReduceBenchmark()
        {
            rng = new Random();
            board = Board.Generate(rng.Next()).ToArray();
            gap_positions = new int[81];

            for (int i = 0; i < 81; i++)
            {
                gap_positions[i] = i;
            }
        }


        [Benchmark]
        public void Reduce20()
        {
            Board.Reduce(board, gap_positions, 0, 0, 20);
        }

        [Benchmark]
        public void Reduce30() 
        {
            Board.Reduce(board, gap_positions, 0, 0, 30);
        }

        [Benchmark]
        public void Reduce40()
        {
            Board.Reduce(board, gap_positions, 0, 0, 40);
        }

    }

}
