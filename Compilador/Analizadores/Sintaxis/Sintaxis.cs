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
            Blanco, Referencia, NameSpace, Clase, Metodo, Instruccion, Incremento, Declaracion, Asignacion
        }
        private enum Instruccion { Using, NameSpace, Class, If, For }
        
        private string _BuffTipo;
        private string _BuffValor;
        private string _BuffNombre;
        private string _BuffAccesor;

        private IDSintaxis _IDSintax;
         
        private TablaAtributos _TblAtrib;
        private Dictionary<string, IDTokens> _PReservadas;
        private Dictionary<string, Func<double, double, double>> _OpFactor;
        private Dictionary<string, Func<double, double, double>> _OpTermino;
        private Dictionary<string, Func<double, double, double>> _OpPotencia;

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
                    //if (arista.Pass)
                    //    arista.Accion?.Invoke();

                    //nodo.Valor.Accion?.Invoke();
                    //if (nodo.Valor.IsMatch)
                    //    if(nodo.Valor.Match == null)
                    //        Match(_ID);
                    //    else
                    //        nodo.Valor.Match();
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

            //if (accion != null)
            //    _GrafoSintaxis[destino].Valor.Accion = accion;

            //if (match != null)
            //    _GrafoSintaxis[destino].Valor.IsMatch = match.Value;
            //_GrafoSintaxis.EnlazarNodos(origen, destino);
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

            //if(accion != null)
            //    _GrafoSintaxis[destino].Valor.Accion = accion;

            //if (free) {
            //    _GrafoSintaxis.EnlazarNodos(origen, destino);
            //    foreach (var item in match)
            //        _GrafoSintaxis[destino].Valor.Match = () => Match(item);
            //} else 
            //    _GrafoSintaxis.EnlazarNodos(origen, destino, match.Select(p => p.ToString()));
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

            //if (accion != null)
            //    _GrafoSintaxis[destino].Valor.Accion = accion;

            //if (force) {
            //    _GrafoSintaxis.EnlazarNodos(origen, destino);
            //    foreach (var item in match)
            //        _GrafoSintaxis[destino].Valor.Match = () => Match(item);
            //} else
            //    _GrafoSintaxis.EnlazarNodos(origen, destino, match);
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

            //Metodo
            NewNodo(IDSintaxis.Metodo, 10);
            i = _ISint[IDSintaxis.Metodo];
            Enlazar(i[1], i[2], match: accesores, accion: () => _BuffAccesor = _Valor);
            Enlazar(i[1], i[3], match: tipos, accion: () => _BuffTipo = _Valor);
            Enlazar(i[1], i[4], match: IDTokens.Identificador, accion: () => _BuffNombre = _Valor);

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
            Enlazar(i[9], i[10], match: IDTokens.OpAsignacion);
            Enlazar(i[9], i[12], true, match: IDTokens.FinSentencia, accion: () => NewAtrib());
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

        private void NextSent()
        {

        }

        private void Incremento()
        {
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
            } while (_ID != IDTokens.InitBloque);
            Match(IDTokens.InitBloque);
            Clase();
            Match(IDTokens.FinBloque);
        }

        private void Clase()
        {
            if (_ID == IDTokens.Accesor)
                Match(IDTokens.Accesor);
            Match("Class");
            Match(IDTokens.Identificador);
            Match(IDTokens.InitBloque);
            
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
                        case IDTokens.InitParametros:
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

            } while (_ID != IDTokens.FinBloque);
            Match(IDTokens.FinBloque);
        }

        private void Metodo()
        {
            Match(IDTokens.InitParametros);
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
            Match(IDTokens.FinParametros);

            Match(IDTokens.InitBloque);
            while (_ID != IDTokens.FinBloque) {
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
                    //default:
                    //    throw new InvalidDataException(String.Format("No reconocido, en la Linea {0}, Columna {1}",
                    //        _Fila, _Columna));
                }
            }
            Match(IDTokens.FinBloque);
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
            Match(IDTokens.InitParametros);
            if(_ID == IDTokens.Numero || _ID == IDTokens.Identificador) {
                _OutPut.Add("" + Expresion() + "\n");
            } else {
                _OutPut.Add(_Valor.TrimStart('\"').TrimEnd('\"') + "\n");
                Match(IDTokens.Cadena);
            } 
            Match(IDTokens.FinParametros);
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
                    case IDTokens.InitParametros:
                        Match(IDTokens.InitParametros);
                        if (_ID == IDTokens.TipoDato) {
                            var tipo = _Valor;
                            Match(_ID);
                            Match(IDTokens.FinParametros);
                            num = Cast(tipo, Expresion());
                            
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

        public double Cast(string tipo, double valor)
        {
            if(tipo == "char") {
                return (valor - valor % 1) % 256;
            } else if (tipo == "int") {
                return (valor - valor % 1) % 655366;
            } else if (tipo == "float") {
                return valor % 4294967296;
            } else {
                throw new InvalidDataException(String.Format("Cast no reconocido, en la Linea {0}, Columna {1}",
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

        private void Match(IDTokens id)
        {
            if (id == _ID)
                NextTokenTrue();
            else throw new InvalidDataException(
                String.Format("Se espera {0}, en la Linea {1}, Columna {2}",  
                id.ToString(), _Fila, _Columna));
        }

        private void Match(IDSintaxis id)
        {
            if (id == _IDSintax)
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
