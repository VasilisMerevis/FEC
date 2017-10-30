using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FEC
{
    class Node : INode
    {
        public double XCoordinate { get; set; }
        public double YCoordinate { get; set; }
        public double ZCoordinate { get; set; }
        public bool UXDof { get; set; }
        public bool UYDof { get; set; }
        public bool UZDof { get; set; }
        public bool RXDof { get; set; }
        public bool RYDof { get; set; }
        public bool RZDof { get; set; }

        public Node(double xCoordinate, double yCoordinate)
        {
            XCoordinate = xCoordinate;
            YCoordinate = yCoordinate;
            UXDof = true;
            UYDof = true;
            RZDof = true;
        }

        public Node(double xCoordinate, double yCoordinate, double zCoordinate )
        {
            XCoordinate = xCoordinate;
            YCoordinate = yCoordinate;
            ZCoordinate = zCoordinate;
            UXDof = true;
            UYDof = true;
            UZDof = true;
            RXDof = true;
            RYDof = true;
            RZDof = true;
        }
    }
}
