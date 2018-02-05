﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compilador.Analizadores.Lexico {
    public class Token {
        private Lexico.IDTokens _ID;
        private string _Valor;

        public Token(Lexico.IDTokens id, string valor)
        {
            _ID = id;
            _Valor = valor;
        }

        public Lexico.IDTokens ID { get => _ID; set => _ID = value; }
        public string Valor { get => _Valor; set => _Valor = value; }

    }
}
