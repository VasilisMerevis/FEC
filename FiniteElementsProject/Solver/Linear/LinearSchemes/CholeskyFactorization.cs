using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FEC
{
    class CholeskyFactorization : LinearSolution
    {
        private double[,] Cholesky(double[,] stiffnessMatrix)
        {
            int rows = stiffnessMatrix.GetLength(0);
            int cols = stiffnessMatrix.GetLength(1);

            double[,] lowerPart = new double[rows, cols];
            double sumr;
            double sumc;

            for (int j = 0; j < cols; j++)
            {
                sumr = 0;
                for (int k = 0; k <= j - 1; k++)
                {
                    sumr = sumr + Math.Pow(lowerPart[j, k], 2);
                }
                if (stiffnessMatrix[j, j] - sumr < 0)
                {
                    throw new Exception("Cholesky: Negative number in square root");
                }
                lowerPart[j, j] = Math.Sqrt(stiffnessMatrix[j, j] - sumr);
                for (int i = j + 1; i < rows; i++)
                {
                    sumc = 0;
                    for (int k = 0; k <= j - 1; k++)
                    {
                        sumc = sumc + lowerPart[i, k] * lowerPart[j, k];
                    }
                    lowerPart[i, j] = (stiffnessMatrix[i, j] - sumc) / lowerPart[j, j];
                }
            }
            return lowerPart;
        }

        public override double[] Solve(double[,] stiffnessMatrix, double[] forceVector)
        {
            double[,] lowerMatrix = Cholesky(stiffnessMatrix);
            double[,] upperMatrix = MatrixOperations.Transpose(lowerMatrix);
            double[] intermediateVector = ForwardSubstitution(lowerMatrix, forceVector);
            double[] solutionuberVector = BackSubstitution(upperMatrix, intermediateVector);
            return solutionuberVector;
        }
    }
}
