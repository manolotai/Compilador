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

        public enum IDSintaxis {
            Blanco, Referencia, NameSpace, Clase, Metodo, Instruccion, Incremento, Declaracion, Asignacion, InitParametros
        }
        private enum Booleanos { True, False }
        private enum Instruccion { Using, NameSpace, Class, If, For }
        
        private string _BuffTipo;
        private string _BuffValor;
        private string _BuffNombre;
        private string _BuffAccesor;
         
        private TablaAtributos _TblAtrib;
        private Dictionary<string, IDTokens> _PReservadas;
        private Dictionary<string, Func<bool, bool, bool>> _OpLogico;
        private Dictionary<string, Func<double, double, bool>> _OpComparacion;
        private Dictionary<string, Dictionary<string, Func<double, double, double>>> _OpAritm;

        private List<string> _OutPut;
        private List<Token> _LogTokens;
        private List<Atributo> _LogAtributos;

        private Grafo<NodoSintaxis, string> _GrafoSintaxis;
        private Dictionary<IDSintaxis, List<int>> _ISint;

        public Sintaxis(StreamReader texto) : base(texto)
        {
            _GrafoSintaxis = new Grafo<NodoSintaxis, string>();
            _ISint = new Dictionary<IDSintaxis, List<int>>();

            ResetBuffer();
            
            _TblAtrib = new TablaAtributos();
            _LogTokens = new List<Token>();
            _LogAtributos = new List<Atributo>();
            _OutPut = new List<string>();
            _PReservadas = new Dictionary<string, IDTokens>();
            _OpLogico = new Dictionary<string, Func<bool, bool, bool>>() {
                { "||", (x, y) => x || y },
                { "&&", (x, y) => x && y }
            };
            _OpComparacion = new Dictionary<string, Func<double, double, bool>>() {
                { "<", (x, y) => x < 1 },
                { ">", (x, y) => x > y },
                { "==", (x, y) => x == y },
                { "<=", (x, y) => x <= y },
                { ">=", (x, y) => x >= y },
                { "!=", (x, y) => x != y }
            };
            _OpAritm = new Dictionary<string, Dictionary<string, Func<double, double, double>>>() {
                { IDTokens.OpTermino.ToString(), new Dictionary<string, Func<double, double, double>>() {
                    { "+", (x, y) => x + y },
                    { "-", (x, y) => x - y }
                } },
                { IDTokens.OpFactor.ToString(), new Dictionary<string, Func<double, double, double>>() {
                    { "*", (x, y) => x * y },
                    { "/", (x, y) => x / y },
                    { "%", (x, y) => x % y }
                } },
                { IDTokens.OpPotencia.ToString(), new Dictionary<string, Func<double, double, double>>() {
                    { "^", (x, y) => Math.Pow(x, y) },
                    { "!^", (x, y) => Math.Pow(x, 1 / y) }
                } },
                { IDTokens.OpIncremento.ToString(), new Dictionary<string, Func<double, double, double>>() {
                    { "++", (x, y) => x + 1 },
                    { "--", (x, y) => x - 1 },
                    { "+=", (x, y) => x + y },
                    { "-=", (x, y) => x - y },
                    { "*=", (x, y) => x * y },
                    { "/=", (x, y) => x / y },
                } },
                { "Cast", new Dictionary<string, Func<double, double, double>>() {
                    { "char", (x, y) => (x - x % 1) % 256 },
                    { "int", (x, y) => (x - x % 1) % 655366 },
                    { "float", (x, y) => x % 4294967296}
                } },
            };
            
            PReservadas(typeof(Booleanos), IDTokens.Booleano);
            PReservadas(typeof(Instruccion), IDTokens.Instruccion);
            PReservadas(typeof(Atributo.Accesor), IDTokens.Accesor);
            PReservadas(typeof(Atributo.TypeDato), IDTokens.TipoDato);

            NextTokenTrue();
        }    

        private void PReservadas(Type infoEnum, IDTokens asignToken)
        {
            foreach (var item in Enum.GetNames(infoEnum)) {
                _PReservadas.Add(item.ToLower(), asignToken);
            }
        }

        public void AnalisisSintactico()
        {
            _OutPut.Clear();
            while (_Valor == "using") {
                Referencia();
            }
            NameSpace();
        }

        private void NewAtrib(bool valido)
        {
            try {
                if (valido) {
                    var atrib = new Atributo(_BuffNombre, Double.Parse(_BuffValor),
                    _BuffTipo, _BuffAccesor);
                    _TblAtrib.Add(atrib);
                    _LogAtributos.Add(atrib);
                    ResetBuffer();
                }
            } catch (FormatException) {
                throw new FormatException(String.Format("{0} No tiene el formato correspondiente, en la Linea {1}, Columna {2}",
                        _BuffValor, _Fila, _Columna));
            }
        }

        private void ResetBuffer()
        {
            _BuffValor = "0";
            _BuffNombre = "";
            _BuffTipo = "Unknown";
            _BuffAccesor = "private";
        }

        private void Referencia()
        {
            Match("using");
            do {
                Match(IDTokens.Identificador);
                if (!IsAndMatch(IDTokens.Punto))
                    break;
            } while (true);
            Match(IDTokens.FinSentencia);
        }
        
        private void NameSpace()
        {
            Match("NameSpace");
            do {
                Match(IDTokens.Identificador);
                if (!IsAndMatch(IDTokens.Punto))
                    break; ;
            } while (true);
            Match(IDTokens.InitBloque);
            Clase();
            Match(IDTokens.FinBloque);
        }

        private void Clase()
        {
            IsAndMatch(IDTokens.Accesor);
            Match("Class");
            Match(IDTokens.Identificador);
            Match(IDTokens.InitBloque);
            
            do {
                if (IsAndMatch(IDTokens.Accesor)) {
                    if (IsAndMatch(IDTokens.Identificador)) {
                        Metodo();
                        ResetBuffer();
                    } else {
                        AtribOrMetodo();
                        ResetBuffer();
                    }
                } else if (_ID == IDTokens.TipoDato) {
                    AtribOrMetodo();
                    ResetBuffer();
                } else if (_ID == IDTokens.Identificador) {
                    Metodo();
                    ResetBuffer();
                } else break;
            } while (true);
            Match(IDTokens.FinBloque);
        }

        private void AtribOrMetodo()
        {
            //_BuffTipo = _Valor;
            Match(IDTokens.TipoDato);
            //_BuffNombre = _Valor;
            Match(IDTokens.Identificador);

            if (_ID == IDTokens.InitParametros) {
                Metodo();
                ResetBuffer();
            } else {
                Match(IDTokens.FinSentencia);
                NewAtrib(true);
            }
        }

        private void Metodo()
        {
            Match(IDTokens.InitParametros);
            _TblAtrib.NewAmbito();

            if(_ID == IDTokens.TipoDato) {
                do {
                    _BuffTipo = _Valor;
                    Match(IDTokens.TipoDato);
                    _BuffNombre = _Valor;
                    Match(IDTokens.Identificador);
                    if (IsAndMatch(IDTokens.OpAsignacion))
                        _BuffValor = "" + Expresion();
                    NewAtrib(true);
                    if (!IsAndMatch(IDTokens.Coma))
                        break;
                } while (true);
            }
            Match(IDTokens.FinParametros);

            Cuerpo(true);
        }

        private void Declaracion(bool valido)
        {
            var auxTipo = _BuffTipo = _Valor;
            Match(IDTokens.TipoDato);
            do {
                _BuffNombre = _Valor;
                Match(IDTokens.Identificador);
                if (IsAndMatch(IDTokens.OpAsignacion)) {
                    if (IsAndMatch("Console")) {
                        Match(IDTokens.Punto);
                        _BuffValor = ReadConsole(valido);
                    }
                    else
                        _BuffValor = "" + Expresion();
                }  

                if (IsAndMatch(IDTokens.Coma)) {
                    NewAtrib(valido);
                    _BuffTipo = auxTipo;
                } else break;
            } while (true);

            Match(IDTokens.FinSentencia);
            NewAtrib(valido);
        }

        private void Definicion(string auxTipo, bool valido)
        {
            do {
                if (IsAndMatch(IDTokens.OpAsignacion))
                    _BuffValor = "" + Expresion();
                if (IsAndMatch(IDTokens.Coma)) {
                    NewAtrib(valido);
                    _BuffTipo = auxTipo;
                    _BuffNombre = _Valor;
                    Match(IDTokens.Identificador);
                } else break;
            } while (true);

            Match(IDTokens.FinSentencia);
            NewAtrib(valido);
        }

        private void WriteConsole(bool valido)
        {
            bool isLine = true;
            if (IsAndMatch("Write"))
                isLine = false;
            else
                Match("WriteLine");
            Match(IDTokens.InitParametros);
            if(_ID == IDTokens.Numero || _ID == IDTokens.Identificador) {
                _OutPut.Add("" + Expresion() + (isLine ? "\n" : ""));
            } else {
                _OutPut.Add(_Valor.TrimStart('\"').TrimEnd('\"') + (isLine ? "\n" : ""));
                Match(IDTokens.Cadena);
            }
            if (!valido) {
                if (_OutPut.Count > 0)
                    _OutPut.RemoveAt(_OutPut.Count - 1);
            } else {
                Console.Write(_OutPut.Last());
            }
                
            Match(IDTokens.FinParametros);
            Match(IDTokens.FinSentencia);
        }

        private string ReadConsole(bool valido)
        {
            string read = "";
            Match("ReadLine");
            Match(IDTokens.InitParametros);
            Match(IDTokens.FinParametros);
            if (valido)
                read = Console.ReadLine();
            return read;
        }

        private void While()
        {
            var memory = new MemoryStream();
            var writer = new StreamWriter(memory);
            
            if (IsAndMatch("while")) {
                bool valido = Condicion();
                Cuerpo(valido);
            }
        }

        private void If(bool valido)
        {
            if (IsAndMatch("if")) {
                bool validar = Condicion();
                CuerpoOrSentencia(validar && valido);
                do {
                    if (IsAndMatch("else")) {
                        if (IsAndMatch("if")) {
                            bool validar2 = Condicion();
                            validar2 = !validar ? (validar = validar2) : false;
                            CuerpoOrSentencia(validar2 && valido);
                        } else {
                            CuerpoOrSentencia(!validar && valido);
                            break;
                        }
                    } else break;
                } while (true);
            }
        }

        private bool Sentencia(bool valido)
        {
            if (_ID == IDTokens.TipoDato) {
                Declaracion(valido);
            } else if (_Valor == "if") {
                If(valido);
            } else if (IsAndMatch("Console")) {
                Match(IDTokens.Punto);
                WriteConsole(valido);
            } else
                return false;
            return true;
        }

        private void Cuerpo(bool valido)
        {
            Match(IDTokens.InitBloque);
            _TblAtrib.NewAmbito();

            do {
                if(!Sentencia(valido))
                    break;
            } while (true);

            Match(IDTokens.FinBloque);
            _TblAtrib.DelAmbito();
        }

        private void CuerpoOrSentencia(bool valido)
        {
            if (_ID == IDTokens.InitBloque)
                Cuerpo(valido);
            else {
                if (!Sentencia(valido))
                    throw new InvalidDataException(String.Format("Expresion {0} no valida, en la Linea {1}, Columna {2}",
                        _Valor, _Fila, _Columna));
            }
        }

        private void Incremento(string variable)
        {//solo funcioan en ++ y --
            Func<double, double, double> incr;
            if (_OpAritm[IDTokens.OpIncremento.ToString()].TryGetValue(_Valor, out incr)) {
                _TblAtrib[variable].Valor = incr(_TblAtrib[variable].Valor, 0);
            }
            Match(IDTokens.OpIncremento);
        }

        //Logica
        private bool Logica()
        {
            Func<bool, bool, bool> logica;
            bool booleano = Comparacion();
            if (_OpLogico.TryGetValue(_Valor, out logica)) {
                Match(IDTokens.OpLogico);
                booleano = logica(booleano, Logica());
            }
            return booleano;
        }
        
        private bool Comparacion()
        {
            switch (_ID) {
                case IDTokens.InitParametros:
                case IDTokens.Identificador:
                case IDTokens.Numero: //añadir + y - para negativos
                    double num = Expresion();
                    Func<double, double, bool> compara;
                    if (!_OpComparacion.TryGetValue(_Valor, out compara))
                        throw new InvalidDataException(String.Format("Se espera una expresion booleana valida, en la Linea {0}, Columna {1}",
                            _Fila, _Columna));
                    Match(IDTokens.OpComparacion);
                    return compara(num, Expresion());

                case IDTokens.Booleano:
                    if (IsAndMatch("true"))
                        return true;
                    else {
                        Match("false");
                        return false;
                    }
                default:
                    if(IsAndMatch("!"))
                        return !Logica(); 
                    else
                        throw new InvalidDataException(String.Format("Se espera una expresion booleana valida, en la Linea {0}, Columna {1}",
                            _Fila, _Columna));
            }
        }

        private bool Condicion()
        {
            Match(IDTokens.InitParametros);
            bool booleano = Logica();
            Match(IDTokens.FinParametros);
            return booleano;
        }

        //Aritmetica
        private double Expresion()
        {
            Func<double, double, double> op;
            double num = Termino();
            
            if (_OpAritm[IDTokens.OpTermino.ToString()].TryGetValue(_Valor, out op)) {
                Match(IDTokens.OpTermino);
                num = op(num, Expresion());
            }
            return num;
        }

        private double Termino()
        {
            Func<double, double, double> op;
            double num = Factor();

            if (_OpAritm[IDTokens.OpFactor.ToString()].TryGetValue(_Valor, out op)) {
                Match(IDTokens.OpFactor);
                num = op(num, Termino());
            }
            return num;
        }

        private double Factor()
        {
            Func<double, double, double> op;
            double num = Potencia();

            while (_OpAritm[IDTokens.OpPotencia.ToString()].TryGetValue(_Valor, out op)) {
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
                    case IDTokens.InitParametros:
                        Match(IDTokens.InitParametros);
                        if (_ID == IDTokens.TipoDato) {
                            var tipo = _Valor;
                            Match(_ID);
                            Match(IDTokens.FinParametros);
                            num = _OpAritm["Cast"][tipo](Potencia(), 0);
                            
                        } else {
                            num = Expresion();
                            Match(IDTokens.FinParametros);
                        }
                            
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
                        throw new InvalidDataException(String.Format("Se espera una expresion valida, en la Linea {0}, Columna {1}",
                            _Fila, _Columna));
                }
            } catch (NullReferenceException) {
                throw new NullReferenceException(String.Format("No se encontro la referencia en la Linea {0}, Columna {1}",
                            _Fila, _Columna));
            }
        }

        //Match
        protected void Match(string valor)
        {
            if(valor == _Valor)
                NextTokenTrue();
            else throw new InvalidDataException(
                String.Format("Se espera '{0}', en la Linea {1}, Columna {2}",
                valor, _Fila, _Columna));
        }

        protected bool Match(IDTokens id)
        {
            if (id == _ID) {
                NextTokenTrue();
                return true;
            }
            else throw new InvalidDataException(
                String.Format("Se espera {0}, en la Linea {1}, Columna {2}",  
                id.ToString(), _Fila, _Columna));
        }

        protected bool IsAndMatch(IDTokens id, Action doBefore = null)
        {
            if (_ID == id) {
                doBefore?.Invoke();
                Match(id);
                return true;
            }
            return false;
        }

        protected bool IsAndMatch(string valor, Action doBefore = null)
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
