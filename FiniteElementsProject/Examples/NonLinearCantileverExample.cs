using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FEC
{
    public static class NonLinearCantileverExample
    {
        private static Dictionary<int, INode> CreateNodes()
        {
            Dictionary<int, INode> nodes = new Dictionary<int, INode>();
            nodes[1] = new Node(0.0, 0.0);
            nodes[2] = new Node(0.3, 0.0);
            nodes[3] = new Node(0.6, 0.0);
            nodes[4] = new Node(0.9, 0.0);
            nodes[5] = new Node(1.2, 0.0);
            nodes[6] = new Node(1.5, 0.0);
            nodes[7] = new Node(1.8, 0.0);
            nodes[8] = new Node(2.1, 0.0);
            nodes[9] = new Node(2.4, 0.0);
            nodes[10] = new Node(2.7, 0.0);
            nodes[11] = new Node(3.0, 0.0);
            return nodes;
        }

        private static Dictionary<int, Dictionary<int, int>> CreateConnectivity()
        {
            Dictionary<int, Dictionary<int, int>> connectivity = new Dictionary<int, Dictionary<int, int>>();
            connectivity[1] = new Dictionary<int, int>() { { 1, 1 }, { 2, 2 } };
            connectivity[2] = new Dictionary<int, int>() { { 1, 2 }, { 2, 3 } };
            connectivity[3] = new Dictionary<int, int>() { { 1, 3 }, { 2, 4 } };
            connectivity[4] = new Dictionary<int, int>() { { 1, 4 }, { 2, 5 } };
            connectivity[5] = new Dictionary<int, int>() { { 1, 5 }, { 2, 6 } };
            connectivity[6] = new Dictionary<int, int>() { { 1, 6 }, { 2, 7 } };
            connectivity[7] = new Dictionary<int, int>() { { 1, 7 }, { 2, 8 } };
            connectivity[8] = new Dictionary<int, int>() { { 1, 8 }, { 2, 9 } };
            connectivity[9] = new Dictionary<int, int>() { { 1, 9 }, { 2, 10 } };
            connectivity[10] = new Dictionary<int, int>() { { 1, 10 }, { 2, 11 } };
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
            nodeFAT[7] = new bool[] { true, true, false, false, false, true };
            nodeFAT[8] = new bool[] { true, true, false, false, false, true };
            nodeFAT[9] = new bool[] { true, true, false, false, false, true };
            nodeFAT[10] = new bool[] { true, true, false, false, false, true };
            nodeFAT[11] = new bool[] { true, true, false, false, false, true };
            return nodeFAT;
        }

        private static Dictionary<int, IElementProperties> CreateElementProperties()
        {
            double E = 200.0e9;
            double A = 0.01;
            double I = 8.333e-6;
            string type = "BeamNL2D";
            Dictionary<int, IElementProperties> elementProperties = new Dictionary<int, IElementProperties>();
            elementProperties[1] = new ElementProperties(E, A, I, type);
            elementProperties[2] = new ElementProperties(E, A, I, type);
            elementProperties[3] = new ElementProperties(E, A, I, type);
            elementProperties[4] = new ElementProperties(E, A, I, type);
            elementProperties[5] = new ElementProperties(E, A, I, type);
            elementProperties[6] = new ElementProperties(E, A, I, type);
            elementProperties[7] = new ElementProperties(E, A, I, type);
            elementProperties[8] = new ElementProperties(E, A, I, type);
            elementProperties[9] = new ElementProperties(E, A, I, type);
            elementProperties[10] = new ElementProperties(E, A, I, type);
            return elementProperties;
        }

        private static IAssembly CreateAssembly()
        {
            IAssembly assembly = new Assembly();
            assembly.Nodes = CreateNodes();
            assembly.ElementsConnectivity = CreateConnectivity();
            assembly.ElementsProperties = CreateElementProperties();
            assembly.NodeFreedomAllocationList = CreateNodeFAT();
            assembly.BoundedDOFsVector = new int[] { 1, 2, 3 };
            return assembly;
        }

        public static void RunExample()
        {
            IAssembly elementsAssembly = CreateAssembly();
            elementsAssembly.CreateElementsAssembly();
            elementsAssembly.ActivateBoundaryConditions = true;
            double[,] globalStiffnessMatrix = elementsAssembly.CreateTotalStiffnessMatrix();

            ISolver newSolu = new StaticSolver();
            newSolu.LinearScheme = new CholeskyFactorization();
            newSolu.NonLinearScheme = new LoadControlledNewtonRaphson();
            newSolu.ActivateNonLinearSolver = true;

            double[] externalForces = new double[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, - 2000000, 0 };
            newSolu.AssemblyData = elementsAssembly;
            newSolu.Solve(externalForces);
            newSolu.PrintSolution();
        }

    }
}
