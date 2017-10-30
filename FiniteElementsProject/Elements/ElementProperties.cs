using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FEC
{
    public class ElementProperties : IElementProperties
    {
        public double YoungMod { get; set; }
        public double SectionArea { get; set; }
        public double MomentOfInertia { get; set; }
        public string ElementType { get; set; }
        public double Density { get; set; }

        public ElementProperties(double youngMod, double sectionArea, string elementType)
        {
            YoungMod = youngMod;
            SectionArea = sectionArea;
            ElementType = elementType;
        }

        public ElementProperties(double youngMod, double sectionArea, double momentOfInertia, string elementType)
        {
            YoungMod = youngMod;
            SectionArea = sectionArea;
            MomentOfInertia = momentOfInertia;
            ElementType = elementType;
        }
    }
}
