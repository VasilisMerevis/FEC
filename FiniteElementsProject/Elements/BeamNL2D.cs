using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FEC
{
    public class BeamNL2D : IElement
    {
        

        public Dictionary<int, INode> Nodes { get; }
        public IElementProperties Properties { get; set; }
        public Dictionary<int, bool[]> ElementFreedomSignature { get; } = new Dictionary<int, bool[]>();
        public List<int> ElementFreedomList { get; set; }
        public double[] DisplacementVector { get; set; }
        public double[] AccelerationVector { get; set; }
        private bool ActivateLumbedMassMatrix = true;


        public BeamNL2D(IElementProperties properties, Dictionary<int, INode> nodes)
        {
            Properties = properties;
            this.Nodes = nodes;
            ElementFreedomSignature[1] = new bool[] { true, true, false, false, false, true };
            ElementFreedomSignature[2] = new bool[] { true, true, false, false, false, true };
            DisplacementVector = new double[6];
        }

        public double CalculateElementLength() //Initial length
        {
            double X1 = Nodes[1].XCoordinate;
            double Y1 = Nodes[1].YCoordinate;
            double X2 = Nodes[2].XCoordinate;
            double Y2 = Nodes[2].YCoordinate;
            double elementLength = Math.Sqrt(Math.Pow((X2 - X1), 2) + Math.Pow((Y2 - Y1), 2));
            return elementLength;
        }

        private double CalculateElementCurrentLength()
        {
            double X1 = Nodes[1].XCoordinate + DisplacementVector[0];
            double Y1 = Nodes[1].YCoordinate + DisplacementVector[1];
            double X2 = Nodes[2].XCoordinate + DisplacementVector[3];
            double Y2 = Nodes[2].YCoordinate + DisplacementVector[4];
            double elementLength = Math.Sqrt(Math.Pow((X2 - X1), 2) + Math.Pow((Y2 - Y1), 2));
            return elementLength;
        }

        public double CalculateElementCurrentSinus()
        {
            double Y1 = Nodes[1].YCoordinate + DisplacementVector[1];
            double Y2 = Nodes[2].YCoordinate + DisplacementVector[4];
            double L = CalculateElementCurrentLength();
            double sinus = (Y2 - Y1) / L;
            return sinus;
        }

        public double CalculateElementSinus() //Initial
        {
            double Y1 = Nodes[1].YCoordinate;
            double Y2 = Nodes[2].YCoordinate;
            double L = CalculateElementLength();
            double sinus = (Y2 - Y1) / L;
            return sinus;
        }

        public double CalculateElementCurrentCosinus()
        {
            double X1 = Nodes[1].XCoordinate + DisplacementVector[0];
            double X2 = Nodes[2].XCoordinate + DisplacementVector[3];
            double L = CalculateElementCurrentLength();
            double cosinus = (X2 - X1) / L;
            return cosinus;
        }

        public double CalculateElementCosinus() //Initial
        {
            double X1 = Nodes[1].XCoordinate;
            double X2 = Nodes[2].XCoordinate;
            double L = CalculateElementLength();
            double cosinus = (X2 - X1) / L;
            return cosinus;
        }

        private double CalculateElementInitialBetaAngle()
        {
            double X1 = Nodes[1].XCoordinate;
            double Y1 = Nodes[1].YCoordinate;
            double X2 = Nodes[2].XCoordinate;
            double Y2 = Nodes[2].YCoordinate;
            double betaAngle = Math.Atan2(Y2 - Y1, X2 - X1);
            return betaAngle;
        }

        private double CalculateElementCurrentBetaAngle()
        {
            double X1 = Nodes[1].XCoordinate + DisplacementVector[0];
            double Y1 = Nodes[1].YCoordinate + DisplacementVector[1];
            double X2 = Nodes[2].XCoordinate + DisplacementVector[3];
            double Y2 = Nodes[2].YCoordinate + DisplacementVector[4];
            double betaAngle = Math.Atan2(Y2 - Y1, X2 - X1);
            return betaAngle;
        }

        #region Local_Values_Calculations
        private double[] CalculateLocalDisplacementVector()
        {
            double lengthCurrent = CalculateElementCurrentLength();
            double lengthInitial = CalculateElementLength();
            double node1Rotation = DisplacementVector[2];
            double node2Rotation = DisplacementVector[5];
            double betaAngleInitial = CalculateElementInitialBetaAngle();
            double betaAngleCurrent = CalculateElementCurrentBetaAngle();
            double[] localDisplacementVector = new[]
            {
                lengthCurrent - lengthInitial,
                node1Rotation - betaAngleCurrent + betaAngleInitial,
                node2Rotation - betaAngleCurrent + betaAngleInitial
            };
            return localDisplacementVector;
        }

        private double[] CalculateInternalLocalForcesVector()
        {
            double lengthInitial = CalculateElementLength();
            double E = Properties.YoungMod;
            double A = Properties.SectionArea;
            double I = Properties.MomentOfInertia;
            double[,] Dmatrix = new[,]
            {
                { E * A / lengthInitial, 0, 0 },
                { 0 , 4 * E * I / lengthInitial, 2 * E * I / lengthInitial },
                { 0 , 2 * E * I / lengthInitial, 4 * E * I / lengthInitial }
            };

            double[] localDisplacementVector = CalculateLocalDisplacementVector();
            double[] internalLocalForcesVector = VectorOperations.MatrixVectorProduct(Dmatrix, localDisplacementVector);
            return internalLocalForcesVector;
        }
        #endregion

        #region Global_Values_Calculations
        public double[] CreateInternalGlobalForcesVector()
        {
            double lengthInitial = CalculateElementLength();
            double lengthCurrent = CalculateElementCurrentLength();
            double E = Properties.YoungMod;
            double A = Properties.SectionArea;
            double I = Properties.MomentOfInertia;
            double node1Rotation = DisplacementVector[2];
            double node2Rotation = DisplacementVector[5];
            double sinCurrent = CalculateElementCurrentSinus();
            double cosCurrent = CalculateElementCurrentCosinus();
            double betaAngleInitial = CalculateElementInitialBetaAngle();
            double betaAngleCurrent = CalculateElementCurrentBetaAngle();

            double N = A * E * (lengthCurrent - lengthInitial) / lengthInitial;
            double M1 = 2 * E * I * (node2Rotation - betaAngleCurrent + betaAngleInitial) / lengthInitial + 4 * E * I * (node1Rotation - betaAngleCurrent + betaAngleInitial) / lengthInitial;
            double M2 = 4 * E * I * (node2Rotation - betaAngleCurrent + betaAngleInitial) / lengthInitial + 2 * E * I * (node1Rotation - betaAngleCurrent + betaAngleInitial) / lengthInitial;
            double[] intforceV = new double[6];
            intforceV[0] = -(M2 * sinCurrent / lengthCurrent) - (M1 * sinCurrent / lengthCurrent) - N * cosCurrent;
            intforceV[1] = (M2 * cosCurrent / lengthCurrent) + (M1 * cosCurrent / lengthCurrent) - N * sinCurrent;
            intforceV[2] = M1;
            intforceV[3] = (M2 * sinCurrent / lengthCurrent) + (M1 * sinCurrent / lengthCurrent) + N * cosCurrent;
            intforceV[4] = -(M2 * cosCurrent / lengthCurrent) - (M1 * cosCurrent / lengthCurrent) + N * sinCurrent;
            intforceV[5] = M2;

            return intforceV;
        }

        public double[,] CreateGlobalStiffnessMatrix()
        {
            double lengthInitial = CalculateElementLength();
            double lengthCurrent = CalculateElementCurrentLength();
            double E = Properties.YoungMod;
            double A = Properties.SectionArea;
            double I = Properties.MomentOfInertia;
            double node1Rotation = DisplacementVector[2];
            double node2Rotation = DisplacementVector[5];
            double sinCurrent = CalculateElementCurrentSinus();
            double cosCurrent = CalculateElementCurrentCosinus();
            double betaAngleInitial = CalculateElementInitialBetaAngle();
            double betaAngleCurrent = CalculateElementCurrentBetaAngle();

            double N = A * E * (lengthCurrent - lengthInitial) / lengthInitial;
            double M1 = 2 * E * I * (node2Rotation - betaAngleCurrent + betaAngleInitial) / lengthInitial + 4 * E * I * (node1Rotation - betaAngleCurrent + betaAngleInitial) / lengthInitial;
            double M2 = 4 * E * I * (node2Rotation - betaAngleCurrent + betaAngleInitial) / lengthInitial + 2 * E * I * (node1Rotation - betaAngleCurrent + betaAngleInitial) / lengthInitial;
            double sinb = sinCurrent;
            double cosb = cosCurrent;
            double cosb2 = cosCurrent * cosCurrent;
            double sinb2 = sinCurrent * sinCurrent;
            double cosbsinb = cosCurrent * sinCurrent;
            double Lc = lengthCurrent;
            double L0 = lengthInitial;
            double Lc2 = lengthCurrent * lengthCurrent;

            double[,] globalStiffnessMatrix = new double[6, 6];
            globalStiffnessMatrix[0, 0] = N * sinb2 / Lc + 12 * E * I * sinb2 / (L0 * Lc2) - 2 * (M2 + M1) * cosbsinb / Lc2 + A * E * cosb2 / L0;
            globalStiffnessMatrix[0, 1] = (M2 + M1) * (cosb2 - sinb2) / Lc2 - N * cosbsinb / Lc - 12 * E * I * cosbsinb / (L0 * Lc2) + A * E * cosbsinb / L0;
            globalStiffnessMatrix[0, 2] = -6 * E * I * sinb / (L0 * Lc);
            globalStiffnessMatrix[0, 3] = -N * sinb2 / Lc - 12 * E * I * sinb2 / (L0 * Lc2) + 2 * (M2 + M1) * cosbsinb / Lc2 - A * E * cosb2 / L0;
            globalStiffnessMatrix[0, 4] = (M2 + M1) * (sinb2 - cosb2) / Lc2 + N * cosbsinb / Lc + 12 * E * I * cosbsinb / (L0 * Lc2) - A * E * cosbsinb / L0;
            globalStiffnessMatrix[0, 5] = -6 * E * I * sinb / (L0 * Lc);

            globalStiffnessMatrix[1, 0] = globalStiffnessMatrix[0, 1];
            globalStiffnessMatrix[1, 1] = A * E * sinb2 / L0 + 2 * (M2 + M1) * cosbsinb / Lc2 + N * cosb2 / Lc + 12 * E * I * cosb2 / (L0 * Lc2);
            globalStiffnessMatrix[1, 2] = 6 * E * I * cosb / (L0 * Lc);
            globalStiffnessMatrix[1, 3] = (M2 + M1) * (sinb2 - cosb2) / Lc2 + N * cosbsinb / Lc + 12 * E * I * cosbsinb / (L0 * Lc2) - A * E * cosbsinb / L0;
            globalStiffnessMatrix[1, 4] = -A * E * sinb2 / L0 - 2 * (M2 + M1) * cosbsinb / Lc2 - N * cosb2 / Lc - 12 * E * I * cosb2 / (L0 * Lc2);
            globalStiffnessMatrix[1, 5] = 6 * E * I * cosb / (L0 * Lc);

            globalStiffnessMatrix[2, 0] = globalStiffnessMatrix[0, 2];
            globalStiffnessMatrix[2, 1] = globalStiffnessMatrix[1, 2];
            globalStiffnessMatrix[2, 2] = 4 * E * I / L0;
            globalStiffnessMatrix[2, 3] = 6 * E * I * sinb / (L0 * Lc);
            globalStiffnessMatrix[2, 4] = -6 * E * I * cosb / (L0 * Lc);
            globalStiffnessMatrix[2, 5] = 2 * E * I / L0;

            globalStiffnessMatrix[3, 0] = globalStiffnessMatrix[0, 3];
            globalStiffnessMatrix[3, 1] = globalStiffnessMatrix[1, 3];
            globalStiffnessMatrix[3, 2] = globalStiffnessMatrix[2, 3];
            globalStiffnessMatrix[3, 3] = N * sinb2 / Lc + 12 * E * I * sinb2 / (L0 * Lc2) - 2 * (M2 + M1) * cosbsinb / Lc2 + A * E * cosb2 / L0;
            globalStiffnessMatrix[3, 4] = (M2 + M1) * (cosb2 - sinb2) / Lc2 - N * cosbsinb / Lc - 12 * E * I * cosbsinb / (L0 * Lc2) + A * E * cosbsinb / L0;
            globalStiffnessMatrix[3, 5] = 6 * E * I * sinb / (L0 * Lc);

            globalStiffnessMatrix[4, 0] = globalStiffnessMatrix[0, 4];
            globalStiffnessMatrix[4, 1] = globalStiffnessMatrix[1, 4];
            globalStiffnessMatrix[4, 2] = globalStiffnessMatrix[2, 4];
            globalStiffnessMatrix[4, 3] = globalStiffnessMatrix[3, 4];
            globalStiffnessMatrix[4, 4] = A * E * sinb2 / L0 + 2 * (M2 + M1) * cosbsinb / Lc2 + N * cosb2 / Lc + 12 * E * I * cosb2 / (L0 * Lc2);
            globalStiffnessMatrix[4, 5] = -6 * E * I * cosb / (L0 * Lc);

            globalStiffnessMatrix[5, 0] = globalStiffnessMatrix[0, 5];
            globalStiffnessMatrix[5, 1] = globalStiffnessMatrix[1, 5];
            globalStiffnessMatrix[5, 2] = globalStiffnessMatrix[2, 5];
            globalStiffnessMatrix[5, 3] = globalStiffnessMatrix[3, 5];
            globalStiffnessMatrix[5, 4] = globalStiffnessMatrix[4, 5];
            globalStiffnessMatrix[5, 5] = 4 * E * I / L0;

            return globalStiffnessMatrix;
        }
        #endregion

        public double[,] CreateLumpedMassMatrix() //based on Bathe page 817 (mixed truss and beam elements)
        {
            double length = CalculateElementLength();

            double a = Properties.Density * Properties.SectionArea * length / 24.0;
            double b = Properties.Density * length / 2.0;

            double[,] localMassMatrix = new double[6, 6];
            localMassMatrix[0, 0] = b;
            localMassMatrix[1, 1] = a * 12.0;
            localMassMatrix[2, 2] = a * Math.Pow(length, 2);
            localMassMatrix[3, 3] = localMassMatrix[0, 0];
            localMassMatrix[4, 4] = localMassMatrix[1, 1];
            localMassMatrix[5, 5] = localMassMatrix[2, 2];

            return localMassMatrix;
        }

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
            return new double[6, 6];
        }

        public double[,] CreateLocalStiffnessMatrix()
        {
            throw new Exception("Local stiffness matrix not implemented");
        }
    }
}
