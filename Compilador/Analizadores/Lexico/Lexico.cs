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
            Blanco, Identificador, Numero, OpTermino, OpFactor, OpAsignacion, OpIncremento,
            OpLogico, OpComparacion, ParametrosInicio, ParametrosFin, BloqueInicio, BloqueFin,
            Cadena, Caracter, Comentario, FinSentencia, Error
        }

        private int _Fila;
        private int _Columna;
        private StreamReader _Texto;
        private Grafo<IDTokens, char> _GrafoTokens;
        private Dictionary<IDTokens, List<int>> _DictNodos;

        public Lexico(StreamReader texto)
        {
            _Fila = 1;
            _Columna = 0;
            _Texto = texto;
            _GrafoTokens = new Grafo<IDTokens, char>();
            _DictNodos = new Dictionary<IDTokens, List<int>>();

            StartGrafoTokens();
        }

        public bool NextToken()
        {
            //o tambien heredar de Token ***
            char c; _Valor = "";
            bool isToken = !_Texto.EndOfStream;
            Nodo<IDTokens, char>.Arista arista;
            var nodo = _GrafoTokens.IndiceNodos[0];

            while ((nodo = (arista = nodo[(char)_Texto.Peek()]).Nodo) != null && !_Texto.EndOfStream) {
                if ((c = (char)_Texto.Read()) == (char)10) {
                    _Fila++;
                    _Columna = 0;
                }
                if (arista.Pass)
                    _Valor += c;
                _Columna++;
                _ID = nodo.Valor;
            }
            if (_ID == IDTokens.Comentario)
                NextToken();

            return isToken ? true : false;
        }

        private void NewNodo(IDTokens valor, int cantidad)
        {
            List<int> listaNodos;
            bool newKey;
            listaNodos = (newKey = !_DictNodos.TryGetValue(valor, out listaNodos)) ?
                new List<int>() { 0 } : listaNodos;
            for (int i = 0; i < cantidad; i++) {
                listaNodos.Add(_GrafoTokens.Add(valor));
            }

            if (newKey)
                _DictNodos.Add(valor, listaNodos);
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
                _GrafoTokens.EnlazarNodos(origen, destino, concatenar, restriccion);
        }

        public void StartGrafoTokens()
        {
            List<int> idx;
            _GrafoTokens.Add(IDTokens.Blanco);

            //Inicio del grafo, nodo 0
            NewNodo(IDTokens.Blanco, 0);
            idx = _DictNodos[IDTokens.Blanco];
            Enlazar(idx[0], idx[0], false, (char)9, (char)10, (char)32);

            //ErrorLexico
            NewNodo(IDTokens.Error, 3);

            //Identificador
            NewNodo(IDTokens.Identificador, 1);
            idx = _DictNodos[IDTokens.Identificador];
            Enlazar(idx[0], idx[1], Letras);
            Enlazar(idx[1], idx[1], LetrasNumeros);

            //OpTerminos
            NewNodo(IDTokens.OpTermino, 2);
            idx = _DictNodos[IDTokens.OpTermino];
            Enlazar(idx[0], idx[1], '+');
            Enlazar(idx[0], idx[2], '-');

            //Numero
            NewNodo(IDTokens.Numero, 3);
            idx = _DictNodos[IDTokens.Numero];
            Enlazar(idx[0], idx[1], Numeros);
            Enlazar(idx[1], idx[1], Numeros);
            Enlazar(idx[1], _DictNodos[IDTokens.Error][1], '.');
            Enlazar(_DictNodos[IDTokens.Error][1], idx[2], Numeros);
            Enlazar(idx[2], idx[2], Numeros);
            Enlazar(idx[2], _DictNodos[IDTokens.Error][2], 'e');
            Enlazar(_DictNodos[IDTokens.Error][2], _DictNodos[IDTokens.Error][3], '+', '-');
            Enlazar(_DictNodos[IDTokens.Error][2], idx[3], Numeros);
            Enlazar(_DictNodos[IDTokens.Error][3], idx[3], Numeros);
            Enlazar(idx[3], idx[3], Numeros);
            //Enlazar(_DictNodos[IDTokens.OpTermino][1], idx[1], Numeros);//"+"
            //Enlazar(_DictNodos[IDTokens.OpTermino][2], idx[1], Numeros);//"-"

            //OpFactores
            NewNodo(IDTokens.OpFactor, 2);
            idx = _DictNodos[IDTokens.OpFactor];
            Enlazar(idx[0], idx[1], '*');
            Enlazar(idx[0], idx[2], '/');

            //OpAsignacion
            NewNodo(IDTokens.OpAsignacion, 1);
            idx = _DictNodos[IDTokens.OpAsignacion];
            Enlazar(idx[0], idx[1], '=');

            //OpIncremento
            NewNodo(IDTokens.OpIncremento, 1);
            idx = _DictNodos[IDTokens.OpIncremento];
            Enlazar(_DictNodos[IDTokens.OpTermino][1], idx[1], '+', '=');
            Enlazar(_DictNodos[IDTokens.OpTermino][2], idx[1], '-', '=');
            Enlazar(_DictNodos[IDTokens.OpFactor][1], idx[1], '=');
            Enlazar(_DictNodos[IDTokens.OpFactor][2], idx[1], '=');

            //OpLogico
            NewNodo(IDTokens.OpLogico, 3);
            idx = _DictNodos[IDTokens.OpLogico];
            Enlazar(idx[0], idx[1], '&');
            Enlazar(idx[0], idx[2], '|');
            Enlazar(idx[0], idx[3], '!');
            Enlazar(idx[1], idx[3], '&');
            Enlazar(idx[2], idx[3], '|');

            //OpComparacion
            NewNodo(IDTokens.OpComparacion, 2);
            idx = _DictNodos[IDTokens.OpComparacion];
            Enlazar(idx[0], idx[1], '>', '<');
            Enlazar(idx[1], idx[2], '=');
            Enlazar(_DictNodos[IDTokens.OpLogico][3], idx[2], '=');
            Enlazar(_DictNodos[IDTokens.OpAsignacion][1], idx[2], '=');

            //Comentario
            NewNodo(IDTokens.Comentario, 4);
            idx = _DictNodos[IDTokens.Comentario];
            Enlazar(_DictNodos[IDTokens.OpFactor][2], idx[1], '/');
            Enlazar(idx[1], idx[1]);
            Enlazar(idx[1], idx[4], false, (char)10);
            Enlazar(_DictNodos[IDTokens.OpFactor][2], idx[2], '*');
            Enlazar(idx[2], idx[2]);
            Enlazar(idx[2], idx[3], '*');
            Enlazar(idx[3], idx[2]);
            Enlazar(idx[3], idx[4], '/');

            //Cadena
            NewNodo(IDTokens.Cadena, 2);
            idx = _DictNodos[IDTokens.Cadena];
            Enlazar(idx[0], idx[1], '"');
            Enlazar(idx[1], idx[1]);
            Enlazar(idx[1], idx[2], '"');

            //Caracter
            NewNodo(IDTokens.Caracter, 2);
            idx = _DictNodos[IDTokens.Caracter];
            Enlazar(idx[0], idx[1], '\'');
            Enlazar(idx[1], idx[1]);
            Enlazar(idx[1], idx[2], '\'');

            //InicioParametros
            NewNodo(IDTokens.ParametrosInicio, 1);
            idx = _DictNodos[IDTokens.ParametrosInicio];
            Enlazar(idx[0], idx[1], '(');

            //FinParametros
            NewNodo(IDTokens.ParametrosFin, 1);
            idx = _DictNodos[IDTokens.ParametrosFin];
            Enlazar(idx[0], idx[1], ')');

            //InicioBloque
            NewNodo(IDTokens.BloqueInicio, 1);
            idx = _DictNodos[IDTokens.BloqueInicio];
            Enlazar(idx[0], idx[1], '{');

            //FinBloque
            NewNodo(IDTokens.BloqueFin, 1);
            idx = _DictNodos[IDTokens.BloqueFin];
            Enlazar(idx[0], idx[1], '}');

            //FinSentencia
            NewNodo(IDTokens.FinSentencia, 1);
            idx = _DictNodos[IDTokens.FinSentencia];
            Enlazar(idx[0], idx[1], ';');

        }
    }
}
