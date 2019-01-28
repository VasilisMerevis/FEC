using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FEC
{
    public class Assembly : IAssembly
    {
        public Dictionary<int, IElementProperties> ElementsProperties { get; set; }
        public Dictionary<int, INode> Nodes { get; set; }
        public Dictionary<int, Dictionary<int,int>> ElementsConnectivity { get; set; }
        public Dictionary<int, IElement> ElementsAssembly { get; set; }
        private int totalDOF;
        public Dictionary<int,bool[]> NodeFreedomAllocationList { get; set; }
        public bool ActivateBoundaryConditions { get; set; }
        public int[] BoundedDOFsVector { get; set; }
        //private int[] boundedDOFsVector;
        
        //public int[] BoundedDOFsVector
        //{
        //    get
        //    {
        //        return boundedDOFsVector;
        //    }

        //    set
        //    {
        //        boundedDOFsVector = value;
                
        //    }
        //}

        private Dictionary<int, INode> AssignElementNodes(Dictionary<int,int> elementConnectivity)
        {
            Dictionary<int, INode> elementNodes = new Dictionary<int, INode>();
            for (int i = 1; i <= elementConnectivity.Count; i++)
            {
                int node = elementConnectivity[i];
                elementNodes[i] = Nodes[node];
            }
            return elementNodes;
        }

        private Dictionary<int,int> CreateNodeFreedomMapList()
        {
            Dictionary<int, int> nodeFMT = new Dictionary<int, int>();
            int baselineCounter = 0;
            for (int node = 1; node <= NodeFreedomAllocationList.Count; node++)
            {
                nodeFMT[node] = baselineCounter;
                bool[] nodeActiveDofs = NodeFreedomAllocationList[node];
                int nodeActiveDofsCount = nodeActiveDofs.Count(c => c == true);
                baselineCounter = baselineCounter + nodeActiveDofsCount;
            }
            totalDOF = baselineCounter;
            return nodeFMT;
        }

        private List<int> CreateElementFreedomList(Dictionary<int, int> singleElementConnectivity, Dictionary<int, int> nodeFMT, Dictionary<int, bool[]> elementFreedomSignature)
        {
            List<int> globalDOFs = new List<int>();
            foreach (KeyValuePair<int, int> node in singleElementConnectivity)
            {
                int localNode = node.Key;
                int globalNode = node.Value;
                int countActiveDOFs = elementFreedomSignature[localNode].Count(c => c == true);
                for (int i = 0; i < countActiveDOFs; i++)
                {
                    globalDOFs.Add(nodeFMT[globalNode] + i);
                }

            }
            return globalDOFs;
        }

        public void CreateElementsAssembly()
        {
            ElementsAssembly = new Dictionary<int, IElement>();
            Dictionary<int, int> nodefmt = CreateNodeFreedomMapList();
            for (int elem = 1; elem <= ElementsConnectivity.Count; elem++)
            {
                Dictionary<int, INode> elementNodes = AssignElementNodes(ElementsConnectivity[elem]);

                switch (ElementsProperties[elem].ElementType)
                {
                    case "Bar2D":
                        ElementsAssembly[elem] = new Bar2D(ElementsProperties[elem], elementNodes);
                        break;
                    case "Beam2D":
                        ElementsAssembly[elem] = new Beam2D(ElementsProperties[elem], elementNodes);
                        break;
                    case "BeamNL2D":
                        ElementsAssembly[elem] = new BeamNL2D(ElementsProperties[elem], elementNodes);
                        break;
                    case "ContactNtN2D":
                        ElementsAssembly[elem] = new ContactNtN2D(ElementsProperties[elem], elementNodes);
                        break;
                    case "ContactNtS2D":
                        ElementsAssembly[elem] = new ContactNtS2D(ElementsProperties[elem], elementNodes);
                        break;
                    case "Quad4":
                        ElementsAssembly[elem] = new Quad4(ElementsProperties[elem], elementNodes);
                        break;
                    case "ContactNtS2Df":
                        ElementsAssembly[elem] = new ContactNtS2Df(ElementsProperties[elem], elementNodes);
                        break;
                }
                Dictionary<int, bool[]> efs = ElementsAssembly[elem].ElementFreedomSignature;
                Dictionary<int, int> elemConnectivity = ElementsConnectivity[elem];
                List<int> eft = CreateElementFreedomList(elemConnectivity, nodefmt, efs);
                ElementsAssembly[elem].ElementFreedomList = eft;
            }
        }

        public void UpdateDisplacements(double[] totalDisplacementVector)
        {
            double[] fullTotalDisplacementVector = BoundaryConditionsImposition.CreateFullVectorFromReducedVector(totalDisplacementVector, BoundedDOFsVector);
            for (int element = 1; element <= ElementsConnectivity.Count; element++)
            {
                int elementDofs = ElementsAssembly[element].ElementFreedomList.Count;
                double[] elementDisplacementVector = new double[elementDofs];
                for (int i = 0; i < elementDofs; i++)
                {
                    int localRow = i;
                    int globalRow = ElementsAssembly[element].ElementFreedomList[i];
                    elementDisplacementVector[localRow] = fullTotalDisplacementVector[globalRow];
                }
                ElementsAssembly[element].DisplacementVector = elementDisplacementVector;
            }
        }

        public double[,] CreateTotalStiffnessMatrix()
        {
            double[,] totalStiffnessMatrix = new double[totalDOF, totalDOF];
            for (int element = 1; element <= ElementsConnectivity.Count; element++)
            {
                int elementDofs = ElementsAssembly[element].ElementFreedomList.Count;
                double[,] elementStiffnessMatrix = ElementsAssembly[element].CreateGlobalStiffnessMatrix();

                for (int i = 0; i < elementDofs; i++)
                {
                    int localRow = i;
                    int globalRow = ElementsAssembly[element].ElementFreedomList[i];
                    for (int j = 0; j < elementDofs; j++)
                    {
                        int localColumn = j;
                        int globalColumn = ElementsAssembly[element].ElementFreedomList[j];
                        totalStiffnessMatrix[globalRow, globalColumn] = totalStiffnessMatrix[globalRow, globalColumn] + elementStiffnessMatrix[localRow, localColumn];
                    }
                }
            }

            if (ActivateBoundaryConditions)
            {
                double[,] reducedStiffnessMatrix = BoundaryConditionsImposition.ReducedTotalStiff(totalStiffnessMatrix, BoundedDOFsVector);
                return reducedStiffnessMatrix;
            }
            else
            {
                return totalStiffnessMatrix;
            }
        }

        public double[,] CreateTotalMassMatrix()
        {
            //Array.Clear(TotalMassMatrix, 0, TotalMassMatrix.Length);
            //for (int element = 0; element < localNode1.Length; element++)
            //{
            //    List<int> dof = beamElementsList[element].ElementDOFs(localNode1, localNode2, element);
            //    for (int i = 0; i < dof.Count; i++)
            //    {
            //        for (int j = 0; j < dof.Count; j++)
            //        {
            //            totalMassMatrix[dof[i] - 1, dof[j] - 1] = totalMassMatrix[dof[i] - 1, dof[j] - 1] + beamElementsList[element].massMatrix[i, j];
            //        }
            //    }
            //}
            //double[,] reducedMassMatrix = BoundaryConditionsImposition.ReducedTotalStiff(totalMassMatrix, boundedDOFsVector);
            //return reducedMassMatrix;
            double[,] totalMassMatrix = new double[totalDOF, totalDOF];
            for (int element = 1; element <= ElementsConnectivity.Count; element++)
            {
                int elementDofs = ElementsAssembly[element].ElementFreedomList.Count;
                double[,] elementMassMatrix = ElementsAssembly[element].CreateMassMatrix();

                for (int i = 0; i < elementDofs; i++)
                {
                    int localRow = i;
                    int globalRow = ElementsAssembly[element].ElementFreedomList[i];
                    for (int j = 0; j < elementDofs; j++)
                    {
                        int localColumn = j;
                        int globalColumn = ElementsAssembly[element].ElementFreedomList[j];
                        totalMassMatrix[globalRow, globalColumn] = totalMassMatrix[globalRow, globalColumn] + elementMassMatrix[localRow, localColumn];
                    }
                }
            }

            if (ActivateBoundaryConditions)
            {
                double[,] reducedMassMatrix = BoundaryConditionsImposition.ReducedTotalStiff(totalMassMatrix, BoundedDOFsVector);
                return reducedMassMatrix;
            }
            else
            {
                return totalMassMatrix;
            }

        }

        public double[,] CreateTotalDampingMatrix()
        {
            double[,] totalDampingMatrix = new double[totalDOF, totalDOF];
            for (int element = 1; element <= ElementsConnectivity.Count; element++)
            {
                int elementDofs = ElementsAssembly[element].ElementFreedomList.Count;
                double[,] elementDampingMatrix = ElementsAssembly[element].CreateDampingMatrix();

                for (int i = 0; i < elementDofs; i++)
                {
                    int localRow = i;
                    int globalRow = ElementsAssembly[element].ElementFreedomList[i];
                    for (int j = 0; j < elementDofs; j++)
                    {
                        int localColumn = j;
                        int globalColumn = ElementsAssembly[element].ElementFreedomList[j];
                        totalDampingMatrix[globalRow, globalColumn] = totalDampingMatrix[globalRow, globalColumn] + elementDampingMatrix[localRow, localColumn];
                    }
                }
            }

            if (ActivateBoundaryConditions)
            {
                double[,] reducedDampingMatrix = BoundaryConditionsImposition.ReducedTotalStiff(totalDampingMatrix, BoundedDOFsVector);
                return reducedDampingMatrix;
            }
            else
            {
                return totalDampingMatrix;
            }

        }

        public double[] CreateTotalInternalForcesVector()
        {
            double[] internalForcesTotalVector = new double[totalDOF];
            for (int element = 1; element <= ElementsConnectivity.Count; element++)
            {
                int elementDofs = ElementsAssembly[element].ElementFreedomList.Count;
                double[] elementInternalGlobalForcesVector = ElementsAssembly[element].CreateInternalGlobalForcesVector();
                for (int i = 0; i < elementDofs; i++)
                {
                    int localLine = i;
                    int globalLine = ElementsAssembly[element].ElementFreedomList[i];
                    internalForcesTotalVector[globalLine] = internalForcesTotalVector[globalLine] + elementInternalGlobalForcesVector[localLine];
                }
            }
            if (ActivateBoundaryConditions)
            {
                double[] reducedInternalForcesVector = BoundaryConditionsImposition.ReducedVector(internalForcesTotalVector, BoundedDOFsVector);
                return reducedInternalForcesVector;
            }
            return internalForcesTotalVector;
        }
    }
}
