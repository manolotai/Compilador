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
                { "||", (x, y) => { _ASM.AritLog("||"); return x || y; } },
                { "&&", (x, y) => { _ASM.AritLog("&&"); return x && y; } }
            };
            _OpComparacion = new Dictionary<string, Func<double, double, bool>>() {
                { "<", (x, y) => { _ASM.Comp("<"); return x < y; } },
                { ">", (x, y) => { _ASM.Comp(">"); return x > y; } },
                { "==", (x, y) => { _ASM.Comp("=="); return x == y; } },
                { "<=", (x, y) => { _ASM.Comp("<="); return x <= y; } },
                { ">=", (x, y) => { _ASM.Comp(">="); return x >= y; } },
                { "!=", (x, y) => { _ASM.Comp("!="); return x != y; } }
            };
            _OpAritm = new Dictionary<string, Dictionary<string, Func<Atributo, Atributo, Atributo>>>() {
                { IDTokens.OpTermino.ToString(), new Dictionary<string, Func<Atributo, Atributo, Atributo>>() {
                    { "+", (x, y) => { _ASM.AritLog("+"); return x + y; } },
                    { "-", (x, y) => { _ASM.AritLog("-"); return x - y; } }
                } },
                { IDTokens.OpFactor.ToString(), new Dictionary<string, Func<Atributo, Atributo, Atributo>>() {
                    { "*", (x, y) => { _ASM.AritLog("*"); return x * y; } },
                    { "/", (x, y) => { _ASM.AritLog("/"); return x / y; } },
                    { "%", (x, y) => { _ASM.AritLog("%"); return x % y; } }
                } },
                { IDTokens.OpPotencia.ToString(), new Dictionary<string, Func<Atributo, Atributo, Atributo>>() {
                    //{ "^", (x, y) => Math.Pow(x, y) },
                    //{ "!^", (x, y) => Math.Pow(x, 1 / y) }
                } }
            };
        }

        //Logica
        protected bool Logica()
        {
            bool booleano = Comparacion();
            if (_OpLogico.TryGetValue(_Valor, out var logica)) {
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
                case IDTokens.NumeroFlt:
                case IDTokens.NumeroInt: //añadir + y - para negativos
                    Atributo atrib = Expresion();
                    if (!_OpComparacion.TryGetValue(_Valor, out var compara))
                        throw new InvalidDataException(String.Format("Se espera una expresion booleana valida, en la Linea {0}, Columna {1}",
                            _Fila, _Columna));
                    Match(IDTokens.OpComparacion);
                    return compara((double)atrib.Valor, (double)Expresion().Valor);

                case IDTokens.Booleano:
                    if (IsMatch("true")) {
                        _ASM.WR("push 1");
                        return true;
                    }
                    else {
                        Match("false");
                        _ASM.WR("push 0");
                        return false;
                    }
                default:
                    if (IsMatch("!")) {
                        bool booleano;
                        if (_ID == IDTokens.InitParametros)
                        {
                            booleano = !Condicion();
                            _ASM.Not();
                        }
                        else if(_ID == IDTokens.Booleano)
                        {
                            booleano = !Comparacion();
                            _ASM.Not();
                        }
                        else
                            throw new InvalidDataException(String.Format("El operador ! no se puede aplicar a {0}, en la Linea {1}, Columna {2}",
                            _ID, _Fila, _Columna));
                        return booleano;
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
            try {
                Atributo atrib;
                switch (_ID) {
                    case IDTokens.InitParametros:
                        Match(IDTokens.InitParametros);
                        if (_ID == IDTokens.TipoDato) {
                            var tipo = _Valor;
                            Match(_ID);
                            Match(IDTokens.FinParametros);
                            atrib = Factor().Cast(tipo);

                        } else {
                            atrib = Expresion();
                            Match(IDTokens.FinParametros);
                        }

                        return atrib;

                    case IDTokens.Identificador:
                        atrib = _TblAtrib[_Valor];
                        Match(IDTokens.Identificador);
                        _ASM.WR($"push {atrib.Nombre}");
                        return atrib;

                    //case IDTokens.OpTermino:
                    //    string signo = _Valor;
                    //    Match(IDTokens.OpTermino);
                    //    return double.Parse(signo + Potencia());

                    case IDTokens.NumeroInt:
                    case IDTokens.NumeroFlt: //provisional
                        _ASM.WR($"push {_Valor}");
                        return new Atributo("", _Valor, Match(_ID), "");

                    default:
                        throw new InvalidDataException(String.Format("Se espera una expresion aritmetica valida, en la Linea {0}, Columna {1}",
                            _Fila, _Columna));
                }
            } catch (NullReferenceException) {
                throw new NullReferenceException(String.Format("No se encontro la referencia en la Linea {0}, Columna {1}",
                            _Fila, _Columna));
            }
        }

        //protected Atributo Factor()
        //{
        //    Atributo atrib = Potencia();

        //    while (_ID == IDTokens.OpPotencia)
        //    {
        //        atrib = _OpAritm[_ID.ToString()][Match(_Valor)](atrib, Potencia());
        //    }
        //    return atrib;
        //}
    }
}
