using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FEC
{
    public interface IElementProperties
    {
        double YoungMod { get; set; }
        double SectionArea { get; set; }
        double MomentOfInertia { get; set; }
        string ElementType { get; set; }
        double Density { get; set; }
        double Thickness { get; set; }
    }
}
