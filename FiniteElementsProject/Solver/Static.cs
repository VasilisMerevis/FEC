using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FEC
{
    class StaticSolver : ISolver
    {
        private double[] staticSolutionVector;
        public IAssembly AssemblyData { get; set; }
        public ILinearSolution LinearScheme { get; set; }
        public INonLinearSolution NonLinearScheme { get; set; }
        public bool ActivateNonLinearSolver { get; set; }
        public double[,] CustomStiffnessMatrix { get; set; }

        public void Solve(double[] rhsVector)
        {
            if (ActivateNonLinearSolver == true)
            {
                staticSolutionVector = NonLinearScheme.Solve(AssemblyData, LinearScheme, rhsVector);
            }
            else
            {
                if (CustomStiffnessMatrix == null)
                {
                    double[,] coefMatrix = AssemblyData.CreateTotalStiffnessMatrix();
                    staticSolutionVector = LinearScheme.Solve(coefMatrix, rhsVector);
                }
                //double[,] coefMatrix = AssemblyData.CreateTotalStiffnessMatrix();
                //staticSolutionVector = LinearScheme.Solve(coefMatrix, rhsVector);
                else
                {
                    double[,] coefMatrix = CustomStiffnessMatrix;
                    staticSolutionVector = LinearScheme.Solve(coefMatrix, rhsVector);
                }
            }
        }

        public void PrintSolution()
        {
            VectorOperations.PrintVector(staticSolutionVector);
        }

    }
}