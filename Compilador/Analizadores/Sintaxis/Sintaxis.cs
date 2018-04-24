using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Compilador; //no es lo mejor
using Compilador.Grafo;
using Compilador.Analizadores.Lexico;
using Compilador.Analizadores.Semantica;
using Compilador.Generador;

namespace Compilador.Analizadores.Sintaxis {
    public class Sintaxis : Lexico.Lexico {
        
        protected enum Booleanos { True, False }

        protected List<string> _OutPut;
        protected List<Token> _LogTokens;
        protected List<Atributo> _LogAtributos;

        protected Ensamblador _TextoASM;
        protected TablaAtributos _TblAtrib;
        protected Dictionary<string, IDTokens> _PReservadas;

        public Sintaxis(StreamReader texto) : base(texto)
        {
            _OutPut = new List<string>();
            _TblAtrib = new TablaAtributos();
            _LogTokens = new List<Token>();
            _LogAtributos = new List<Atributo>();
            _PReservadas = new Dictionary<string, IDTokens>();

            PReservadas(typeof(Booleanos), IDTokens.Booleano);
            PReservadas(typeof(Atributo.Accesor), IDTokens.Accesor);
            PReservadas(typeof(Atributo.TypeReturn), IDTokens.TipoDato);
        }

        private void PReservadas(Type infoEnum, IDTokens asignToken)
        {
            foreach (var item in Enum.GetNames(infoEnum)) {
                _PReservadas.Add(item.ToLower(), asignToken);
            }
        }

        //Match
        protected string Match(string valor)
        {
            if(valor == _Valor) {
                NextTokenTrue();
                return valor;
            }
            else throw new InvalidDataException(
                String.Format("Se espera '{0}', en la Linea {1}, Columna {2}",
                valor, _Fila, _Columna));
        }

        protected IDTokens Match(IDTokens id)
        {
            if (id == _ID) {
                NextTokenTrue();
                return id;
            }
            else throw new InvalidDataException(
                String.Format("Se espera {0}, en la Linea {1}, Columna {2}",  
                id.ToString(), _Fila, _Columna));
        }

        protected bool IsMatch(IDTokens id, Action doBefore = null)
        {
            if (_ID == id) {
                doBefore?.Invoke();
                Match(id);
                return true;
            }
            return false;
        }

        protected bool IsMatch(string valor, Action doBefore = null)
        {
            if (_Valor == valor) {
                doBefore?.Invoke();
                Match(valor);
                return true;
            }
            return false;
        }

        protected void NextTokenTrue()
        {
            bool noEnd;
            do {
                if (!_IsRepeat) {
                    _PenPosicion = _ActPosicion;
                }
                noEnd = NextToken();
                if (_ID == IDTokens.Identificador) {
                    var temp = _ID;
                    _ID = _PReservadas.TryGetValue(_Valor, out _ID) ?   //evitar _ID = null
                        _ID : temp;
                }
                _LogTokens.Add(new Token(_ID, _Valor));
                if (!noEnd)
                    break;
            } while (_ID == IDTokens.Comentario);
            
        }

        public List<string> OutPut { get => _OutPut; }
        public List<Token> LogTokens { get => _LogTokens; }
        public List<Atributo> LogAtributos { get => _LogAtributos; }

    }
}
