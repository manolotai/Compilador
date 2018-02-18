using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Compilador.Grafo;
using Compilador.Analizadores.Lexico;

namespace Compilador.Analizadores.Sintaxis {
    public class Sintaxis : Lexico.Lexico {

        //Dictionary<> //atributos
        public enum IDSentence { Blanco, Referencia, Declaracion, Asignacion, Metodo, Fin }
        private enum Instruccion { If, For }
        
        private string _BuffTipo;
        private string _BuffValor;
        private string _BuffNombre;
        private string _BuffAccesor;

        private Dictionary<string, IDTokens> _PReservadas;
        private Grafo<Action, string> _GrafoGram;
        private Dictionary<string, Func<double, double, double>> _OpFactor;
        private Dictionary<string, Func<double, double, double>> _OpTermino;
        private Dictionary<IDSentence, List<int>> _NodosSint;
        private TablaAtributos _TblAtrib;

        public Sintaxis(StreamReader texto) : base(texto)
        {
            _BuffValor = "0";
            _BuffNombre = "";
            _BuffTipo = "Unknown";
            _BuffAccesor = "Private";

            _TblAtrib = new TablaAtributos();
            _GrafoGram = new Grafo<Action, string>();
            _NodosSint = new Dictionary<IDSentence, List<int>>();
            _OpFactor = new Dictionary<string, Func<double, double, double>>();
            _OpTermino = new Dictionary<string, Func<double, double, double>>();
            _PReservadas = new Dictionary<string, IDTokens>();

            PReservadas(typeof(Instruccion), IDTokens.Instruccion);
            PReservadas(typeof(Atributo.Accesor), IDTokens.Accesor);
            PReservadas(typeof(Atributo.TypeDato), IDTokens.TipoDato);


            _OpTermino.Add("+", (x, y) => x + y);
            _OpTermino.Add("-", (x, y) => x - y);
            _OpFactor.Add("*", (x, y) => x * y);
            _OpFactor.Add("/", (x, y) => x / y);
            _OpFactor.Add("%", (x, y) => x % y);
            //InitGrafoGram();
            
            NextTokenTrue();
            Metodo();
        }

        private void NewAtrib()
        {
            _TblAtrib.Add(new Atributo(_BuffNombre, Double.Parse(_BuffValor), 
                _BuffTipo, _BuffAccesor));
            _BuffValor = "0";
            _BuffNombre = "";
            _BuffTipo = "Unknown";
            _BuffAccesor = "Private";
        }

        private void PReservadas(Type infoEnum, IDTokens asignToken){
            foreach (var item in Enum.GetNames(infoEnum)) {
                _PReservadas.Add(item, asignToken);
            }
        }

        private void Definicion()
        {
            Match(IDTokens.Identificador);
            if (_Valor == ",") {
                Match(",");
                Definicion();
            }
        }

        private void Metodo()
        {
            Match(IDTokens.TipoDato);
            Match(IDTokens.Identificador);
            Match(IDTokens.ParametrosInicio);
            _TblAtrib.NewAmbito();

            while (_ID == IDTokens.TipoDato) {
                _BuffTipo = _Valor;
                Match(IDTokens.TipoDato);
                _BuffNombre = Valor;
                Match(IDTokens.Identificador);
                if(_ID == IDTokens.OpAsignacion) {
                    Match(IDTokens.OpAsignacion);
                    _BuffValor = "" + Expresion();
                }
                if (_Valor == ",") {
                    Match(",");
                }
                NewAtrib();
            }

            Match(IDTokens.ParametrosFin);
            Match(IDTokens.BloqueInicio);
            while (_ID != IDTokens.BloqueFin) {
                switch (_ID) {
                    case IDTokens.TipoDato:
                        Declaracion();
                        break;
                    case IDTokens.Instruccion:
                        
                        break;
                }
            }
            Match(IDTokens.BloqueFin);
        }

        private void Declaracion()
        {
            _BuffTipo = _Valor;
            Match(IDTokens.TipoDato);
            _BuffNombre = _Valor;
            Match(IDTokens.Identificador);
            if(_ID == IDTokens.OpAsignacion) {
                Match(IDTokens.OpAsignacion);
                _BuffValor = "" + Expresion();
            }
            NewAtrib();
            Match(IDTokens.FinSentencia);
        }

        private void Parametros() // irrelevante?
        {
            string valor = "0";
            string nombre = "";
            string tipo = _Valor;

            Match(IDTokens.TipoDato);
            nombre = _Valor;
            Match(IDTokens.Identificador);
            if(_ID == IDTokens.OpAsignacion) {
                Match(IDTokens.OpAsignacion);

            }
            if(_Valor == ",") {

            }

        }

        public double Expresion()
        {
            Func<double, double, double> op;
            double num = Termino();
            
            if (_OpTermino.TryGetValue(_Valor, out op)) {
                Match(IDTokens.OpTermino);
                num = op(num, Expresion());
                if (_ID == IDTokens.ParametrosFin)  //se movio esto
                    Match(IDTokens.ParametrosFin);
            }
            return num;
        }

        private double Termino()
        {
            Func<double, double, double> op;
            double num = Factor();

            if (_OpFactor.TryGetValue(_Valor, out op)) {
                Match(IDTokens.OpFactor);
                num = op(num, Termino());
            }
            return num;
        }

