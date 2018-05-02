using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FEC
{
    public static class NonSymmetricSystem
    {
        public static void Solve()
        {
            double[,] matrixA = new double[,]
                { { 10,1,-5 }, {-20,3,20 }, {5,3,5 } };
            double[] RHS = new double[] { 1, 2, 6 };
            BiCGSTABSolver test = new BiCGSTABSolver();
            double[] solution = test.Solve(matrixA, RHS);
            VectorOperations.PrintVector(solution);
        }
    }
}
