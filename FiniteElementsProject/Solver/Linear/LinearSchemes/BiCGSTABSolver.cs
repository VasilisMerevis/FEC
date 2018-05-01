using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FEC
{
    class BiCGSTABSolver : LinearSolution
    {
        int maxIterations = 1000;
        double tolerance = 1e-12;

        private double[] BiCGSTAB(double[,] stiffnessMatrix, double[] forceVector)
        {
            double[] solutionVector = new double[forceVector.Length];

            double[] xVector = new double[forceVector.Length];
            double[] pVector = new double[forceVector.Length];
            double[] vVector = new double[forceVector.Length];

            xVector = new double[] { 1, 1, 1 };

            double[,] K = stiffnessMatrix;

            double[] bVector = forceVector;
            double[] rVector = VectorOperations.VectorVectorSubtraction(bVector, VectorOperations.MatrixVectorProduct(K, xVector));
            double[] r0hatVector = rVector;

            double rho0 = 1.0;
            double w = 1.0;
            double a = 1.0;
            double rho1 = VectorOperations.VectorDotProduct(r0hatVector, rVector);
            double b;

            double[] sVector;
            double[] tVector;
            int iters = 100;
            double converged;
            for (int i = 0; i < iters; i++)
            {
                b = (rho1 / rho0) * (a / w);
                pVector = VectorOperations.VectorVectorAddition(rVector,
                    VectorOperations.VectorScalarProductNew(
                        VectorOperations.VectorVectorSubtraction(pVector,
                        VectorOperations.VectorScalarProductNew(vVector, w)), b));
                vVector = VectorOperations.MatrixVectorProduct(K, pVector);
                a = rho1 / VectorOperations.VectorDotProduct(r0hatVector, vVector);
                sVector = VectorOperations.VectorVectorSubtraction(rVector,
                    VectorOperations.VectorScalarProductNew(vVector, a));
                tVector = VectorOperations.MatrixVectorProduct(K, sVector);
                w = VectorOperations.VectorDotProduct(tVector, sVector) / VectorOperations.VectorDotProduct(tVector, tVector);
                rho0 = rho1;
                rho1 = -w * VectorOperations.VectorDotProduct(r0hatVector, tVector);
                xVector = VectorOperations.VectorVectorAddition(xVector,
                    VectorOperations.VectorScalarProductNew(pVector, a));
                xVector = VectorOperations.VectorVectorAddition(xVector,
                    VectorOperations.VectorScalarProductNew(sVector, w));
                rVector = VectorOperations.VectorVectorSubtraction(sVector,
                    VectorOperations.VectorScalarProductNew(tVector, w));
                converged = VectorOperations.VectorNorm2(rVector);
                if (i == iters | converged < 0.0001)
                {
                    break;
                }
            }
            solutionVector = xVector;
            return solutionVector;
        }

        public override double[] Solve(double[,] stiffnessMatrix, double[] forceVector)
        {
            double[] solution = BiCGSTAB(stiffnessMatrix, forceVector);
            return solution;
        }
    }
}
