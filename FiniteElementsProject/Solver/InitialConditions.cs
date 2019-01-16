using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FEC
{
    public struct InitialConditions
    {
        public double[] InitialDisplacementVector { get; set; }
        public double[] InitialVelocityVector { get; set; }
        public double[] InitialAccelerationVector { get; set; }
        public double InitialTime { get; set; }
    }
}
