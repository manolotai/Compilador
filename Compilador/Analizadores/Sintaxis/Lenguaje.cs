using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Compilador.Analizadores.Lexico;
using Compilador.Analizadores.Semantica;
using Compilador.Generador;

namespace Compilador.Analizadores.Sintaxis {
    public class Lenguaje : LogicaAritmetica {

        private string _BuffTipo;
        private Atributo _BuffValor;
        private string _BuffNombre;
        private string _BuffAccesor;

        public Lenguaje(StreamReader texto) : base(texto)
        {
            ResetBuffer();
            NextTokenTrue();
        }
        
        public void Compilar(string outPath)
        {
            using (var streamWr = new StreamWriter(outPath)) {
                _TextoASM = new Ensamblador(streamWr);

                _OutPut.Clear();
                while (_Valor == "using") {
                    Referencia();
                }
                NameSpace();
            }
        }

        private void NewAtrib(bool valido)
        {
            try {
                if (valido) {
                    var atrib = new Atributo(_BuffNombre, _BuffValor,
                            _BuffTipo, _BuffAccesor);
                    _TblAtrib.Add(atrib);
                    _LogAtributos.Add(atrib);
                    ResetBuffer();
                }
            } catch (InvalidDataException) {
                throw new InvalidDataException(String.Format("No se puede asignar {0} a {1}, en la Linea {2}, Columna {3}",
                        _BuffValor.TipoDato, _BuffTipo, _Fila, _Columna));
            } 
        }

        private void ResetBuffer()
        {
            _BuffValor = new Atributo("", "", Atributo.TypeDato.Char, "");
            _BuffNombre = "";
            _BuffTipo = "Unknown";
            _BuffAccesor = "private";
        }

        private void Referencia()
        {
            Match("using");
            do {
                Match(IDTokens.Identificador);
                if (!IsMatch(IDTokens.Punto))
                    break;
            } while (true);
            Match(IDTokens.FinSentencia);
        }

        private void NameSpace()
        {
            Match("NameSpace");
            do {
                Match(IDTokens.Identificador);
                if (!IsMatch(IDTokens.Punto))
                    break; ;
            } while (true);
            Match(IDTokens.InitBloque);
            Clase();
            Match(IDTokens.FinBloque);
        }

        private void Clase()
        {
            IsMatch(IDTokens.Accesor);
            Match("Class");
            Match(IDTokens.Identificador);
            Match(IDTokens.InitBloque);

            do {
                if (IsMatch(IDTokens.Accesor)) {
                    if (IsMatch(IDTokens.Identificador)) {
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
                    if (IsMatch(IDTokens.OpAsignacion))
                        _BuffValor = Expresion();
                    NewAtrib(true);
                    if (!IsMatch(IDTokens.Coma))
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
                if (IsMatch(IDTokens.OpAsignacion)) {
                    if (IsMatch("Console")) {
                        Match(IDTokens.Punto);
                        int auxInt;
                        string aux = ReadConsole(valido);
                        if (Int32.TryParse(aux, out auxInt))
                            _BuffValor = new Atributo("", aux, Atributo.TypeDato.Int, "");
                        else
                            _BuffValor = new Atributo("", aux, Atributo.TypeDato.Float, "");
                    } else
                        _BuffValor = Expresion();
                }

                if (IsMatch(IDTokens.Coma)) {
                    NewAtrib(valido);
                    _BuffTipo = auxTipo;
                } else break;
            } while (true);

            NewAtrib(valido);
        }

        private void Definicion(string auxTipo, bool valido)
        {
            do {
                if (IsMatch(IDTokens.OpAsignacion))
                    _BuffValor = Expresion();
                if (IsMatch(IDTokens.Coma)) {
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
            if (IsMatch("Write"))
                isLine = false;
            else
                Match("WriteLine");
            Match(IDTokens.InitParametros);
            if (_ID == IDTokens.NumeroInt || _ID == IDTokens.Identificador) {
                _OutPut.Add("" + Expresion().Valor + (isLine ? "\n" : ""));
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

        //private void For(bool valido)
        //{
        //    if (IsMatch("for")) {
        //        int posCond;
        //        Func<Atributo, Atributo, Atributo> incr = null;
        //        _TblAtrib.NewAmbito();

        //        Match(IDTokens.InitParametros);
        //        Declaracion(valido);
        //        posCond = _ActPosicion;
        //        Match(IDTokens.FinSentencia);

        //        bool ciclo = Logica();
        //        Match(IDTokens.FinSentencia);
        //        string vIncr = _Valor;
        //        Match(IDTokens.Identificador);
        //        incr = Incremento(vIncr, true);
        //        Match(IDTokens.FinParametros);

        //        do {
        //            CuerpoOrSentencia(ciclo && valido);

        //            if (!_IsRepeat) {
        //                _IsRepeat = true;
        //            }

        //            if (ciclo) {
        //                _Texto.DiscardBufferedData();
        //                _Texto.BaseStream.Position = posCond;

        //                NextTokenTrue();
        //                _TblAtrib[vIncr].Valor = incr(_TblAtrib[vIncr], null).Valor;
        //                ciclo = Logica();
        //                Match(IDTokens.FinSentencia);
        //                Match(IDTokens.Identificador);
        //                Incremento(vIncr, true);
        //                Match(IDTokens.FinParametros);
        //            }
                    
        //        } while (ciclo);
        //        _Texto.DiscardBufferedData();
        //        _Texto.BaseStream.Position = _PenPosicion;
        //        NextTokenTrue();
        //        _IsRepeat = false;
        //        _TblAtrib.DelAmbito();

        //    }
        //}

        private void If(bool valido)
        {
            if (IsMatch("if")) {
                bool validar = Condicion();
                CuerpoOrSentencia(validar && valido);
                do {
                    if (IsMatch("else")) {
                        if (IsMatch("if")) {
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
                Match(IDTokens.FinSentencia);
            } else if (_Valor == "if") {
                If(valido);
            } else if (_Valor == "for") {
                //For(valido);
            } else if (IsMatch("Console")) {
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
                _TblAtrib.NewAmbito();
                if (!Sentencia(valido))
                    throw new InvalidDataException(String.Format("Expresion {0} no valida, en la Linea {1}, Columna {2}",
                        _Valor, _Fila, _Columna));
                _TblAtrib.DelAmbito();
            }
        }

        private Func<Atributo, Atributo, Atributo> Incremento(string variable, bool valido)
        {//solo funcioan en ++ y --
            Func<Atributo, Atributo, Atributo> incr = null;
            if (valido) {
                if (_OpAritm[IDTokens.OpIncremento.ToString()].TryGetValue(_Valor, out incr)) {
                    //_TblAtrib[variable].Valor = incr(_TblAtrib[variable], null).Valor;//Revisar despues uso practico o eliminacion
                }
            }
            Match(IDTokens.OpIncremento);
            return incr;
        }
    }
}
