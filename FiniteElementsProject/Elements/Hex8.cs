using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FEC
{
    class Hex8 : IElement
    {
        public Dictionary<int, INode> Nodes { get; }
        public IElementProperties Properties { get; set; }
        public Dictionary<int, bool[]> ElementFreedomSignature { get; } = new Dictionary<int, bool[]>();
        public List<int> ElementFreedomList { get; set; }
        public double[] DisplacementVector { get; set; }
        public double poisson { get; set; } 


        public Hex8(IElementProperties properties, Dictionary<int, INode> nodes)
        {
            Properties = properties;
            this.Nodes = nodes;
            ElementFreedomSignature[1] = new bool[] { true, true, true, false, false, false };
            ElementFreedomSignature[2] = new bool[] { true, true, true, false, false, false };
            ElementFreedomSignature[3] = new bool[] { true, true, true, false, false, false };
            ElementFreedomSignature[4] = new bool[] { true, true, true, false, false, false };
            ElementFreedomSignature[5] = new bool[] { true, true, true, false, false, false };
            ElementFreedomSignature[6] = new bool[] { true, true, true, false, false, false };
            ElementFreedomSignature[7] = new bool[] { true, true, true, false, false, false };
            ElementFreedomSignature[8] = new bool[] { true, true, true, false, false, false };
        }

        private double[] UpdateNodalCoordinates(double[] displacementVector)
        {
            double[] updatedCoor = new double[24];
            for (int i = 1; i <= 8; i++)
            {
                updatedCoor[3 * i - 3] = Nodes[i].XCoordinate + displacementVector[3 * i - 3];
                updatedCoor[3 * i - 2] = Nodes[i].YCoordinate + displacementVector[3 * i - 2];
                updatedCoor[3 * i - 1] = Nodes[i].ZCoordinate + displacementVector[3 * i - 1];
            }
            return updatedCoor;
        }

        private Dictionary<int, double> CalculateShapeFunctions(double ksi, double ihta, double mhi)
        {
            Dictionary<int, double> shapeFunctions = new Dictionary<int, double>();
            double N1 = 1 / 8 * (1 - ksi) * (1 - ihta) * (1 - mhi); shapeFunctions.Add(1, N1);
            double N2 = 1 / 8 * (1 + ksi) * (1 - ihta) * (1 - mhi); shapeFunctions.Add(2, N2);
            double N3 = 1 / 8 * (1 + ksi) * (1 + ihta) * (1 - mhi); shapeFunctions.Add(3, N3);
            double N4 = 1 / 8 * (1 - ksi) * (1 + ihta) * (1 - mhi); shapeFunctions.Add(4, N4);
            double N5 = 1 / 8 * (1 - ksi) * (1 - ihta) * (1 + mhi); shapeFunctions.Add(5, N5);
            double N6 = 1 / 8 * (1 + ksi) * (1 - ihta) * (1 + mhi); shapeFunctions.Add(6, N6);
            double N7 = 1 / 8 * (1 + ksi) * (1 + ihta) * (1 + mhi); shapeFunctions.Add(7, N7);
            double N8 = 1 / 8 * (1 - ksi) * (1 + ihta) * (1 + mhi); shapeFunctions.Add(8, N8);

            return shapeFunctions;
        }

        private Dictionary<string,double[]> CalculateShapeFunctionsLocalDerivatives (double[] naturalCoordinates)
        {
            double ksi = naturalCoordinates[0];
            double ihta = naturalCoordinates[1];
            double mhi = naturalCoordinates[2];

            double[] dN_ksi = new double[]
            {
                (-1/8*(1-ihta)*(1-mhi)),
                (1/8*(1-ihta)*(1-mhi)),
                (1/8*(1+ihta)*(1-mhi)),
                (-1/8*(1+ihta)*(1-mhi)),
                (-1/8*(1-ihta)*(1+mhi)),
                (1/8*(1-ihta)*(1+mhi)),
                (1/8*(1+ihta)*(1+mhi)),
                (-1/8*(1+ihta)*(1+mhi))
            };

            double[] dN_ihta = new double[]
            {
                (-1/8*(1-ksi)*(1-mhi)),
                (-1/8*(1+ksi)*(1-mhi)),
                (1/8*(1+ksi)*(1-mhi)),
                (1/8*(1-ksi)*(1-mhi)),
                (-1/8*(1-ksi)*(1+mhi)),
                (-1/8*(1+ksi)*(1+mhi)),
                (1/8*(1+ksi)*(1+mhi)),
                (1/8*(1-ksi)*(1+mhi))
            };

            double[] dN_mhi = new double[]
            {
                (-1/8*(1-ksi)*(1-ihta)),
                (-1/8*(1+ksi)*(1-ihta)),
                (-1/8*(1+ksi)*(1+ihta)),
                (-1/8*(1-ksi)*(1+ihta)),
                (1/8*(1-ksi)*(1-ihta)),
                (1/8*(1+ksi)*(1-ihta)),
                (1/8*(1+ksi)*(1+ihta)),
                (1/8*(1-ksi)*(1+ihta))
            };

            Dictionary<string, double[]> dN = new Dictionary<string, double[]>();
            dN.Add("ksi", dN_ksi);
            dN.Add("ihta", dN_ihta);
            dN.Add("mhi", dN_mhi);
            return dN;
        }

        private double[,] CalculateJacobian(Dictionary<string, double[]> dN)
        {
            double[,] jacobianMatrix = new double[3, 3];
            DisplacementVector = new double[24];
            double[] xUpdated = UpdateNodalCoordinates(DisplacementVector);
            
            int k = 0;
            for (int i = 0; i < 8; i++)
            {
                jacobianMatrix[0, 0] = jacobianMatrix[0, 0] + xUpdated[k] * dN["ksi"][i];
                k = k + 3;
            }
            k = 1;
            for (int i = 0; i < 8; i++)
            {
                jacobianMatrix[0, 1] = jacobianMatrix[0, 1] + xUpdated[k] * dN["ksi"][i];
                k = k + 3;
            }
            k = 2;
            for (int i = 0; i < 8; i++)
            {
                jacobianMatrix[0, 2] = jacobianMatrix[0, 2] + xUpdated[k] * dN["ksi"][i];
                k = k + 3;
            }

            k = 0;
            for (int i = 0; i < 8; i++)
            {
                jacobianMatrix[1, 0] = jacobianMatrix[1, 0] + xUpdated[k] * dN["ihta"][i];
                k = k + 3;
            }
            k = 1;
            for (int i = 0; i < 8; i++)
            {
                jacobianMatrix[1, 1] = jacobianMatrix[1, 1] + xUpdated[k] * dN["ihta"][i];
                k = k + 3;
            }
            k = 2;
            for (int i = 0; i < 8; i++)
            {
                jacobianMatrix[1, 2] = jacobianMatrix[1, 2] + xUpdated[k] * dN["ihta"][i];
                k = k + 3;
            }

            k = 0;
            for (int i = 0; i < 8; i++)
            {
                jacobianMatrix[2, 0] = jacobianMatrix[2, 0] + xUpdated[k] * dN["mhi"][i];
                k = k + 3;
            }
            k = 1;
            for (int i = 0; i < 8; i++)
            {
                jacobianMatrix[2, 1] = jacobianMatrix[2, 01] + xUpdated[k] * dN["ihta"][i];
                k = k + 3;
            }
            k = 2;
            for (int i = 0; i < 8; i++)
            {
                jacobianMatrix[2, 2] = jacobianMatrix[2, 2] + xUpdated[k] * dN["ihta"][i];
                k = k + 3;
            }

            return jacobianMatrix;
        }

        private Tuple<double[,], double> CalculateInverseJacobian(double[,] jacobianMatrix)
        {
            double[,] jacobianInverseMatrix = new double[3, 3];

            jacobianInverseMatrix[0, 0] = jacobianMatrix[1, 1] * jacobianMatrix[2, 2] - jacobianMatrix[1, 2] * jacobianMatrix[2, 1];
            jacobianInverseMatrix[1, 1] = jacobianMatrix[2, 2] * jacobianMatrix[0, 0] - jacobianMatrix[2, 0] * jacobianMatrix[2, 2];
            jacobianInverseMatrix[2, 2] = jacobianMatrix[0, 0] * jacobianMatrix[1, 1] - jacobianMatrix[0, 1] * jacobianMatrix[1, 0];

            jacobianInverseMatrix[0, 1] = jacobianMatrix[1, 2] * jacobianMatrix[2, 0] - jacobianMatrix[1, 0] * jacobianMatrix[2, 2];
            jacobianInverseMatrix[1, 2] = jacobianMatrix[2, 0] * jacobianMatrix[0, 1] - jacobianMatrix[2, 1] * jacobianMatrix[0, 0];
            jacobianInverseMatrix[2, 0] = jacobianMatrix[0, 1] * jacobianMatrix[1, 2] - jacobianMatrix[0, 2] * jacobianMatrix[1, 1];

            jacobianInverseMatrix[1, 0] = jacobianMatrix[2, 1] * jacobianMatrix[0, 2] - jacobianMatrix[0, 1] * jacobianMatrix[2, 2];
            jacobianInverseMatrix[2, 1] = jacobianMatrix[0, 2] * jacobianMatrix[1, 0] - jacobianMatrix[1, 2] * jacobianMatrix[0, 0];
            jacobianInverseMatrix[0, 2] = jacobianMatrix[1, 0] * jacobianMatrix[1, 1] - jacobianMatrix[2, 0] * jacobianMatrix[1, 1];

            double detj = jacobianMatrix[0, 0] * jacobianInverseMatrix[0, 0] + jacobianMatrix[0, 1] * jacobianInverseMatrix[1, 0] + jacobianMatrix[0, 2] * jacobianInverseMatrix[2, 0];

            jacobianInverseMatrix[0, 0] = jacobianInverseMatrix[0, 0] / detj;
            jacobianInverseMatrix[1, 1] = jacobianInverseMatrix[1, 1] / detj;
            jacobianInverseMatrix[2, 2] = jacobianInverseMatrix[2, 2] / detj;

            jacobianInverseMatrix[0, 1] = jacobianInverseMatrix[0, 1] / detj;
            jacobianInverseMatrix[1, 2] = jacobianInverseMatrix[1, 2] / detj;
            jacobianInverseMatrix[2, 0] = jacobianInverseMatrix[2, 0] / detj;

            jacobianInverseMatrix[1, 0] = jacobianInverseMatrix[1, 0] / detj;
            jacobianInverseMatrix[2, 1] = jacobianInverseMatrix[2, 1] / detj;
            jacobianInverseMatrix[0, 2] = jacobianInverseMatrix[0, 2] / detj;

            return new Tuple<double[,], double>(jacobianInverseMatrix, detj);
        }

        private Dictionary<int, double[]> CalculateShapeFunctionsGlobalDerivatives(Dictionary<string, double[]> dN, double[,] Jinv)
        {
            Dictionary<int, double[]> dNg = new Dictionary<int, double[]>();

            for (int i = 0; i < 8; i++)
            {
                double[] dNlocal = new double[] { dN["ksi"][i], dN["ihta"][i], dN["mhi"][i] };
                double[] dNglobal = VectorOperations.MatrixVectorProduct(Jinv, dNlocal);
                dNg.Add(i, dNglobal);
            }
            return dNg;
        }

        private double[] CalculateStrainsVector(double[,] Bmatrix)
        {
            double[] strains = VectorOperations.MatrixVectorProduct(Bmatrix, DisplacementVector);
            return strains;
        }

        private double[,] CalculateBMatrix(Dictionary<int, double[]> dNglobal)
        {
            double[,] Bmatrix = new double[6, 24];

            for (int i = 0; i < 8; i++)
            {
                Bmatrix[0, i * 3] = dNglobal[i][0];
                Bmatrix[1, i * 3 + 1] = dNglobal[i][1];
                Bmatrix[2, i * 3 + 2] = dNglobal[i][2];
                Bmatrix[3, i * 3] = dNglobal[i][1];
                Bmatrix[3, i * 3 + 1] = dNglobal[i][0];
                Bmatrix[4, i * 3 + 1] = dNglobal[i][2];
                Bmatrix[4, i * 3 + 2] = dNglobal[i][1];
                Bmatrix[5, i * 3] = dNglobal[i][2];
                Bmatrix[3, i * 3 + 2] = dNglobal[i][0];
            }
            return Bmatrix;
        }

        private double[,] CalculateStressStrainMatrix(double E, double v)
        {
            double[,] Ematrix = new double[6, 6];
            double Ehat = E / ((1 - 2 * v) * (1 + v));
            double G = (1 / 2) * (E / (1 + v));

            Ematrix[0, 0] = Ehat * (1 - v);
            Ematrix[0, 1] = Ehat * v;
            Ematrix[0, 2] = Ehat * v;
            Ematrix[1, 0] = Ehat * v;
            Ematrix[1, 1] = Ehat * (1 - v);
            Ematrix[1, 2] = Ehat * v;
            Ematrix[2, 0] = Ehat * v;
            Ematrix[2, 1] = Ehat * v;
            Ematrix[2, 2] = Ehat * (1 - v);
            Ematrix[3, 3] = G;
            Ematrix[4, 4] = G;
            Ematrix[5, 5] = G;
            return Ematrix;
        }

        private Tuple<double[], double[]> GaussPoints(int i, int j, int k)
        {
            double[] gaussPoints = new double[] { -1.0 / Math.Sqrt(3), 1.0 / Math.Sqrt(3) };
            double[] gaussWeights = new double[] { 1.0, 1.0 };

            double[] vectorWithPoints = new double[] { gaussPoints[i], gaussPoints[j], gaussPoints[k] };
            double[] vectorWithWeights = new double[] { gaussWeights[i], gaussWeights[j], gaussWeights[k] };
            return new Tuple<double[], double[]>(vectorWithPoints, vectorWithWeights);
        }

        public double[,] CreateGlobalStiffnessMatrix()
        {
            double[,] K = new double[24, 24];
            double[,] E = CalculateStressStrainMatrix(Properties.YoungMod, poisson);

            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    for (int k = 0; k < 2; k++)
                    {
                        double[] gP = GaussPoints(i, j, k).Item1;
                        double[] gW = GaussPoints(i, j, k).Item2;
                        Dictionary<string, double[]> localdN = CalculateShapeFunctionsLocalDerivatives(gP);
                        double[,] J = CalculateJacobian(localdN);
                        double[,] invJ = CalculateInverseJacobian(J).Item1;
                        double detJ = CalculateInverseJacobian(J).Item2;
                        Dictionary<int, double[]> globaldN = CalculateShapeFunctionsGlobalDerivatives(localdN, invJ);
                        double[,] B = CalculateBMatrix(globaldN);
                        K = MatrixOperations.MatrixAddition(K, MatrixOperations.ScalarMatrixProductNew(detJ * gW[0] * gW[1] * gW[2],
                            MatrixOperations.MatrixProduct(MatrixOperations.Transpose(B), MatrixOperations.MatrixProduct(E, B))));
                        
                    }
                }
            }
            return K;
        }

        public double[,] CreateMassMatrix()
        {
            return new double[24, 24];
        }

        public double[] CreateInternalGlobalForcesVector()
        {
            return new double[24];
        }
    }
}
