using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Compilador.Generador
{
    public class Ensamblador
    {
        private StreamWriter _Ensamblado;
        public Ensamblador(StreamWriter stream)
        {
            _Ensamblado = stream;
        }

        public void WR(string codigo)
        {
            _Ensamblado.WriteLine(codigo);
        }
    }
}
