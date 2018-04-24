using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Compilador.Analizadores.Semantica;

namespace Compilador.Analizadores.Sintaxis {
    public class LogicaAritmetica : Sintaxis {
        protected Dictionary<string, Func<bool, bool, bool>> _OpLogico;
        protected Dictionary<string, Func<double, double, bool>> _OpComparacion;
        protected Dictionary<string, Dictionary<string, Func<Atributo, Atributo, Atributo>>> _OpAritm;

        public LogicaAritmetica(StreamReader texto) : base(texto)
        {
            _OpLogico = new Dictionary<string, Func<bool, bool, bool>>() {
                { "||", (x, y) => x || y },
                { "&&", (x, y) => x && y }
            };
            _OpComparacion = new Dictionary<string, Func<double, double, bool>>() {
                { "<", (x, y) => x < y },
                { ">", (x, y) => x > y },
                { "==", (x, y) => x == y },
                { "<=", (x, y) => x <= y },
                { ">=", (x, y) => x >= y },
                { "!=", (x, y) => x != y }
            };
            _OpAritm = new Dictionary<string, Dictionary<string, Func<Atributo, Atributo, Atributo>>>() {
                { IDTokens.OpTermino.ToString(), new Dictionary<string, Func<Atributo, Atributo, Atributo>>() {
                    { "+", (x, y) => x + y },
                    { "-", (x, y) => x - y }
                } },
                { IDTokens.OpFactor.ToString(), new Dictionary<string, Func<Atributo, Atributo, Atributo>>() {
                    { "*", (x, y) => x * y },
                    { "/", (x, y) => x / y },
                    { "%", (x, y) => x % y }
                } },
                { IDTokens.OpPotencia.ToString(), new Dictionary<string, Func<Atributo, Atributo, Atributo>>() {
                    //{ "^", (x, y) => Math.Pow(x, y) },
                    //{ "!^", (x, y) => Math.Pow(x, 1 / y) }
                } },
                { IDTokens.OpIncremento.ToString(), new Dictionary<string, Func<Atributo, Atributo, Atributo>>() {
                    { "++", (x, y) => { return (x + 1); } },
                    { "--", (x, y) => x - 1 },
                    { "+=", (x, y) => x + y },
                    { "-=", (x, y) => x - y },
                    { "*=", (x, y) => x * y },
                    { "/=", (x, y) => x / y },
                } },
            };
        }

        //Logica
        protected bool Logica()
        {
            Func<bool, bool, bool> logica;
            bool booleano = Comparacion();
            if (_OpLogico.TryGetValue(_Valor, out logica)) {
                Match(IDTokens.OpLogico);
                booleano = logica(booleano, Logica());
            }
            return booleano;
        }

        protected bool Comparacion()
        {
            switch (_ID) {
                case IDTokens.InitParametros:
                    return Condicion();

                case IDTokens.Identificador:
                case IDTokens.NumeroInt: //añadir + y - para negativos //resolver distincion () para logica y Expresion()
                    Atributo atrib = Expresion();
                    Func<double, double, bool> compara;
                    if (!_OpComparacion.TryGetValue(_Valor, out compara))
                        throw new InvalidDataException(String.Format("Se espera una expresion booleana valida, en la Linea {0}, Columna {1}",
                            _Fila, _Columna));
                    Match(IDTokens.OpComparacion);
                    return compara((double)atrib.Valor, (double)Expresion().Valor);

                case IDTokens.Booleano:
                    if (IsMatch("true"))
                        return true;
                    else {
                        Match("false");
                        return false;
                    }
                default:
                    if (IsMatch("!")) {
                        if (_ID == IDTokens.InitParametros)
                            return !Condicion();
                        else if(_ID == IDTokens.Booleano)
                            return !Comparacion();
                        else
                            throw new InvalidDataException(String.Format("El operador ! no se puede aplicar a {0}, en la Linea {1}, Columna {2}",
                            _ID, _Fila, _Columna));
                    }
                    else
                        throw new InvalidDataException(String.Format("Se espera una expresion booleana valida, en la Linea {0}, Columna {1}",
                            _Fila, _Columna));
            }
        }

        protected bool Condicion()
        {
            Match(IDTokens.InitParametros);
            bool booleano = Logica();
            Match(IDTokens.FinParametros);
            return booleano;
        }

        //Aritmetica
        protected Atributo Expresion()
        {
            Atributo atrib = Termino();

            if (_ID == IDTokens.OpTermino)
                atrib = _OpAritm[_ID.ToString()][Match(_Valor)](atrib, Expresion());
            return atrib;
        }

        protected Atributo Termino()
        {
            Atributo atrib = Factor();

            if(_ID == IDTokens.OpFactor)
                atrib = _OpAritm[_ID.ToString()][Match(_Valor)](atrib, Termino());
            return atrib;
        }

        protected Atributo Factor()
        {
            Atributo atrib = Potencia();

            while (_ID == IDTokens.OpPotencia)
            {
                atrib = _OpAritm[_ID.ToString()][Match(_Valor)](atrib, Potencia());
            }
            return atrib;
        }

        protected Atributo Potencia()
        {
            try {
                Atributo atrib;
                switch (_ID) {
                    case IDTokens.InitParametros:
                        Match(IDTokens.InitParametros);
                        if (_ID == IDTokens.TipoDato) {
                            var tipo = _Valor;
                            Match(_ID);
                            Match(IDTokens.FinParametros);
                            atrib = Potencia().Cast(tipo);

                        } else {
                            atrib = Expresion();
                            Match(IDTokens.FinParametros);
                        }

                        return atrib;

                    case IDTokens.Identificador:
                        atrib = _TblAtrib[_Valor];
                        Match(IDTokens.Identificador);
                        return atrib;

                    //case IDTokens.OpTermino:
                    //    string signo = _Valor;
                    //    Match(IDTokens.OpTermino);
                    //    return double.Parse(signo + Potencia());

                    case IDTokens.NumeroInt:
                    case IDTokens.NumeroFlt: //provisional
                        return new Atributo("", _Valor, Match(_ID), "");
                        //int auxInt;
                        //if(Int32.TryParse(_Valor, out auxInt)) 
                        //    atrib = new Atributo("", auxInt, Atributo.TypeDato.Int, "");
                        //else
                        //    atrib = new Atributo("", float.Parse(_Valor), Atributo.TypeDato.Float, "");

                        //IsMatch(IDTokens.NumeroInt);
                        //IsMatch(IDTokens.NumeroFlt);
                        //return atrib;

                    default:
                        //if (IsMatch("Console"))
                        //{
                        //    Match(IDTokens.Punto);

                        //} else
                            throw new InvalidDataException(String.Format("Se espera una expresion aritmetica valida, en la Linea {0}, Columna {1}",
                                _Fila, _Columna));
                        //break;
                }
            } catch (NullReferenceException) {
                throw new NullReferenceException(String.Format("No se encontro la referencia en la Linea {0}, Columna {1}",
                            _Fila, _Columna));
            }
        }
    }
}
