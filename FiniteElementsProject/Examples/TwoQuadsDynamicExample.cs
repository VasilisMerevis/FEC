using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FEC
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
            connectivity[1] = new Dictionary<int, int>() { { 1, 1 }, { 2, 2 }, { 3, 6 }, { 4, 5 } };
            connectivity[2] = new Dictionary<int, int>() { { 1, 2 }, { 2, 3 }, { 3, 7 }, { 4, 6 } };
            connectivity[3] = new Dictionary<int, int>() { { 1, 3 }, { 2, 4 }, { 3, 8 }, { 4, 7 } };
            connectivity[4] = new Dictionary<int, int>() { { 1, 5 }, { 2, 6 }, { 3, 10 }, { 4, 9 } };
            connectivity[5] = new Dictionary<int, int>() { { 1, 6 }, { 2, 7 }, { 3, 11 }, { 4, 10 } };

            connectivity[6] = new Dictionary<int, int>() { { 1, 7 }, { 2, 8 }, { 3, 12 }, { 4, 11 } };
            connectivity[7] = new Dictionary<int, int>() { { 1, 9 }, { 2, 10 }, { 3, 14 }, { 4, 13 } };
            connectivity[8] = new Dictionary<int, int>() { { 1, 10 }, { 2, 11 }, { 3, 15 }, { 4, 14 } };
            connectivity[9] = new Dictionary<int, int>() { { 1, 11 }, { 2, 12 }, { 3, 16 }, { 4, 15 } };
            connectivity[10] = new Dictionary<int, int>() { { 1, 13 }, { 2, 14 }, { 3, 18 }, { 4, 17 } };

            connectivity[11] = new Dictionary<int, int>() { { 1, 14 }, { 2, 15 }, { 3, 19 }, { 4, 18 } };
            connectivity[12] = new Dictionary<int, int>() { { 1, 15 }, { 2, 16 }, { 3, 20 }, { 4, 19 } };
            connectivity[13] = new Dictionary<int, int>() { { 1, 17 }, { 2, 18 }, { 3, 22 }, { 4, 21 } };
            connectivity[14] = new Dictionary<int, int>() { { 1, 18 }, { 2, 19 }, { 3, 23 }, { 4, 22 } };
            connectivity[15] = new Dictionary<int, int>() { { 1, 19 }, { 2, 20 }, { 3, 24 }, { 4, 23 } };

            return connectivity;
        }

        private static Dictionary<int, bool[]> CreateNodeFAT()
        {
            Dictionary<int, bool[]> nodeFAT = new Dictionary<int, bool[]>();
            for (int node = 1; node <= 24; node++)
            {
                nodeFAT[node] = new bool[] { true, true, false, false, false, false };
            }
            return nodeFAT;
        }

        private static Dictionary<int, IElementProperties> CreateElementProperties()
        {
            double E = 200.0e9;
            double A = 1.0;
            string type = "Quad4";
            string type2 = "ContactNtN2D";
            Dictionary<int, IElementProperties> elementProperties = new Dictionary<int, IElementProperties>();
            for (int element = 1; element <= 15; element++)
            {
                elementProperties[element] = new ElementProperties(E, A, type);
            }
            return elementProperties;
        }

        private static IAssembly CreateAssembly()
        {
            IAssembly assembly = new Assembly();
            assembly.Nodes = CreateNodes();
            assembly.ElementsConnectivity = CreateConnectivity();
            assembly.ElementsProperties = CreateElementProperties();
            assembly.NodeFreedomAllocationList = CreateNodeFAT();
            assembly.BoundedDOFsVector = new int[] { 1, 2, 3, 4, 5, 6, 7, 8 };
            return assembly;
        }

        private static double[] CreateExternalForcesVector(Dictionary<int, double> DOFLoads)
        {
            double[] forces = new double[40];
            foreach (KeyValuePair<int, double> loads in DOFLoads)
            {
                forces[loads.Key - 1] = loads.Value;
            }
            return forces;
        }

        public static void RunExample()
        {
            IAssembly elementsAssembly = CreateAssembly();
            elementsAssembly.CreateElementsAssembly();
            elementsAssembly.ActivateBoundaryConditions = true;


            InitialConditions initialValues = new InitialConditions();
            initialValues.InitialAccelerationVector = new double[40];
            initialValues.InitialDisplacementVector = new double[40];
            initialValues.InitialVelocityVector = new double[40];
            initialValues.InitialTime = 0.0;

            ExplicitSolver newSolver = new ExplicitSolver(0.1, 10);
            newSolver.Assembler = elementsAssembly;

            Dictionary<int, double> externalLoads = new Dictionary<int, double>()
            {
                {37, 5000.0 },
                {38, 5000.0 },
                {39, 5000.0 },
                {40, 5000.0 }
            };

            newSolver.InitialValues = initialValues;
            newSolver.ExternalForcesVector = CreateExternalForcesVector(externalLoads);
            newSolver.LinearSolver = new BiCGSTABSolver();
            newSolver.ActivateNonLinearSolution = false;
            newSolver.SolveExplicit();
            newSolver.PrintExplicitSolution();
        }
    }
}
