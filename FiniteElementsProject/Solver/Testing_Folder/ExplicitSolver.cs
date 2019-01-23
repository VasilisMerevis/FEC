using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FEC
{
    class ExplicitSolver
    {
        public IAssembly Assembler { get; set; }
        public ILinearSolution LinearSolver { get; set; }
        public double[] ExternalForcesVector { get; set; }
        public InitialConditions InitialValues { get; set; }
        private int numberOfLoadSteps;
        private double tolerance;
        private int maxIterations;
        private double lambda;
        private double totalTime;
        private int timeStepsNumber;
        private double timeStep;
        private double[,] dampingMatrix;
        private double a0, a1, a2, a3;
        private Dictionary<int, double[]> explicitSolution = new Dictionary<int, double[]>();

        public ExplicitSolver(double totalTime, int timeStepsNumber)
        {
            this.totalTime = totalTime;
            this.timeStepsNumber = timeStepsNumber;
            timeStep = totalTime / timeStepsNumber;
            
            //dampingMatrix = new double[totalDOFs, totalDOFs];

            a0 = 1.0 / (timeStep * timeStep);
            a1 = 1.0 / (2.0 * timeStep);
            a2 = 2.0 * a0;
            a3 = 1.0 / a2;
        }

        private double[] LoadControlledNR(double[] forceVector)
        {

            lambda = 1.0 / numberOfLoadSteps;
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
                Assembler.UpdateDisplacements(solutionVector);
                internalForcesTotalVector = Assembler.CreateTotalInternalForcesVector();
                double[,] stiffnessMatrix = Assembler.CreateTotalStiffnessMatrix();
                dU = LinearSolver.Solve(stiffnessMatrix, incrementDf);
                solutionVector = VectorOperations.VectorVectorAddition(solutionVector, dU);
                residual = VectorOperations.VectorVectorSubtraction(internalForcesTotalVector, incrementalExternalForcesVector);
                residualNorm = VectorOperations.VectorNorm2(residual);
                int iteration = 0;
                Array.Clear(deltaU, 0, deltaU.Length);
                while (residualNorm > tolerance && iteration < maxIterations)
                {
                    stiffnessMatrix = Assembler.CreateTotalStiffnessMatrix();
                    deltaU = VectorOperations.VectorVectorSubtraction(deltaU, LinearSolver.Solve(stiffnessMatrix, residual));
                    tempSolutionVector = VectorOperations.VectorVectorAddition(solutionVector, deltaU);
                    Assembler.UpdateDisplacements(tempSolutionVector);
                    internalForcesTotalVector = Assembler.CreateTotalInternalForcesVector();
                    residual = VectorOperations.VectorVectorSubtraction(internalForcesTotalVector, incrementalExternalForcesVector);
                    residualNorm = VectorOperations.VectorNorm2(residual);
                    iteration = iteration + 1;
                }
                solutionVector = VectorOperations.VectorVectorAddition(solutionVector, deltaU);
                if (iteration >= maxIterations) Console.WriteLine("Newton-Raphson: Solution not converged at current iterations");
            }

            return solutionVector;
        }

        

        private double[] CalculatePreviousDisplacementVector()
        {
            double[] previousDisp = VectorOperations.VectorVectorAddition(
                                    VectorOperations.VectorVectorSubtraction(InitialValues.InitialDisplacementVector,
                                    VectorOperations.VectorScalarProductNew(InitialValues.InitialVelocityVector, timeStep)),
                                    VectorOperations.VectorScalarProductNew(InitialValues.InitialAccelerationVector, a3));
            return previousDisp;
        }

        private double[,] CalculateHatMMatrix()
        {
            double[,] a0M = MatrixOperations.ScalarMatrixProductNew(a0, Assembler.CreateTotalMassMatrix());
            double[,] a1C = MatrixOperations.ScalarMatrixProductNew(a1, dampingMatrix);
            double[,] hutM = MatrixOperations.MatrixAddition(a0M, a1C);
            return hutM;
        }

        private double[,] CalculateHatKMatrix()
        {
            double[,] hatK = MatrixOperations.MatrixSubtraction(Assembler.CreateTotalStiffnessMatrix(),
                                MatrixOperations.ScalarMatrixProductNew(a2, Assembler.CreateTotalMassMatrix()));
            return hatK;
        }

        private double[] CalculateHatRVector(int i)
        {
            double[,] hatKMatrix = CalculateHatKMatrix();
            double[,] hatMMatrix = CalculateHatMMatrix();
            double[] hatCurrentU = VectorOperations.MatrixVectorProduct(hatKMatrix, explicitSolution[i - 1]);
            double[] hatPreviousU = VectorOperations.MatrixVectorProduct(hatMMatrix, explicitSolution[i - 2]);

            double[] hatR = VectorOperations.VectorVectorSubtraction(ExternalForcesVector,
                            VectorOperations.VectorVectorAddition(hatCurrentU, hatPreviousU));
            return hatR;
        }


        public void SolveExplicit()
        {
            double[,] hatMassMatrix = CalculateHatMMatrix();
            explicitSolution.Add(-1, CalculatePreviousDisplacementVector());
            explicitSolution.Add(0, InitialValues.InitialDisplacementVector);
            for (int i = 1; i < timeStepsNumber; i++)
            {
                double time = i * timeStep + InitialValues.InitialTime;
                double[] hatRVector = CalculateHatRVector(i);
                double[] nextSolution = LinearSolver.Solve(hatMassMatrix, hatRVector);
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
    }
}
