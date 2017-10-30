using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FEC
{
    class BoundaryConditionsImposition
    {
        public static double[,] ReducedTotalStiff(double[,] totalstiff, int[] boundaryDof)
        {
            int rows = totalstiff.GetLength(0);
            int cols = totalstiff.GetLength(1);
            int dofValues = boundaryDof.GetLength(0);
            int newDim = rows - dofValues;
            double[,] reducedMatrix = new double[newDim, newDim];
            int m;
            int n;
            m = 0;
            for (int i = 0; i < rows; i++)
            {

                if (boundaryDof.Contains(i + 1))   //i+1 because C# is zero based//
                    continue;
                else
                    n = 0;
                for (int j = 0; j < cols; j++)
                {

                    if (boundaryDof.Contains(j + 1))
                        continue;
                    else
                    {
                        reducedMatrix[m, n] = totalstiff[i, j];
                        n = n + 1;
                    }
                }
                m = m + 1;
            }
            return reducedMatrix;
        }

        public static double[] ReducedVector(double[] vectorToReduce, int[] boundaryDOF)
        {
            int reducedVectorLength = vectorToReduce.Length - boundaryDOF.Length;
            double[] reducedVector = new double[reducedVectorLength];
            int newRow = 0;
            for (int oldRow = 0; oldRow < vectorToReduce.Length; oldRow++)
            {
                if (boundaryDOF.Contains(oldRow+1))
                {
                    continue;
                }
                else
                {
                    reducedVector[newRow] = vectorToReduce[oldRow];
                    newRow = newRow + 1;
                }
            }
            return reducedVector;
        }

        public static double[] CreateFullVectorFromReducedVector(double[] reducedVector, int[] boundaryDOF)
        {
            int fullVectorLength = reducedVector.Length + boundaryDOF.Length;
            double[] fullVector = new double[fullVectorLength];
            int reducedVectorRow = 0;
            for (int fullVectorRow = 0; fullVectorRow < fullVectorLength; fullVectorRow++)
            {
                if (boundaryDOF.Contains(fullVectorRow + 1))
                {
                    fullVector[fullVectorRow] = 0;
                }
                else
                {
                    fullVector[fullVectorRow] = reducedVector[reducedVectorRow];
                    reducedVectorRow = reducedVectorRow + 1;
                }
            }
            return fullVector;
        }
    }
}

