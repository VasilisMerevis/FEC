using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FEC
{
    class Bar2D : IElement
    {
        public Dictionary<int, INode> Nodes { get; }
        public IElementProperties Properties { get; set; }
        public Dictionary<int, bool[]> ElementFreedomSignature { get; } = new Dictionary<int, bool[]>();
        public List<int> ElementFreedomList { get; set; }
        public double[] DisplacementVector { get; set; }
        

        public Bar2D(IElementProperties properties, Dictionary<int, INode> nodes)
        {
            Properties = properties;
            this.Nodes = nodes;
            ElementFreedomSignature[1] = new bool[] { true, true, false, false, false, false };
            ElementFreedomSignature[2] = new bool[] { true, true, false, false, false, false };
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

        private double CalculateElementCurrentLength()
        {
            double X1 = Nodes[1].XCoordinate + DisplacementVector[0];
            double Y1 = Nodes[1].YCoordinate + DisplacementVector[1];
            double X2 = Nodes[2].XCoordinate + DisplacementVector[2];
            double Y2 = Nodes[2].YCoordinate + DisplacementVector[3];
            double elementLength = Math.Sqrt(Math.Pow((X2 - X1), 2) + Math.Pow((Y2 - Y1), 2));
            return elementLength;
        }

        public double CalculateElementCurrentSinus()
        {
            double Y1 = Nodes[1].YCoordinate + DisplacementVector[1];
            double Y2 = Nodes[2].YCoordinate + DisplacementVector[3];
            double L = CalculateElementCurrentLength();
            double sinus = (Y2 - Y1) / L;
            return sinus;
        }

        public double CalculateElementCurrentCosinus()
        {
            double X1 = Nodes[1].XCoordinate + DisplacementVector[0];
            double X2 = Nodes[2].XCoordinate + DisplacementVector[2];
            double L = CalculateElementCurrentLength();
            double cosinus = (X2 - X1) / L;
            return cosinus;
        }

        public double[,] CreateLocalStiffnessMatrix()
        {
            double E = Properties.YoungMod;
            double A = Properties.SectionArea;
            double length = CalculateElementLength();
            double[,] localStiffnessMatrix = new[,]
            { 
                { E * A / length, 0, 0, -E * A / length, 0, 0 },
                { 0, 0, 0, 0, 0, 0 },
                { 0, 0, 0, 0, 0, 0},
                { -E * A / length, 0, 0, E* A/ length, 0, 0 },
                { 0, 0, 0, 0, 0, 0 },
                { 0, 0, 0, 0, 0, 0}
            };

            return localStiffnessMatrix;
        }

        public double[,] CreateGlobalStiffnessMatrix()
        {
            double A = Properties.SectionArea;
            double E = Properties.YoungMod;
            double L = CalculateElementLength(); 
            double c = CalculateElementCosinus();
            double s = CalculateElementSinus();
            double cs = c * s;
            double c2 = c * c;
            double s2 = s * s;
            double[,] globalStiffnessMatrix = new double[,]
                {
                    {A*E*c2/L, A*E*cs/L, -A*E*c2/L, -A*E*cs/L },
                    {A*E*cs/L, A*E*s2/L, -A*E*cs/L, -A*E*s2/L },
                    {-A*E*c2/L, -A*E*cs/L, A*E*c2/L, A*E*cs/L },
                    {-A*E*cs/L, -A*E*s2/L, A*E*cs/L, A*E*s2/L }
                };
            return globalStiffnessMatrix;
        }

        public double[,] CreateMassMatrix()
        {
            double A = Properties.SectionArea;
            double density = Properties.Density;
            double length = CalculateElementLength();
            double elementMass = density * A * length;
            double[,] massMatrix = new double[,]
            {
                {elementMass/2, 0, 0, 0},
                {0, elementMass/2, 0, 0},
                {0, 0, elementMass/2, 0},
                {0, 0, 0, elementMass/2},
            };

            return massMatrix;
        }

        public double[,] CreateDampingMatrix()
        {
            throw new Exception("Not implemented");
        }

        public double[] CreateInternalGlobalForcesVector()
        {
            double c = CalculateElementCosinus();
            double s = CalculateElementSinus();
            double Linitial = CalculateElementLength();
            double Lcurrent = CalculateElementCurrentLength();
            double deltaL = Lcurrent - Linitial;
            double E = Properties.YoungMod;
            double A = Properties.SectionArea;
            double[] internalGlobalForcesVector = new double[]
            {
                -c*A*E*(deltaL/Linitial),
                -s*A*E*(deltaL/Linitial),
                c*A*E*(deltaL/Linitial),
                s*A*E*(deltaL/Linitial)
            };
            
            return internalGlobalForcesVector;
        }
        
    }
}
