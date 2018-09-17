using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FEC
{
    public static class CentralDifferencesTest
    {
        public static void SolveExample()
        {
            double[,] M = new double[,]
            {
                { 2.0 , 0.0 },
                { 0.0 , 1.0 }
            };

            double[,] K = new double[,]
            {
                { 6.0 , -2.0 },
                { -2.0 , 4.0 }
            };

            double[] F = new double[] { 0.0, 10.0 };

            InitialConditions initialValues = new InitialConditions();
            initialValues.InitialAccelerationVector = new double[] { 0.0, 10.0 };
            initialValues.InitialDisplacementVector = new double[] { 0.0, 0.0 };
            initialValues.InitialVelocityVector = new double[] { 0.0, 0.0 };
            initialValues.InitialTime = 0.0;

            ILinearSolution linearSolver = new LUFactorization();
            CentralDifferences dynamicSolver = new CentralDifferences(linearSolver, initialValues, 2.8, 10, K, M, F);
            dynamicSolver.SolveExplicit();
        }
    }
}
