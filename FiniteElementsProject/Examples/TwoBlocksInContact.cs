using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FEC
{
    public class TwoBlocksInContact
    {
        double BlockLength { get; set; } = 1.0;
        double ElementSize { get; set; } = 0.1;
        int ElementsNumber { get; set; }
        double Gap { get; set; } = 0.01;
        int nodesPerSide;

        private Dictionary<int, INode> CreateNodes()
        {
            nodesPerSide = (int)(BlockLength / ElementSize + 1.0);
            ElementsNumber = (int)Math.Pow(BlockLength / ElementSize, 2.0);
            Dictionary<int, INode> nodes = new Dictionary<int, INode>();
            
            int nodeNumber = 0;
            for (int j = 0; j < nodesPerSide; j++)
            {
                for (int i = 0; i < nodesPerSide; i++)
                {
                    nodeNumber = nodeNumber + 1;
                    double x = i * ElementSize;
                    double y = j * ElementSize;
                    nodes[nodeNumber] = new Node(x, y);
                }
            }

            for (int j = 0; j < nodesPerSide; j++)
            {
                for (int i = 0; i < nodesPerSide; i++)
                {
                    nodeNumber = nodeNumber + 1;
                    double x = i * ElementSize;
                    double y = j * ElementSize + BlockLength + Gap;
                    nodes[nodeNumber] = new Node(x, y);
                }
            }

            return nodes;
        }

        private Dictionary<int, Dictionary<int, int>> CreateConnectivity()
        {
            Dictionary<int, Dictionary<int, int>> connectivity = new Dictionary<int, Dictionary<int, int>>();
            for (int i = 1; i <= ElementsNumber; i++)
            {
                connectivity[i] = new Dictionary<int, int>() { { 1, i }, { 2, i+1 }, { 3, 3 }, { 4, 6 } };
            }
            connectivity[1] = new Dictionary<int, int>() { { 1, 1 }, { 2, 2 }, { 3, 3 }, { 4, 6 } };
            connectivity[2] = new Dictionary<int, int>() { { 1, 6 }, { 2, 3 }, { 3, 4 }, { 4, 5 } };

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
            double A = 1.0;
            string type = "Quad4";

            Dictionary<int, IElementProperties> elementProperties = new Dictionary<int, IElementProperties>();
            elementProperties[1] = new ElementProperties(E, A, type);
            elementProperties[2] = new ElementProperties(E, A, type);
            elementProperties[1].Density = 8000.0;
            elementProperties[2].Density = 8000.0;
            elementProperties[1].Thickness = 0.1;
            elementProperties[2].Thickness = 0.1;

            return elementProperties;
        }

        private static IAssembly CreateAssembly()
        {
            IAssembly assembly = new Assembly();
            assembly.Nodes = CreateNodes();
            assembly.ElementsConnectivity = CreateConnectivity();
            assembly.ElementsProperties = CreateElementProperties();
            assembly.NodeFreedomAllocationList = CreateNodeFAT();
            assembly.BoundedDOFsVector = new int[] { 1, 2, 3, 4 };
            return assembly;
        }

        public static void RunStaticExample()
        {
            IAssembly elementsAssembly = CreateAssembly();
            elementsAssembly.CreateElementsAssembly();
            elementsAssembly.ActivateBoundaryConditions = true;
            double[,] globalStiffnessMatrix = elementsAssembly.CreateTotalStiffnessMatrix();

            ISolver newSolu = new StaticSolver();
            newSolu.LinearScheme = new PCGSolver();
            newSolu.NonLinearScheme = new LoadControlledNewtonRaphson();
            newSolu.ActivateNonLinearSolver = false;
            newSolu.NonLinearScheme.numberOfLoadSteps = 10;

            double[] externalForces = new double[] { 0, 0, 1e9, -1e9, 0, -1e9, 0, 0 };
            newSolu.AssemblyData = elementsAssembly;
            newSolu.Solve(externalForces);
            newSolu.PrintSolution();
        }

        public static void RunDynamicExample()
        {
            IAssembly elementsAssembly = CreateAssembly();
            elementsAssembly.CreateElementsAssembly();
            elementsAssembly.ActivateBoundaryConditions = true;



            InitialConditions initialValues = new InitialConditions();
            initialValues.InitialAccelerationVector = new double[8];
            initialValues.InitialDisplacementVector = new double[8];
            initialValues.InitialDisplacementVector[7] = -0.02146;
            initialValues.InitialVelocityVector = new double[8];
            initialValues.InitialTime = 0.0;

            ExplicitSolver newSolver = new ExplicitSolver(1.0, 10000);
            newSolver.Assembler = elementsAssembly;

            newSolver.InitialValues = initialValues;
            newSolver.ExternalForcesVector = new double[] { 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0 }; //{ 0, 0, 1e5, -1e5, 0, -1e5, 0, 0 };
            newSolver.LinearSolver = new CholeskyFactorization();
            newSolver.ActivateNonLinearSolution = false;
            newSolver.SolveExplicit();
            newSolver.PrintExplicitSolution();
        }

    }
}
