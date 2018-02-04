using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Compilador.Grafo;

namespace Compilador.Analizadores.Lexico {
    class Lexico {

        public enum IDTokens {
            Identificador, Numero, OpTermino, OpFactor, OpAsignacion, OpIncremento,
            OpLogico, OpComparacion, ParametrosInicio, ParametrosFin, BloqueInicio, BloqueFin,
            Cadena, Caracter, Comentario, FinSentencia
        }
        static private char[] Numeros = Enumerable.Range('0', 10).Select(n => (char)n).ToArray();
        static private char[] LetrasMinus = Enumerable.Range('a', 25).Select(n => (char)n).ToArray();
        static private char[] LetrasMayus = Enumerable.Range('A', 25).Select(n => (char)n).ToArray();
        static private char[] Letras = LetrasMayus.Concat(LetrasMinus).ToArray();
        static private char[] LetrasNumeros = Letras.Concat(Numeros).ToArray();

        private IntStream _iDStream;
        private StreamReader _Texto;
        private Grafo<int, char> _GrafoTokens;
        private Dictionary<int, IDTokens> _NodoToToken;
        private Dictionary<IDTokens, InfoTokenNodos> _IDNodos;
        public Lexico(StreamReader texto)
        {
            _Texto = texto;
            _iDStream = new IntStream(1);
            _GrafoTokens = new Grafo<int, char>();
            _IDNodos = new Dictionary<IDTokens, InfoTokenNodos>();
            _NodoToToken = new Dictionary<int, IDTokens>();

            var listaIDNodos = new List<Tuple<IDTokens, int>>() {
                Tuple.Create(IDTokens.Identificador, 2),
                Tuple.Create(IDTokens.Numero, 6),
                Tuple.Create(IDTokens.OpTermino, 2),
                Tuple.Create(IDTokens.OpFactor, 2),
                Tuple.Create(IDTokens.OpLogico, 3),
                Tuple.Create(IDTokens.OpComparacion, 2),
                Tuple.Create(IDTokens.OpIncremento, 1),
                Tuple.Create(IDTokens.OpAsignacion, 1),
                Tuple.Create(IDTokens.ParametrosInicio, 1),
                Tuple.Create(IDTokens.ParametrosFin, 1),
                Tuple.Create(IDTokens.BloqueInicio, 1),
                Tuple.Create(IDTokens.BloqueFin, 1),
                Tuple.Create(IDTokens.FinSentencia, 1),
                Tuple.Create(IDTokens.Comentario, 4),
                Tuple.Create(IDTokens.Cadena, 2),
                Tuple.Create(IDTokens.Caracter, 2)
            };

            listaIDNodos.ForEach(
                p => _IDNodos.Add(p.Item1, new InfoTokenNodos(_iDStream.Read(p.Item2))));
            
            //diccionario nodo - token
            foreach (var item in _IDNodos) {
                item.Value.Nodos.ForEach(p => {
                    if (!_NodoToToken.Keys.Contains(p))
                        _NodoToToken.Add(p, item.Key);
                    return;
                });
            }

            StartGrafoTokens();
        }
        private void Enlazar(int origen, int destino, params char[] restriccion)
        {
            _GrafoTokens.EnlazarNodos(origen, destino, restriccion);
        }

        private void Enlazar(int origen, int destino, bool concatenar, params char[] restriccion)
        {
            _GrafoTokens.EnlazarNodos(origen, destino, concatenar, restriccion);
        }

        public Token NextToken()
        {
            //o tambien heredar de Token ***
            //agregar contador filas y columnas
            char c; int estado = 0; string cadena = "";
            Nodo<int, char>.Arista arista; var nodo = _GrafoTokens.IndiceNodos[0];

            while ((nodo = (arista = nodo[(char)_Texto.Peek()]).Nodo) != null && !_Texto.EndOfStream) {
                c = (char)_Texto.Read();
                estado = nodo.Valor;
                if (arista.Pass)
                    cadena += c;
            }
            return new Token((int)_NodoToToken[estado], cadena);
        }

