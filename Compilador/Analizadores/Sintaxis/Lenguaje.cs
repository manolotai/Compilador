using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Compilador.Analizadores.Lexico;

namespace Compilador.Analizadores.Sintaxis {
    public class Lenguaje : LogicaAritmetica {

        private string _BuffTipo;
        private string _BuffValor;
        private string _BuffNombre;
        private string _BuffAccesor;

        public Lenguaje(StreamReader texto) : base(texto)
        {
            ResetBuffer();
            NextTokenTrue();
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

            if (_ID == IDTokens.TipoDato) {
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
                    } else
                        _BuffValor = "" + _OpAritm["Cast"][_BuffTipo](Expresion(), 0);//cambiar esta linea
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
            if (_ID == IDTokens.Numero || _ID == IDTokens.Identificador) {
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

        private void For(bool valido)
        {
            if (IsAndMatch("For")) {
                _TblAtrib.NewAmbito();
                Declaracion(valido);
                bool ciclo = Logica();
                Match(IDTokens.FinSentencia);
                _BuffNombre = _Valor;
                Match("Console");
                Match(IDTokens.Punto);
                if (_Valor == "Write" || _Valor == "WriteLine") {
                    WriteConsole(valido);
                } else {
                    ReadConsole(valido);
                    Match(IDTokens.FinSentencia);
                }
                _TblAtrib.DelAmbito();
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
                if (_Valor == "Write" || _Valor == "WriteLine") {
                    WriteConsole(valido);
                } else {
                    ReadConsole(valido);
                    Match(IDTokens.FinSentencia);
                }
            } else
                return false;
            return true;
        }

        private void Cuerpo(bool valido)
        {
            Match(IDTokens.InitBloque);
            _TblAtrib.NewAmbito();

            do {
                if (!Sentencia(valido))
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
    }
}
