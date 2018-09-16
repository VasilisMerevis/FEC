using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FEC
{
    class Dynamic : ISolver
    {
        private double[] dynamicSolutionVector;
        public IAssembly AssemblyData { get; set; }
        public ILinearSolution LinearScheme { get; set; }
        public INonLinearSolution NonLinearScheme { get; set; }
        public bool ActivateNonLinearSolver { get; set; }
        public double[,] CustomStiffnessMatrix { get; set; }
        public double[,] CustomMassMatrix { get; set; }
        public InitialConditions InitialConditionValues { get; set; }
        public double TotalSimulationTime { get; set; }
        public int TotalTimeSteps { get; set; }

        public void Solve(double[] rhsVector)
        {
            if (ActivateNonLinearSolver == true)
            {
                dynamicSolutionVector = NonLinearScheme.Solve(AssemblyData, LinearScheme, rhsVector);
            }
            else
            {
                if (CustomStiffnessMatrix == null)
                {
                    double[,] coefMatrix = AssemblyData.CreateTotalStiffnessMatrix();
                    dynamicSolutionVector = LinearScheme.Solve(coefMatrix, rhsVector);
                }
                //double[,] coefMatrix = AssemblyData.CreateTotalStiffnessMatrix();
                //staticSolutionVector = LinearScheme.Solve(coefMatrix, rhsVector);
                else
                {
                    CentralDifferences dynamicScheme = new CentralDifferences(LinearScheme, InitialConditionValues, TotalSimulationTime, TotalTimeSteps, CustomStiffnessMatrix, CustomMassMatrix, rhsVector);
                    dynamicSolutionVector = dynamicScheme.GetExplicitSolution;
                }
            }
        }

        public void PrintSolution()
        {
            VectorOperations.PrintVector(dynamicSolutionVector);
        }

    }
}
