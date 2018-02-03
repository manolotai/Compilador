using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compilador.Grafo {
    public class Nodo<TValor, TAristas> {
        private TValor _Valor;
        private List<Arista?> _AristasFree;
        private Dictionary<TAristas, Arista?> _AristasLock;

        public Nodo(TValor valor)
        {
            _Valor = valor;
            _AristasFree = new List<Arista?>();
            _AristasLock = new Dictionary<TAristas, Arista?>();
        }

        public void AddNodo(Nodo<TValor, TAristas> nodo, bool pass = true)
        {
            _AristasFree.Add(new Arista(nodo, pass));
        }

        public void AddNodo(Nodo<TValor, TAristas> nodo, TAristas restriccion, bool pass = true)
        {
            AristasLock.Add(restriccion, new Arista(nodo, pass));
        }

        //public Nodo<TValor, TAristas> this[TAristas idx] {
        //    get {
        //        try { return _AristasLock[idx].Nodo;
        //        } /*catch (KeyNotFoundException) {
        //            return _AristasFree.FirstOrDefault(arst => arst.Pass).Nodo;
        //        }*/ catch {
        //            return _AristasFree.FirstOrDefault(arst => arst.Pass).Nodo;
        //        }
        //    }
        //}

        public Arista? this[TAristas idx] {
            get {
                try {
                    return _AristasLock[idx];
                } /*catch (KeyNotFoundException) {
                    return _AristasFree.FirstOrDefault(arst => arst.Pass).Nodo;
                }*/ catch {
                    return _AristasFree.FirstOrDefault(arst => arst.Value.Pass);
                }
            }
        }

        public TValor Valor { get => _Valor; set => _Valor = value; }
        public List<Arista?> AristasFree { get => _AristasFree; }
        public Dictionary<TAristas, Arista?> AristasLock { get => _AristasLock; }

        public struct Arista {
            public bool Pass;
            public Nodo<TValor, TAristas> Nodo;

            public Arista(Nodo<TValor, TAristas> nodo, bool pass = true)
            {
                Pass = pass;
                Nodo = nodo;
            }
        }
    }
}
