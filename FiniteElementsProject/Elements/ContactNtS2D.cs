using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FEC
{
    class ContactNtS2D : IElement
    {
        public Dictionary<int, INode> Nodes { get; }
        public IElementProperties Properties { get; set; }
        public Dictionary<int, bool[]> ElementFreedomSignature { get; } = new Dictionary<int, bool[]>();
        public List<int> ElementFreedomList { get; set; }
        public double[] DisplacementVector { get; set; }
        private double PenaltyFactor { get; set; }

        public ContactNtS2D(IElementProperties properties, Dictionary<int, INode> nodes)
        {
            Properties = properties;
            this.Nodes = nodes;
            ElementFreedomSignature[1] = new bool[] { true, true, false, false, false, false };
            ElementFreedomSignature[2] = new bool[] { true, true, false, false, false, false };
            ElementFreedomSignature[3] = new bool[] { true, true, false, false, false, false };
            DisplacementVector = new double[6];
            PenaltyFactor = properties.YoungMod * 2.0;
        }

        //private double[] CalculateNormalUnitVector(double detm)
        //{
        //    double X1 = Nodes[1].XCoordinate;
        //    double Y1 = Nodes[1].YCoordinate;
        //    double X2 = Nodes[2].XCoordinate;
        //    double Y2 = Nodes[2].YCoordinate;
        //    double[] vector = new double[] { Y2 - Y1, X2 - X1 };
        //    double scalarCoef = -1 / (2 * Math.Sqrt(detm));
        //    double[] normalUnitVec = VectorOperations.VectorScalarProductNew(vector, scalarCoef);
        //    return normalUnitVec;
        //}

        private double ClosestPointProjection()
        {
            double Xm1 = Nodes[1].XCoordinate + DisplacementVector[0];
            double Ym1 = Nodes[1].YCoordinate + DisplacementVector[1];
            double Xm2 = Nodes[2].XCoordinate + DisplacementVector[2];
            double Ym2 = Nodes[2].YCoordinate + DisplacementVector[3];
            double Xs = Nodes[3].XCoordinate + DisplacementVector[4];
            double Ys = Nodes[3].YCoordinate + DisplacementVector[5];

            double ksi1 = (2.0 * (Xs * (Xm2 - Xm1) + Ys * (Ym2 - Ym1)) - Math.Pow(Xm2, 2) - Math.Pow(Ym2, 2) + Math.Pow(Xm1, 2) + Math.Pow(Ym1,2)) / (Math.Pow(Xm2 - Xm1, 2) + Math.Pow(Ym2 - Ym1, 2));
            return ksi1;
        }

        private Tuple<double[,],double[,]> CalculatePositionMatrix(double ksi1)
        {
            double N1 = 1.0 / 2.0 * (1.0 - ksi1);
            double N2 = 1.0 / 2.0 * (1.0 + ksi1);
            double dN1 = -1.0 / 2.0;
            double dN2 = 1.0 / 2.0;
            double[,] aMatrix = new double[,]
                {
                    { -N1 ,0.0 ,-N2 ,0.0 ,1.0 ,0.0 },
                    {0.0, -N1 , 0.0 ,-N2, 0.0, 1.0 }
                };

            double[,] daMatrix = new double[,]
                {
                    { -dN1 ,0.0 ,-dN2 ,0.0 ,0.0 ,0.0 },
                    {0.0, -dN1 , 0.0 ,-dN2, 0.0, 0.0 }
                };
            return new Tuple<double[,], double[,]>(aMatrix, daMatrix);
        }

        private Tuple<double[], double, double[]> SurfaceGeometry(double[,] daMatrix)
        {
            double Xm1 = Nodes[1].XCoordinate + DisplacementVector[0];
            double Ym1 = Nodes[1].YCoordinate + DisplacementVector[1];
            double Xm2 = Nodes[2].XCoordinate + DisplacementVector[2];
            double Ym2 = Nodes[2].YCoordinate + DisplacementVector[3];
            double Xs = Nodes[3].XCoordinate + DisplacementVector[4];
            double Ys = Nodes[3].YCoordinate + DisplacementVector[5];

            double[] xupd = new double[] { -Xm1, -Ym1, -Xm2, -Ym2, -Xs, -Ys };
            double[] surfaceVector = VectorOperations.MatrixVectorProduct(daMatrix, xupd);
            double detm = VectorOperations.VectorDotProduct(surfaceVector, surfaceVector);
            double m11 = 1.0 / detm;
            double[] vector = new double[] { Ym2 - Ym1, Xm1 - Xm2 };
            double scalarCoef = -1.0 / (2.0 * Math.Sqrt(detm));
            double[] normalUnitVec = VectorOperations.VectorScalarProductNew(vector, scalarCoef);

            return new Tuple<double[], double, double[]>(surfaceVector, m11, normalUnitVec);
        }

        private double CalculateNormalGap(double[,] aMatrix, double[] n)
        {
            double[,] AT = MatrixOperations.Transpose(aMatrix);
            double[] AT_n = VectorOperations.MatrixVectorProduct(AT, n);
            double[] xupd = new double[] {
                Nodes[1].XCoordinate + DisplacementVector[0],
                Nodes[1].YCoordinate + DisplacementVector[1],
                Nodes[2].XCoordinate + DisplacementVector[2],
                Nodes[2].YCoordinate + DisplacementVector[3],
                Nodes[3].XCoordinate + DisplacementVector[4],
                Nodes[3].YCoordinate + DisplacementVector[5]
            };
            double normalGap = VectorOperations.VectorDotProduct(xupd, AT_n);
            return normalGap;
        }

        private double[,] CalculateMainStiffnessPart(double ksi1, double[] n)
        {
            double[,] mainStiffnessMatrix = new double[6, 6];
            double N1 = 1.0 / 2.0 * (1.0 - ksi1);
            double N2 = 1.0 / 2.0 * (1.0 + ksi1);
            Tuple<double[,], double[,]> positionMatrices = CalculatePositionMatrix(ksi1);
            double[,] A = positionMatrices.Item1;
            double[,] nxn = VectorOperations.VectorVectorTensorProduct(n, n);
            double[,] nxn_A = MatrixOperations.MatrixProduct(nxn, A);
            double[,] AT_nxn_A = MatrixOperations.MatrixProduct(MatrixOperations.Transpose(A), nxn_A);
            mainStiffnessMatrix = MatrixOperations.ScalarMatrixProductNew(PenaltyFactor, AT_nxn_A);
            //mainStiffnessMatrix[0, 0] = N1 * N1 * n[0] * n[0] * PenaltyFactor;
            //mainStiffnessMatrix[0, 1] = N1 * N1 * n[0] * n[1] * PenaltyFactor;
            //mainStiffnessMatrix[0, 2] = N1 * N2 * n[0] * n[0] * PenaltyFactor;
            //mainStiffnessMatrix[0, 3] = N1 * N1 * n[0] * n[1] * PenaltyFactor;
            //mainStiffnessMatrix[0, 4] = -N1 * n[0] * n[0] * PenaltyFactor;
            //mainStiffnessMatrix[0, 5] = -N1 * n[0] * n[1] * PenaltyFactor;

            //mainStiffnessMatrix[1, 0] = mainStiffnessMatrix[0, 1];
            //mainStiffnessMatrix[1, 1] = N1 * N1 * n[1] * n[1] * PenaltyFactor;
            //mainStiffnessMatrix[1, 2] = N1 * N2 * n[0] * n[1] * PenaltyFactor;
            //mainStiffnessMatrix[1, 3] = N1 * N2 * n[1] * n[1] * PenaltyFactor;
            //mainStiffnessMatrix[1, 4] = -N1 * n[0] * n[1] * PenaltyFactor;
            //mainStiffnessMatrix[1, 5] = -N1 * n[1] * n[1] * PenaltyFactor;

            //mainStiffnessMatrix[2, 0] = mainStiffnessMatrix[0, 2];
            //mainStiffnessMatrix[2, 1] = mainStiffnessMatrix[1, 2];
            //mainStiffnessMatrix[2, 2] = N2 * N2 * n[0] * n[0] * PenaltyFactor;
            //mainStiffnessMatrix[2, 3] = N2 * N2 * n[0] * n[1] * PenaltyFactor;
            //mainStiffnessMatrix[2, 4] = -N2 * n[0] * n[0] * PenaltyFactor;
            //mainStiffnessMatrix[2, 5] = -N2 * n[0] *n[1] * PenaltyFactor;

            //mainStiffnessMatrix[3, 0] = mainStiffnessMatrix[0, 3];
            //mainStiffnessMatrix[3, 1] = mainStiffnessMatrix[1, 3];
            //mainStiffnessMatrix[3, 2] = mainStiffnessMatrix[2, 3];
            //mainStiffnessMatrix[3, 3] = N2 * N2 * n[1] * n[1] * PenaltyFactor;
            //mainStiffnessMatrix[3, 4] = -N2 * n[0] * n[1] * PenaltyFactor;
            //mainStiffnessMatrix[3, 5] = -N2 * n[1] * n[1] * PenaltyFactor;

            //mainStiffnessMatrix[4, 0] = mainStiffnessMatrix[0, 4];
            //mainStiffnessMatrix[4, 1] = mainStiffnessMatrix[1, 4];
            //mainStiffnessMatrix[4, 2] = mainStiffnessMatrix[2, 4];
            //mainStiffnessMatrix[4, 3] = mainStiffnessMatrix[3, 4];
            //mainStiffnessMatrix[4, 4] = n[0] * n[0] * PenaltyFactor;
            //mainStiffnessMatrix[4, 5] = n[0] * n[1] * PenaltyFactor;

            //mainStiffnessMatrix[5, 0] = mainStiffnessMatrix[0, 5];
            //mainStiffnessMatrix[5, 1] = mainStiffnessMatrix[1, 5];
            //mainStiffnessMatrix[5, 2] = mainStiffnessMatrix[2, 5];
            //mainStiffnessMatrix[5, 3] = mainStiffnessMatrix[3, 5];
            //mainStiffnessMatrix[5, 4] = mainStiffnessMatrix[4, 5];
            //mainStiffnessMatrix[5, 5] = n[1] * n[1] * PenaltyFactor;
            return mainStiffnessMatrix;
        }

        private double[,] CalculateRotationalStiffnessPart(double[,] A, double[,] dA, double[] n, double ksi3, double m11, double[] dRho)
        {
            double coef = PenaltyFactor * ksi3 * m11;
            double[,] rotationalPart;
            double[,] n_x_dRho = VectorOperations.VectorVectorTensorProduct(n, dRho);
            double[,] dRho_x_n = VectorOperations.VectorVectorTensorProduct(dRho, n);
            double[,] firstTerm = MatrixOperations.MatrixProduct(
                                                                    MatrixOperations.Transpose(dA),
                                                                    MatrixOperations.MatrixProduct(n_x_dRho, A)
                                                                    );
            double[,] secondTerm = MatrixOperations.MatrixProduct(
                                                                    MatrixOperations.Transpose(A),
                                                                    MatrixOperations.MatrixProduct(dRho_x_n, dA)
                                                                    );
            rotationalPart = MatrixOperations.ScalarMatrixProductNew(
                                                                        coef,
                                                                        MatrixOperations.MatrixAddition(firstTerm, secondTerm)
                                                                        );
            //double[,] rotationalPart = new double[6, 6];
            //double N1 = 1 / 2 * (1 - ksi1);
            //double N2 = 1 / 2 * (1 + ksi1);
            //double coef = PenaltyFactor * ksi3 * m11;
            //rotationalPart[0, 0] = -coef * N1 * drho[0] * n[0];
            //rotationalPart[0, 1] = -coef * (N1 * drho[0] * n[1] / 2) - coef * (N1 * drho[1] * n[0] / 2);
            //rotationalPart[0, 2] = coef * (N1 * drho[0] * n[0] / 2) - coef * (N2 * drho[0] * n[0] / 2);
            //rotationalPart[0, 3] = coef * (N1 * drho[0] * n[1] / 2) - coef * (N2 * drho[1] * n[0] / 2);
            //rotationalPart[0, 4] = coef * (drho[0] * n[0] / 2);
            //rotationalPart[0, 5] = coef * (drho[1] * n[0] / 2) - coef * (N1 * drho[0] * n[1]);

            //rotationalPart[1, 0] = rotationalPart[0, 1];
            //rotationalPart[1, 1] = -coef * N1 * drho[1] * n[1];
            //rotationalPart[1, 2] = coef * (N1 * drho[1] * n[0] / 2) - coef * (N2 * drho[0] * n[1] / 2);
            //rotationalPart[1, 3] = coef * (N1 * drho[1] * n[1] / 2) - coef * (N2 * drho[1] * n[1] / 2);
            //rotationalPart[1, 4] = coef * drho[0] * n[1] / 2;
            //rotationalPart[1, 5] = coef * (drho[1] * n[1] / 2) - coef * (N1 * drho[1] * n[1]);
            return rotationalPart;
        }

        public double[,] CreateGlobalStiffnessMatrix()
        {
            double ksi1 = ClosestPointProjection();
            if (Math.Abs(ksi1) <= 1.05)
            {
                Tuple<double[,], double[,]> positionMatrices = CalculatePositionMatrix(ksi1);
                double[,] aMatrix = positionMatrices.Item1;
                double[,] daMatrix = positionMatrices.Item2;

                Tuple<double[], double, double[]> surfaceCharacteristics = SurfaceGeometry(daMatrix);
                double m11 = surfaceCharacteristics.Item2;
                double[] dRho = surfaceCharacteristics.Item1;
                double[] n = surfaceCharacteristics.Item3;
                double ksi3 = CalculateNormalGap(aMatrix, n);
                if (ksi3 <= 0)
                {
                    double[,] mainPart = CalculateMainStiffnessPart(ksi1, n);
                    double[,] rotationalPart = CalculateRotationalStiffnessPart(aMatrix, daMatrix, n, ksi3, m11, dRho);
                    double[,] globalStiffnessMatrix = MatrixOperations.MatrixAddition(mainPart, rotationalPart);
                    return globalStiffnessMatrix;
                }
                else
                {
                    double[,] globalStifnessMatrix = new double[6, 6];
                    return globalStifnessMatrix;
                }
            }
            else
            {
                double[,] globalStifnessMatrix = new double[6, 6];
                return globalStifnessMatrix;
            }
        }

        public double[] CreateInternalGlobalForcesVector()
        {
            double ksi1 = ClosestPointProjection();
            if (Math.Abs(ksi1) <= 1.05)
            {
                Tuple<double[,], double[,]> positionMatrices = CalculatePositionMatrix(ksi1);
                double[,] aMatrix = positionMatrices.Item1;
                double[,] daMatrix = positionMatrices.Item2;

                Tuple<double[], double, double[]> surfaceCharacteristics = SurfaceGeometry(daMatrix);
                double m11 = surfaceCharacteristics.Item2;
                double[] n = surfaceCharacteristics.Item3;
                double ksi3 = CalculateNormalGap(aMatrix, n);
                if (ksi3 <= 0)
                {
                    double[,] AT = MatrixOperations.Transpose(aMatrix);
                    double[] AT_n = VectorOperations.MatrixVectorProduct(AT, n);
                    double[] internalGlobalForcesVector = VectorOperations.VectorScalarProductNew(AT_n, PenaltyFactor * ksi3);
                    return internalGlobalForcesVector;
                }
                else
                {
                    double[] internalGlobalForcesVector = new double[6];
                    return internalGlobalForcesVector;
                }
            }
            else
            {
                double[] internalGlobalForcesVector = new double[6];
                return internalGlobalForcesVector;
            }
        }

        public double[,] CreateMassMatrix()
        {
            return new double[6, 6];
        }

        public double[,] CreateDampingMatrix()
        {
            return new double[6, 6];
        }
    }
}

