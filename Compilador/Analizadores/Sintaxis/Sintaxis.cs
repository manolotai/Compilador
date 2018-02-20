using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Compilador; //no es lo mejor
using Compilador.Grafo;
using Compilador.Analizadores.Lexico;

namespace Compilador.Analizadores.Sintaxis {
    public class Sintaxis : Lexico.Lexico {

        //Dictionary<> //atributos
        public enum IDSentence { Blanco, Referencia, Declaracion, Asignacion, Metodo, Fin }
        private enum Instruccion { Using, NameSpace, Class, If, For }
        
        private string _BuffTipo;
        private string _BuffValor;
        private string _BuffNombre;
        private string _BuffAccesor;
         
        private TablaAtributos _TblAtrib;
        private Dictionary<string, IDTokens> _PReservadas;
        private Dictionary<string, Func<double, double, double>> _OpFactor;
        private Dictionary<string, Func<double, double, double>> _OpTermino;
        private Dictionary<string, Func<double, double, double>> _OpPotencia;

        private List<string> _OutPut;
        private List<Token> _LogTokens;
        private List<Atributo> _LogAtributos;

        public Sintaxis(StreamReader texto) : base(texto)
        {
            ResetBuffer();
            
            _TblAtrib = new TablaAtributos();
            _LogTokens = new List<Token>();
            _LogAtributos = new List<Atributo>();
            _OutPut = new List<string>();
            _PReservadas = new Dictionary<string, IDTokens>();
            _OpFactor = new Dictionary<string, Func<double, double, double>>();
            _OpTermino = new Dictionary<string, Func<double, double, double>>();
            _OpPotencia = new Dictionary<string, Func<double, double, double>>();

            _OpTermino.Add("+", (x, y) => x + y);
            _OpTermino.Add("-", (x, y) => x - y);
            _OpFactor.Add("*", (x, y) => x * y);
            _OpFactor.Add("/", (x, y) => x / y);
            _OpFactor.Add("%", (x, y) => x % y);
            _OpPotencia.Add("^", (x, y) => Math.Pow(x, y));
            _OpPotencia.Add("^!", (x, y) => Math.Pow(x, 1 / y));
            PReservadas(typeof(Instruccion), IDTokens.Instruccion);
            PReservadas(typeof(Atributo.Accesor), IDTokens.Accesor);
            PReservadas(typeof(Atributo.TypeDato), IDTokens.TipoDato);
            
            NextTokenTrue();
        }

        private void PReservadas(Type infoEnum, IDTokens asignToken)
        {
            foreach (var item in Enum.GetNames(infoEnum)) {
                _PReservadas.Add(item, asignToken);
            }
        }

        private void NewAtrib()
        {
            var atrib = new Atributo(_BuffNombre, Double.Parse(_BuffValor),
                _BuffTipo, _BuffAccesor);
            _TblAtrib.Add(atrib);
            _LogAtributos.Add(atrib);
            ResetBuffer();
        }

        private void ResetBuffer()
        {
            _BuffValor = "0";
            _BuffNombre = "";
            _BuffTipo = "Unknown";
            _BuffAccesor = "Private";
        }

        public void AnalisisSintactico()
        {
            _OutPut.Clear();
            while (_Valor == "Using") {
                Referencia();
            }
            NameSpace();
        }

        private void Referencia()
        {
            Match(IDTokens.Instruccion);
            do {
                Match(IDTokens.Identificador);
                if (_ID == IDTokens.Punto) {
                    Match(IDTokens.Punto);
                    continue;
                }
            } while (_ID != IDTokens.FinSentencia);
            Match(IDTokens.FinSentencia);
        }
        
        private void NameSpace()
        {
            Match("NameSpace");
            do {
                Match(IDTokens.Identificador);
                if (_ID == IDTokens.Punto) {
                    Match(IDTokens.Punto);
                    continue;
                }
            } while (_ID != IDTokens.BloqueInicio);
            Match(IDTokens.BloqueInicio);
            Clase();
            Match(IDTokens.BloqueFin);
        }

        private void Clase()
        {
            if (_ID == IDTokens.Accesor)
                Match(IDTokens.Accesor);
            Match("Class");
            Match(IDTokens.Identificador);
            Match(IDTokens.BloqueInicio);
            
            do {
                if(_ID == IDTokens.Accesor) {
                    _BuffAccesor = _Valor;
                    Match(IDTokens.Accesor);
                }
                if(_ID == IDTokens.TipoDato) {
                    var auxTipo = _BuffTipo = _Valor;
                    Match(IDTokens.TipoDato);
                    _BuffNombre = _Valor;
                    Match(IDTokens.Identificador);

                    switch (_ID) {
                        case IDTokens.ParametrosInicio:
                            Metodo();
                            ResetBuffer();
                            break;

                        case IDTokens token 
                        when (token == IDTokens.Coma || token == IDTokens.OpAsignacion):
                            Definicion(auxTipo);
                            break;
                        
                        case IDTokens.FinSentencia:
                            NewAtrib();
                            break;
                    }
                } else if (_ID == IDTokens.Identificador) {
                    Match(IDTokens.Identificador);
                    Metodo();
                    ResetBuffer();
                }

            } while (_ID != IDTokens.BloqueFin);
            Match(IDTokens.BloqueFin);
        }

