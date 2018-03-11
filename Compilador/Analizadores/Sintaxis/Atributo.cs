using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compilador.Analizadores.Sintaxis {
    public class Atributo {
        public enum Accesor{ Public, Private, Protected }
        public enum TypeDato { Unknown, Void, Char, Int, Float, Double }

        private string _Nombre;
        private double _Valor;
        private TypeDato _TipoDato;
        private Accesor _Acceso;
        
        //float 4 bytes -> 0 - 4294967296 //int 0 - 655335 // char 0 - 255
        //cast tiene que ver con la parte baja y parte alta de ensamblador
        //cast explicito
        //overflow
        public Atributo(string nombre, double valor, string tipoDato, string accesor)
        {
            _Nombre = nombre;
            _Valor = valor;
            if(Enum.IsDefined(typeof(TypeDato), tipoDato)) {
                _TipoDato = (TypeDato)Enum.Parse(typeof(TypeDato), tipoDato, true);
            } else {
                //arrojar exception
            }

            if (Enum.IsDefined(typeof(Accesor), accesor)) {
                _Acceso = (Accesor)Enum.Parse(typeof(Accesor), accesor, true);
            } else {
                //arrojar exception
            }
            
        }

        public Atributo(string nombre, double valor, 
            TypeDato tipoDato, Accesor acceso = Accesor.Private)
        {
            _Nombre = nombre;
            _Valor = valor;
            _TipoDato = tipoDato;
            _Acceso = acceso;
        }

        public string Nombre { get => _Nombre; set => _Nombre = value; }
        public double Valor { get => _Valor; set => _Valor = value; }
        public TypeDato TipoDato { get => _TipoDato; set => _TipoDato = value; }
        public Accesor Acceso { get => _Acceso; set => _Acceso = value; }

    }
}
