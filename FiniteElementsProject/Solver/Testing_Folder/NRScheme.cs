using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FEC
{
    public class NRScheme : DynamicSolver
    {
        double tolerance = 1e-5;
        int maxIterations = 1000;
        int numberOfLoadSteps = 10;
        double lambda;

        private double[] LoadControlledNR(double[] forceVector)
        {
            double[] incrementDf = VectorOperations.VectorScalarProductNew(forceVector, lambda);
            double[] solutionVector = new double[forceVector.Length];
            double[] incrementalExternalForcesVector = new double[forceVector.Length];
            double[] tempSolutionVector = new double[solutionVector.Length];
            double[] deltaU = new double[solutionVector.Length];
            double[] internalForcesTotalVector;
            double[] dU;
            double[] residual;
            double residualNorm;
            
            for (int i = 0; i < numberOfLoadSteps; i++)
            {
                incrementalExternalForcesVector = VectorOperations.VectorVectorAddition(incrementalExternalForcesVector, incrementDf);
                base.UpdateDisplacements(solutionVector);
                internalForcesTotalVector = base.CreateTotalInternalForcesVector();
                double[,] stiffnessMatrix = base.CreateTotalStiffnessMatrix();
                dU = linearSolver.Solve(stiffnessMatrix, incrementDf);
                solutionVector = VectorOperations.VectorVectorAddition(solutionVector, dU);
                residual = VectorOperations.VectorVectorSubtraction(internalForcesTotalVector, incrementalExternalForcesVector);
                residualNorm = VectorOperations.VectorNorm2(residual);
                int iteration = 0;
                Array.Clear(deltaU, 0, deltaU.Length);
                while (residualNorm > tolerance && iteration < maxIterations)
                {
                    stiffnessMatrix = base.CreateTotalStiffnessMatrix();
                    deltaU = VectorOperations.VectorVectorSubtraction(deltaU, linearSolver.Solve(stiffnessMatrix, residual));
                    tempSolutionVector = VectorOperations.VectorVectorAddition(solutionVector, deltaU);
                    base.UpdateDisplacements(tempSolutionVector);
                    internalForcesTotalVector = base.CreateTotalInternalForcesVector();
                    residual = VectorOperations.VectorVectorSubtraction(internalForcesTotalVector, incrementalExternalForcesVector);
                    residualNorm = VectorOperations.VectorNorm2(residual);
                    iteration = iteration + 1;
                }
                solutionVector = VectorOperations.VectorVectorAddition(solutionVector, deltaU);
                if (iteration >= maxIterations) Console.WriteLine("Newton-Raphson: Solution not converged at current iterations");
            }

            return solutionVector;
        }

        protected override double[] Solve(double[,] stiffnessMatrix, double[] forceVector)
        {
            lambda = 1.0 / numberOfLoadSteps;
            double[] solution = LoadControlledNR(forceVector);
            return solution;
        }

    }
}


