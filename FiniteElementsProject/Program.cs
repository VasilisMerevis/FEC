using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FEC
{
    class Program
    {
        static void Main(string[] args)
        {
            //LinearTrussExample.RunExample();
            //Console.WriteLine();
            //LinearFrameExample.RunExample();
            //Console.WriteLine();
            //LinearFrameTrussHybridExample.RunExample();
            //Console.WriteLine();
            NonLinearCantileverExample.RunExample();
            Console.WriteLine();
            //LinearTrussInContactExample.RunExample();
            //Console.WriteLine();
            //CantileverInContact.RunExample();
            //Console.WriteLine();
            TwoBeamsInContactExample.RunExample();
            Console.WriteLine();

            double[,] matrixA = new double[,]
                { { 10,1,-5 }, {-20,3,-20 }, {5,3,5 } };
            double[] RHS = new double[] { 1, 2, 6 };
            BiCGSTABSolver test = new BiCGSTABSolver();
            double[] solution = test.Solve(matrixA, RHS);
        }
    }
}
