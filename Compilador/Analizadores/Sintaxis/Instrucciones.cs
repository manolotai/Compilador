using Compilador.Analizadores.Semantica;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compilador.Analizadores.Sintaxis
{
    public partial class Lenguaje
    {
        private void Declaracion()
        {
            var auxTipo = _BuffTipo = _Valor;
            Match(IDTokens.TipoDato);
            do {
                _BuffNombre = _Valor;
                Match(IDTokens.Identificador);
                if (IsMatch(IDTokens.OpAsignacion)) {
                    if (IsMatch("Console")) {
                        Match(IDTokens.Punto);
                        ReadConsole();
                    } else {
                        _BuffValor = Expresion();
                        _ASM.WR("pop {0}\n", _BuffNombre);
                    }
                }

                if (IsMatch(IDTokens.Coma)) {
                    NewAtrib();
                    _BuffTipo = auxTipo;
                } else break;
            } while (true);

            NewAtrib();
        }

        private void Definicion(Atributo atrib)
        {
            Match(IDTokens.OpAsignacion);
            Expresion();
            _ASM.WR("pop {0}", atrib.Nombre);
            Match(IDTokens.FinSentencia);
        }

        private void WriteConsole()
        {
            bool isLine = true;
            if (IsMatch("Write"))
                isLine = false;
            else
                Match("WriteLine");
            Match(IDTokens.InitParametros);
            if (_ID == IDTokens.Identificador) {
                var atrib = _TblAtrib[Match(_Valor)];
                _ASM.WR($"INVOKE dwtoa, {atrib.Nombre}, ADDR {_ASM.OutBuff}");
                _ASM.WR($"lea ebx, {_ASM.OutBuff}");
                _ASM.WR($"INVOKE WriteConsoleA, {_ASM.OutHand}, ebx, {("" + (double)atrib.Valor).Length}, offset {_ASM.BytesWr}, 0");
                if (isLine) {
                    _ASM.WR("lea ebx, NewL");
                    _ASM.WR($"INVOKE WriteConsoleA, {_ASM.OutHand}, ebx, 2, offset {_ASM.BytesWr}, 0");
                }

            } else if (_ID == IDTokens.Cadena) {
                _OutPut.Add(_Valor.TrimStart('\"').TrimEnd('\"') + (isLine ? "\n" : ""));
                Match(IDTokens.Cadena);
            }

            Match(IDTokens.FinParametros);
            Match(IDTokens.FinSentencia);
        }

        private void ReadConsole()
        {
            bool isLine = true;
            if (IsMatch("ReadKey"))
                isLine = false;
            else
                Match("ReadLine");

            Match(IDTokens.InitParametros);
            Match(IDTokens.FinParametros);
            Match(IDTokens.FinSentencia);
            if (isLine) {
                _ASM.WR($"INVOKE ReadConsoleA, {_ASM.InHand}, offset {_ASM.InBuff}, 10, offset {_ASM.BytesWr}, 0");
            } else {
                _ASM.WR($"INVOKE SetConsoleMode, {_ASM.InHand}, not 2");
                _ASM.WR($"INVOKE ReadConsoleA, {_ASM.InHand}, offset {_ASM.KeyBuff}, 1, offset {_ASM.BytesWr}, 0");
                _ASM.WR($"INVOKE SetConsoleMode, {_ASM.InHand}, not 6");
            }
                
        }

        private bool Sentencia()
        {
            if (_ID == IDTokens.TipoDato) {
                Declaracion();
                Match(IDTokens.FinSentencia);
            } else if (_Valor == "if") {
                If();
            } else if (_Valor == "for") {
                For();
            } else if (IsMatch("Console")) {
                Match(IDTokens.Punto);
                if (_Valor == "Write" || _Valor == "WriteLine") {
                    WriteConsole();
                } else {
                    ReadConsole();
                }
            } else {
                if (_ID == IDTokens.Identificador) {
                    var atrib = _TblAtrib[Match(_Valor)];
                    if (_ID == IDTokens.OpIncremento) {
                        Incremento(atrib);
                        Match(IDTokens.FinSentencia);
                    } else if (_ID == IDTokens.OpAsignacion)
                        Definicion(atrib);
                    else
                        return false;
                } else
                    return false;
            }
            return true;
        }

        private void Cuerpo()
        {
            Match(IDTokens.InitBloque);
            _TblAtrib.NewAmbito();

            do {
                if (!Sentencia())
                    break;
            } while (true);

            Match(IDTokens.FinBloque);
            _TblAtrib.DelAmbito();
        }

        private void CuerpoOrSentencia()
        {
            if (_ID == IDTokens.InitBloque)
                Cuerpo();
            else {
                _TblAtrib.NewAmbito();
                if (!Sentencia())
                    throw new InvalidDataException(String.Format("Expresion {0} no valida, en la Linea {1}, Columna {2}",
                        _Valor, _Fila, _Columna));
                _TblAtrib.DelAmbito();
            }
        }

        private void Incremento(Atributo atrib)
        {
            switch (Match(_Valor)) {
                case "++": _ASM.Add(atrib.Nombre, "1"); break;
                case "--": _ASM.Sub(atrib.Nombre, "1"); break;
                case "+=": _ASM.Add(atrib.Nombre, $"{(double)Expresion().Valor}"); break;
                case "-=": _ASM.Sub(atrib.Nombre, $"{(double)Expresion().Valor}"); break;
                case "*=":
                    _ASM.WR($"mov eax, {atrib.Nombre}");
                    _ASM.WR($"mul {(double)Expresion().Valor}");
                    _ASM.WR($"mov {atrib.Nombre}, eax");
                    break;
                case "/=":
                    _ASM.WR($"mov eax, {atrib.Nombre}");
                    _ASM.WR($"div {(double)Expresion().Valor}");
                    _ASM.WR($"mov {atrib.Nombre}, eax");
                    break;
                default:
                    throw new InvalidDataException("Operador de incremento no valido");
            }
        }
    }
}