        public void StartGrafoTokens()
        {
            InfoTokenNodos idx;
            Enlazar(0, 0, false, (char)9, (char)10, (char)32);
            //Identificador
            idx = _IDNodos[IDTokens.Identificador];
            Enlazar(idx[0], idx[1], Letras);
            Enlazar(idx[1], idx[1], LetrasNumeros);

            //Numero
            idx = _IDNodos[IDTokens.Numero];
            Enlazar(idx[0], idx[1], Numeros);
            Enlazar(idx[1], idx[1], Numeros);
            Enlazar(idx[1], idx[2], '.');
            Enlazar(idx[1], idx[4], 'e');
            Enlazar(idx[2], idx[3], Numeros);
            Enlazar(idx[3], idx[3], Numeros);
            Enlazar(idx[3], idx[4], 'e');
            Enlazar(idx[4], idx[5], '+', '-');
            Enlazar(idx[4], idx[6], Numeros);
            Enlazar(idx[5], idx[6], Numeros);
            Enlazar(idx[6], idx[6], Numeros);
            //Enlazar(_IDNodos[IDTokens.OpTermino][1], idx[1], Numeros);//"+"
            Enlazar(_IDNodos[IDTokens.OpTermino][2], idx[1], Numeros);//"-"

            //OpTerminos
            idx = _IDNodos[IDTokens.OpTermino];
            Enlazar(idx[0], idx[1], '+');
            Enlazar(idx[0], idx[2], '-');

            //OpFactores
            idx = _IDNodos[IDTokens.OpFactor];
            Enlazar(idx[0], idx[1], '*');
            Enlazar(idx[0], idx[2], '/');

            //OpAsignacion
            idx = _IDNodos[IDTokens.OpAsignacion];
            Enlazar(idx[0], idx[1], '=');

            //OpIncremento
            idx = _IDNodos[IDTokens.OpIncremento];
            Enlazar(_IDNodos[IDTokens.OpTermino][1], idx[1], '+', '=');
            Enlazar(_IDNodos[IDTokens.OpTermino][2], idx[1], '-', '=');
            Enlazar(_IDNodos[IDTokens.OpFactor][1], idx[1], '=');
            Enlazar(_IDNodos[IDTokens.OpFactor][2], idx[1], '=');

            //OpLogico
            idx = _IDNodos[IDTokens.OpLogico];
            Enlazar(idx[0], idx[1], '&');
            Enlazar(idx[0], idx[2], '|');
            Enlazar(idx[0], idx[3], '!');
            Enlazar(idx[1], idx[3], '&');
            Enlazar(idx[2], idx[3], '|');

            //OpComparacion
            idx = _IDNodos[IDTokens.OpComparacion];
            Enlazar(idx[0], idx[1], '>', '<');
            Enlazar(idx[1], idx[2], '=');
            Enlazar(_IDNodos[IDTokens.OpLogico][3], idx[2], '=');
            Enlazar(_IDNodos[IDTokens.OpAsignacion][1], idx[2], '=');

            //Comentario
            idx = _IDNodos[IDTokens.Comentario];
            Enlazar(_IDNodos[IDTokens.OpFactor][2], idx[1], '/');
            Enlazar(idx[1], idx[1]);
            Enlazar(idx[1], idx[4], false, (char)10);
            Enlazar(_IDNodos[IDTokens.OpFactor][2], idx[2], '*');
            Enlazar(idx[2], idx[2]);
            Enlazar(idx[2], idx[3], '*');
            Enlazar(idx[3], idx[2]);
            Enlazar(idx[3], idx[4], '/');

            //Cadena
            idx = _IDNodos[IDTokens.Cadena];
            Enlazar(idx[0], idx[1], '"');
            Enlazar(idx[1], idx[1]);
            Enlazar(idx[1], idx[2], '"');

            //Caracter
            idx = _IDNodos[IDTokens.Caracter];
            Enlazar(idx[0], idx[1], '\'');
            Enlazar(idx[1], idx[1]);
            Enlazar(idx[1], idx[2], '\'');

            //InicioParametros
            idx = _IDNodos[IDTokens.ParametrosInicio];
            Enlazar(idx[0], idx[1], '(');

            //FinParametros
            idx = _IDNodos[IDTokens.ParametrosFin];
            Enlazar(idx[0], idx[1], ')');

            //InicioBloque
            idx = _IDNodos[IDTokens.BloqueInicio];
            Enlazar(idx[0], idx[1], '{');

            //FinBloque
            idx = _IDNodos[IDTokens.BloqueFin];
            Enlazar(idx[0], idx[1], '}');

            //FinSentencia
            idx = _IDNodos[IDTokens.FinSentencia];
            Enlazar(idx[0], idx[1], ';');

        }
    }
}
