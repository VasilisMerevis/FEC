using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FEC
{
    public static class CantileverInContact
    {
        private static Dictionary<int, INode> CreateNodes()
        {
            Dictionary<int, INode> nodes = new Dictionary<int, INode>();
            double xCoord = 0.0;
            double cantiLength = 20.0;
            for (int i = 1; i <= 11; i++)
            {
                nodes[i] = new Node(xCoord, 0.5);
                xCoord = xCoord + cantiLength / 10;
            }

            xCoord = cantiLength - 9.0;
            for (int i = 12; i <= 22; i++)
            {
                nodes[i] = new Node(xCoord, 0.0);
                xCoord = xCoord + cantiLength / 10;
            }

            return nodes;
        }

        private static Dictionary<int, Dictionary<int, int>> CreateConnectivity()
        {
            Dictionary<int, Dictionary<int, int>> connectivity = new Dictionary<int, Dictionary<int, int>>();
            int nodeID = 1;
            for (int elem = 1; elem <= 10; elem++)
            {
                connectivity[elem] = new Dictionary<int, int>() { { 1, nodeID }, { 2, nodeID+1 } };
                nodeID = nodeID + 1;
            }

            nodeID = 12;
            for (int elem = 11; elem <= 20; elem++)
            {
                connectivity[elem] = new Dictionary<int, int>() { { 1, nodeID }, { 2, nodeID + 1 } };
                nodeID = nodeID + 1;
            }

            connectivity[21] = new Dictionary<int, int>() { { 1, 16 }, { 2, 17 }, { 3, 11 } };
            return connectivity;
        }

        private static Dictionary<int, bool[]> CreateNodeFAT()
        {
            Dictionary<int, bool[]> nodeFAT = new Dictionary<int, bool[]>();
            for (int node = 1; node <= 22; node++)
            {
                nodeFAT[node] = new bool[] { true, true, false, false, false, true };
            }
            
            return nodeFAT;
        }

        private static Dictionary<int, IElementProperties> CreateElementProperties()
        {
            double E = 10.0e5;
            double A = 1.0;
            double I = 1.0;
            string type = "Beam2D";
            string type2 = "ContactNtS2D";
            Dictionary<int, IElementProperties> elementProperties = new Dictionary<int, IElementProperties>();
            for (int elem = 1; elem <= 20; elem++)
            {
                elementProperties[elem] = new ElementProperties(E, A, I, type);
                elementProperties[elem].Density = 1.0;
            }
            elementProperties[21] = new ElementProperties(E, A, type2);
            elementProperties[21].Density = 8000.0;

            return elementProperties;
        }

        private static IAssembly CreateAssembly()
        {
            IAssembly assembly = new Assembly();
            assembly.Nodes = CreateNodes();
            assembly.ElementsConnectivity = CreateConnectivity();
            assembly.ElementsProperties = CreateElementProperties();
            assembly.NodeFreedomAllocationList = CreateNodeFAT();
            assembly.BoundedDOFsVector = new int[] { 1, 2, 3, 64, 65, 66 };
            assembly.CreateElementsAssembly();
            assembly.ActivateBoundaryConditions = true;
            return assembly;
        }

        public static void RunStaticExample()
        {
            IAssembly elementsAssembly = CreateAssembly();
            //elementsAssembly.CreateElementsAssembly();
            //elementsAssembly.ActivateBoundaryConditions = true;
            //double[,] globalStiffnessMatrix = elementsAssembly.CreateTotalStiffnessMatrix();

            ISolver newSolu = new StaticSolver();
            newSolu.LinearScheme = new LUFactorization();
            newSolu.NonLinearScheme = new LoadControlledNewtonRaphson();
            newSolu.ActivateNonLinearSolver = true;

            double[] externalForces = new double[60];
            externalForces[28] = -5000.0;
            newSolu.AssemblyData = elementsAssembly;
            newSolu.Solve(externalForces);
            newSolu.PrintSolution();
        }

        public static void RunDynamicExample()
        {
            IAssembly elementsAssembly = CreateAssembly();
            
            InitialConditions initialValues = new InitialConditions();
            initialValues.InitialAccelerationVector = new double[60];
            initialValues.InitialDisplacementVector = new double[60];
            initialValues.InitialVelocityVector = new double[60];
            initialValues.InitialTime = 0.0;

            ExplicitSolver newSolver = new ExplicitSolver(1.0, 100);
            newSolver.Assembler = elementsAssembly;

            double[] externalForces = new double[60];
            externalForces[28] = -5000.0;
            
            newSolver.InitialValues = initialValues;
            newSolver.ExternalForcesVector = externalForces;
            newSolver.LinearSolver = new LUFactorization();
            newSolver.ActivateNonLinearSolution = true;
            newSolver.SolveExplicit();
            newSolver.PrintExplicitSolution();
        }
    }
}
