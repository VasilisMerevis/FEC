using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FEC
{
    class NLExplicit
    {
        private IAssembly discretization;
        private ILinearSolution linearSolver;

        private double totalTime, timeStep;
        private int timeStepsNumber;
        private Dictionary<int, double[]> explicitSolution = new Dictionary<int, double[]>();
        int totalDOFs;
        double[,] massMatrix, dampingMatrix;
        double[,] stiffnessMatrix;
        double[] externalForcesVector;
        double a0, a1, a2, a3;
        double[] initialDisplacementVector, initialVelocityVector, initialAccelerationVector;
        double initialTime;
        double lambda;
        double numberOfLoadSteps;
        double tolerance = 1e-5;
        int maxIterations = 1000;

        public NLExplicit(IAssembly discretization, ILinearSolution linearSolver, InitialConditions initialValues, double totalTime, int timeStepsNumber, double[,] stiffnessMatrix, double[,] massMatrix, double[] externalForcesVector)
        {
            this.discretization = discretization;
            totalDOFs = stiffnessMatrix.GetLength(0);
            this.totalTime = totalTime;
            this.timeStepsNumber = timeStepsNumber;
            timeStep = totalTime / timeStepsNumber;
            this.massMatrix = massMatrix;
            this.stiffnessMatrix = stiffnessMatrix;
            dampingMatrix = new double[totalDOFs, totalDOFs];
            this.externalForcesVector = externalForcesVector;
            initialDisplacementVector = initialValues.InitialDisplacementVector;
            initialVelocityVector = initialValues.InitialVelocityVector;
            initialAccelerationVector = initialValues.InitialAccelerationVector;
            initialTime = initialValues.InitialTime;
            this.linearSolver = linearSolver;
            a0 = 1.0 / (timeStep * timeStep);
            a1 = 1.0 / (2.0 * timeStep);
            a2 = 2.0 * a0;
            a3 = 1.0 / a2;
            numberOfLoadSteps = 10.0;
        }

        #region Explicit
        private double[] CalculatePreviousDisplacementVector()
        {
            double[] previousDisp = VectorOperations.VectorVectorAddition(
                                    VectorOperations.VectorVectorSubtraction(initialDisplacementVector,
                                    VectorOperations.VectorScalarProductNew(initialVelocityVector, timeStep)),
                                    VectorOperations.VectorScalarProductNew(initialAccelerationVector, a3));
            return previousDisp;
        }

        private double[,] CalculateHatMMatrix()
        {
            double[,] a0M = MatrixOperations.ScalarMatrixProductNew(a0, massMatrix);
            double[,] a1C = MatrixOperations.ScalarMatrixProductNew(a1, dampingMatrix);
            double[,] hutM = MatrixOperations.MatrixAddition(a0M, a1C);
            return hutM;
        }

        private double[,] CalculateHatKMatrix()
        {
            double[,] hatK = MatrixOperations.MatrixSubtraction(stiffnessMatrix,
                                MatrixOperations.ScalarMatrixProductNew(a2, massMatrix));
            return hatK;
        }

        private double[] CalculateHatRVector(int i)
        {
            double[,] hatKMatrix = CalculateHatKMatrix();
            double[,] hatMMatrix = CalculateHatMMatrix();
            double[] hatCurrentU = VectorOperations.MatrixVectorProduct(hatKMatrix, explicitSolution[i - 1]);
            double[] hatPreviousU = VectorOperations.MatrixVectorProduct(hatMMatrix, explicitSolution[i - 2]);

            double[] hatR = VectorOperations.VectorVectorSubtraction(externalForcesVector,
                            VectorOperations.VectorVectorAddition(hatCurrentU, hatPreviousU));
            return hatR;
        }


        public void SolveExplicit()
        {
            double[,] hatMassMatrix = CalculateHatMMatrix();
            explicitSolution.Add(-1, CalculatePreviousDisplacementVector());
            explicitSolution.Add(0, initialDisplacementVector);
            for (int i = 1; i < timeStepsNumber; i++)
            {
                double time = i * timeStep + initialTime;
                double[] hatRVector = CalculateHatRVector(i);
                double[] nextSolution = NLSolve(hatMassMatrix, hatRVector);
                explicitSolution.Add(i, nextSolution);
            }
        }

        public void PrintExplicitSolution()
        {
            foreach (KeyValuePair<int, double[]> element in explicitSolution)
            {
                int step = element.Key;
                double[] solutionInStep = element.Value;
                Console.WriteLine("Step is{0}", step);
                VectorOperations.PrintVector(solutionInStep);
            }
        }
        #endregion

        #region NonLinear
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
                if (iteration >= maxIterations) Console.WriteLine("Newton-Raphson: Solution not converged at current iterations");
            }

            return solutionVector;
        }

        private double[] NLSolve()
        {
            lambda = 1.0 / numberOfLoadSteps;
            double[] solution = LoadControlledNR(externalForcesVector);
            return solution;
        }
        #endregion
    }
}
