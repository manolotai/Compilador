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

        public int Add()
        {
            _IndiceNodos.Add(_IntLiberados.Count == 0 ? _IdxHead++ : _IntLiberados.Dequeue(),
                new Nodo<TID, TAristas>());
            return _IdxHead - 1;
        }

        public int Add(TID valor)
        {
            _IndiceNodos.Add(_IntLiberados.Count == 0 ? _IdxHead++ : _IntLiberados.Dequeue(),
                new Nodo<TID, TAristas>(valor));
            return _IdxHead - 1;
        }

        public void EnlazarNodos(int origen, int destino,
            IEnumerable<TAristas> key, bool pass = true, Action accion = null)
        {
            foreach (var item in key) {
                _IndiceNodos[origen].AddNodo(_IndiceNodos[destino], item, pass, accion);
            }
        }

        public void EnlazarNodos(int origen, int destino, bool pass = true, Action accion = null,
            params TAristas[] key)
        {
            if (key.Length == 0)
                _IndiceNodos[origen].AddNodo(_IndiceNodos[destino], pass: pass, accion: accion);
            else
                foreach (var item in key)
                    _IndiceNodos[origen].AddNodo(_IndiceNodos[destino], item, pass, accion);
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
