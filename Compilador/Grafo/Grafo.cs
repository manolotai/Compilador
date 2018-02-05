using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compilador.Grafo {
    public class Grafo<TID, TAristas> {
        private int _IdxHead;
        private Queue<int> _IntLiberados;
        private Dictionary<int, Nodo<TID, TAristas>> _IndiceNodos;
        public Grafo(int seed = 0)
        {
            _IdxHead = seed;
            _IntLiberados = new Queue<int>();
            _IndiceNodos = new Dictionary<int, Nodo<TID, TAristas>>();
        }

        public int Add(TID valor)
        {
            _IndiceNodos.Add(_IntLiberados.Count == 0 ? _IdxHead++ : _IntLiberados.Dequeue(),
                    new Nodo<TID, TAristas>(valor));
            return _IdxHead - 1;
        }

        public void EnlazarNodos(int origen, int destino,
            IEnumerable<TAristas> restriccion, bool pass = true)
        {
            foreach (var item in restriccion) {
                _IndiceNodos[origen].AddNodo(_IndiceNodos[destino], item, pass);
            }
        }

        public void EnlazarNodos(int origen, int destino, params TAristas[] restriccion)
        {
            if(restriccion.Length == 0)
                _IndiceNodos[origen].AddNodo(_IndiceNodos[destino]);
            else
                foreach (var item in restriccion)
                    _IndiceNodos[origen].AddNodo(_IndiceNodos[destino], item);
        }

        public void EnlazarNodos(int origen, int destino, bool pass,
            params TAristas[] restriccion)
        {
            if (restriccion.Length == 0)
                _IndiceNodos[origen].AddNodo(_IndiceNodos[destino], pass);
            else
                foreach (var item in restriccion)
                    _IndiceNodos[origen].AddNodo(_IndiceNodos[destino], item, pass);
        }

        public Dictionary<int, Nodo<TID, TAristas>> IndiceNodos { get => _IndiceNodos; }
        public Nodo<TID, TAristas> this[int idx] {
            get {
                Nodo<TID, TAristas> nodo = _IndiceNodos.TryGetValue(idx, out nodo) ?
                    nodo : null;
                return nodo;
            }
        }
    }
}
