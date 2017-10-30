using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FEC
{
    class LoadControlledNewtonRaphson : NonLinearSolution
    {
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
                discretization.UpdateDisplacements(solutionVector);
                internalForcesTotalVector = discretization.CreateTotalInternalForcesVector();
                double[,] stiffnessMatrix = discretization.CreateTotalStiffnessMatrix();
                dU = linearSolver.Solve(stiffnessMatrix, incrementDf);
                solutionVector = VectorOperations.VectorVectorAddition(solutionVector, dU);
                residual = VectorOperations.VectorVectorSubtraction(internalForcesTotalVector, incrementalExternalForcesVector);
                residualNorm = VectorOperations.VectorNorm2(residual);
                int iteration = 0;
                Array.Clear(deltaU, 0, deltaU.Length);
                while (residualNorm > tolerance && iteration < maxIterations)
                {
                    stiffnessMatrix = discretization.CreateTotalStiffnessMatrix();
                    deltaU = VectorOperations.VectorVectorSubtraction(deltaU, linearSolver.Solve(stiffnessMatrix, residual));
                    tempSolutionVector = VectorOperations.VectorVectorAddition(solutionVector, deltaU);
                    discretization.UpdateDisplacements(tempSolutionVector);
                    internalForcesTotalVector = discretization.CreateTotalInternalForcesVector();
                    residual = VectorOperations.VectorVectorSubtraction(internalForcesTotalVector, incrementalExternalForcesVector);
                    residualNorm = VectorOperations.VectorNorm2(residual);
                    iteration = iteration + 1;
                }
                solutionVector = VectorOperations.VectorVectorAddition(solutionVector, deltaU);
                if(iteration >= maxIterations)  Console.WriteLine("Solution not converged at current iterations") ;
            }
            
            return solutionVector;
        }

        public override double[] Solve(IAssembly assembly, ILinearSolution linearScheme, double[] forceVector)
        {
            discretization = assembly;
            linearSolver = linearScheme;
            lambda = 1.0 / numberOfLoadSteps;
            double[] solution = LoadControlledNR(forceVector);
            return solution;
        }

    }
}

