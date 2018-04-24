using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Compilador.Analizadores.Lexico.Lexico;

namespace Compilador.Analizadores.Semantica {
    public class Atributo {
        public enum Accesor { Public, Private, Protected }
        public enum TypeNum { Char, Int, Float }
        public enum TypeDato { Empty, Char, Int, Float, String }
        public enum TypeReturn { Empty, Void, Char, Int, FLoat }


        private string _Nombre;
        private string _Valor;
        private TypeDato _TipoDato;
        private string _Acceso;

        public Atributo(string nombre, string valor, string tipoDato, string accesor)
        {
            _Nombre = nombre;
            _Valor = valor;
            _TipoDato = (TypeDato)Enum.Parse(typeof(TypeDato), tipoDato, true);
            _Acceso = accesor;
        }

        public Atributo(string nombre, string valor,
            TypeDato tipoDato, string acceso)
        {
            _Nombre = nombre;
            _Valor = valor;
            _TipoDato = tipoDato;
            _Acceso = acceso;
        }

        public Atributo(string nombre, Atributo valor,
            string tipoDato, string acceso)
        {
            _Nombre = nombre;
            _Acceso = acceso;
            _Valor = valor._Valor;

            
            if (Enum.TryParse(tipoDato, true, out TypeNum aux))
            {
                var tmpDato = (TypeDato)Enum.Parse(typeof(TypeDato), tipoDato, true);
                if (tmpDato >= valor.TipoDato)
                    _TipoDato = tmpDato;
                else
                    throw new InvalidDataException();
            }
        }

        public Atributo(string nombre, string valor,
            IDTokens tipoDato, string acceso)
        {
            _Nombre = nombre;
            _Acceso = acceso;
            switch (tipoDato)
            {
                case IDTokens.NumeroInt:
                    _TipoDato = TypeDato.Int;
                    _Valor = valor;
                    break;
                case IDTokens.NumeroFlt:
                    _TipoDato = TypeDato.Float;
                    _Valor = valor;
                    break;
                case IDTokens.Caracter:
                    _TipoDato = TypeDato.Char;
                    _Valor = valor;
                    break;
                default:
                    _TipoDato = TypeDato.Empty;
                    _Valor = valor;
                    break;
            }
        }

        public string Nombre { get => _Nombre; set => _Nombre = value; }
        public TypeDato TipoDato { get => _TipoDato; set => _TipoDato = value; }
        public string Acceso { get => _Acceso; set => _Acceso = value; }

        public object Valor {
            get {
                switch (_TipoDato)
                {
                    case TypeDato.Char:
                    case TypeDato.Int:
                    case TypeDato.Float:
                        return double.Parse(_Valor);
                    default:
                        return _Valor;
                }
            } 
        }

        public Atributo Cast(string tipo)
        {
            Atributo atrib;
            switch (Enum.Parse(typeof(TypeNum), tipo, true))
            {
                case TypeNum.Char:
                    atrib = (this - this % 1) % 256;
                    atrib.TipoDato = TypeDato.Char;
                    break;
                case TypeNum.Int:
                    atrib = (this - this % 1) % 655366;
                    atrib.TipoDato = TypeDato.Int;
                    break;
                case TypeNum.Float:
                    atrib = this % 4294967296;
                    atrib.TipoDato = TypeDato.Float;
                    break;
                default:
                    atrib = null;
                    break;
            }

            return atrib;
        }

        public static Atributo operator +(Atributo x, Atributo y)
        {
            var tipo = x.TipoDato >= y.TipoDato ? x.TipoDato : y.TipoDato;
            string tmp = "" + ((double)x.Valor + (double)y.Valor);
            return new Atributo("", tmp, tipo, "" );
        }

        public static Atributo operator -(Atributo x, Atributo y)
        {
            var tipo = x.TipoDato >= y.TipoDato ? x.TipoDato : y.TipoDato;
            string tmp = "" + ((double)x.Valor - (double)y.Valor);
            return new Atributo("", tmp, tipo, "");
        }
        public static Atributo operator *(Atributo x, Atributo y)
        {
            var tipo = x.TipoDato >= y.TipoDato ? x.TipoDato : y.TipoDato;
            string tmp = "" + ((double)x.Valor * (double)y.Valor);
            return new Atributo("", tmp, tipo, "");
        }
        public static Atributo operator /(Atributo x, Atributo y)
        {
            var tipo = x.TipoDato >= y.TipoDato ? x.TipoDato : y.TipoDato;
            string tmp = "" + ((double)x.Valor / (double)y.Valor);
            return new Atributo("", tmp, tipo, "");
        }
        public static Atributo operator %(Atributo x, Atributo y)
        {
            var tipo = x.TipoDato >= y.TipoDato ? x.TipoDato : y.TipoDato;
            string tmp = "" + ((double)x.Valor % (double)y.Valor);
            return new Atributo("", tmp, tipo, "");
        }

        public static Atributo operator +(Atributo x, double y)
        {
            return new Atributo("", "" + ((double)x.Valor + y), x.TipoDato, "");
        }
        public static Atributo operator -(Atributo x, double y)
        {
            return new Atributo("", "" + ((double)x.Valor - y), x.TipoDato, "");
        }
        public static Atributo operator *(Atributo x, double y)
        {
            return new Atributo("", "" + ((double)x.Valor * y), x.TipoDato, "");
        }
        public static Atributo operator /(Atributo x, double y)
        {
            return new Atributo("", "" + ((double)x.Valor / y), x.TipoDato, "");
        }
        public static Atributo operator %(Atributo x, double y)
        {
            return new Atributo("", "" + ((double)x.Valor % y), x.TipoDato, "");
        }


        //public static Atributo operator ^(Atributo x, double y)
        //{
        //    return new Atributo("", x.Valor % y, x.TipoDato, "");
        //}
    }
}
