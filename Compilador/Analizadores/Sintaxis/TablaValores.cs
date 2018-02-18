using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compilador.Analizadores.Sintaxis {
    public class TablaAtributos {

        private TablaAtributos _NextAmbito;
        private TablaAtributos _BackAmbito;
        private TablaAtributos _ThisAmbito;
        private Dictionary<string, Atributo> _Atributos;

        public TablaAtributos()
        {
            _Atributos = new Dictionary<string, Atributo>();
            _BackAmbito = _ThisAmbito = this;
            _NextAmbito = null;
        }

        public void Add(Atributo atrib)
        {
            _ThisAmbito._Atributos.Add(atrib.Nombre, atrib);
        }

        public void NewAmbito()
        {
            var aux = _ThisAmbito;
            _ThisAmbito = _ThisAmbito._NextAmbito = new TablaAtributos();
            _ThisAmbito._BackAmbito = aux;
        }

        public void DelAmbito()
        {
            _ThisAmbito = _ThisAmbito._BackAmbito;
            _ThisAmbito.NextAmbito = null;
        }

        public Dictionary<string, Atributo> Atributos { get => _Atributos; set => _Atributos = value; }
        public TablaAtributos NextAmbito { get => _NextAmbito; set => _NextAmbito = value; }

        public Atributo this[string idx] {
            get {
                Atributo atrib = null;
                TablaAtributos thisTbl = this;
                do {
                    if (thisTbl._Atributos.TryGetValue(idx, out atrib))
                        return atrib;
                } while ((thisTbl = thisTbl._NextAmbito) != null);
                return atrib;
            }

            set { this[idx] = value; }
        }
    }
}
