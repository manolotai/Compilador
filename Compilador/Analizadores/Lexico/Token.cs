using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compilador {
    public class Token {
        private int _ID;
        private string _Valor;

        public Token(int id, string valor)
        {
            _ID = id;
            _Valor = valor;
        }

        public int ID { get => _ID; set => _ID = value; }
        public string Valor { get => _Valor; set => _Valor = value; }

    }
}
