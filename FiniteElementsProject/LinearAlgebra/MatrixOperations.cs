using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FEC
{
    static class MatrixOperations
    {
        public static void PrintMatrix(double [,] matrix)
        {
            int matrixRows = matrix.GetLength(0);
            int matrixCols = matrix.GetLength(1);
            for (int row = 0; row < matrixRows; row++)
            {
                for (int col = 0; col < matrixCols; col++)
                {
                    Console.Write(String.Format("{0}\t", matrix[row, col]));
                }

                Console.WriteLine();
            }

        }

        public static double[,] CreateDiagonalMatrix(int dimension, double diagonalNumber)
        {
            double[,] diagMatrix = new double[dimension, dimension];
            for (int i = 0; i < dimension; i++)
            {
                diagMatrix[i, i] = diagonalNumber;
            }
            return diagMatrix;
        }

        public static double [,] Transpose(double [,] matrix)
        {
            int matrixRows = matrix.GetLength(0);
            int matrixCols = matrix.GetLength(1);
            double[,] m = new double[matrixCols,matrixRows]; 
            for (int row = 0; row < matrixRows; row++)
            {
                for (int col = 0; col < matrixCols; col++)
                {
                    m[col, row] = matrix[row, col]; 
                }
            }
            return m;
        }

        public static double [,] MatrixProduct(double [,] matrix1, double [,] matrix2)
        {
            int matrix1rows = matrix1.GetLength(0);
            int matrix2cols = matrix2.GetLength(1);
            double[,] productMatrix = new double[matrix1rows, matrix2cols];
            for (int i = 0; i < matrix1rows; i++)
            {
                for (int j = 0; j < matrix2cols; j++)
                {
                    double sum = 0;
                    for (int k = 0; k < matrix2.GetLength(0); k++)
                    {
                        sum = sum + matrix1[i, k] * matrix2[k, j];
                    }
                    productMatrix[i, j] = sum;
                }
            }
            return productMatrix;
        }

        public static double [,] MatrixAddition(double [,] matrix1, double [,] matrix2)
        {
            if (matrix1.GetLength(0) != matrix2.GetLength(0) || matrix1.GetLength(1) != matrix2.GetLength(1))
            {
                throw new Exception("Not equally sized matrices");
            }
            int matrixrows = matrix1.GetLength(0);
            int matrixcols = matrix1.GetLength(1);
            double[,] sumMatrix = new double[matrixrows, matrixcols];

            for (int row = 0; row < matrixrows; row++)
            {
                for (int col = 0; col < matrixcols; col++)
                {
                    sumMatrix[row, col] = matrix1[row, col] + matrix2[row, col];
                }
            }
            return sumMatrix;
        }

        public static double[,] MatrixSubtraction(double[,] matrix1, double[,] matrix2)
        {
            int matrixrows = matrix1.GetLength(0);
            int matrixcols = matrix1.GetLength(1);
            double[,] sumMatrix = new double[matrixrows, matrixcols];

            for (int row = 0; row < matrixrows; row++)
            {
                for (int col = 0; col < matrixcols; col++)
                {
                    sumMatrix[row, col] = matrix1[row, col] - matrix2[row, col];
                }
            }
            return sumMatrix;
        }

        public static double[,] ScalarMatrixProduct(double scalar, double[,] matrix)
        {
            int matrixrows = matrix.GetLength(0);
            int matrixcols = matrix.GetLength(1);
            for (int row = 0; row < matrixrows; row++)
            {
                for (int col = 0; col < matrixcols; col++)
                {
                    matrix[row, col] = scalar * matrix[row, col];
                }
            }
            return matrix;
        }

        public static double[,] ScalarMatrixProductNew(double scalar, double[,] matrix)
        {
            int matrixrows = matrix.GetLength(0);
            int matrixcols = matrix.GetLength(1);
            double[,] resultMatrix = new double[matrixrows, matrixcols];
            for (int row = 0; row < matrixrows; row++)
            {
                for (int col = 0; col < matrixcols; col++)
                {
                    resultMatrix[row, col] = scalar * matrix[row, col];
                }
            }
            return resultMatrix;
        }

        public static double [] ScalarByVectorProduct(double scalarFactor, double [] initialVector )
        {
            int dimension = initialVector.GetLength(0);
            double[] finalVector = new double[dimension];

            for (int row = 0; row < dimension; row++)
            {
                finalVector[row] = scalarFactor * initialVector[row];
            }
            return finalVector;
        }

        public static double[,] PutZerosInDiag(double[,] matrix)
        {
            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                matrix[i, i] = 0;
            }
            return matrix;
        }

        public static double[,] GetDiagMatrix(double[,] matrix)
        {
            double[,] diagMatrix = new double[matrix.GetLength(0), matrix.GetLength(1)];
            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                diagMatrix[i,i] = matrix[i, i];
            }
            return diagMatrix;
        }

        public static double[,] InvertDiagMatrix(double[,] matrix)
        {
            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                matrix[i, i] = 1 / matrix[i, i];
            }
            return matrix;
        }

        

    }
}
