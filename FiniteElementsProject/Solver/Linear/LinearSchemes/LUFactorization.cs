using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FEC
{
    class LUFactorization : LinearSolution
    {
        private Tuple<double[,], double[,]> LU(double[,] stiffnessMatrix)
        {
            int rows = stiffnessMatrix.GetLength(0);
            int cols = stiffnessMatrix.GetLength(1);

            double[,] lowerPart = new double[rows, cols];
            double[,] upperPart = new double[rows, cols];
            double sumu;
            double suml;

            for (int j = 0; j < cols; j++)
            {
                upperPart[0, j] = stiffnessMatrix[0, j];
            }

            for (int k = 0; k < rows; k++)
            {
                lowerPart[k, k] = 1.0;
            }

            for (int j = 0; j < cols; j++)
            {
                for (int i = 0; i < rows; i++)
                {
                    if (i<=j)
                    {
                        sumu = 0.0;
                        for (int k = 0; k < i; k++)
                        {
                            sumu = sumu + lowerPart[i, k] * upperPart[k, j];
                        }
                        upperPart[i, j] = (stiffnessMatrix[i, j] - sumu) / lowerPart[i, i];
                    }
                    else if(i>j)
                    {
                        suml = 0.0;
                        for (int k = 0; k < j; k++)
                        {
                            suml = suml + lowerPart[i, k] * upperPart[k, j];
                        }
                        lowerPart[i, j] = (stiffnessMatrix[i, j] - suml) / upperPart[j, j];
                    }
                }
            }
            return new Tuple<double[,], double[,]>(lowerPart, upperPart);
        }

        public override double[] Solve(double[,] stiffnessMatrix, double[] forceVector)
        {
            Tuple<double[,],double[,]> factorizedMatrices = LU(stiffnessMatrix);
            double[,] lowerMatrix = factorizedMatrices.Item1;
            double[,] upperMatrix = factorizedMatrices.Item2;
            double[] intermediateVector = ForwardSubstitution(lowerMatrix, forceVector);
            double[] solutionuberVector = BackSubstitution(upperMatrix, intermediateVector);
            
            return solutionuberVector;
        }
    }
}
