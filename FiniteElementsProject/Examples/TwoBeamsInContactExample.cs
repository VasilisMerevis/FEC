using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FEC
{
    public static class TwoBeamsInContactExample
    {
        private static Dictionary<int, INode> CreateNodes()
        {
            Dictionary<int, INode> nodes = new Dictionary<int, INode>();
            nodes[1] = new Node(0.0, 0.01);
            nodes[2] = new Node(0.3, 0.01);
            nodes[3] = new Node(0.6, 0.01);
            nodes[4] = new Node(0.45, 0.0);
            nodes[5] = new Node(0.75, 0.0);
            nodes[6] = new Node(0.105, 0.0);
            return nodes;
        }

        private static Dictionary<int, Dictionary<int, int>> CreateConnectivity()
        {
            Dictionary<int, Dictionary<int, int>> connectivity = new Dictionary<int, Dictionary<int, int>>();
            connectivity[1] = new Dictionary<int, int>() { { 1, 1 }, { 2, 2 } };
            connectivity[2] = new Dictionary<int, int>() { { 1, 2 }, { 2, 3 } };
            connectivity[3] = new Dictionary<int, int>() { { 1, 4 }, { 2, 5 } };
            connectivity[4] = new Dictionary<int, int>() { { 1, 5 }, { 2, 6 } };
            connectivity[5] = new Dictionary<int, int>() { { 1, 4 }, { 2, 5 }, { 3, 3 } };            
            return connectivity;
        }

        private static Dictionary<int, bool[]> CreateNodeFAT()
        {
            Dictionary<int, bool[]> nodeFAT = new Dictionary<int, bool[]>();
            nodeFAT[1] = new bool[] { true, true, false, false, false, true };
            nodeFAT[2] = new bool[] { true, true, false, false, false, true };
            nodeFAT[3] = new bool[] { true, true, false, false, false, true };
            nodeFAT[4] = new bool[] { true, true, false, false, false, true };
            nodeFAT[5] = new bool[] { true, true, false, false, false, true };
            nodeFAT[6] = new bool[] { true, true, false, false, false, true };
            return nodeFAT;
        }

        private static Dictionary<int, IElementProperties> CreateElementProperties()
        {
            double E = 200.0e9;
            double A = 0.01;
            double I = 8.333e-6;
            string type = "Beam2D";
            string type2 = "ContactNtS2D";
            Dictionary<int, IElementProperties> elementProperties = new Dictionary<int, IElementProperties>();
            elementProperties[1] = new ElementProperties(E, A, I, type);
            elementProperties[2] = new ElementProperties(E, A, I, type);
            elementProperties[3] = new ElementProperties(E, A, I, type);
            elementProperties[4] = new ElementProperties(E, A, I, type);
            elementProperties[5] = new ElementProperties(E, A, type2);
            return elementProperties;
        }

        private static IAssembly CreateAssembly()
        {
            IAssembly assembly = new Assembly();
            assembly.Nodes = CreateNodes();
            assembly.ElementsConnectivity = CreateConnectivity();
            assembly.ElementsProperties = CreateElementProperties();
            assembly.NodeFreedomAllocationList = CreateNodeFAT();
            assembly.BoundedDOFsVector = new int[] { 1, 2, 3, 16, 17, 18 };
            return assembly;
        }

        public static void RunExample()
        {
            IAssembly elementsAssembly = CreateAssembly();
            elementsAssembly.CreateElementsAssembly();
            elementsAssembly.ActivateBoundaryConditions = true;
            double[,] globalStiffnessMatrix = elementsAssembly.CreateTotalStiffnessMatrix();

            ISolver newSolu = new StaticSolver();
            newSolu.LinearScheme = new PCGSolver();
            newSolu.NonLinearScheme = new LoadControlledNewtonRaphson();
            newSolu.ActivateNonLinearSolver = true;

            double[] externalForces = new double[] { 0, 0, 0, 0, -220000, 0, 0, 0, 0, 0, 0, 0 };
            newSolu.AssemblyData = elementsAssembly;
            newSolu.Solve(externalForces);
            newSolu.PrintSolution();
        }

    }
}
