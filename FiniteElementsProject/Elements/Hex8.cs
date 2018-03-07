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

        private Dictionary<string,double[]> CalculateShapeFunctionsDerivatives (double[] globalCoordinates, double[] naturalCoordinates)
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

        public double[,] CreateGlobalStiffnessMatrix()
        {
            return new double[24,24];
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