        private void Metodo()
        {
            Match(IDTokens.ParametrosInicio);
            _TblAtrib.NewAmbito();

            while (_ID == IDTokens.TipoDato) {
                _BuffTipo = _Valor;
                Match(IDTokens.TipoDato);
                _BuffNombre = Valor;
                Match(IDTokens.Identificador);
                if(_ID == IDTokens.OpAsignacion) {
                    Match(IDTokens.OpAsignacion);
                    _BuffValor = "" + Expresion();
                }
                if (_ID == IDTokens.Coma) {
                    Match(IDTokens.Coma);
                }
                NewAtrib();
            }
            Match(IDTokens.ParametrosFin);

            Match(IDTokens.BloqueInicio);
            while (_ID != IDTokens.BloqueFin) {
                if (_Valor == "System") {
                    WriteConsole();
                }
                switch (_ID) {
                    case IDTokens.TipoDato:
                        Declaracion();
                        break;
                    case IDTokens.FinSentencia:
                        Match(IDTokens.FinSentencia);
                        break;
                }
            }
            Match(IDTokens.BloqueFin);
            _TblAtrib.DelAmbito();
        }

        private void Declaracion()
        {
            var auxTipo = _BuffTipo = _Valor;
            Match(IDTokens.TipoDato);
            do {
                _BuffNombre = _Valor;
                Match(IDTokens.Identificador);
                if (_ID == IDTokens.OpAsignacion) {
                    Match(IDTokens.OpAsignacion);
                    _BuffValor = "" + Expresion();
                }
                if (_ID == IDTokens.Coma) {
                    Match(IDTokens.Coma);
                    NewAtrib();
                    _BuffTipo = auxTipo;
                } else break;
            } while (true); //provisional, no puede ser FinSent debido a ",;"

            NewAtrib();
            Match(IDTokens.FinSentencia);
        }

        private void Definicion(string auxTipo)
        {
            do {
                if (_ID == IDTokens.OpAsignacion) {
                    Match(IDTokens.OpAsignacion);
                    _BuffValor = "" + Expresion();
                }
                if (_ID == IDTokens.Coma) {
                    Match(IDTokens.Coma);
                    NewAtrib();
                    _BuffTipo = auxTipo;
                    _BuffNombre = _Valor;
                    Match(IDTokens.Identificador);
                } else break;
            } while (true); //provisional, no puede ser FinSent debido a ",;"

            NewAtrib();
            Match(IDTokens.FinSentencia);
        }

        private void WriteConsole()
        {
            Match("System");
            Match(IDTokens.Punto);
            Match("Console");
            Match(IDTokens.Punto);
            Match("WriteLine");
            Match(IDTokens.ParametrosInicio);
            if(_ID == IDTokens.Numero || _ID == IDTokens.Identificador) {
                _OutPut.Add("" + Expresion() + "\n");
            } else {
                _OutPut.Add(_Valor.TrimStart('\"').TrimEnd('\"') + "\n");
                Match(IDTokens.Cadena);
            } 
            Match(IDTokens.ParametrosFin);
            Match(IDTokens.FinSentencia);
        }

        //Aritmetica
        private double Expresion()
        {
            Func<double, double, double> op;
            double num = Termino();
            
            if (_OpTermino.TryGetValue(_Valor, out op)) {
                Match(IDTokens.OpTermino);
                num = op(num, Expresion());
            }
            return num;
        }

        private double Termino()
        {
            Func<double, double, double> op;
            double num = Factor();

            if (_OpFactor.TryGetValue(_Valor, out op)) {
                Match(IDTokens.OpFactor);
                num = op(num, Termino());
            }
            return num;
        }

        private double Factor()
        {
            Func<double, double, double> op;
            double num = Potencia();

            while (_OpPotencia.TryGetValue(_Valor, out op)) {
                Match(IDTokens.OpPotencia);
                num = op(num, Potencia());
            }
            return num;
        }

        private double Potencia()
        {
            try {
                double num;
                switch (_ID) {
                    case IDTokens.ParametrosInicio:
                        Match(IDTokens.ParametrosInicio);
                        num = Expresion();
                        Match(IDTokens.ParametrosFin);
                        return num;

                    case IDTokens.Identificador:
                        num = _TblAtrib[_Valor].Valor;
                        Match(IDTokens.Identificador);
                        return num;

                    case IDTokens.OpTermino:
                        string signo = _Valor;
                        Match(IDTokens.OpTermino);
                        return double.Parse(signo + Potencia());

                    case IDTokens.Numero:
                        num = double.Parse(_Valor);
                        Match(IDTokens.Numero);
                        return num;

                    default:
                        throw new InvalidDataException(String.Format("Se espera Numero, en la Linea {0}, Columna {1}",
                            _Fila, _Columna));
                }
            } catch (NullReferenceException) {
                throw new NullReferenceException(String.Format("No se encontro la referencia en la Linea {0}, Columna {1}",
                            _Fila, _Columna));
            } 
            
        }

        //Match
        private void Match(string valor)
        {
            if(valor == _Valor)
                NextTokenTrue();
            else throw new InvalidDataException(
                String.Format("Se espera '{0}', en la Linea {1}, Columna {2}",
                valor, _Fila, _Columna));
        }

        private void Match(IDTokens id)
        {
            if (id == _ID)
                NextTokenTrue();
            else throw new InvalidDataException(
                String.Format("Se espera {0}, en la Linea {1}, Columna {2}",  
                id.ToString(), _Fila, _Columna));
        }

        private void NextTokenTrue()
        {
            bool end;
            do {
                end = NextToken();
                if (_ID == IDTokens.Identificador) {
                    var temp = _ID;
                    _ID = _PReservadas.TryGetValue(_Valor, out _ID) ?   //evitar _ID = null
                        _ID : temp;
                }
                _LogTokens.Add(new Token(_ID, _Valor));
            } while (_ID == IDTokens.Comentario && end);
            
        }

        public List<string> OutPut { get => _OutPut; }
        public List<Token> LogTokens { get => _LogTokens; }
        public List<Atributo> LogAtributos { get => _LogAtributos; }

    }
}
