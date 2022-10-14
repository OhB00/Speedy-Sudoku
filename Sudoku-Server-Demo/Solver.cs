using CommunityToolkit.HighPerformance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sudoku_Server_Demo
{
    public abstract class Solver
    {
        protected Solver(int difficulty, string name)
        {
            Difficulty = difficulty;
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        public int Difficulty { get; set; }
        public string Name { get; set; }

        public abstract int Solve(Span<byte> board, int position);

        public static int GetFlagNumber(int flag)
        {
            var bits = (int)System.Runtime.Intrinsics.X86.Popcnt.PopCount((uint)flag);

            if (9 - bits != 1)
            {
                return 0;
            }
            else
            {
                byte p = 0;
                while (p < 9)
                {
                    if ((flag & (1 << p++)) == 0)
                    {
                        return p;
                    }
                }
            }

            return 0;
        }
    }

    public class SquareSolver : Solver
    {
        public SquareSolver() : base(1, "Sqaure Solver")
        {
        }

        public override int Solve(Span<byte> board, int position)
        {
            (int x, int y) = Board.IndexToXY(position);

            int flag = 0;
            foreach (var b in Board.GetSquare(board.AsSpan2D(9, 9), x, y))
            {
                flag |= 256 >> (9 - b);
            }

            return GetFlagNumber(flag);
        }
    }

    public class LineSolver : Solver
    {
        public LineSolver() : base(2, "Line Solver")
        {
        }

        public override int Solve(Span<byte> board, int position)
        {
            (int x, int y) = Board.IndexToXY(position);

            var board2d = board.AsSpan2D(9, 9);

            int flag = 0;
            foreach (var b in board2d.GetColumn(x))
            {
                flag |= 256 >> (9 - b);
            }

            foreach (var b in board2d.GetRow(y))
            {
                flag |= 256 >> (9 - b);
            }

            return GetFlagNumber(flag);
        }
    }

    public class LegalSolver : Solver
    {
        public LegalSolver() : base(3, "Legal Solver")
        {
        }

        public override int Solve(Span<byte> board, int position)
        { 
            var flag = Board.GetLegalValues(board, position);

            return GetFlagNumber(flag);
        }
    }
}
