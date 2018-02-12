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

        public Grafo<Action, string> _GrafoGram;
        public Stack<double> _StckEvalua;
        public Dictionary<string, Func<double, double, double>> _OpTermino;
        public Dictionary<string, Func<double, double, double>> _OpFactor;

        public Sintaxis(StreamReader texto) : base(texto)
        {
            _StckEvalua = new Stack<double>();
            _GrafoGram = new Grafo<Action, string>();
            _OpTermino = new Dictionary<string, Func<double, double, double>>();
            _OpFactor = new Dictionary<string, Func<double, double, double>>();
            _OpTermino.Add("+", (x, y) => x + y);
            _OpTermino.Add("-", (x, y) => x - y);
            _OpFactor.Add("*", (x, y) => x * y);
            _OpFactor.Add("/", (x, y) => x / y);
            _OpFactor.Add("%", (x, y) => x % y);
            NextToken();
        }

        public void InitGrafoGram()
        {
            _GrafoGram.Add(() => { return; });
        }

        public double Expresion()
        {
            //_StckEvalua.Push(_Operacion[""](_StckEvalua.Pop(),_StckEvalua.Pop()));
            Func<double, double, double> op;
            //Match(IDTokens.Numero);
            //double num = Double.Parse(_Valor);
            double num = Termino();

            //_StckEvalua.Push(Double.Parse(_Valor));
            if (_OpTermino.TryGetValue(_Valor, out op)) {
                //_StckEvalua.Push(op(_StckEvalua.Pop(), Double.Parse(_Valor)));

                Match(IDTokens.OpTermino);
                num = op(num, Expresion());
                //Match(IDTokens.Numero);
                
            }
            return num;

        }

        private double Termino()
        {
            //_StckEvalua.Push(_Operacion[""](_StckEvalua.Pop(),_StckEvalua.Pop()));

            Func<double, double, double> op;
            //Match(IDTokens.Numero);
            //double num = Double.Parse(_Valor);

            double num = Factor();
            //_StckEvalua.Push(Double.Parse(_Valor));
            if (_OpFactor.TryGetValue(_Valor, out op)) {
                //_StckEvalua.Push(op(_StckEvalua.Pop(), Double.Parse(_Valor)));
                //num = op(num, Double.Parse(_Valor));
                Match(IDTokens.OpFactor);
                num = op(num, Termino());
                
            }
            return num;
        }

        private double Factor()
        {
            double a = double.Parse(_Valor);
            Match(IDTokens.Numero);
            return a;
        }

        public void Match(string s)
        {
            if(s == _Valor)
                NextToken();
            else
                Console.WriteLine("se espera " + s);
        }

        public void Match(IDTokens t)
        {
            if (t == _ID)
                NextToken();
            else
                Console.WriteLine("se espera " + t);
        }


        public int this[int x] { get => 3; }
        public int this[string x] { get => 4; }
    }
}
