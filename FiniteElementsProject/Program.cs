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
            //NonLinearCantileverExample.RunExample();
            //Console.WriteLine();
            //LinearTrussInContactExample.RunExample();
            //Console.WriteLine();
            //CantileverInContact.RunExample();
            //Console.WriteLine();
            //TwoBeamsInContactExample.RunExample();
            //Console.WriteLine();
            TwoQuadsExample.RunExample();
            //Console.WriteLine();
            //TwoBeamsInContactQuadsExample.RunExample();
            //CantileverQuadsExample.RunExample();
            Console.ReadLine();

            //NonSymmetricSystem.Solve();
            //Console.WriteLine();


            //double a = 1.0;
            //double b = 1.0;
            //double c = 1.0;
            //Dictionary<int, INode> nodes = new Dictionary<int, INode>();
            //nodes[1] = new Node(-a, -b, -c);
            //nodes[2] = new Node(a, -b, -c);
            //nodes[3] = new Node(a, b, -c);
            //nodes[4] = new Node(-a, b, -c);
            //nodes[5] = new Node(-a, -b, c);
            //nodes[6] = new Node(a, -b, c);
            //nodes[7] = new Node(a, b, c);
            //nodes[8] = new Node(-a, b, c);

            //double E = 32.0;
            //string type = "Hex8";
            //double A = 1.0;

            //IElementProperties elementProperties = new ElementProperties(E, A, type);

            //Hex8 test = new Hex8(elementProperties, nodes);
            //test.CreateGlobalStiffnessMatrix();

            //double a = 1.0;
            //Dictionary<int, INode> nodes = new Dictionary<int, INode>();
            //nodes[1] = new Node(0.0, 0.0);
            //nodes[2] = new Node(2*a, 0.0);
            //nodes[3] = new Node(2*a, a);
            //nodes[4] = new Node(0, a);

            //double E = 96.0;
            //string type = "Quad4";
            //double A = 1.0;

            //IElementProperties elementProperties = new ElementProperties(E, A, type);

            //Quad4 test = new Quad4(elementProperties, nodes);
            //test.CreateGlobalStiffnessMatrix();
        }
    }
}
