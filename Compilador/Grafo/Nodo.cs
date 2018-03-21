using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compilador.Grafo {
    public class Nodo<TValor, TAristas> {
        private TValor _Valor;
        private List<Arista> _AristasFree;
        private Dictionary<TAristas, Arista> _AristasLock;

        public Nodo()
        {
            _Valor = default(TValor);
            _AristasFree = new List<Arista>();
            _AristasLock = new Dictionary<TAristas, Arista>();
        }

        public Nodo(TValor valor)
        {
            _Valor = valor;
            _AristasFree = new List<Arista>();
            _AristasLock = new Dictionary<TAristas, Arista>();
        }

        public void AddNodo(Nodo<TValor, TAristas> nodo,
            bool pass = true, Action accion = null)
        {
            _AristasFree.Add(new Arista(nodo, accion, pass));
        }

        public void AddNodo(Nodo<TValor, TAristas> nodo, TAristas restriccion, 
            bool pass = true, Action accion = null)
        {
            AristasLock.Add(restriccion, new Arista(nodo, accion, pass));
        }

        public Arista? TryGetPass(TAristas idx)
        {
            Arista arista;
            if (!_AristasLock.TryGetValue(idx, out arista))
                return null;
            return arista;
        }

        public Arista this[TAristas idx] {
            get {
                Arista arista = !_AristasLock.TryGetValue(idx, out arista) ?
                    _AristasFree.FirstOrDefault(/*arst => arst.Pass*/) : arista;
                return arista;
            }
        }

        public TValor Valor { get => _Valor; set => _Valor = value; }
        public List<Arista> AristasFree { get => _AristasFree; }
        public Dictionary<TAristas, Arista> AristasLock { get => _AristasLock; }

        public struct Arista {
            public bool Pass;
            public Action Accion;
            public Nodo<TValor, TAristas> Nodo;

            public Arista(Nodo<TValor, TAristas> nodo, 
                Action accion = null, bool pass = true)
            {
                Pass = pass;
                Nodo = nodo;
                Accion = accion;
            }
        }
    }
}
