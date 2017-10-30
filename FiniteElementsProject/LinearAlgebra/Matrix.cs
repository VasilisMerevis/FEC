using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FEC
{
    public class Matrix<T>
    {
        private readonly T[,] matrixData;

        public Matrix(T[,] matrixData)
        {
            this.matrixData = matrixData;
        }

        public static Matrix<T> operator*(double scalar, Matrix<T> matrixObject)  
        {
            if (typeof(T) != typeof(double) && typeof(T) != typeof(int))
            {
                throw new NotSupportedException("The specific type argument is not supported.");
            }

            int matrixrows = matrixObject.matrixData.GetLength(0);
            int matrixcols = matrixObject.matrixData.GetLength(1);
            double[,] resultMatrixData = new double[matrixrows, matrixcols];
            double[,] matrix2 = matrixObject.matrixData as double[,];
            for (int row = 0; row < matrixrows; row++)
            {
                for (int col = 0; col < matrixcols; col++)
                {
                    resultMatrixData[row, col] = scalar * matrix2[row, col];
                }
            }
            Matrix<T> resultMatrixObject = new Matrix<double>(resultMatrixData) as Matrix<T>;
            return resultMatrixObject;
        }

        public void PrintMatrix()
        {
            int matrixRows = matrixData.GetLength(0);
            int matrixCols = matrixData.GetLength(1);
            for (int row = 0; row < matrixRows; row++)
            {
                for (int col = 0; col < matrixCols; col++)
                {
                    Console.Write(String.Format("{0}\t", matrixData[row, col]));
                }

                Console.WriteLine();
            }

        }
    }
}
