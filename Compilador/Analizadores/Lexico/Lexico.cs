using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Compilador.Grafo;

namespace Compilador.Analizadores.Lexico {
    public class Lexico : Token {

        static private char[] Numeros = Enumerable.Range('0', 10).Select(n => (char)n).ToArray();
        static private char[] LetrasMinus = Enumerable.Range('a', 25).Select(n => (char)n).ToArray();
        static private char[] LetrasMayus = Enumerable.Range('A', 25).Select(n => (char)n).ToArray();
        static private char[] Letras = LetrasMayus.Concat(LetrasMinus).ToArray();
        static private char[] LetrasNumeros = Letras.Concat(Numeros).ToArray();
        public enum IDTokens {
            Blanco, Identificador, Numero, OpTermino, OpFactor, OpPotencia, OpAsignacion,
            OpIncremento, OpLogico, OpComparacion, InitParametros, FinParametros, InitBloque,
            FinBloque, Cadena, Caracter, Comentario, FinSentencia, Punto, Coma,
            Booleano, TipoDato, Accesor, Instruccion, Unknown, Error
        }

        protected bool _IsRepeat;
        protected int _Fila;
        protected int _Columna;
        protected int _ActPosicion;
        protected int _PenPosicion;
        protected StreamReader _Texto;
        private Grafo<IDTokens, char> _GrafoTokens;
        private Dictionary<IDTokens, List<int>> _NodosLex;

        public Lexico(StreamReader texto)
        {
            _IsRepeat = false;
            _Fila = 1;
            _Columna = 0;
            _ActPosicion = 0;
            _PenPosicion = 0;
            _Texto = texto;
            _GrafoTokens = new Grafo<IDTokens, char>();
            _NodosLex = new Dictionary<IDTokens, List<int>>();

            StartGrafoTokens();
        }

        protected bool NextToken()
        {
            char c; _Valor = "";
            bool isToken = !_Texto.EndOfStream;
            Nodo<IDTokens, char>.Arista arista;
            var nodo = _GrafoTokens.IndiceNodos[0];

            while ((nodo = (arista = nodo[(char)_Texto.Peek()]).Nodo) != null && !_Texto.EndOfStream) {
                if ((c = (char)_Texto.Read()) == (char)10) {
                    if (!_IsRepeat) {
                        _Fila++;
                        _Columna = -1;
                    }
                }
                if (arista.Pass)
                    _Valor += c;

                if (!_IsRepeat) {
                    _Columna++;
                    _ActPosicion++;
                }
                    
                _ID = nodo.Valor;
            }

            return isToken ? true : false;
        }

        private void NewNodo(IDTokens valor, int cantidad)
        {
            List<int> listaNodos;
            bool newKey;
            listaNodos = (newKey = !_NodosLex.TryGetValue(valor, out listaNodos)) ?
                new List<int>() { 0 } : listaNodos;
            for (int i = 0; i < cantidad; i++) {
                listaNodos.Add(_GrafoTokens.Add(valor));
            }

            if (newKey)
                _NodosLex.Add(valor, listaNodos);
        }

        private void Enlazar(int origen, int destino, params char[] restriccion)
        {
            if (restriccion.Length == 0)
                _GrafoTokens.EnlazarNodos(origen, destino);
            else
                _GrafoTokens.EnlazarNodos(origen, destino, restriccion);
        }

        private void Enlazar(int origen, int destino, bool concatenar, params char[] restriccion)
        {
            if (restriccion.Length == 0)
                _GrafoTokens.EnlazarNodos(origen, destino, concatenar);
            else
                _GrafoTokens.EnlazarNodos(origen, destino, concatenar, key: restriccion);
        }

