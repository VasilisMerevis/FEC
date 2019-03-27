using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FEC
{
    class Beam2D : IElement
    {
        public Dictionary<int, INode> Nodes { get; }
        public IElementProperties Properties { get; set; }
        public Dictionary<int, bool[]> ElementFreedomSignature { get; } = new Dictionary<int, bool[]>();
        public List<int> ElementFreedomList { get; set; }
        public double[] DisplacementVector { get; set; }
        private bool ActivateLumbedMassMatrix = false;


        public Beam2D(IElementProperties properties, Dictionary<int, INode> nodes)
        {
            Properties = properties;
            this.Nodes = nodes;
            ElementFreedomSignature[1] = new bool[] { true, true, false, false, false, true };
            ElementFreedomSignature[2] = new bool[] { true, true, false, false, false, true };
        }

        public double CalculateElementLength()
        {
            double X1 = Nodes[1].XCoordinate;
            double Y1 = Nodes[1].YCoordinate;
            double X2 = Nodes[2].XCoordinate;
            double Y2 = Nodes[2].YCoordinate;
            double elementLength = Math.Sqrt(Math.Pow((X2 - X1), 2) + Math.Pow((Y2 - Y1), 2));
            return elementLength;
        }

        public double CalculateElementSinus()
        {
            double Y1 = Nodes[1].YCoordinate;
            double Y2 = Nodes[2].YCoordinate;
            double L = CalculateElementLength();
            double sinus = (Y2 - Y1) / L;
            return sinus;
        }

        public double CalculateElementCosinus()
        {
            double X1 = Nodes[1].XCoordinate;
            double X2 = Nodes[2].XCoordinate;
            double L = CalculateElementLength();
            double cosinus = (X2 - X1) / L;
            return cosinus;
        }

        public double[,] CreateLocalStiffnessMatrix()
        {
            double length = CalculateElementLength();
            double A = Properties.SectionArea;
            double E = Properties.YoungMod;
            double I = Properties.MomentOfInertia;
            double[,] localStiffnessMatrix = new[,]
                { { E * A / length, 0, 0, -E * A / length, 0, 0 },
                { 0, 12 * E * I / Math.Pow(length, 3), 6 * E * I / Math.Pow(length, 2), 0, -12 * E * I / Math.Pow(length, 3), 6 * E * I / Math.Pow( length, 2) },
                { 0, 6 * E * I / Math.Pow(length, 2), 4 * E * I / length, 0, -6 * E * I / Math.Pow(length, 2), 2 * E * I / length},
                { -E * A / length, 0, 0, E* A/ length, 0, 0 },
                { 0, -12 * E * I / Math.Pow(length, 3), -6 * E * I / Math.Pow(length, 2), 0, 12 * E * I / Math.Pow(length, 3), -6 * E * I / Math.Pow(length, 2) },
                { 0, 6 * E * I / Math.Pow( length, 2), 2 * E * I / length, 0, -6 * E * I / Math.Pow(length, 2), 4 * E * I / length}};

            return localStiffnessMatrix;
        }

        private double[,] CreateLambdaMatrix()
        {
            double cosine = CalculateElementCosinus();
            double sine = CalculateElementSinus();
            double[,] lambdaMatrix = new[,]
            { { cosine, sine, 0, 0, 0, 0 },
            { -sine, cosine, 0, 0, 0, 0 },
            { 0, 0, 1, 0, 0, 0 },
            { 0, 0, 0, cosine, sine, 0 },
            { 0, 0, 0, -sine, cosine, 0 },
            { 0, 0, 0, 0, 0, 1 }};

            return lambdaMatrix;
        }

        public double[,] CreateGlobalStiffnessMatrix()
        {
            double[,] globalStiffnessMatrix = new double[6, 6];
            //double E = Properties.YoungMod;
            //double A = Properties.SectionArea;
            //double I = Properties.MomentOfInertia;
            //double L = CalculateElementLength();
            //double c = CalculateElementCosinus();
            //double s = CalculateElementSinus();
            //globalStiffnessMatrix[0, 0] = (12 * E * I * s * s / Math.Pow(L, 3)) + (A * E * c * c / L);
            //globalStiffnessMatrix[0, 1] = (A * E * c * s / L) - (12 * E * I * c * s / Math.Pow(L, 3));
            //globalStiffnessMatrix[0, 2] = -6 * E * I * s / Math.Pow(L, 2);
            //globalStiffnessMatrix[0, 3] = (-12 * E * I * s * s / Math.Pow(L, 3)) - (A * E * c * c / L);
            //globalStiffnessMatrix[0, 4] = (12 * E * I * c * s / Math.Pow(L, 3)) - (A * E * c * s / L);
            //globalStiffnessMatrix[0, 5] = -6 * E * I * s / Math.Pow(L, 2);

            //globalStiffnessMatrix[1, 0] = globalStiffnessMatrix[0, 1];
            //globalStiffnessMatrix[1, 1] = (A * E * s * s / L) + (12 * E * I * c * c / Math.Pow(L, 3));
            //globalStiffnessMatrix[1, 2] = 6 * E * I * c / Math.Pow(L, 2);
            //globalStiffnessMatrix[1, 3] = (12 * E * I * c * s / Math.Pow(L, 3)) - (A * E * c * s / L);
            //globalStiffnessMatrix[1, 4] = -(A * E * s * s / L) - (12 * E * I * c * c / Math.Pow(L, 3));
            //globalStiffnessMatrix[1, 5] = 6 * E * I * c / Math.Pow(L, 2);

            //globalStiffnessMatrix[2, 0] = globalStiffnessMatrix[0, 2];
            //globalStiffnessMatrix[2, 1] = globalStiffnessMatrix[1, 2];
            //globalStiffnessMatrix[2, 2] = 4 * E * I / L;
            //globalStiffnessMatrix[2, 3] = 6 * E * I * s / Math.Pow(L, 2);
            //globalStiffnessMatrix[2, 4] = -6 * E * I * c / Math.Pow(L, 2);
            //globalStiffnessMatrix[2, 5] = 2 * E * I / L;

            //globalStiffnessMatrix[3, 0] = globalStiffnessMatrix[0, 3];
            //globalStiffnessMatrix[3, 1] = globalStiffnessMatrix[1, 3];
            //globalStiffnessMatrix[3, 2] = globalStiffnessMatrix[2, 3];
            //globalStiffnessMatrix[3, 3] = (12 * E * I * s * s / Math.Pow(L, 3)) + (A * E * c * c / L);
            //globalStiffnessMatrix[3, 4] = (A * E * c * s / L) - (12 * E * I * c * s / Math.Pow(L, 3));
            //globalStiffnessMatrix[3, 5] = 6 * E * I * s / Math.Pow(L, 2);

            //globalStiffnessMatrix[4, 0] = globalStiffnessMatrix[0, 4];
            //globalStiffnessMatrix[4, 1] = globalStiffnessMatrix[1, 4];
            //globalStiffnessMatrix[4, 2] = globalStiffnessMatrix[2, 4];
            //globalStiffnessMatrix[4, 3] = globalStiffnessMatrix[3, 4];
            //globalStiffnessMatrix[4, 4] = (A * E * s * s / L) + (12 * E * I * c * c / Math.Pow(L, 3));
            //globalStiffnessMatrix[4, 5] = -6 * E * I * c / Math.Pow(L, 2);

            //globalStiffnessMatrix[5, 0] = globalStiffnessMatrix[0, 5];
            //globalStiffnessMatrix[5, 1] = globalStiffnessMatrix[1, 5];
            //globalStiffnessMatrix[5, 2] = globalStiffnessMatrix[2, 5];
            //globalStiffnessMatrix[5, 3] = globalStiffnessMatrix[3, 5];
            //globalStiffnessMatrix[5, 4] = globalStiffnessMatrix[4, 5];
            //globalStiffnessMatrix[5, 5] = 4 * E * I / L;
            double[,] lambda = CreateLambdaMatrix();
            double[,] localStiff = CreateLocalStiffnessMatrix();
            double[,] transposeLocalStiff = MatrixOperations.Transpose(lambda);
            double[,] KxL = MatrixOperations.MatrixProduct(localStiff, lambda);
            globalStiffnessMatrix = MatrixOperations.MatrixProduct(transposeLocalStiff, KxL);

            return globalStiffnessMatrix;
        }

        /// <summary>
        /// Creates a Consistent Mass Matrix in local coordinate system
        /// </summary>
        /// <returns></returns>
        public double[,] CreateConsistentMassMatrix() //based on Chandrupatla page 410
        {
            double length = CalculateElementLength();

            double a = Properties.Density * Properties.SectionArea * length / 6.0;
            double b = Properties.Density * Properties.SectionArea * length / 420.0;

            double[,] localMassMatrix = new double[,]
            {
                {2.0*a, 0, 0, a, 0, 0 },
                {0, 156.0*b, 22.0*Math.Pow(length,2)*b, 0, 54.0*b, -13.0*length*b },
                {0, 22.0*Math.Pow(length,2)*b, 4.0*Math.Pow(length,2)*b, 0, 13.0*length*b, -3.0*Math.Pow(length,2)*b },
                {a, 0, 0, 2.0*a, 0, 0 },
                {0, 54.0*b, 13.0*length*b, 0, 156.0*b, -22.0*length*b },
                {0, -13.0*length*b, -3.0*Math.Pow(length,2)*b, 0, -22.0*length*b, 4.0*Math.Pow(length,2)*b }
            };
            return localMassMatrix;
        }

        /// <summary>
        /// Creates a Lumped Mass Matrix in local coordinate system
        /// </summary>
        /// <returns></returns>
        public double[,] CreateLumpedMassMatrix() //based on Bathe page 817 (mixed truss and beam elements)
        {
            double length = CalculateElementLength();

            double a = Properties.Density * Properties.SectionArea * length / 24.0;

            double[,] localMassMatrix = new double[6, 6];
            localMassMatrix[0, 0] = a * 12.0;
            localMassMatrix[1, 1] = localMassMatrix[0, 0];
            localMassMatrix[2, 2] = a * Math.Pow(length, 2);
            localMassMatrix[3, 3] = localMassMatrix[0, 0];
            localMassMatrix[4, 4] = localMassMatrix[0, 0];
            localMassMatrix[5, 5] = localMassMatrix[2, 2];

            return localMassMatrix;
        }

        public double[,] CreateMassMatrix()
        {
            double[,] lambda = CreateLambdaMatrix();
            double[,] localMassMatrix;
            if (ActivateLumbedMassMatrix == true)
            {
                localMassMatrix = CreateLumpedMassMatrix();
            }
            else
            {
                localMassMatrix = CreateConsistentMassMatrix();
            }
            double[,] globalMassMatrix = MatrixOperations.MatrixProduct
                (
                    MatrixOperations.Transpose(lambda), MatrixOperations.MatrixProduct(localMassMatrix, lambda)
                );
            return globalMassMatrix;
        }

        public double[,] CreateDampingMatrix()
        {
            return new double[6,6];
        }

        public double[] CreateInternalGlobalForcesVector()
        {
            double[,] globalStiffnessMatrix = new double[6, 6];
            double[,] lambda = CreateLambdaMatrix();
            double[,] localStiff = CreateLocalStiffnessMatrix();
            double[,] transposeLocalStiff = MatrixOperations.Transpose(lambda);
            double[,] KxL = MatrixOperations.MatrixProduct(localStiff, lambda);
            globalStiffnessMatrix = MatrixOperations.MatrixProduct(transposeLocalStiff, KxL);
            double[] globalInternalForcesVector = VectorOperations.MatrixVectorProduct(globalStiffnessMatrix, DisplacementVector);
            MatrixOperations.PrintMatrix(globalStiffnessMatrix);
            return globalInternalForcesVector;
        }
    }
}
