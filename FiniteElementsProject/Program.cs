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
        }
    }
}