        private void StartGrafoTokens()
        {
            List<int> idx;
            _GrafoTokens.Add(IDTokens.Blanco);

            //Inicio del grafo, nodo 0
            NewNodo(IDTokens.Blanco, 0);
            idx = _NodosLex[IDTokens.Blanco];
            Enlazar(idx[0], idx[0], false, (char)9, (char)10, (char)32);

            //ErrorLexico
            NewNodo(IDTokens.Error, 3);

            //Identificador
            NewNodo(IDTokens.Identificador, 1);
            idx = _NodosLex[IDTokens.Identificador];
            Enlazar(idx[0], idx[1], Letras);
            Enlazar(idx[1], idx[1], LetrasNumeros);

            //OpTerminos
            NewNodo(IDTokens.OpTermino, 2);
            idx = _NodosLex[IDTokens.OpTermino];
            Enlazar(idx[0], idx[1], '+');
            Enlazar(idx[0], idx[2], '-');

            //Numero
            NewNodo(IDTokens.Numero, 3);
            idx = _NodosLex[IDTokens.Numero];
            Enlazar(idx[0], idx[1], Numeros);
            Enlazar(idx[1], idx[1], Numeros);
            Enlazar(idx[1], _NodosLex[IDTokens.Error][1], '.');
            Enlazar(_NodosLex[IDTokens.Error][1], idx[2], Numeros);
            Enlazar(idx[2], idx[2], Numeros);
            Enlazar(idx[1], _NodosLex[IDTokens.Error][2], 'e');
            Enlazar(idx[2], _NodosLex[IDTokens.Error][2], 'e');
            Enlazar(_NodosLex[IDTokens.Error][2], _NodosLex[IDTokens.Error][3], '+', '-');
            Enlazar(_NodosLex[IDTokens.Error][2], idx[3], Numeros);
            Enlazar(_NodosLex[IDTokens.Error][3], idx[3], Numeros);
            Enlazar(idx[3], idx[3], Numeros);

            //OpFactores
            NewNodo(IDTokens.OpFactor, 3);
            idx = _NodosLex[IDTokens.OpFactor];
            Enlazar(idx[0], idx[1], '*');
            Enlazar(idx[0], idx[2], '/');
            Enlazar(idx[0], idx[3], '%');

            //OpPotencia
            NewNodo(IDTokens.OpPotencia, 2);
            idx = _NodosLex[IDTokens.OpPotencia];
            Enlazar(idx[0], idx[1], '^');
            Enlazar(idx[1], idx[2], '!');

            //OpAsignacion
            NewNodo(IDTokens.OpAsignacion, 1);
            idx = _NodosLex[IDTokens.OpAsignacion];
            Enlazar(idx[0], idx[1], '=');

            //OpIncremento
            NewNodo(IDTokens.OpIncremento, 1);
            idx = _NodosLex[IDTokens.OpIncremento];
            Enlazar(_NodosLex[IDTokens.OpTermino][1], idx[1], '+', '=');
            Enlazar(_NodosLex[IDTokens.OpTermino][2], idx[1], '-', '=');
            Enlazar(_NodosLex[IDTokens.OpFactor][1], idx[1], '=');
            Enlazar(_NodosLex[IDTokens.OpFactor][2], idx[1], '=');

            //OpLogico
            NewNodo(IDTokens.OpLogico, 3);
            idx = _NodosLex[IDTokens.OpLogico];
            Enlazar(idx[0], idx[1], '&');
            Enlazar(idx[0], idx[2], '|');
            Enlazar(idx[0], idx[3], '!');
            Enlazar(idx[1], idx[3], '&');
            Enlazar(idx[2], idx[3], '|');

            //OpComparacion
            NewNodo(IDTokens.OpComparacion, 2);
            idx = _NodosLex[IDTokens.OpComparacion];
            Enlazar(idx[0], idx[1], '>', '<');
            Enlazar(idx[1], idx[2], '=');
            Enlazar(_NodosLex[IDTokens.OpLogico][3], idx[2], '=');
            Enlazar(_NodosLex[IDTokens.OpAsignacion][1], idx[2], '=');

            //Comentario
            NewNodo(IDTokens.Comentario, 4);
            idx = _NodosLex[IDTokens.Comentario];
            Enlazar(_NodosLex[IDTokens.OpFactor][2], idx[1], '/');
            Enlazar(idx[1], idx[1]);
            Enlazar(idx[1], idx[4], false, (char)10);
            Enlazar(_NodosLex[IDTokens.OpFactor][2], idx[2], '*');
            Enlazar(idx[2], idx[2]);
            Enlazar(idx[2], idx[3], '*');
            Enlazar(idx[3], idx[2]);
            Enlazar(idx[3], idx[3], '*');
            Enlazar(idx[3], idx[4], '/');

            //Cadena
            NewNodo(IDTokens.Cadena, 2);
            idx = _NodosLex[IDTokens.Cadena];
            Enlazar(idx[0], idx[1], '"');
            Enlazar(idx[1], idx[1]);
            Enlazar(idx[1], idx[2], '"');

            //Caracter
            NewNodo(IDTokens.Caracter, 2);
            idx = _NodosLex[IDTokens.Caracter];
            Enlazar(idx[0], idx[1], '\'');
            Enlazar(idx[1], idx[1]);
            Enlazar(idx[1], idx[2], '\'');

            //InicioParametros
            NewNodo(IDTokens.InitParametros, 1);
            idx = _NodosLex[IDTokens.InitParametros];
            Enlazar(idx[0], idx[1], '(');

            //FinParametros
            NewNodo(IDTokens.FinParametros, 1);
            idx = _NodosLex[IDTokens.FinParametros];
            Enlazar(idx[0], idx[1], ')');

            //InicioBloque
            NewNodo(IDTokens.InitBloque, 1);
            idx = _NodosLex[IDTokens.InitBloque];
            Enlazar(idx[0], idx[1], '{');

            //FinBloque
            NewNodo(IDTokens.FinBloque, 1);
            idx = _NodosLex[IDTokens.FinBloque];
            Enlazar(idx[0], idx[1], '}');

            //FinSentencia
            NewNodo(IDTokens.FinSentencia, 1);
            idx = _NodosLex[IDTokens.FinSentencia];
            Enlazar(idx[0], idx[1], ';');

            //Punto
            NewNodo(IDTokens.Punto, 1);
            idx = _NodosLex[IDTokens.Punto];
            Enlazar(idx[0], idx[1], '.');

            //Coma
            NewNodo(IDTokens.Coma, 1);
            idx = _NodosLex[IDTokens.Coma];
            Enlazar(idx[0], idx[1], ',');

            //Unknown
            NewNodo(IDTokens.Unknown, 1);
            idx = _NodosLex[IDTokens.Unknown];
            Enlazar(idx[0], idx[1]);
        }
    }
}
