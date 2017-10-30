using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FEC
{
    class LinearFrameTrussHybridExample
    {
        private static Dictionary<int, INode> CreateNodes()
        {
            Dictionary<int, INode> nodes = new Dictionary<int, INode>();
            nodes[1] = new Node(0.0, 3.0);
            nodes[2] = new Node(4.0, 0.0);
            nodes[3] = new Node(4.0, 3.0);
            //nodes[4] = new Node(0.0, 0.0);
            nodes[5] = new Node(8.0, 3.0);
            return nodes;
        }

        private static Dictionary<int, Dictionary<int, int>> CreateConnectivity()
        {
            Dictionary<int, Dictionary<int, int>> connectivity = new Dictionary<int, Dictionary<int, int>>();
            connectivity[1] = new Dictionary<int, int>() { { 1, 1 }, { 2, 3 } };
            connectivity[2] = new Dictionary<int, int>() { { 1, 3 }, { 2, 5 } };
            connectivity[3] = new Dictionary<int, int>() { { 1, 1 }, { 2, 2 } };
            connectivity[4] = new Dictionary<int, int>() { { 1, 2 }, { 2, 3 } };
            connectivity[5] = new Dictionary<int, int>() { { 1, 2 }, { 2, 5 } };
            return connectivity;
        }

        private static Dictionary<int, bool[]> CreateNodeFAT()
        {
            Dictionary<int, bool[]> nodeFAT = new Dictionary<int, bool[]>();
            nodeFAT[1] = new bool[] { true, true, false, false, false, true };
            nodeFAT[2] = new bool[] { true, true, false, false, false, false };
            nodeFAT[3] = new bool[] { true, true, false, false, false, true };
            nodeFAT[4] = new bool[] { false, false, false, false, false, false };
            nodeFAT[5] = new bool[] { true, true, false, false, false, true };
            return nodeFAT;
        }

        private static Dictionary<int, IElementProperties> CreateElementProperties()
        {
            double Ebeam = 30000.0;
            double Ebar = 200000.0;
            double Abeam = 0.02;
            double Abar = 0.001;
            double Ambar = 0.003;
            double I = 0.004;
            string type = "Beam2D";
            string type2 = "Bar2D";
            Dictionary<int, IElementProperties> elementProperties = new Dictionary<int, IElementProperties>();
            elementProperties[1] = new ElementProperties(Ebeam, Abeam, I, type);
            elementProperties[2] = new ElementProperties(Ebeam, Abeam, I, type);
            elementProperties[3] = new ElementProperties(Ebar, Abar, type2);
            elementProperties[4] = new ElementProperties(Ebar, Ambar, type2);
            elementProperties[5] = new ElementProperties(Ebar, Abar, type2);
            return elementProperties;
        }

        private static IAssembly CreateAssembly()
        {
            IAssembly assembly = new Assembly();
            assembly.Nodes = CreateNodes();
            assembly.ElementsConnectivity = CreateConnectivity();
            assembly.ElementsProperties = CreateElementProperties();
            assembly.NodeFreedomAllocationList = CreateNodeFAT();
            
            return assembly;
        }

        public static void RunExample()
        {
            IAssembly elementsAssembly = CreateAssembly();
            elementsAssembly.CreateElementsAssembly();
            elementsAssembly.ActivateBoundaryConditions = false;
            double[,] globalStiffnessMatrix = elementsAssembly.CreateTotalStiffnessMatrix();

            MatrixOperations.PrintMatrix(globalStiffnessMatrix);
        }
    }
}