        private double Factor()
        {
            double num;
            switch (_ID) {
                case IDTokens.ParametrosInicio:
                    Match(IDTokens.ParametrosInicio);
                    return Expresion();
                case IDTokens.Identificador:
                    num = _TblAtrib[_Valor].Valor;
                    Match(IDTokens.Identificador);
                    return num;
                default:
                    num = double.Parse(_Valor);
                    Match(IDTokens.Numero);
                    return num;
            }
        }

        private void Match(string valor)
        {
            if(valor == _Valor)
                NextTokenTrue();
            else
                Console.WriteLine("se espera " + valor);
        }

        private void Match(IDTokens id)
        {
            if (id == _ID)
                NextTokenTrue();
            else
                Console.WriteLine("se espera " + id);
        }

        private void NextTokenTrue()
        {
            do {
                NextToken();
            } while (_ID == IDTokens.Comentario);
            if (_ID == IDTokens.Identificador) {
                var temp = _ID;
                _ID = _PReservadas.TryGetValue(_Valor, out _ID) ?   //evitar _ID = null
                    _ID : temp;
            }
        }

        //private void Enlazar(int origen, int destino, params string[] restriccion)
        //{
        //    if (restriccion.Length == 0)
        //        _GrafoGram.EnlazarNodos(origen, destino);
        //    else
        //        _GrafoGram.EnlazarNodos(origen, destino, restriccion);
        //}

        //private void Enlazar(int origen, int destino, bool concatenar, params string[] restriccion)
        //{
        //    if (restriccion.Length == 0)
        //        _GrafoGram.EnlazarNodos(origen, destino, concatenar);
        //    else
        //        _GrafoGram.EnlazarNodos(origen, destino, concatenar, restriccion);
        //}

        //private void NewNodo(IDSentence categoria, int cantidad)
        //{
        //    List<int> listaNodos;
        //    bool newKey;
        //    listaNodos = (newKey = !_NodosSint.TryGetValue(categoria, out listaNodos)) ? 
        //        new List<int>() { 0 } : listaNodos;
        //    for (int i = 0; i < cantidad; i++) {
        //        listaNodos.Add(_GrafoGram.Add(null));
        //    }

        //    if (newKey)
        //        _NodosSint.Add(categoria, listaNodos);
        //}

        //public void NextSentence()
        //{
        //    Nodo<IDTokens, char>.Arista arista;
        //    var nodo = _GrafoGram.IndiceNodos[0];

        //    //Se debe usar match simultaneamente **
        //    while ((nodo = nodo[_Valor].Nodo ?? nodo[_ID.ToString()].Nodo) != null && !_Texto.EndOfStream) {
        //        nodo.Valor?.Invoke();
        //        Match(_Valor);
        //    }

        //}

        //public void InitGrafoGram()
        //{
        //    List<int> idx;

        //    string tipoDato = "Double";
        //    double valor = 0;
        //    string accesor = "Private";
        //    string nombre = "";
        //    //_GrafoGram.Add(() => { return; }); //Ejemplo

        //    _GrafoGram.Add(null);

        //    //Fin
        //    NewNodo(IDSentence.Fin, 1);
        //    idx = _NodosSint[IDSentence.Fin];

        //    //Metodo
        //    NewNodo(IDSentence.Metodo, 10);
        //    idx =_NodosSint[IDSentence.Metodo];
        //    Enlazar(idx[0], idx[1], Enum.GetNames(typeof(Atributo.Accesor)));
        //    //Enlazar(idx[]);



        //    //Referencias using
        //    NewNodo(IDSentence.Referencia, 4);
        //    idx = _NodosSint[IDSentence.Referencia];
        //    Enlazar(idx[0], idx[1], "using");
        //    Enlazar(idx[1], idx[2], IDTokens.Identificador.ToString());
        //    Enlazar(idx[2], idx[3], ";");
        //    Enlazar(idx[2], idx[4], ".");
        //    Enlazar(idx[4], idx[2], IDTokens.Identificador.ToString());

        //    //Asignacion // recursivo
        //    NewNodo(IDSentence.Asignacion, 10);
        //    idx = _NodosSint[IDSentence.Asignacion];
        //    Enlazar(idx[1], idx[2], "=");
        //    Enlazar(idx[2], idx[3], IDTokens.Numero.ToString(), IDTokens.Identificador.ToString());
        //    _GrafoGram[idx[3]].Valor = () => _TblAtrib[nombre].Valor = Expresion();
        //    //Enlazar(idx[3], );

        //    //Declaracion
        //    NewNodo(IDSentence.Declaracion, 10);
        //    idx = _NodosSint[IDSentence.Declaracion];
        //    Enlazar(idx[0], idx[1], Enum.GetNames(typeof(Atributo.TypeDato)));
        //    _GrafoGram[idx[1]].Valor = () => tipoDato = _Valor;

        //    Enlazar(idx[1], idx[2], IDTokens.Identificador.ToString());
        //    _GrafoGram[idx[2]].Valor = () => {
        //        nombre = _Valor;
        //        _TblAtrib.Add(new Atributo(nombre, valor, tipoDato, accesor));
        //        return;
        //    };
        //    Enlazar(idx[2], _NodosSint[IDSentence.Asignacion][2], "=");
        //    Enlazar(idx[2], idx[3], ";");
        //}
        public int this[int x] { get => 3; }
        public int this[string x] { get => 4; }
    }
}
