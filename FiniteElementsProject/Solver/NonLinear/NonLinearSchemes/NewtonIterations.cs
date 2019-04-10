using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FEC
{
    public class NewtonIterations: NonLinearSolution
    {
        private double[] StartNewtonIterations(double[] forceVector)
        {

            lambda = 1.0 / numberOfLoadSteps;

            double[] solutionVector = new double[forceVector.Length];
            double[] tempSolutionVector = new double[solutionVector.Length];
            double[] deltaU = new double[solutionVector.Length];
            double[] internalForcesTotalVector;
            double[] dU;
            double[] residual;
            double residualNorm;

            discretization.UpdateDisplacements(solutionVector);
            residual = VectorOperations.VectorVectorSubtraction(forceVector, discretization.CreateTotalInternalForcesVector());
            int iteration = 0;
            Array.Clear(deltaU, 0, deltaU.Length);
            for (int i = 0; i < maxIterations; i++)
            {
                double[,] tangentMatrix = discretization.CreateTotalStiffnessMatrix();
                deltaU = linearSolver.Solve(tangentMatrix, residual);
                solutionVector = VectorOperations.VectorVectorAddition(solutionVector, deltaU);
                discretization.UpdateDisplacements(solutionVector);

                internalForcesTotalVector = discretization.CreateTotalInternalForcesVector();
                residual = VectorOperations.VectorVectorSubtraction(forceVector, internalForcesTotalVector);
                residualNorm = VectorOperations.VectorNorm2(residual);
                if (residualNorm < tolerance)
                {
                    continue;
                }
                iteration = iteration + 1;
            }
            if (iteration >= maxIterations) Console.WriteLine("Newton-Raphson: Solution not converged at current iterations");

            return solutionVector;
        }

        public override double[] Solve(IAssembly assembly, ILinearSolution linearScheme, double[] forceVector)
        {
            discretization = assembly;
            linearSolver = linearScheme;
            lambda = 1.0 / numberOfLoadSteps;
            double[] solution = StartNewtonIterations(forceVector);
            return solution;
        }
    }
}
