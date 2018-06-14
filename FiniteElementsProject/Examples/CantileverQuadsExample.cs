using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FEC
{
    public static class CantileverQuadsExample
    {
        private static Dictionary<int, INode> CreateNodes()
        {
            Dictionary<int, INode> nodes = new Dictionary<int, INode>();
            nodes[1] = new Node(0.0, 0.01);
            nodes[2] = new Node(0.3, 0.01);
            nodes[3] = new Node(0.6, 0.01);
            nodes[4] = new Node(0.6, 0.31);
            nodes[5] = new Node(0.3, 0.31);
            nodes[6] = new Node(0.0, 0.31);
            
            return nodes;
        }

        private static Dictionary<int, Dictionary<int, int>> CreateConnectivity()
        {
            Dictionary<int, Dictionary<int, int>> connectivity = new Dictionary<int, Dictionary<int, int>>();
            connectivity[1] = new Dictionary<int, int>() { { 1, 1 }, { 2, 2 }, { 3, 5 }, { 4, 6 } };
            connectivity[2] = new Dictionary<int, int>() { { 1, 2 }, { 2, 3 }, { 3, 4 }, { 4, 5 } };
            
            return connectivity;
        }

        private static Dictionary<int, bool[]> CreateNodeFAT()
        {
            Dictionary<int, bool[]> nodeFAT = new Dictionary<int, bool[]>();
            nodeFAT[1] = new bool[] { true, true, false, false, false, false };
            nodeFAT[2] = new bool[] { true, true, false, false, false, false };
            nodeFAT[3] = new bool[] { true, true, false, false, false, false };
            nodeFAT[4] = new bool[] { true, true, false, false, false, false };
            nodeFAT[5] = new bool[] { true, true, false, false, false, false };
            nodeFAT[6] = new bool[] { true, true, false, false, false, false };
            
            return nodeFAT;
        }

        private static Dictionary<int, IElementProperties> CreateElementProperties()
        {
            double E = 200.0e9;
            double A = 0.01;
            string type = "Quad4";
            
            Dictionary<int, IElementProperties> elementProperties = new Dictionary<int, IElementProperties>();
            elementProperties[1] = new ElementProperties(E, A, type);
            elementProperties[2] = new ElementProperties(E, A, type);
            
            return elementProperties;
        }

        private static IAssembly CreateAssembly()
        {
            IAssembly assembly = new Assembly();
            assembly.Nodes = CreateNodes();
            assembly.ElementsConnectivity = CreateConnectivity();
            assembly.ElementsProperties = CreateElementProperties();
            assembly.NodeFreedomAllocationList = CreateNodeFAT();
            assembly.BoundedDOFsVector = new int[] { 1, 2, 11, 12 };
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
            //newSolu.NonLinearScheme = new LoadControlledNewtonRaphson();
            //newSolu.ActivateNonLinearSolver = true;
            //newSolu.NonLinearScheme.numberOfLoadSteps = 15;

            double[] externalForces = new double[] { 0, 0, 0, 0, 0, -88000000, 0, 0 };
            newSolu.AssemblyData = elementsAssembly;
            newSolu.Solve(externalForces);
            newSolu.PrintSolution();
        }

    }
}
