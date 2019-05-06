﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FEC
{
    public class ExplicitSolver
    {
        public IAssembly Assembler { get; set; }
        public ILinearSolution LinearSolver { get; set; }
        public double[] ExternalForcesVector { get; set; }
        public InitialConditions InitialValues { get; set; }
        private int numberOfLoadSteps = 10;
        private double tolerance = 10.0e-2;
        private int maxIterations = 1000;
        private double lambda;
        private double totalTime;
        private int timeStepsNumber;
        private double timeStep;
        private double a0, a1, a2, a3;
        private Dictionary<int, double[]> explicitSolution = new Dictionary<int, double[]>();
        private Dictionary<int, double[]> explicitAcceleration = new Dictionary<int, double[]>();
        public bool ActivateNonLinearSolution { get; set; }
        public double[,] CustomStiffnessMatrix { get; set; }
        public double[,] CustomMassMatrix { get; set; }
        public double[,] CustomDampingMatrix { get; set; }

        public ExplicitSolver(double totalTime, int timeStepsNumber)
        {
            this.totalTime = totalTime;
            this.timeStepsNumber = timeStepsNumber;
            timeStep = totalTime / timeStepsNumber;
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
            //Assembler.UpdateAccelerations(CalculateAccelerations(InitialValues.InitialAccelerationVector));

            for (int i = 0; i < numberOfLoadSteps; i++)
            {
                incrementalExternalForcesVector = VectorOperations.VectorVectorAddition(incrementalExternalForcesVector, incrementDf);
                Assembler.UpdateDisplacements(solutionVector);

                Assembler.UpdateAccelerations(explicitAcceleration.Values.Last());

                internalForcesTotalVector = Assembler.CreateTotalInternalForcesVector();

                double[,] tangentMatrix = CalculateHatMMatrix();
                dU = LinearSolver.Solve(tangentMatrix, incrementDf);
                solutionVector = VectorOperations.VectorVectorAddition(solutionVector, dU);
                
                Assembler.UpdateDisplacements(solutionVector);
                tangentMatrix = CalculateHatMMatrix();
                internalForcesTotalVector = Assembler.CreateTotalInternalForcesVector();

                residual = VectorOperations.VectorVectorSubtraction(internalForcesTotalVector, incrementalExternalForcesVector);
                residualNorm = VectorOperations.VectorNorm2(residual);
                int iteration = 0;
                Array.Clear(deltaU, 0, deltaU.Length);
                //while (residualNorm > tolerance && iteration < maxIterations)
                //{
                //    tangentMatrix = CalculateHatMMatrix();
                //    deltaU = VectorOperations.VectorVectorSubtraction(deltaU, LinearSolver.Solve(tangentMatrix, residual));
                //    tempSolutionVector = VectorOperations.VectorVectorAddition(solutionVector, deltaU);
                //    Assembler.UpdateDisplacements(tempSolutionVector);

                //    Assembler.UpdateAccelerations(CalculateAccelerations(solutionVector));

                //    internalForcesTotalVector = Assembler.CreateTotalInternalForcesVector();
                //    residual = VectorOperations.VectorVectorSubtraction(internalForcesTotalVector, incrementalExternalForcesVector);
                //    residualNorm = VectorOperations.VectorNorm2(residual);
                //    iteration = iteration + 1;
                //}
                //solutionVector = VectorOperations.VectorVectorAddition(solutionVector, deltaU);
                //if (iteration >= maxIterations) Console.WriteLine("Newton-Raphson: Solution not converged at current iterations");
            }

            return solutionVector;
        }

        private double[] NewtonIterations(double[] forceVector)
        {

            lambda = 1.0 / numberOfLoadSteps;
            double[] incrementDf = VectorOperations.VectorScalarProductNew(forceVector, lambda);
            double[] solutionVector = new double[forceVector.Length];
            double[] deltaU = new double[solutionVector.Length];
            double[] internalForcesTotalVector;
            double[] dU;
            double[] residual;
            double residualNorm;

            solutionVector = explicitSolution.Values.Last();

            Assembler.UpdateDisplacements(solutionVector);

            Assembler.UpdateAccelerations(explicitAcceleration.Values.Last());
            residual = VectorOperations.VectorVectorSubtraction(forceVector, Assembler.CreateTotalInternalForcesVector());
            int iteration = 0;
            Array.Clear(deltaU, 0, deltaU.Length);

            for (int i = 0; i < maxIterations; i++)
            {
                double[,] tangentMatrix = CalculateHatMMatrix();
                deltaU = LinearSolver.Solve(tangentMatrix, residual);
                solutionVector = VectorOperations.VectorVectorAddition(solutionVector, deltaU);
                Assembler.UpdateDisplacements(solutionVector);
                //Assembler.UpdateAccelerations(CalculateAccelerations());

                internalForcesTotalVector = Assembler.CreateTotalInternalForcesVector();
                residual = VectorOperations.VectorVectorSubtraction(forceVector, internalForcesTotalVector);
                residualNorm = VectorOperations.VectorNorm2(residual);
                if (residualNorm < tolerance)
                {
                    break;
                }
                iteration = iteration + 1;
            }
            Console.WriteLine(iteration);
            if (iteration >= maxIterations) Console.WriteLine("Newton-Raphson: Solution not converged at current iterations");

            return solutionVector;
        }

        //private double[] UpdatedF(double[] forceVector)
        //{
            

        //    lambda = 1.0 / numberOfLoadSteps;
        //    double[] incrementDf = VectorOperations.VectorScalarProductNew(forceVector, lambda);
        //    double[] solutionVector = new double[forceVector.Length];
        //    double[] incrementalExternalForcesVector = new double[forceVector.Length];
        //    double[] tempSolutionVector = new double[solutionVector.Length];
        //    double[] deltaU = new double[solutionVector.Length];
        //    double[] internalForcesTotalVector;
        //    double[] dU;





        //    double[] currentU = explicitSolution.Values.Last();
        //    Assembler.UpdateDisplacements(currentU);
        //    internalForcesTotalVector = Assembler.CreateTotalInternalForcesVector();
        //    double[,] TotalMassMatrix = Assembler.CreateTotalMassMatrix();
        //    double[] a2_M_ut = VectorOperations.MatrixVectorProduct(
        //                        MatrixOperations.ScalarMatrixProductNew(a2, TotalMassMatrix), currentU);

        //    double[] a0_M_ut = VectorOperations.MatrixVectorProduct(
        //                        MatrixOperations.ScalarMatrixProductNew(a0, TotalMassMatrix), currentU);

        //    double[,] hatMMatrix = CalculateHatMMatrix();
        //    double[] hatM_utprevious = VectorOperations.MatrixVectorProduct(hatMMatrix, )

            

        
        //double[,] hatK = MatrixOperations.MatrixSubtraction(TotalStiffnessMatrix,
        //                    MatrixOperations.ScalarMatrixProductNew(a2, TotalMassMatrix));
        //double[] hatCurrentU = VectorOperations.MatrixVectorProduct(hatKMatrix, explicitSolution[i - 1]);
        //    double[] hatPreviousU = VectorOperations.MatrixVectorProduct(hatMMatrix, explicitSolution[i - 2]);

        //    double[] hatR = VectorOperations.VectorVectorSubtraction(ExternalForcesVector,
        //                    VectorOperations.VectorVectorAddition(hatCurrentU, hatPreviousU));



        //    double[,] tangentMatrix = CalculateHatMMatrix();
        //        dU = LinearSolver.Solve(tangentMatrix, incrementDf);
        //        solutionVector = VectorOperations.VectorVectorAddition(solutionVector, dU);

        //        Assembler.UpdateDisplacements(solutionVector);
        //        tangentMatrix = CalculateHatMMatrix();
        //        internalForcesTotalVector = Assembler.CreateTotalInternalForcesVector();

        //        residual = VectorOperations.VectorVectorSubtraction(internalForcesTotalVector, incrementalExternalForcesVector);
        //        residualNorm = VectorOperations.VectorNorm2(residual);
        //        int iteration = 0;
        //        Array.Clear(deltaU, 0, deltaU.Length);
                
            

        //    return solutionVector;
        //}

        /// <summary>
        /// Calculates accelerations for time t
        /// </summary>
        /// <returns></returns>
        private double[] CalculateAccelerations() //Bathe page 771
        {
            int steps = explicitSolution.Count;
            double[] aCurrent =
                VectorOperations.VectorScalarProductNew(
                    VectorOperations.VectorVectorAddition(explicitSolution[steps - 4],
                        VectorOperations.VectorVectorAddition(
                            VectorOperations.VectorScalarProductNew(explicitSolution[steps - 3], -2.0), explicitSolution[steps-2])), a0);

            return aCurrent;
        }

        private double[] CalculateInitialAccelerations() //Bathe page 771
        {
            int step = explicitSolution.Count - 2;
            Assembler.UpdateDisplacements(explicitSolution[step]);
            double[,] stiffness = Assembler.CreateTotalStiffnessMatrix();
            double[,] mass = Assembler.CreateTotalMassMatrix();

            double[] Ku = VectorOperations.MatrixVectorProduct(stiffness, explicitSolution[step]);
            double[] RHS = VectorOperations.VectorVectorSubtraction(ExternalForcesVector, Ku);

            double[] acceleration = LinearSolver.Solve(mass, RHS);

            return acceleration;
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
            double[,] TotalMassMatrix;
            double[,] TotalDampingMatrix;
            if (CustomMassMatrix != null)
            {
                TotalMassMatrix = CustomMassMatrix;
                TotalDampingMatrix = CustomDampingMatrix;
            }
            else
            {
                TotalMassMatrix = Assembler.CreateTotalMassMatrix();
                TotalDampingMatrix = Assembler.CreateTotalDampingMatrix();
            }
            double[,] a0M = MatrixOperations.ScalarMatrixProductNew(a0, TotalMassMatrix);
            double[,] a1C = MatrixOperations.ScalarMatrixProductNew(a1, TotalDampingMatrix);
            double[,] hutM = MatrixOperations.MatrixAddition(a0M, a1C);
            return hutM;
        }

        private double[,] CalculateHatKMatrix()
        {
            double[,] TotalMassMatrix;
            double[,] TotalStiffnessMatrix;
            if (CustomStiffnessMatrix != null)
            {
                TotalMassMatrix = CustomMassMatrix;
                TotalStiffnessMatrix = CustomStiffnessMatrix;
            }
            else
            {
                TotalMassMatrix = Assembler.CreateTotalMassMatrix();
                TotalStiffnessMatrix = Assembler.CreateTotalStiffnessMatrix();
            }
            double[,] hatK = MatrixOperations.MatrixSubtraction(TotalStiffnessMatrix,
                                MatrixOperations.ScalarMatrixProductNew(a2, TotalMassMatrix));
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

        private double[] CalculateHatRVectorNL(int i)
        {

            Assembler.UpdateDisplacements(explicitSolution[i - 1]);

        double[,] totalMassMatrix = Assembler.CreateTotalMassMatrix();
        double[,] totalDampingMatrix = Assembler.CreateTotalDampingMatrix();
        double[,] a2M = MatrixOperations.ScalarMatrixProductNew(a2, totalMassMatrix);
        double[,] a0M = MatrixOperations.ScalarMatrixProductNew(a0, totalMassMatrix);
        double[,] a1C = MatrixOperations.ScalarMatrixProductNew(-a1, totalDampingMatrix);
        double[,] hutM = MatrixOperations.MatrixAddition(a0M, a1C);

            double[] F = Assembler.CreateTotalInternalForcesVector();
            double[] hatPreviousU = VectorOperations.MatrixVectorProduct(hutM, explicitSolution[i - 2]);
            double[] a2Mut = VectorOperations.MatrixVectorProduct(a2M, explicitSolution[i - 1]);


            double[] hatR1 = VectorOperations.VectorVectorSubtraction(ExternalForcesVector, F);
            double[] hatR2 = VectorOperations.VectorVectorSubtraction(a2Mut, hatPreviousU);
            double[] hatRtotal = VectorOperations.VectorVectorAddition(hatR1, hatR2);
            return hatRtotal;
        }


        public void SolveExplicit()
        {
            double[,] hatMassMatrix = CalculateHatMMatrix();
            explicitSolution.Add(-1, CalculatePreviousDisplacementVector());
            explicitSolution.Add(0, InitialValues.InitialDisplacementVector);
            explicitAcceleration.Add(0, CalculateInitialAccelerations());
            double[] nextSolution;
            for (int i = 1; i < timeStepsNumber; i++)
            {
                double time = i * timeStep + InitialValues.InitialTime;
                
                if (ActivateNonLinearSolution == false)
                {
                    double[] hatRVector = CalculateHatRVector(i);
                    nextSolution = LinearSolver.Solve(hatMassMatrix, hatRVector);
                    Console.WriteLine("Solution for Load Step {0} is:", i);
                    VectorOperations.PrintVector(nextSolution);
                }
                else
                {
                    double[] hatRVector = CalculateHatRVectorNL(i);
                    nextSolution = LinearSolver.Solve(hatMassMatrix, hatRVector);
                    //nextSolution = NewtonIterations(hatRVector);
                    Console.WriteLine("Solution for Load Step {0} is:", i);
                    VectorOperations.PrintVector(nextSolution);
                }
                explicitSolution.Add(i, nextSolution);
                explicitAcceleration.Add(i, CalculateAccelerations());
            }
        }

        public void PrintExplicitSolution()
        {
            foreach (KeyValuePair<int, double[]> element in explicitSolution)
            {
                int step = element.Key;
                double[] solutionInStep = element.Value;
                Console.WriteLine("Solution for Load Step {0} is:", step);
                VectorOperations.PrintVector(solutionInStep);
            }
        }
    }
}
