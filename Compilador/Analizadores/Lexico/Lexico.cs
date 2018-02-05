using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Compilador.Grafo;

namespace Compilador.Analizadores.Lexico {
    public class Lexico {

        public enum IDTokens {
            Blanco, Identificador, Numero, OpTermino, OpFactor, OpAsignacion, OpIncremento,
            OpLogico, OpComparacion, ParametrosInicio, ParametrosFin, BloqueInicio, BloqueFin,
            Cadena, Caracter, Comentario, FinSentencia
        }
        static private char[] Numeros = Enumerable.Range('0', 10).Select(n => (char)n).ToArray();
        static private char[] LetrasMinus = Enumerable.Range('a', 25).Select(n => (char)n).ToArray();
        static private char[] LetrasMayus = Enumerable.Range('A', 25).Select(n => (char)n).ToArray();
        static private char[] Letras = LetrasMayus.Concat(LetrasMinus).ToArray();
        static private char[] LetrasNumeros = Letras.Concat(Numeros).ToArray();
        
        private StreamReader _Texto;
        private Grafo<IDTokens, char> _GrafoTokens;
        private Dictionary<IDTokens, List<int>> _DictNodos;
        public Lexico(StreamReader texto)
        {
            _Texto = texto;
            _GrafoTokens = new Grafo<IDTokens, char>();
            _DictNodos = new Dictionary<IDTokens, List<int>>();

            StartGrafoTokens();
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

        private void Enlazar(int origen, int destino, bool concatenar = true)
        {
            _GrafoTokens.EnlazarNodos(origen, destino, concatenar);
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
            char c; IDTokens estado = 0; string cadena = "";
            Nodo<IDTokens, char>.Arista arista; var nodo = _GrafoTokens.IndiceNodos[0];

            while ((nodo = (arista = nodo[(char)_Texto.Peek()]).Nodo) != null && !_Texto.EndOfStream) {
                c = (char)_Texto.Read();
                estado = nodo.Valor;
                if (arista.Pass)
                    cadena += c;
            }
            return new Token(estado, cadena);
        }

        private void StartGrafoTokens()
        {
            InitGrafo();
            EnlacesGrafo();
        }

        private void InitGrafo()
        {
            _GrafoTokens.Add(IDTokens.Blanco);
            NewNodo(IDTokens.Identificador, 1);
            NewNodo(IDTokens.Numero, 6);
            NewNodo(IDTokens.OpTermino, 2);
            NewNodo(IDTokens.OpFactor, 2);
            NewNodo(IDTokens.OpLogico, 3);
            NewNodo(IDTokens.OpComparacion, 2);
            NewNodo(IDTokens.OpIncremento, 1);
            NewNodo(IDTokens.OpAsignacion, 1);
            NewNodo(IDTokens.ParametrosInicio, 1);
            NewNodo(IDTokens.ParametrosFin, 1);
            NewNodo(IDTokens.BloqueInicio, 1);
            NewNodo(IDTokens.BloqueFin, 1);
            NewNodo(IDTokens.FinSentencia, 1);
            NewNodo(IDTokens.Comentario, 4);
            NewNodo(IDTokens.Cadena, 2);
            NewNodo(IDTokens.Caracter, 2);
        }

        public void EnlacesGrafo()
        {
            List<int> idx;
            Enlazar(0, 0, false, (char)9, (char)10, (char)32);
            //Identificador
            idx = _DictNodos[IDTokens.Identificador];
            Enlazar(idx[0], idx[1], Letras);
            Enlazar(idx[1], idx[1], LetrasNumeros);

            //Numero
            idx = _DictNodos[IDTokens.Numero];
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
            //Enlazar(_DictNodos[IDTokens.OpTermino][1], idx[1], Numeros);//"+"
            Enlazar(_DictNodos[IDTokens.OpTermino][2], idx[1], Numeros);//"-"

            //OpTerminos
            idx = _DictNodos[IDTokens.OpTermino];
            Enlazar(idx[0], idx[1], '+');
            Enlazar(idx[0], idx[2], '-');

            //OpFactores
            idx = _DictNodos[IDTokens.OpFactor];
            Enlazar(idx[0], idx[1], '*');
            Enlazar(idx[0], idx[2], '/');

            //OpAsignacion
            idx = _DictNodos[IDTokens.OpAsignacion];
            Enlazar(idx[0], idx[1], '=');

            //OpIncremento
            idx = _DictNodos[IDTokens.OpIncremento];
            Enlazar(_DictNodos[IDTokens.OpTermino][1], idx[1], '+', '=');
            Enlazar(_DictNodos[IDTokens.OpTermino][2], idx[1], '-', '=');
            Enlazar(_DictNodos[IDTokens.OpFactor][1], idx[1], '=');
            Enlazar(_DictNodos[IDTokens.OpFactor][2], idx[1], '=');

            //OpLogico
            idx = _DictNodos[IDTokens.OpLogico];
            Enlazar(idx[0], idx[1], '&');
            Enlazar(idx[0], idx[2], '|');
            Enlazar(idx[0], idx[3], '!');
            Enlazar(idx[1], idx[3], '&');
            Enlazar(idx[2], idx[3], '|');

            //OpComparacion
            idx = _DictNodos[IDTokens.OpComparacion];
            Enlazar(idx[0], idx[1], '>', '<');
            Enlazar(idx[1], idx[2], '=');
            Enlazar(_DictNodos[IDTokens.OpLogico][3], idx[2], '=');
            Enlazar(_DictNodos[IDTokens.OpAsignacion][1], idx[2], '=');

            //Comentario
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
            idx = _DictNodos[IDTokens.Cadena];
            Enlazar(idx[0], idx[1], '"');
            Enlazar(idx[1], idx[1]);
            Enlazar(idx[1], idx[2], '"');

            //Caracter
            idx = _DictNodos[IDTokens.Caracter];
            Enlazar(idx[0], idx[1], '\'');
            Enlazar(idx[1], idx[1]);
            Enlazar(idx[1], idx[2], '\'');

            //InicioParametros
            idx = _DictNodos[IDTokens.ParametrosInicio];
            Enlazar(idx[0], idx[1], '(');

            //FinParametros
            idx = _DictNodos[IDTokens.ParametrosFin];
            Enlazar(idx[0], idx[1], ')');

            //InicioBloque
            idx = _DictNodos[IDTokens.BloqueInicio];
            Enlazar(idx[0], idx[1], '{');

            //FinBloque
            idx = _DictNodos[IDTokens.BloqueFin];
            Enlazar(idx[0], idx[1], '}');

            //FinSentencia
            idx = _DictNodos[IDTokens.FinSentencia];
            Enlazar(idx[0], idx[1], ';');

        }
    }
}
