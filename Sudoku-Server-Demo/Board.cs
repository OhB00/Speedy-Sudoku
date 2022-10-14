using BenchmarkDotNet.Attributes;
using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Enumerables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sudoku_Server_Demo
{
    public class Board
    {
        public static byte[] MOVES = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
        public static HashSet<byte> EMPTY_SET = new();
        public static Solver[] SOLVERS = new Solver[] 
        {
            new SquareSolver(),
            new LineSolver(),
            new LegalSolver()
        };

        public static (int x, int y) IndexToXY(int i)
        {
            return (i % 9, i / 9);
        }

        public static int XYToIndex(int x, int y)
        {
            return (y * 9) + x;
        }

        public static byte GetValue(Span<byte> buffer, int i)
        {
            return buffer[i];
        }

        public static Span2D<byte> GetSquare(Span2D<byte> buffer, int x, int y)
        {
            x /= 3;
            y /= 3;

            return buffer.Slice(y * 3, x * 3, 3, 3);
        }

        public static Span<byte> GetRowValues(Span2D<byte> buffer, int y)
        {
            Span<byte> row = new byte[9];

            int i = 0;
            foreach (var b in buffer.GetRow(y))
            {
                if (b > 0)
                {
                    row[i++] = b;
                }
            }

            return row[..i];
        }

        public static Span<byte> GetColumnValues(Span2D<byte> buffer, int x)
        {
            Span<byte> col = new byte[9];

            int i = 0;
            foreach (var b in buffer.GetColumn(x))
            {
                if (b > 0)
                {
                    col[i++] = b;
                }
            }

            return col[..i];
        }

        public static Span<byte> GetLineValues(Span2D<byte> buffer, int x, int y)
        {
            var row = GetRowValues(buffer, y);
            var col = GetColumnValues(buffer, x);

            Span<byte> lines = new byte[row.Length + col.Length];

            row.CopyTo(lines);
            col.CopyTo(lines[row.Length..]);

            return lines;
        }

        public static HashSet<byte> Missing(Span<byte> buffer)
        {
            var set = new HashSet<byte>(MOVES);

            foreach (var b in buffer)
            {
                set.Remove(b);
            }

            return set;
        }

        public static int GetLegalValues(Span<byte> buffer, int i)
        {
            var buffer2d = buffer.AsSpan2D(9, 9);

            if (GetValue(buffer, i) != 0)
            {
                return 511;
            }

            (int x, int y) = IndexToXY(i);

            int flag = 0;

            foreach (var b in GetSquare(buffer2d, x, y))
            {
                flag |= 256 >> (9 - b);
            }

            foreach (var b in buffer2d.GetColumn(x))
            {
                flag |= 256 >> (9 - b);
            }

            foreach (var b in buffer2d.GetRow(y))
            {
                flag |= 256 >> (9 - b);
            }

            return flag;
        }

        public static Span<byte> Generate(int seed)
        {
            var rng = new Random(seed);
            var i = 0;

            Span<byte> buffer = stackalloc byte[81];

            var attempt_stack = new Stack<Frame>(72);

            int flag = 0;

            // Perform the first 9 moves with no legal checks
            for (i = 0; i < 9; i++)
            {
                var bits = (int)System.Runtime.Intrinsics.X86.Popcnt.PopCount((uint)flag);
                var move_index = rng.Next(0, 9 - bits);

                byte p = 0;
                int n = 0;
                while (n <= move_index)
                {
                    if ((flag & (1 << p++)) == 0)
                    {
                        n++;
                    }
                }

                flag |= 256 >> (9 - p);

                buffer[i] = p;
            }

            // Perform the rest of the moves with legal checks
            while (i < 81)
            {
                Frame frame;

                if (i >= attempt_stack.Count)
                {
                    frame = new Frame();
                }
                else
                {
                    frame = attempt_stack.Peek();
                }

                flag = GetLegalValues(buffer, i);

                flag |= frame.Attempted;

                if (flag == 511)
                {
                    while (true)
                    {
                        i--;

                        buffer[i] = 0;
                        frame = attempt_stack.Pop();

                        if (frame.Attempted < 511)
                        {
                            flag = GetLegalValues(buffer, i);
                            flag |= frame.Attempted;

                            if (flag < 511)
                            {
                                break;
                            }
                        }
                    }
                }

                var bits = (int)System.Runtime.Intrinsics.X86.Popcnt.PopCount((uint)flag);
                var move_index = rng.Next(0, 9 - bits);

                byte p = 0;
                int n = 0;
                while (n <= move_index)
                {
                    if ((flag & (1 << p++)) == 0)
                    {
                        n++;
                    }
                }

                buffer[i] = p;

                frame.Attempted |= 256 >> (9 - p);

                attempt_stack.Push(frame);

                i++;
            }

            return buffer.ToArray().AsSpan();
        }

        public static void Reduce(Span<byte> board, int[] permitted_gaps, int seed, int difficulty, int required_gaps)
        {
            var rng = new Random(seed);

            var gaps = 0;
            var i = 0;
            var moves = new List<int>(permitted_gaps);
            var impossible = new List<int>(10);

            do
            {
                var spot_index = rng.Next(moves.Count);
                var spot = moves[spot_index];

                var backup = board[spot];
                board[spot] = 0;

                moves.RemoveAt(spot_index);

                if (IsSolvable(board, spot))
                {
                    gaps++;

                    moves.AddRange(impossible);
                    impossible.Clear();
                }
                else
                {
                    board[spot] = backup;

                    impossible.Add(spot);
                }

                if (++i % 200_000 == 0)
                {
                    Console.Write("\rAttempts: {0}    Gaps: {1}", i, gaps);
                }

            } while (gaps < required_gaps);
        }

        public static bool IsSolvable(Span<byte> board, int spot)
        {
            for (int i = 0; i < SOLVERS.Length; i++)
            {
                if (SOLVERS[i].Solve(board, spot) != 0)
                {
                    return true;
                }
            }

            return false;
        }

        public static void Draw(Span2D<byte> board)
        {
            Console.WriteLine("+-------+-------+-------+");

            for (int y = 0; y < 9; y++)
            {
                Console.Write("| ");

                for (int x = 0; x < 9; x++)
                {
                    var value = board[y, x];

                    Console.Write(value != 0 ? value : " ");

                    if (x < 8)
                    {
                        Console.Write(' ');

                        if (x % 3 == 2)
                        {
                            Console.Write("| ");
                        }
                    }
                    else
                    {
                        Console.WriteLine(" |");
                    }
                }

                if (y % 3 == 2)
                {
                    Console.WriteLine("+-------+-------+-------+");
                }
            }
        }
    }

    public struct Frame
    {
        public int Attempted;
    }
}
