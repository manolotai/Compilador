using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compilador.Analizadores.Sintaxis {
    public class LogicaAritmetica : Sintaxis {
        protected Dictionary<string, Func<bool, bool, bool>> _OpLogico;
        protected Dictionary<string, Func<double, double, bool>> _OpComparacion;
        protected Dictionary<string, Dictionary<string, Func<double, double, double>>> _OpAritm;
        
        public LogicaAritmetica(StreamReader texto) : base(texto)
        {
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
                    if (IsAndMatch("!"))
                        return !Logica();
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
        protected double Expresion()
        {
            Func<double, double, double> op;
            double num = Termino();

            if (_OpAritm[IDTokens.OpTermino.ToString()].TryGetValue(_Valor, out op)) {
                Match(IDTokens.OpTermino);
                num = op(num, Expresion());
            }
            return num;
        }

        protected double Termino()
        {
            Func<double, double, double> op;
            double num = Factor();

            if (_OpAritm[IDTokens.OpFactor.ToString()].TryGetValue(_Valor, out op)) {
                Match(IDTokens.OpFactor);
                num = op(num, Termino());
            }
            return num;
        }

        protected double Factor()
        {
            Func<double, double, double> op;
            double num = Potencia();

            while (_OpAritm[IDTokens.OpPotencia.ToString()].TryGetValue(_Valor, out op)) {
                Match(IDTokens.OpPotencia);
                num = op(num, Potencia());
            }
            return num;
        }

        protected double Potencia()
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
    }
}
