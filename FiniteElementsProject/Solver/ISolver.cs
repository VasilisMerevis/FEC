using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FEC
{
    interface ISolver
    {
        ILinearSolution LinearScheme { get; set; }
        IAssembly AssemblyData { get; set; }
        void Solve(double[] rhsVector);
        bool ActivateNonLinearSolver { get; set; }
        INonLinearSolution NonLinearScheme { get; set; }
        void PrintSolution();
        double[,] CustomStiffnessMatrix { get; set; }
    }
}