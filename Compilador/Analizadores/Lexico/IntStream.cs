using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compilador.Analizadores.Lexico {
    public class IntStream {
        private int _NumeroActual;
        public IntStream(int nInicial)
        {
            _NumeroActual = nInicial;
        }

        public int Read()
        {
            int n = _NumeroActual;
            _NumeroActual++;
            return n;
        }

        public IEnumerable<int> Read(int cantidad)
        {
            int[] numeros = new int[cantidad];
            for (int i = 0; i < cantidad; i++) {
                numeros[i] = Read();
            }

            return numeros;
        }

        public int Peek()
        {
            return _NumeroActual;
        }
    }
}
