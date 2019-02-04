using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FiniteElementsProject.Examples
{
    public static class TwoQuadsDynamicExample
    {
        private static Dictionary<int, INode> CreateNodes()
        {
            Dictionary<int, INode> nodes = new Dictionary<int, INode>();
            nodes[1] = new Node(0.0, 0.0);
            nodes[2] = new Node(0.67, 0.0);
            nodes[3] = new Node(1.34, 0.0);
            nodes[4] = new Node(2.0, 0.0);

            nodes[5] = new Node(0.0, 0.8);
            nodes[6] = new Node(0.67, 0.8);
            nodes[7] = new Node(1.34, 0.8);
            nodes[8] = new Node(2.0, 0.8);

            nodes[9] = new Node(0.0, 1.6);
            nodes[10] = new Node(0.67, 1.6);
            nodes[11] = new Node(1.34, 1.6);
            nodes[12] = new Node(2.0, 1.6);

            nodes[13] = new Node(0.0, 2.4);
            nodes[14] = new Node(0.67, 2.4);
            nodes[15] = new Node(1.34, 2.4);
            nodes[16] = new Node(2.0, 2.4);

            nodes[17] = new Node(0.0, 3.2);
            nodes[18] = new Node(0.67, 3.2);
            nodes[19] = new Node(1.34, 3.2);
            nodes[20] = new Node(2.0, 3.2);

            nodes[21] = new Node(0.0, 4.0);
            nodes[22] = new Node(0.67, 4.0);
            nodes[23] = new Node(1.34, 4.0);
            nodes[24] = new Node(2.0, 4.0);

            return nodes;
        }

        private static Dictionary<int, Dictionary<int, int>> CreateConnectivity()
        {
            Dictionary<int, Dictionary<int, int>> connectivity = new Dictionary<int, Dictionary<int, int>>();
            connectivity[1] = new Dictionary<int, int>() { { 1, 1 }, { 2, 2 } };
            connectivity[2] = new Dictionary<int, int>() { { 1, 2 }, { 2, 3 } };
            connectivity[3] = new Dictionary<int, int>() { { 1, 3 }, { 2, 4 } };

            return connectivity;
        }

        private static Dictionary<int, bool[]> CreateNodeFAT()
        {
            Dictionary<int, bool[]> nodeFAT = new Dictionary<int, bool[]>();
            nodeFAT[1] = new bool[] { true, true, false, false, false, false };
            nodeFAT[2] = new bool[] { true, true, false, false, false, false };
            nodeFAT[3] = new bool[] { true, true, false, false, false, false };
            nodeFAT[4] = new bool[] { true, true, false, false, false, false };
            return nodeFAT;
        }

        private static Dictionary<int, IElementProperties> CreateElementProperties()
        {
            double E = 1000.0;
            double A = 1.0;
            string type = "Bar2D";
            string type2 = "ContactNtN2D";
            Dictionary<int, IElementProperties> elementProperties = new Dictionary<int, IElementProperties>();
            elementProperties[1] = new ElementProperties(E, A, type);
            elementProperties[2] = new ElementProperties(E, A, type2);
            elementProperties[3] = new ElementProperties(E, A, type);

            return elementProperties;
        }

        private static IAssembly CreateAssembly()
        {
            IAssembly assembly = new Assembly();
            assembly.Nodes = CreateNodes();
            assembly.ElementsConnectivity = CreateConnectivity();
            assembly.ElementsProperties = CreateElementProperties();
            assembly.NodeFreedomAllocationList = CreateNodeFAT();
            assembly.BoundedDOFsVector = new int[] { 1, 2, 4, 6, 7, 8 };
            return assembly;
        }

        public static void RunExample()
        {
            IAssembly elementsAssembly = CreateAssembly();
            elementsAssembly.CreateElementsAssembly();
            elementsAssembly.ActivateBoundaryConditions = true;


            InitialConditions initialValues = new InitialConditions();
            initialValues.InitialAccelerationVector = new double[] { 0.0, 10.0 };
            initialValues.InitialDisplacementVector = new double[] { 0.0, 0.0 };
            initialValues.InitialVelocityVector = new double[] { 0.0, 0.0 };
            initialValues.InitialTime = 0.0;

            ExplicitSolver newSolver = new ExplicitSolver(1.0, 10);
            newSolver.Assembler = elementsAssembly;

            newSolver.InitialValues = initialValues;
            newSolver.ExternalForcesVector = new double[] { 50.0, 0 };
            newSolver.LinearSolver = new LUFactorization();
            newSolver.ActivateNonLinearSolution = true;
            newSolver.SolveExplicit();
            newSolver.PrintExplicitSolution();
        }
    }
}
