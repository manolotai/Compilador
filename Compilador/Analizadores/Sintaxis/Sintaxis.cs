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

        private IDSintaxis _IDSintax;
         
        private TablaAtributos _TblAtrib;
        private Dictionary<string, IDTokens> _PReservadas;
        private Dictionary<string, Func<double, double, bool>> _OpLogico;
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
            _OpLogico = new Dictionary<string, Func<double, double, bool>>() {
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
            InitGragoSintaxis();
        }

        public void AnalizarSintaxis(int idx = 0)
        {
            while (!_Texto.EndOfStream) {
                var nodo = _GrafoSintaxis[idx]; //poner asi en _grafolexico
                Nodo<NodoSintaxis, string>.Arista? arista; 

                while ((arista = nodo.TryGetPass(_Valor) ??
                        nodo[_ID.ToString()]).Value.Nodo != null) {

                    arista?.Nodo.Valor.Accion?.Invoke();

                    if (arista.Value.Pass)
                        arista?.Accion?.Invoke();
                    nodo = arista?.Nodo;
                }
            }
        }

        private void NewNodo(IDSintaxis valor, int cantidad)
        {
            List<int> listaNodos;
            bool newKey;
            listaNodos = (newKey = !_ISint.TryGetValue(valor, out listaNodos)) ?
                new List<int>() { 0 } : listaNodos;
            for (int i = 0; i < cantidad; i++) {
                listaNodos.Add(_GrafoSintaxis.Add(new NodoSintaxis(valor)));
            }

            if (newKey)
                _ISint.Add(valor, listaNodos);
        }

        private void Enlazar(int origen, int destino,
            bool match = false, Action accion = null)
        {
            if(accion != null)
                _GrafoSintaxis[destino].Valor.Accion = accion;

            if(match)
                _GrafoSintaxis.EnlazarNodos(origen, destino, match, () => Match(_ID));
            else
                _GrafoSintaxis.EnlazarNodos(origen, destino, match);
        }

        private void Enlazar(int origen, int destino, bool? force = false,
            Action accion = null, params IDTokens[] match)
        {
            if (accion != null)
                _GrafoSintaxis[destino].Valor.Accion = accion;

            if (force == null)
                foreach (var item in match) {
                    _GrafoSintaxis.EnlazarNodos(origen, destino, false, () => Match(item), item.ToString());
                }
            else {
                if (force.Value)
                    foreach (var item in match) {
                        _GrafoSintaxis.EnlazarNodos(origen, destino, true, () => Match(item));
                    } 
                else
                    foreach (var item in match) {
                        _GrafoSintaxis.EnlazarNodos(origen, destino, true, () => Match(item), item.ToString());
                    }
            }
        }

        private void Enlazar(int origen, int destino, bool? force = false,
            Action accion = null, params string[] match)
        {
            if (accion != null)
                _GrafoSintaxis[destino].Valor.Accion = accion;

            if (force == null)
                foreach (var item in match) {
                    _GrafoSintaxis.EnlazarNodos(origen, destino, false, () => Match(item), item);
                } 
            else {
                if (force.Value)
                    foreach (var item in match) {
                        _GrafoSintaxis.EnlazarNodos(origen, destino, true, () => Match(item));
                    } 
                else
                    foreach (var item in match) {
                        _GrafoSintaxis.EnlazarNodos(origen, destino, true, () => Match(item), item);
                    }
            }
        }

        private void PReservadas(Type infoEnum, IDTokens asignToken)
        {
            foreach (var item in Enum.GetNames(infoEnum)) {
                _PReservadas.Add(item.ToLower(), asignToken);
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

        private void InitGragoSintaxis()
        {
            var accesores = new string[] { "public", "private", "protected" };
            var tipos = new string[] { "void", "char", "int", "float", "double" };
            List<int> i;
            _GrafoSintaxis.Add();
            
            
            //Incremento
            NewNodo(IDSintaxis.Incremento, 10);
            i = _ISint[IDSintaxis.Incremento];
            //Enlazar(i[1], i[2], match: IDTokens.OpIncremento, accion: () => );

            //Asignacion
            NewNodo(IDSintaxis.Asignacion, 10);
            i = _ISint[IDSintaxis.Asignacion];
            Enlazar(i[1], i[2], match: IDTokens.OpAsignacion);
            Enlazar(i[2], i[3], accion: () => _BuffValor = "" + Expresion());
            Enlazar(i[3], i[4], match: IDTokens.FinSentencia, accion: () => NewAtrib());
            
            //Declaracion
            NewNodo(IDSintaxis.Declaracion, 10);
            i = _ISint[IDSintaxis.Declaracion];
            Enlazar(i[1], i[2], true, () =>  _BuffTipo = _Valor, tipos );
            Enlazar(i[2], i[3], true, () => _BuffNombre = _Valor, IDTokens.Identificador);
            Enlazar(i[3], _ISint[IDSintaxis.Asignacion][1]);
            Enlazar(i[3], i[4], true, match: IDTokens.FinSentencia);

            //Instruccion
            NewNodo(IDSintaxis.Instruccion, 20);
            i = _ISint[IDSintaxis.Instruccion];
            Enlazar(i[1], _ISint[IDSintaxis.Asignacion][1], null, match: IDTokens.OpAsignacion);
            Enlazar(i[1], i[2], match: "Console");
            Enlazar(i[2], i[3], true, match: IDTokens.Punto);
            Enlazar(i[3], i[4], true, match: "WriteLine");
            Enlazar(i[3], i[5], match: "Write");
            Enlazar(i[3], i[6], match: "ReadLine");
            Enlazar(i[4], i[7], true, match: IDTokens.InitParametros);
            Enlazar(i[5], i[8], true, match: IDTokens.InitParametros);
            Enlazar(i[6], i[9], true, match: IDTokens.InitParametros);
            Enlazar(i[7], i[10], match: IDTokens.Cadena, accion: () => _BuffValor = _Valor);
            Enlazar(i[8], i[11], match: IDTokens.Cadena, accion: () => _BuffValor = _Valor);
            Enlazar(i[9], i[12], true, match: IDTokens.FinParametros);
            Enlazar(i[10], i[13], true, match: IDTokens.FinParametros);
            Enlazar(i[11], i[14], true, match: IDTokens.FinParametros);
            Enlazar(i[12], i[15], true, match: IDTokens.FinSentencia, accion: () => Console.ReadLine());

            Enlazar(i[13], i[16], true, match: IDTokens.FinSentencia, accion: () => { Console.WriteLine(_BuffValor); _BuffValor = ""; });
            Enlazar(i[14], i[17], true, match: IDTokens.FinSentencia, accion: () => { Console.Write(_BuffValor); _BuffValor = ""; });

            //Enlazar(i[1], _ISint[IDSintaxis.Incremento][1], match: IDTokens.OpIncremento);
            //Enlazar(i[1], _ISint[IDSintaxis.Llamada][1], match: IDTokens.Punto); //Llamada

            //If
            NewNodo(IDSintaxis.Instruccion, 10);
            i = _ISint[IDSintaxis.Instruccion];
            Enlazar(i[1], i[2], match: "if");
            Enlazar(i[2], i[3], match: IDTokens.InitParametros);

            //InitParametros
            NewNodo(IDSintaxis.InitParametros, 10);
            i = _ISint[IDSintaxis.InitParametros];
            Enlazar(i[1], i[2], true, match: IDTokens.InitParametros, accion: () => _TblAtrib.NewAmbito());
            Enlazar(i[2], i[3], true, match: IDTokens.FinParametros);
            Enlazar(i[2], i[4], null, match: IDTokens.TipoDato);
            Enlazar(i[4], i[5], true, match: IDTokens.TipoDato, accion: () => _BuffTipo = _Valor);
            Enlazar(i[5], i[6], true, match: IDTokens.Identificador, accion: () => _BuffNombre = _Valor);
            Enlazar(i[6], i[4], match: IDTokens.Coma, accion: () => NewAtrib());
            Enlazar(i[6], i[2]);

            //Metodo
            NewNodo(IDSintaxis.Metodo, 10);
            i = _ISint[IDSintaxis.Metodo];
            Enlazar(i[1], _ISint[IDSintaxis.InitParametros][1], null, match: IDTokens.InitParametros);
            Enlazar(_ISint[IDSintaxis.InitParametros][3], i[2]);
            Enlazar(i[2], i[3], true, match: IDTokens.InitBloque);
            Enlazar(i[3], i[4], true, match: IDTokens.FinBloque);

            //Clase
            NewNodo(IDSintaxis.Clase, 20);
            i = _ISint[IDSintaxis.Clase];
            Enlazar(i[1], i[2], match: accesores);
            Enlazar(i[1], i[3], match: "class");
            Enlazar(i[2], i[3], true, match: "class");
            Enlazar(i[3], i[4], true, match: IDTokens.Identificador);

            Enlazar(i[4], i[5], true, match: IDTokens.InitBloque);
            Enlazar(i[5], i[6], true, match: IDTokens.FinBloque);
            Enlazar(i[5], i[7], match: IDTokens.Accesor, accion: () => _BuffAccesor = _Valor);
            Enlazar(i[5], i[8], match: IDTokens.TipoDato, accion: () => _BuffTipo = _Valor);
            Enlazar(i[5], i[9], match: IDTokens.Identificador, accion: () => _BuffNombre = _Valor); //constructor
            Enlazar(i[6], i[1]);
            Enlazar(i[7], i[8], true, match: IDTokens.TipoDato);
            Enlazar(i[7], i[9], match: IDTokens.Identificador);
            Enlazar(i[8], i[9], true, match: IDTokens.Identificador);

            //Enlazar(i[8], _ISint[IDSintaxis.Declaracion][2], match: false); // identi puede ser optativo o no
            Enlazar(i[9], _ISint[IDSintaxis.Metodo][1], null, match: IDTokens.InitParametros);
            Enlazar(_ISint[IDSintaxis.Metodo][3], i[10], true, match: IDTokens.InitParametros);
            //Enlazar(i[9], i[10], match: IDTokens.OpAsignacion);
            //Enlazar(i[9], i[12], true, match: IDTokens.FinSentencia, accion: () => NewAtrib());
            Enlazar(i[10], i[11], accion: () => _BuffValor = "" + Expresion());//revisar aqui
            Enlazar(i[11], i[12], true, match: IDTokens.FinSentencia);
            Enlazar(i[12], i[5]);



            //Enlazar(i[5], _ISint[IDSintaxis.Metodo][1]);
            //Enlazar(_ISint[IDSintaxis.Metodo][1], i[6], true, match: IDTokens.FinBloque);

            //NameSpace
            NewNodo(IDSintaxis.NameSpace, 10);
            i = _ISint[IDSintaxis.NameSpace];
            Enlazar(i[0], i[1], match: "NameSpace");
            Enlazar(i[1], i[2], true, match: IDTokens.Identificador);
            Enlazar(i[2], i[1], match: IDTokens.Punto);
            Enlazar(i[2], i[3], true, match: IDTokens.InitBloque);
            Enlazar(i[3], _ISint[IDSintaxis.Clase][1]);
            Enlazar(_ISint[IDSintaxis.Clase][1], i[4], true, match: IDTokens.FinBloque);

            //Referencia
            NewNodo(IDSintaxis.Referencia, 4);
            i = _ISint[IDSintaxis.Referencia];
            Enlazar(i[0], i[1], match: "using");
            Enlazar(i[1], i[2], true, match: IDTokens.Identificador);
            Enlazar(i[2], i[1], match: IDTokens.Punto);
            Enlazar(i[2], i[3], true, match: IDTokens.FinSentencia);

        }

        public void AnalisisSintactico()
        {
            _OutPut.Clear();
            while (_Valor == "using") {
                Referencia();
            }
            NameSpace();
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
                if (IsAndMatch(IDTokens.Accesor, () => _BuffAccesor = _Valor)) {
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
            _BuffTipo = _Valor;
            Match(IDTokens.TipoDato);
            _BuffNombre = _Valor;
            Match(IDTokens.Identificador);

            if (_ID == IDTokens.InitParametros) {
                Metodo();
                ResetBuffer();
            } else {
                Match(IDTokens.FinSentencia);
                NewAtrib();
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
                    NewAtrib();
                    if (!IsAndMatch(IDTokens.Coma))
                        break;
                } while (true);
            }
            Match(IDTokens.FinParametros);

            Cuerpo();

            //Match(IDTokens.InitBloque);
            //while (_ID != IDTokens.FinBloque) {
            //    if (_Valor == "System") {
            //        WriteConsole();
            //    }
            //    switch (_ID) {
            //        case IDTokens.TipoDato:
            //            Declaracion();
            //            break;
            //        case IDTokens.FinSentencia:
            //            Match(IDTokens.FinSentencia);
            //            break;
            //    }
            //}
            //Match(IDTokens.FinBloque);
            //_TblAtrib.DelAmbito();
        }

        private void Declaracion()
        {
            var auxTipo = _BuffTipo = _Valor;
            Match(IDTokens.TipoDato);
            do {
                _BuffNombre = _Valor;
                Match(IDTokens.Identificador);
                if (IsAndMatch(IDTokens.OpAsignacion)) 
                    _BuffValor = "" + Expresion();

                if (IsAndMatch(IDTokens.Coma)) {
                    NewAtrib();
                    _BuffTipo = auxTipo;
                } else break;
            } while (true);

            Match(IDTokens.FinSentencia);
            NewAtrib();
        }

        private void Definicion(string auxTipo)
        {
            do {
                if (IsAndMatch(IDTokens.OpAsignacion))
                    _BuffValor = "" + Expresion();
                if (IsAndMatch(IDTokens.Coma)) {
                    NewAtrib();
                    _BuffTipo = auxTipo;
                    _BuffNombre = _Valor;
                    Match(IDTokens.Identificador);
                } else break;
            } while (true);

            Match(IDTokens.FinSentencia);
            NewAtrib();
        }

        private void WriteConsole()
        {
            bool isLine = true;
            Match("System");
            Match(IDTokens.Punto);
            Match("Console");
            Match(IDTokens.Punto);
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
            Match(IDTokens.FinParametros);
            Match(IDTokens.FinSentencia);
        }

        public void If()
        {
            if (IsAndMatch("if")) {
                Match(IDTokens.InitParametros);
                if (Logica()) {
                    Match(IDTokens.FinParametros);

                } else {
                    Match(IDTokens.FinParametros);
                    Match(IDTokens.InitBloque);
                    do {
                        if (IsAndMatch(IDTokens.FinBloque))
                            break;
                    } while (Match(_ID));


                }
            }
        }

        private void Cuerpo()
        {
            Match(IDTokens.InitBloque);
            _TblAtrib.NewAmbito();

            do {
                if (_ID == IDTokens.TipoDato) {
                    Declaracion();
                } else if (_Valor == "if") {
                    If();
                } else if (_Valor == "System") {
                    WriteConsole();
                }
                else break;
            } while (true);

            Match(IDTokens.FinBloque);
            _TblAtrib.DelAmbito();
        }

        private void Sentencia()
        {

        }

        private void Incremento(string variable)
        {//solo funcioan en ++ y --
            Func<double, double, double> incr;
            if (_OpAritm[IDTokens.OpIncremento.ToString()].TryGetValue(_Valor, out incr)) {
                _TblAtrib[variable].Valor = incr(_TblAtrib[variable].Valor, 0);
            }
            Match(IDTokens.OpIncremento);
        }

        private bool Logica()
        {//falta implementar el uso de !
            Func<double, double, bool> _Op;

            double num1 = Expresion();
            _OpLogico.TryGetValue(_Valor, out _Op);
            Match(IDTokens.OpLogico);
            return _Op(num1, Expresion());
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

                    case IDTokens.Booleano:
                        if (IsAndMatch("true"))
                            return 1;
                        else {
                            Match("false");
                            return 0;
                        }

                    default:
                        throw new InvalidDataException(String.Format("Se espera una expresion valida, en la Linea {0}, Columna {1}",
                            _Fila, _Columna));
                }
            } catch (NullReferenceException) {
                throw new NullReferenceException(String.Format("No se encontro la referencia en la Linea {0}, Columna {1}",
                            _Fila, _Columna));
            }
        }

        //Logica
        //public bool ExpresionBool()
        //{
        //    double expr = Expresion();
        //}

        //Match
        private void Match(string valor)
        {
            if(valor == _Valor)
                NextTokenTrue();
            else throw new InvalidDataException(
                String.Format("Se espera '{0}', en la Linea {1}, Columna {2}",
                valor, _Fila, _Columna));
        }

        private bool Match(IDTokens id)
        {
            if (id == _ID) {
                NextTokenTrue();
                return true;
            }
            else throw new InvalidDataException(
                String.Format("Se espera {0}, en la Linea {1}, Columna {2}",  
                id.ToString(), _Fila, _Columna));
        }

        private bool Match(IDSintaxis id)
        {
            if (id == _IDSintax) {
                NextTokenTrue();
                return true;
            }
            else throw new InvalidDataException(
                String.Format("Se espera {0}, en la Linea {1}, Columna {2}",
                id.ToString(), _Fila, _Columna));
        }

        private bool IsAndMatch(IDTokens id, Action doBefore = null)
        {
            if (_ID == id) {
                doBefore?.Invoke();
                Match(id);
                return true;
            }
            return false;
        }

        private bool IsAndMatch(string valor, Action doBefore = null)
        {
            if (_Valor == valor) {
                doBefore?.Invoke();
                Match(valor);
                return true;
            }
            return false;
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
