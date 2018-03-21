using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Compilador.Analizadores.Sintaxis.Sintaxis;

namespace Compilador.Analizadores.Sintaxis {
    public class NodoSintaxis {
        private IDSintaxis _IDSintaxis;
        private Action _Accion;

        public NodoSintaxis(IDSintaxis idSintaxis)
        {
            _IDSintaxis = idSintaxis;
            _Accion = null;
        }

        public Action Accion { get => _Accion; set => _Accion = value; }
        public IDSintaxis IDSintaxis { get => _IDSintaxis; set => _IDSintaxis = value; }
    }
}
