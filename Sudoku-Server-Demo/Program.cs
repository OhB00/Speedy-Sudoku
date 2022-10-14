using BenchmarkDotNet.Running;
using CommunityToolkit.HighPerformance;
using Sudoku_Server_Demo;
using Sudoku_Server_Demo.Benchmarks;
using System.Diagnostics;

Console.WriteLine("Starting...");

//BenchmarkRunner.Run<GenerateBenchmark>();
//BenchmarkRunner.Run<SolveBenchmark>();
// BenchmarkRunner.Run<ReduceBenchmark>();


var gap_positions = new int[81];
for (int i = 0; i < 81; i++)
{
    gap_positions[i] = i;
}

var board = Board.Generate(8);

Board.Draw(board.AsSpan2D(9, 9));

Board.Reduce(board, gap_positions, 8, 0, 45);

Board.Draw(board.AsSpan2D(9, 9));