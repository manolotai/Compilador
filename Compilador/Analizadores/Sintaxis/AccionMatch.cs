using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compilador.Analizadores.Sintaxis {
    public class AccionMatch {
        private Action _Accion;
        private Action _Match;
        private bool _IsMatch;

        public AccionMatch()
        {
            _IsMatch = true;
            _Match = null;
        }

        public Action Accion { get => _Accion; set => _Accion = value; }
        public Action Match { get => _Match; set => _Match = value; }
        public bool IsMatch { get => _IsMatch; set => _IsMatch = value; }
    }
}
