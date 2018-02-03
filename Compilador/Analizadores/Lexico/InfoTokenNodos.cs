using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compilador.Analizadores.Lexico {
    public class InfoTokenNodos {
        private List<int> _Registrados;
        private List<int> _Validados;

        public InfoTokenNodos(IEnumerable<int> nodosID)
        {
            _Registrados = new List<int>();
            _Validados = new List<int>();

            _Registrados.AddRange(new int[] { 0 }.Concat(nodosID));
        }

        //public bool validar(int nodoID)
        //{

        //}
        public int this[int idx] { get => _Registrados[idx]; }
        public List<int> Nodos{ get => _Registrados;}
    }
}
