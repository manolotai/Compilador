using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compilador.Analizadores.Sintaxis {
    public class Atributo {
        public enum Accesor { Public, Private, Protected }
        public enum TypeDato { Unknown, Void, Char, Int, Float }

        private string _Nombre;
        private double _Valor;
        private TypeDato _TipoDato;
        private string _Acceso;

        public Atributo(string nombre, double valor, string tipoDato, string accesor)
        {
            _Nombre = nombre;
            _Valor = valor;
            _TipoDato = (TypeDato)Enum.Parse(typeof(TypeDato), tipoDato, true);
            _Acceso = accesor;
        }

        public Atributo(string nombre, double valor,
            TypeDato tipoDato, string acceso)
        {
            _Nombre = nombre;
            _Valor = valor;
            _TipoDato = tipoDato;
            _Acceso = acceso;
        }

        public string Nombre { get => _Nombre; set => _Nombre = value; }
        public double Valor { get => _Valor; set => _Valor = value; }
        public TypeDato TipoDato { get => _TipoDato; set => _TipoDato = value; }
        public string Acceso { get => _Acceso; set => _Acceso = value; }

        public static Atributo operator +(Atributo x, Atributo y)
        {
            var tipo = x.TipoDato >= y.TipoDato ? x.TipoDato : y.TipoDato;
            return new Atributo("", x.Valor + y.Valor, tipo, "" );
        }

        public static Atributo operator -(Atributo x, Atributo y)
        {
            var tipo = x.TipoDato >= y.TipoDato ? x.TipoDato : y.TipoDato;
            return new Atributo("", x.Valor - y.Valor, tipo, "");
        }
        public static Atributo operator *(Atributo x, Atributo y)
        {
            var tipo = x.TipoDato >= y.TipoDato ? x.TipoDato : y.TipoDato;
            return new Atributo("", x.Valor * y.Valor, tipo, "");
        }
        public static Atributo operator /(Atributo x, Atributo y)
        {
            var tipo = x.TipoDato >= y.TipoDato ? x.TipoDato : y.TipoDato;
            return new Atributo("", x.Valor / y.Valor, tipo, "");
        }
        public static Atributo operator %(Atributo x, Atributo y)
        {
            var tipo = x.TipoDato >= y.TipoDato ? x.TipoDato : y.TipoDato;
            return new Atributo("", x.Valor % y.Valor, tipo, "");
        }

        public static Atributo operator +(Atributo x, double y)
        {
            return new Atributo("", x.Valor + y, x.TipoDato, "");
        }
        public static Atributo operator -(Atributo x, double y)
        {
            return new Atributo("", x.Valor - y, x.TipoDato, "");
        }
        public static Atributo operator *(Atributo x, double y)
        {
            return new Atributo("", x.Valor * y, x.TipoDato, "");
        }
        public static Atributo operator /(Atributo x, double y)
        {
            return new Atributo("", x.Valor / y, x.TipoDato, "");
        }
        public static Atributo operator %(Atributo x, double y)
        {
            return new Atributo("", x.Valor % y, x.TipoDato, "");
        }
    }
}
