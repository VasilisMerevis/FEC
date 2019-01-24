using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FEC
{
    class ContactNtN2Df : IElement
    {
        public Dictionary<int, INode> Nodes { get; }
        public IElementProperties Properties { get; set; }
        public Dictionary<int, bool[]> ElementFreedomSignature { get; } = new Dictionary<int, bool[]>();
        public List<int> ElementFreedomList { get; set; }
        public double[] DisplacementVector { get; set; }
        private double PenaltyFactor { get; set; }
        private double FrictionCoef { get; set; }

        public ContactNtN2Df(IElementProperties properties, Dictionary<int, INode> nodes)
        {
            Properties = properties;
            this.Nodes = nodes;
            ElementFreedomSignature[1] = new bool[] { true, true, false, false, false, false };
            ElementFreedomSignature[2] = new bool[] { true, true, false, false, false, false };
            DisplacementVector = new double[4];
            PenaltyFactor = properties.YoungMod * 100.0;
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

        private double[] CalculateTangentUnitVector()
        {
            double[] tangentUnitVector = new double[] { 0.0, 1.0 };
            return tangentUnitVector;
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

        private double CalculateTangentialTraction()
        {
            double[,] A = CalculatePositionMatrix();
            double[,] AT = MatrixOperations.Transpose(A);
            double[] displacementVector = DisplacementVector;
            double[] tangentUnitvector = CalculateTangentUnitVector();
            double[] aT_t = VectorOperations.MatrixVectorProduct(AT, tangentUnitvector);
            double tangentGap = VectorOperations.VectorDotProduct(displacementVector, aT_t);
            double tangentialTraction = -PenaltyFactor * tangentGap;
            return tangentialTraction;
        }

        private double ReturnMappingScheme(double traction, double penetration)
        {
            double phi;
            phi = Math.Sqrt(traction * traction) - FrictionCoef * PenaltyFactor * Math.Abs(penetration);
            return phi;
        }

        private double[,] CalculateNormalStiffnessMatrix()
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

        private double[,] CalculateTangentialStiffnessMatrixForStick()
        {
            double[] t = CalculateTangentUnitVector();
            double[,] A = CalculatePositionMatrix();
            double[,] AT = MatrixOperations.Transpose(A);
            double[,] txt = VectorOperations.VectorVectorTensorProduct(t, t);
            double[,] txt_A = MatrixOperations.MatrixProduct(txt, A);
            double[,] AT_txt_A = MatrixOperations.MatrixProduct(AT, txt_A);
            double[,] tangentialStiffnessMatrix = MatrixOperations.ScalarMatrixProductNew(-PenaltyFactor, AT_txt_A);
            return tangentialStiffnessMatrix;
        }

        private double[,] CalculateTangentialStiffnessMatrixForSlip(double tangentialTraction)
        {
            double Tr = tangentialTraction;
            double[] t = CalculateTangentUnitVector();
            double[] n = CalculateNormalUnitVector();
            double[,] A = CalculatePositionMatrix();
            double[,] AT = MatrixOperations.Transpose(A);
            double[,] txn = VectorOperations.VectorVectorTensorProduct(t, n);
            double[,] txn_A = MatrixOperations.MatrixProduct(txn, A);
            double[,] AT_txn_A = MatrixOperations.MatrixProduct(AT, txn_A);
            double scalarFactor = -FrictionCoef * PenaltyFactor * (Tr / Math.Abs(Tr));
            double[,] tangentialStiffnessMatrix = MatrixOperations.ScalarMatrixProductNew(scalarFactor, AT_txn_A);
            return tangentialStiffnessMatrix;
        }

        public double[,] CreateGlobalStiffnessMatrix()
        {
            double[,] globalStiffnessMatrix;
            double[,] normalStiffnessMatrix;
            double[,] tangentialStiffnessMatrix;
            double penetration = CalculateNormalGap();
            if (penetration <= 0)
            {
                normalStiffnessMatrix = CalculateNormalStiffnessMatrix();
                double tangentialTraction = CalculateTangentialTraction();
                double phi = ReturnMappingScheme(tangentialTraction, penetration);
                if (phi<=0)
                {
                    tangentialStiffnessMatrix = CalculateTangentialStiffnessMatrixForStick();
                }
                else
                {
                    tangentialStiffnessMatrix = CalculateTangentialStiffnessMatrixForSlip(tangentialTraction);
                }
                globalStiffnessMatrix = MatrixOperations.MatrixAddition(normalStiffnessMatrix, tangentialStiffnessMatrix);
                return globalStiffnessMatrix;
            }
            else
            {
                globalStiffnessMatrix = new double[4, 4];
                return globalStiffnessMatrix;
            }

        }

        public double[,] CreateDampingMatrix()
        {
            throw new Exception("Not implemented");
        }

        public double[] CreateInternalGlobalForcesVector()
        {
            double penetration = CalculateNormalGap();
            if (penetration <= 0)
            {
                double[,] A = CalculatePositionMatrix();
                double[,] AT = MatrixOperations.Transpose(A);
                double[] n = CalculateNormalUnitVector();
                double[] t = CalculateTangentUnitVector();
                double[] AT_n = VectorOperations.MatrixVectorProduct(AT, n);
                double[] AT_t = VectorOperations.MatrixVectorProduct(AT, t);
                double ksi = CalculateNormalGap();
                double Tr = CalculateTangentialTraction();
                double[] ksi_AT_n = VectorOperations.VectorScalarProductNew(AT_n, ksi);
                double[] e_ksi_AT_n = VectorOperations.VectorScalarProductNew(ksi_AT_n, PenaltyFactor);
                double[] Tr_AT_t = VectorOperations.VectorScalarProductNew(AT_t, Tr);
                double[] internalForcesvector = VectorOperations.VectorVectorAddition(e_ksi_AT_n, Tr_AT_t);
                return internalForcesvector;
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

