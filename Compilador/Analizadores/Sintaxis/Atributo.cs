using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compilador.Analizadores.Sintaxis {
    public class Atributo {
        enum Accesor{ Publico, Privado, Protegido }
        enum Tipo { Char, Int, Float, Double }
        private Tipo _TipoDato;
        private string _Nombre;
        private Accesor _Accesibilidad;
        private double _Valor;
        int a = 0;
        
        Atributo()
        {
            
        }
    }
}
