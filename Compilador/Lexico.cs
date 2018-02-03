using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using MyBackPack.Grafos;

namespace Compilador {
    class Lexico {

        public enum IDTokens {
            Identificador, Numero, OpTermino, OpFactor, OpAsignacion, OpIncremento,
            OpLogico, OpComparacion, ParametrosInicio, ParametrosFin, BloqueInicio, BloqueFin,
            Cadena, Caracter, Comentario, FinSentencia
        }
        static private IEnumerable<char> Numeros = Enumerable.Range('0', 10).Select(n => (char)n);
        static private IEnumerable<char> LetrasMinus = Enumerable.Range('a', 25).Select(n => (char)n);
        static private IEnumerable<char> LetrasMayus = Enumerable.Range('A', 25).Select(n => (char)n);
        static private IEnumerable<char> Letras = LetrasMayus.Concat(LetrasMinus);
        
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

        public void StartGrafoTokens()
        {
            InfoTokenNodos idx;
            Enlazar(0, 0, false, (char)10, (char)9, (char)32);
            //Identificador
            idx = _IDNodos[IDTokens.Identificador];
            _GrafoTokens.EnlazarNodos(idx[0], idx[1], Letras);
            _GrafoTokens.EnlazarNodos(idx[1], idx[1], Letras.Concat(Numeros));
            
            //Numero
            idx = _IDNodos[IDTokens.Numero];
            _GrafoTokens.EnlazarNodos(idx[0], idx[1], Numeros);
            _GrafoTokens.EnlazarNodos(idx[1], idx[1], Numeros);
            _GrafoTokens.EnlazarNodos(idx[1], idx[2], '.');
            _GrafoTokens.EnlazarNodos(idx[1], idx[4], 'e');
            _GrafoTokens.EnlazarNodos(idx[2], idx[3], Numeros);
            _GrafoTokens.EnlazarNodos(idx[3], idx[3], Numeros);
            _GrafoTokens.EnlazarNodos(idx[3], idx[4], 'e');
            _GrafoTokens.EnlazarNodos(idx[4], idx[5], '+', '-');
            _GrafoTokens.EnlazarNodos(idx[4], idx[6], Numeros);
            _GrafoTokens.EnlazarNodos(idx[5], idx[6], Numeros);
            _GrafoTokens.EnlazarNodos(idx[6], idx[6], Numeros);
            //_GrafoTokens.EnlazarNodos(_IDNodos[IDTokens.OpTermino][1], idx[1], Numeros);//"+"
            _GrafoTokens.EnlazarNodos(_IDNodos[IDTokens.OpTermino][2], idx[1], Numeros);//"-"
            
            //OpTerminos
            idx = _IDNodos[IDTokens.OpTermino];
            _GrafoTokens.EnlazarNodos(idx[0], idx[1], '+');
            _GrafoTokens.EnlazarNodos(idx[0], idx[2], '-');
            
            //OpFactores
            idx = _IDNodos[IDTokens.OpFactor];
            _GrafoTokens.EnlazarNodos(idx[0], idx[1], '*');
            _GrafoTokens.EnlazarNodos(idx[0], idx[2], '/');
            
            //OpAsignacion
            idx = _IDNodos[IDTokens.OpAsignacion];
            _GrafoTokens.EnlazarNodos(idx[0], idx[1], '=');
            
            //OpIncremento
            idx = _IDNodos[IDTokens.OpIncremento];
            _GrafoTokens.EnlazarNodos(_IDNodos[IDTokens.OpTermino][1], idx[1], '+', '=');
            _GrafoTokens.EnlazarNodos(_IDNodos[IDTokens.OpTermino][2], idx[1], '-', '=');
            _GrafoTokens.EnlazarNodos(_IDNodos[IDTokens.OpFactor][1], idx[1], '=');
            _GrafoTokens.EnlazarNodos(_IDNodos[IDTokens.OpFactor][2], idx[1], '=');
            
            //OpLogico
            idx = _IDNodos[IDTokens.OpLogico];
            _GrafoTokens.EnlazarNodos(idx[0], idx[1], '&');
            _GrafoTokens.EnlazarNodos(idx[0], idx[2], '|');
            _GrafoTokens.EnlazarNodos(idx[0], idx[3], '!');
            _GrafoTokens.EnlazarNodos(idx[1], idx[3], '&');
            _GrafoTokens.EnlazarNodos(idx[2], idx[3], '|');

            //OpComparacion
            idx = _IDNodos[IDTokens.OpComparacion];
            _GrafoTokens.EnlazarNodos(idx[0], idx[1], '>', '<');
            _GrafoTokens.EnlazarNodos(idx[1], idx[2], '=' );
            _GrafoTokens.EnlazarNodos(_IDNodos[IDTokens.OpLogico][3], idx[2], '=');
            _GrafoTokens.EnlazarNodos(_IDNodos[IDTokens.OpAsignacion][1], idx[2], '=');

            //Comentario
            idx = _IDNodos[IDTokens.Comentario];
            _GrafoTokens.EnlazarNodos(_IDNodos[IDTokens.OpFactor][2], idx[1], '/');
            _GrafoTokens.EnlazarNodos(idx[1], idx[1]);
            _GrafoTokens.EnlazarNodos(idx[1], idx[4], false, (char)10);
            _GrafoTokens.EnlazarNodos(_IDNodos[IDTokens.OpFactor][2], idx[2], '*');
            _GrafoTokens.EnlazarNodos(idx[2], idx[2]);
            _GrafoTokens.EnlazarNodos(idx[2], idx[3], '*');
            _GrafoTokens.EnlazarNodos(idx[3], idx[2]);
            _GrafoTokens.EnlazarNodos(idx[3], idx[4], '/');

            //Cadena
            idx = _IDNodos[IDTokens.Cadena];
            _GrafoTokens.EnlazarNodos(idx[0], idx[1], '"');
            _GrafoTokens.EnlazarNodos(idx[1], idx[1]);
            _GrafoTokens.EnlazarNodos(idx[1], idx[2], '"');

            //Caracter
            idx = _IDNodos[IDTokens.Caracter];
            _GrafoTokens.EnlazarNodos(idx[0], idx[1], '\'');
            _GrafoTokens.EnlazarNodos(idx[1], idx[1]);
            _GrafoTokens.EnlazarNodos(idx[1], idx[2], '\'');

            //InicioParametros
            idx = _IDNodos[IDTokens.ParametrosInicio];
            _GrafoTokens.EnlazarNodos(idx[0], idx[1], '(');

            //FinParametros
            idx = _IDNodos[IDTokens.ParametrosFin];
            _GrafoTokens.EnlazarNodos(idx[0], idx[1], ')');

            //InicioBloque
            idx = _IDNodos[IDTokens.BloqueInicio];
            _GrafoTokens.EnlazarNodos(idx[0], idx[1], '{');

            //FinBloque
            idx = _IDNodos[IDTokens.BloqueFin];
            _GrafoTokens.EnlazarNodos(idx[0], idx[1], '}');

            //FinSentencia
            idx = _IDNodos[IDTokens.FinSentencia];
            _GrafoTokens.EnlazarNodos(idx[0], idx[1], ';');
            
        }

        public Token NextToken()
        {
            return NextToken(_GrafoTokens.IndiceNodos[0]);
        }
        private Token NextToken(Nodo<int, char> nodo)
        {
            //o tambien heredar de Token ***
            //agregar contador filas y columnas
            char c;
            int key = 0;
            string valor = "";
            Nodo<int, char>.Arista? arst; 
            while ((nodo = ( arst = nodo[c = (char)_Texto.Peek()]).Value.Nodo) != null && !_Texto.EndOfStream) {
                //valor += Char.IsWhiteSpace((char)_Texto.Read()) ?
                //IsNodo(nodo, 0, _IDNodos[IDTokens.Comentario][4]) ?
                //"" : "" + c : "" + c;
                _Texto.Read();
                if (arst.Value.Pass)
                    valor += c;
                key = nodo.Valor;
            }
            return new Token((int)_NodoToToken[key], valor);
        }

        private bool IsNodo(Nodo<int, char> nodo, params int[] estado) //buscar mejor solucion?
        {
            return estado.Contains(nodo.Valor);
        }
    }
}
