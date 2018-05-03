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
    public partial class Lenguaje : LogicaAritmetica {

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
                _ASM = new Ensamblador(streamWr);

                _OutPut.Clear();
                while (_Valor == "using") {
                    Referencia();
                }
                NameSpace();
                
                _ASM.EndASM();
            }
            string tmp1 = Path.GetTempFileName();
            using (var streamRd = new StreamReader(outPath)) {
                using (var streamWr = new StreamWriter(tmp1))
                {
                    streamWr.WriteLine($";Fecha de compilacion: {DateTime.Now.ToString()}");
                    streamWr.WriteLine($";Angel Emmanuel Ruiz Alcaraz");
                    for (int i = 0; i < 6; i++) {
                        streamWr.WriteLine(streamRd.ReadLine());
                    }
                    streamWr.WriteLine();
                    streamWr.WriteLine(".data");
                    foreach (var atrib in _LogAtributos){
                        switch (atrib.TipoDato) {
                            case Atributo.TypeDato.Char: streamWr.WriteLine($"{atrib.Nombre} db ?"); break;
                            case Atributo.TypeDato.Int: streamWr.WriteLine($"{atrib.Nombre} dw ?"); break;
                            case Atributo.TypeDato.Float: streamWr.WriteLine($"{atrib.Nombre} dd ?"); break;
                        }
                    }
                    streamWr.WriteLine($"{_ASM.InHand} dd ?");
                    streamWr.WriteLine($"{_ASM.OutHand} dd ?");
                    streamWr.WriteLine($"{_ASM.InBuff} db \"0\"");
                    streamWr.WriteLine($"{_ASM.OutBuff} db \"0\"");
                    streamWr.WriteLine($"{_ASM.KeyBuff} db \"0\"");
                    streamWr.WriteLine($"{_ASM.BytesWr} dd ?");
                    streamWr.WriteLine($"NewL db 13,10");
                    streamWr.WriteLine(streamRd.ReadToEnd());
                }
            }
            File.Copy(tmp1, outPath, true);
        }

        private void NewAtrib()
        {
            try {
                var atrib = new Atributo(_BuffNombre, _BuffValor,
                           _BuffTipo, _BuffAccesor);
                _TblAtrib.Add(atrib);
                _LogAtributos.Add(atrib);
                ResetBuffer();
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
            Match(IDTokens.TipoDato);
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

            if (_ID == IDTokens.TipoDato) {
                do {
                    _BuffTipo = _Valor;
                    Match(IDTokens.TipoDato);
                    _BuffNombre = _Valor;
                    Match(IDTokens.Identificador);
                    if (IsMatch(IDTokens.OpAsignacion))
                        _BuffValor = Expresion();
                    NewAtrib();
                    if (!IsMatch(IDTokens.Coma))
                        break;
                } while (true);
            }
            Match(IDTokens.FinParametros);

            Cuerpo();
        }
    }
}
