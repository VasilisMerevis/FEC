using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FEC
{
    public interface ILinearSolution
    {
        double[] Solve(double[,] stiffnessMatrix, double[] forceVector);
    }
}
