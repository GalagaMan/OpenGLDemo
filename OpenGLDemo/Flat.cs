using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenGLDemo
{
    class Flat : Vertex
    {
        public readonly int Numeral1, Numeral2, Numeral3;
        public Flat(int num1, int num2, int num3)
        {
            Numeral1 = num1;
            Numeral2 = num2;
            Numeral3 = num3;
        }
    }
}
