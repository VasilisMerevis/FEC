using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FEC
{
    class Quad4 : IElement
    {
        public Dictionary<int, INode> Nodes { get; }
        public IElementProperties Properties { get; set; }
        public Dictionary<int, bool[]> ElementFreedomSignature { get; } = new Dictionary<int, bool[]>();
        public List<int> ElementFreedomList { get; set; }
        public double[] DisplacementVector { get; set; }
        public double poisson { get; set; }
        private double thickness = 1.0; //To be included in Element Properties
        private double density = 1.0; //To be included in Element Properties

        public Quad4(IElementProperties properties, Dictionary<int, INode> nodes)
        {
            Properties = properties;
            this.Nodes = nodes;
            ElementFreedomSignature[1] = new bool[] { true, true, false, false, false, false };
            ElementFreedomSignature[2] = new bool[] { true, true, false, false, false, false };
            ElementFreedomSignature[3] = new bool[] { true, true, false, false, false, false };
            ElementFreedomSignature[4] = new bool[] { true, true, false, false, false, false };
            DisplacementVector = new double[8];
        }

        private double[] UpdateNodalCoordinates(double[] displacementVector)
        {
            double[] updatedCoor = new double[8];
            for (int i = 1; i <= 4; i++)
            {
                updatedCoor[2 * i - 2] = Nodes[i].XCoordinate + displacementVector[2 * i - 2];
                updatedCoor[2 * i - 1] = Nodes[i].YCoordinate + displacementVector[2 * i - 1];
            }
            return updatedCoor;
        }

        private Dictionary<int, double> CalculateShapeFunctions(double ksi, double ihta)
        {
            Dictionary<int, double> shapeFunctions = new Dictionary<int, double>();
            double N1 = 1.0 / 4.0 * (1 - ksi) * (1 - ihta); shapeFunctions.Add(1, N1);
            double N2 = 1.0 / 4.0 * (1 + ksi) * (1 - ihta); shapeFunctions.Add(2, N2);
            double N3 = 1.0 / 4.0 * (1 + ksi) * (1 + ihta); shapeFunctions.Add(3, N3);
            double N4 = 1.0 / 4.0 * (1 - ksi) * (1 + ihta); shapeFunctions.Add(4, N4);
            
            return shapeFunctions;
        }

        private double[,] CalculateShapeFunctionMatrix(double ksi, double ihta)
        {
            Dictionary<int, double> shapeFunctions = CalculateShapeFunctions(ksi, ihta);
            double[,] N = new double[,]
            {
                {(1.0/4.0)*shapeFunctions[1], 0, (1.0/4.0)*shapeFunctions[2], 0, (1.0/4.0)*shapeFunctions[3], 0, (1.0/4.0)*shapeFunctions[4], 0 },
                {0, (1.0/4.0)* shapeFunctions[1], 0, (1.0/4.0)*shapeFunctions[2], 0, (1.0/4.0)*shapeFunctions[3], 0, (1.0/4.0)*shapeFunctions[4] }
            };
            return N;
        }

        private Dictionary<string, double[]> CalculateShapeFunctionsLocalDerivatives(double[] naturalCoordinates)
        {
            double ksi = naturalCoordinates[0];
            double ihta = naturalCoordinates[1];

            double[] dN_ksi = new double[]
            {
                (-1.0/4.0*(1-ihta)),
                (1.0/4.0*(1-ihta)),
                (1.0/4.0*(1+ihta)),
                (-1.0/4.0*(1+ihta)),
            };

            double[] dN_ihta = new double[]
            {
                (-1.0/4.0*(1-ksi)),
                (-1.0/4.0*(1+ksi)),
                (1.0/4.0*(1+ksi)),
                (1.0/4.0*(1-ksi)),
            };

            Dictionary<string, double[]> dN = new Dictionary<string, double[]>();
            dN.Add("ksi", dN_ksi);
            dN.Add("ihta", dN_ihta);
            return dN;
        }

        private double[,] CalculateJacobian(Dictionary<string, double[]> dN)
        {
            double[,] jacobianMatrix = new double[2, 2];
            
            double[] xUpdated = UpdateNodalCoordinates(DisplacementVector);

            int k = 0;
            for (int i = 0; i < 4; i++)
            {
                jacobianMatrix[0, 0] = jacobianMatrix[0, 0] + xUpdated[k] * dN["ksi"][i];
                k = k + 2;
            }
            k = 1;
            for (int i = 0; i < 4; i++)
            {
                jacobianMatrix[0, 1] = jacobianMatrix[0, 1] + xUpdated[k] * dN["ksi"][i];
                k = k + 2;
            }
            
            k = 0;
            for (int i = 0; i < 4; i++)
            {
                jacobianMatrix[1, 0] = jacobianMatrix[1, 0] + xUpdated[k] * dN["ihta"][i];
                k = k + 2;
            }
            k = 1;
            for (int i = 0; i < 4; i++)
            {
                jacobianMatrix[1, 1] = jacobianMatrix[1, 1] + xUpdated[k] * dN["ihta"][i];
                k = k + 2;
            }
            
            return jacobianMatrix;
        }

        private Tuple<double[,], double> CalculateInverseJacobian(double[,] jacobianMatrix)
        {
            double[,] jacobianInverseMatrix = new double[2, 2];

            double detj = jacobianMatrix[0, 0] * jacobianMatrix[1, 1] - jacobianMatrix[0, 1] * jacobianMatrix[1, 0];

            jacobianInverseMatrix[0, 0] = jacobianMatrix[1, 1] / detj;
            jacobianInverseMatrix[0, 1] = -jacobianMatrix[0, 1] / detj;
            jacobianInverseMatrix[1, 0] = -jacobianMatrix[1, 0] / detj;
            jacobianInverseMatrix[1, 1] = jacobianMatrix[0, 0] / detj;

            return new Tuple<double[,], double>(jacobianInverseMatrix, detj);
        }

        private Dictionary<int, double[]> CalculateShapeFunctionsGlobalDerivatives(Dictionary<string, double[]> dN, double[,] Jinv)
        {
            Dictionary<int, double[]> dNg = new Dictionary<int, double[]>();

            for (int i = 0; i < 4; i++)
            {
                double[] dNlocal = new double[] { dN["ksi"][i], dN["ihta"][i] };
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
            double[,] Bmatrix = new double[3, 8];

            for (int i = 0; i < 4; i++)
            {
                Bmatrix[0, i * 2] = dNglobal[i][0];
                Bmatrix[1, i * 2 + 1] = dNglobal[i][1];
                Bmatrix[2, i * 2] = dNglobal[i][1];
                Bmatrix[2, i * 2 + 1] = dNglobal[i][0];
            }
            return Bmatrix;
        }

        private double[,] CalculateStressStrainMatrix(double E, double v)
        {
            double[,] Ematrix = new double[3, 3];
            double Ehat = E / ((1.0 - Math.Pow(v,2)));

            Ematrix[0, 0] = Ehat;
            Ematrix[0, 1] = Ehat * v;
            Ematrix[1, 0] = Ehat * v;
            Ematrix[1, 1] = Ehat;
            Ematrix[2, 2] = Ehat * (1.0 / 2.0) * (1.0 - v);
            
            return Ematrix;
        }

        private double[] CalculateStressVector(double[,] E, double[] strain)
        {
            double[] stressVector = VectorOperations.MatrixVectorProduct(E,strain);
            return stressVector;
        }

        private Tuple<double[], double[]> GaussPoints(int i, int j)
        {
            double[] gaussPoints = new double[] { -1.0 / Math.Sqrt(3), 1.0 / Math.Sqrt(3) };
            double[] gaussWeights = new double[] { 1.0, 1.0 };

            double[] vectorWithPoints = new double[] { gaussPoints[i], gaussPoints[j] };
            double[] vectorWithWeights = new double[] { gaussWeights[i], gaussWeights[j] };
            return new Tuple<double[], double[]>(vectorWithPoints, vectorWithWeights);
        }

        public double[,] CreateGlobalStiffnessMatrix()
        {
            double[,] K = new double[8, 8];
            double[,] E = CalculateStressStrainMatrix(Properties.YoungMod, 1.0 / 3.0); //needs fixing in poisson v

            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    double[] gP = GaussPoints(i, j).Item1;
                    double[] gW = GaussPoints(i, j).Item2;
                    Dictionary<string, double[]> localdN = CalculateShapeFunctionsLocalDerivatives(gP);
                    double[,] J = CalculateJacobian(localdN);
                    double[,] invJ = CalculateInverseJacobian(J).Item1;
                    double detJ = CalculateInverseJacobian(J).Item2;
                    Dictionary<int, double[]> globaldN = CalculateShapeFunctionsGlobalDerivatives(localdN, invJ);
                    double[,] B = CalculateBMatrix(globaldN);
                    K = MatrixOperations.MatrixAddition(K, MatrixOperations.ScalarMatrixProductNew(detJ * gW[0] * gW[1],
                        MatrixOperations.MatrixProduct(MatrixOperations.Transpose(B), MatrixOperations.MatrixProduct(E, B))));
                }
            }
            return K;
        }

        public double[,] CreateMassMatrix()
        {
            double[,] M = new double[8, 8];

            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    double[] gP = GaussPoints(i, j).Item1;
                    double[] gW = GaussPoints(i, j).Item2;
                    Dictionary<string, double[]> localdN = CalculateShapeFunctionsLocalDerivatives(gP);
                    double[,] J = CalculateJacobian(localdN);
                    double[,] invJ = CalculateInverseJacobian(J).Item1;
                    double detJ = CalculateInverseJacobian(J).Item2;
                    double[,] Nmatrix = CalculateShapeFunctionMatrix(gP[i], gP[j]);
                    M = MatrixOperations.MatrixAddition(M, MatrixOperations.ScalarMatrixProductNew(density * thickness * detJ * gW[i] * gW[j],
                        MatrixOperations.MatrixProduct(MatrixOperations.Transpose(Nmatrix), Nmatrix)));
                }
            }
            return M;
        }

        public double[,] CreateDampingMatrix()
        {
            return new double[8, 8];
        }

        public double[] CreateInternalGlobalForcesVector()
        {
            double[] F = new double[8];
            double[,] E = CalculateStressStrainMatrix(Properties.YoungMod, 1.0 / 3.0); //needs fixing in poisson v

            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    double[] gP = GaussPoints(i, j).Item1;
                    double[] gW = GaussPoints(i, j).Item2;
                    Dictionary<string, double[]> localdN = CalculateShapeFunctionsLocalDerivatives(gP);
                    double[,] J = CalculateJacobian(localdN);
                    double[,] invJ = CalculateInverseJacobian(J).Item1;
                    double detJ = CalculateInverseJacobian(J).Item2;
                    Dictionary<int, double[]> globaldN = CalculateShapeFunctionsGlobalDerivatives(localdN, invJ);
                    double[,] B = CalculateBMatrix(globaldN);
                    double[] strainVector = CalculateStrainsVector(B);
                    double[] stressVector = CalculateStressVector(E,strainVector);
                    F = VectorOperations.VectorVectorAddition(F, VectorOperations.VectorScalarProductNew(
                        VectorOperations.MatrixVectorProduct(MatrixOperations.Transpose(B), stressVector),detJ * gW[0] * gW[1]));
                }
            }
            return F;
        }
    }
}

