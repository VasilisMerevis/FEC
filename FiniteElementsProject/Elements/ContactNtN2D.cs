using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FEC
{
    class ContactNtN2D : IElement
    {
        public Dictionary<int, INode> Nodes { get; }
        public IElementProperties Properties { get; set; }
        public Dictionary<int, bool[]> ElementFreedomSignature { get; } = new Dictionary<int, bool[]>();
        public List<int> ElementFreedomList { get; set; }
        public double[] DisplacementVector { get; set; }
        private double PenaltyFactor { get; set; }

        public ContactNtN2D(IElementProperties properties, Dictionary<int, INode> nodes)
        {
            Properties = properties;
            this.Nodes = nodes;
            ElementFreedomSignature[1] = new bool[] { true, true, false, false, false, false };
            ElementFreedomSignature[2] = new bool[] { true, true, false, false, false, false };
            DisplacementVector = new double[4];
            PenaltyFactor = properties.YoungMod*100.0;
        }

        private double[] CalculateNormalUnitVector()
        {
            double X1 = Nodes[1].XCoordinate;
            double Y1 = Nodes[1].YCoordinate;
            double X2 = Nodes[2].XCoordinate;
            double Y2 = Nodes[2].YCoordinate;
            double[] normalVector = new double[] { X2 - X1, Y2 - Y1 };
            double normalVectorLength = VectorOperations.VectorNorm2(normalVector);
            double[] normalUnitVec = new double[] { normalVector[0] / normalVectorLength, normalVector[1] / normalVectorLength };
            return normalUnitVec;
        }

        private double[,] CalculatePositionMatrix()
        {
            double[,] aMatrix = new double[,]
                {
                    { -1,0,1,0},
                    {0,-1,0,1 }
                };
            return aMatrix;
        }

        private double CalculateNormalGap()
        {
            double[,] A = CalculatePositionMatrix();
            double[,] AT = MatrixOperations.Transpose(A);
            double[] n = CalculateNormalUnitVector();
            double[] AT_n = VectorOperations.MatrixVectorProduct(AT, n);
            double[] xupd = new double[] {
                Nodes[1].XCoordinate + DisplacementVector[0],
                Nodes[1].YCoordinate + DisplacementVector[1],
                Nodes[2].XCoordinate + DisplacementVector[2],
                Nodes[2].YCoordinate + DisplacementVector[3]
            };
            double normalGap = VectorOperations.VectorDotProduct(xupd, AT_n);
            return normalGap;
        }

        public double[,] CreateGlobalStiffnessMatrix()
        {
            double penetration = CalculateNormalGap();
            if (penetration<=0)
            {
                double[] n = CalculateNormalUnitVector();
                double[,] A = CalculatePositionMatrix();
                double[,] AT = MatrixOperations.Transpose(A);
                double[,] nxn = VectorOperations.VectorVectorTensorProduct(n, n);
                double[,] nxn_A = MatrixOperations.MatrixProduct(nxn, A);
                double[,] AT_nxn_A = MatrixOperations.MatrixProduct(AT, nxn_A);
                double[,] globalStiffnessMatrix = MatrixOperations.ScalarMatrixProductNew(PenaltyFactor, AT_nxn_A);
                return globalStiffnessMatrix;
            }
            else
            {
                double[,] globalStifnessMatrix = new double[4, 4];
                return globalStifnessMatrix;
            }
            
        }

        public double[] CreateInternalGlobalForcesVector()
        {
            double penetration = CalculateNormalGap();
            if (penetration<=0)
            {
                double[,] A = CalculatePositionMatrix();
                double[,] AT = MatrixOperations.Transpose(A);
                double[] n = CalculateNormalUnitVector();
                double[] AT_n = VectorOperations.MatrixVectorProduct(AT, n);
                double ksi = CalculateNormalGap();
                double[] ksi_AT_n = VectorOperations.VectorScalarProductNew(AT_n, ksi);
                double[] e_ksi_AT_n = VectorOperations.VectorScalarProductNew(ksi_AT_n, PenaltyFactor);
                return e_ksi_AT_n;
            }
            else
            {
                double[] internalGlobalForcesVector = new double[4];
                return internalGlobalForcesVector;
            }
        }

        public double[,] CreateMassMatrix()
        {
            throw new Exception("Mass matrix not implemented");
        }
    }
}

