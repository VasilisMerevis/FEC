using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FEC
{
    public abstract class DynamicSolver
    {
        public DynamicScheme dynamicScheme;
        public IAssembly assembly;
        public ILinearSolution linearSolver;
        public double[] ExternalVector { get; set; }

        protected void UpdateDisplacements(double[] solutionVector)
        {
            assembly.UpdateDisplacements(solutionVector);
        }

        protected double[] CreateTotalInternalForcesVector()
        {
            return assembly.CreateTotalInternalForcesVector();
        }

        protected virtual double[,] CreateTotalStiffnessMatrix()
        {
            return assembly.CreateTotalStiffnessMatrix();
        }

        public double[] SolveDynamic()
        {
            dynamicScheme.SolveExplicit();
            return dynamicScheme.GetExplicitSolution;
        }

        protected virtual double[] Solve(double[,] matrix, double[] rhsVector)
        {
            throw new Exception("Not implemented");
        }

    }
}
