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
        private int nodesPerSide;
        private Dictionary<int, INode> nodes;

        private Dictionary<int, INode> CreateNodes()
        {
            nodesPerSide = (int)(BlockLength / ElementSize + 1.0);
            //ElementsNumber = (int)Math.Pow(BlockLength / ElementSize, 2.0);
            nodes = new Dictionary<int, INode>();
            
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
            int element = 1;
            for (int i = 1; i < nodesPerSide; i++)
            {
                for (int j = 1; j < nodesPerSide; j++)
                {
                    int localNode1 = (i - 1) * nodesPerSide + j;
                    int localNode2 = (i - 1) * nodesPerSide + j + 1;
                    int localNode3 = i * nodesPerSide + j;
                    int localNode4 = i * nodesPerSide + j + 1;
                    connectivity[element] = new Dictionary<int, int>() { { 1, localNode1 }, { 2, localNode2 }, { 3, localNode3 }, { 4, localNode4 } };
                    element = element + 1;
                }
            }
            int firstBodyTotalNodes = nodesPerSide * nodesPerSide;
            for (int i = 1; i < nodesPerSide; i++)
            {
                for (int j = 1; j < nodesPerSide; j++)
                {
                    int localNode1 = (i - 1) * nodesPerSide + j + firstBodyTotalNodes;
                    int localNode2 = (i - 1) * nodesPerSide + j + 1 + firstBodyTotalNodes;
                    int localNode3 = i * nodesPerSide + j + firstBodyTotalNodes;
                    int localNode4 = i * nodesPerSide + j + 1 + firstBodyTotalNodes;
                    connectivity[element] = new Dictionary<int, int>() { { 1, localNode1 }, { 2, localNode2 }, { 3, localNode3 }, { 4, localNode4 } };
                    element = element + 1;
                }
            }

            for (int i = 1; i <= nodesPerSide; i++)
            {
                int localNode1 = i + firstBodyTotalNodes;
                int localNode2 = i + firstBodyTotalNodes + nodesPerSide;
                connectivity[element] = new Dictionary<int, int>() { { 1, localNode1 }, { 2, localNode2 } };
                element = element + 1;
            }
            ElementsNumber = connectivity.Count;
            return connectivity;
        }

        private Dictionary<int, bool[]> CreateNodeFAT()
        {
            Dictionary<int, bool[]> nodeFAT = new Dictionary<int, bool[]>();
            for (int i = 1; i <= nodes.Count; i++)
            {
                nodeFAT[i] = new bool[] { true, true, false, false, false, false };
            }
            return nodeFAT;
        }

        private Dictionary<int, IElementProperties> CreateElementProperties()
        {
            double E = 200.0e9;
            double A = 1.0;
            string type = "Quad4";
            string type2 = "ContactNtN2D";

            Dictionary<int, IElementProperties> elementProperties = new Dictionary<int, IElementProperties>();
            for (int i = 1; i <= ElementsNumber-11; i++)
            {
                elementProperties[i] = new ElementProperties(E, A, type);
                elementProperties[i].Density = 8000.0;
                elementProperties[i].Thickness = 0.1;
            }
            for (int i = 201; i <= 211; i++)
            {
                elementProperties[i] = new ElementProperties(E, A, type2);
            }
            return elementProperties;
        }

        private IAssembly CreateAssembly()
        {
            IAssembly assembly = new Assembly();
            assembly.Nodes = CreateNodes();
            assembly.ElementsConnectivity = CreateConnectivity();
            assembly.ElementsProperties = CreateElementProperties();
            assembly.NodeFreedomAllocationList = CreateNodeFAT();

            assembly.BoundedDOFsVector = new int[22];
            for (int i = 1; i <= nodesPerSide*2; i++)
            {
                assembly.BoundedDOFsVector[i-1] = i;
            }
            return assembly;
        }

        public void RunStaticExample()
        {
            IAssembly elementsAssembly = CreateAssembly();
            elementsAssembly.CreateElementsAssembly();
            elementsAssembly.ActivateBoundaryConditions = true;
            //double[,] globalStiffnessMatrix = elementsAssembly.CreateTotalStiffnessMatrix();

            ISolver newSolu = new StaticSolver();
            newSolu.LinearScheme = new PCGSolver();
            newSolu.NonLinearScheme = new LoadControlledNewtonRaphson();
            newSolu.ActivateNonLinearSolver = true;
            newSolu.NonLinearScheme.numberOfLoadSteps = 10;

            double[] externalForces = new double[462];
            for (int i = 441; i <= 462; i+=2)
            {
                externalForces[i] = -1.0e9;
            }
            newSolu.AssemblyData = elementsAssembly;
            newSolu.Solve(externalForces);
            newSolu.PrintSolution();
        }

        public void RunDynamicExample()
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
