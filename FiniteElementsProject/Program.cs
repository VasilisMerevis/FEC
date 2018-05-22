using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FEC
{
    class Program
    {
        static void Main(string[] args)
        {
            //LinearTrussExample.RunExample();
            //Console.WriteLine();
            //LinearFrameExample.RunExample();
            //Console.WriteLine();
            //LinearFrameTrussHybridExample.RunExample();
            //Console.WriteLine();
            NonLinearCantileverExample.RunExample();
            Console.WriteLine();
            //LinearTrussInContactExample.RunExample();
            //Console.WriteLine();
            //CantileverInContact.RunExample();
            //Console.WriteLine();
            TwoBeamsInContactExample.RunExample();
            Console.WriteLine();

            NonSymmetricSystem.Solve();
            Console.WriteLine();

            Dictionary<int, INode> nodes = new Dictionary<int, INode>();
            nodes[1] = new Node(1.0, -1.0, -1.0);
            nodes[2] = new Node(1.0, 1.0, -1.0);
            nodes[3] = new Node(-1.0, 1.0, -1.0);
            nodes[4] = new Node(-1.0, -1.0, -1.0);
            nodes[5] = new Node(1.0, -1.0, 1.0);
            nodes[6] = new Node(1.0, 1.0, 1.0);
            nodes[7] = new Node(-1.0, 1.0, 1.0);
            nodes[8] = new Node(-1.0, -1.0, 1.0);

            double E = 32.0;
            string type = "Hex8";
            double A = 1.0;
            
            IElementProperties elementProperties = new ElementProperties(E, A, type);

            Hex8 test = new Hex8(elementProperties, nodes);
            test.CreateGlobalStiffnessMatrix();
        }
    }
}
