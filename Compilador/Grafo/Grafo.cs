using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compilador.Grafo {
    public class Grafo<TID, TAristas> {
        private Dictionary<TID, Nodo<TID, TAristas>> _IndiceNodos;
        public Grafo()
        {
            _IndiceNodos = new Dictionary<TID, Nodo<TID, TAristas>>();
        }

        private void Prevent(TID key)
        {
            if (!_IndiceNodos.Keys.Contains(key))//Poner en un solo metodo?
                Add(key);
        }

        public void Add(TID valor)
        {
            try {
                _IndiceNodos.Add(valor, new Nodo<TID, TAristas>(valor));
            } catch (ArgumentException) {
                return;
            }
        }

        public void EnlazarNodos(TID origen, TID destino, bool pass = true)
        {
            //Poner metodo add nodo?
            Prevent(origen);
            Prevent(destino);
            _IndiceNodos[origen].AddNodo(_IndiceNodos[destino], pass);
        }

        public void EnlazarNodos(TID origen, TID destino,
            IEnumerable<TAristas> restriccion, bool pass = true)
        {
            Prevent(origen);
            Prevent(destino);
            foreach (var item in restriccion) {
                _IndiceNodos[origen].AddNodo(_IndiceNodos[destino], item);
            }
        }

        public void EnlazarNodos(TID origen, TID destino, params TAristas[] restriccion)
        {
            Prevent(origen);
            Prevent(destino);
            foreach (var item in restriccion) {
                _IndiceNodos[origen].AddNodo(_IndiceNodos[destino], item);
            }
        }

        public void EnlazarNodos(TID origen, TID destino, bool pass,
            params TAristas[] restriccion)
        {
            Prevent(origen);
            Prevent(destino);
            foreach (var item in restriccion) {
                _IndiceNodos[origen].AddNodo(_IndiceNodos[destino], item, pass);
            }
        }

        public Dictionary<TID, Nodo<TID, TAristas>> IndiceNodos { get => _IndiceNodos; }
    }
}
