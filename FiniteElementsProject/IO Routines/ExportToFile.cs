using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace FEC
{
    public  static class ExportToFile
    {
        public static void ExportExplicitResults(Dictionary<int,double[]> solution, Dictionary<int, double> timeAtEachStep, int dofNumber, int intervals)
        {
            string[] lines = new string[solution.Count / intervals];
            int step = 0;
            int line = 0;
            while (step < solution.Count-1)
            {
                double[] sol = solution[step];
                lines[line] = line.ToString() + " " + timeAtEachStep[step].ToString() + " " + sol[dofNumber].ToString();
                line = line + 1;
                step = step + intervals;
                //if (step >= solution.Count-1)
                //{
                //    break;
                //}
            }
            File.WriteAllLines(@"D:\WriteLines2.txt", lines);
        }
    }
}
