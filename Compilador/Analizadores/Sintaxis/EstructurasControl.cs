using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compilador.Analizadores.Sintaxis
{
    public partial class Lenguaje
    {
        protected void For()
        {
            if (IsMatch("for")) {
                int label = _LabelID;
                _LabelID++;
                _TblAtrib.NewAmbito();
                Match(IDTokens.InitParametros);
                Declaracion();
                Match(IDTokens.FinSentencia);
                _ASM.WR("ForCon{0}:", label);
                Logica();
                Match(IDTokens.FinSentencia);
                _ASM.WR("pop eax");
                _ASM.WR("cmp eax, 1");
                _ASM.WR("jne ForFin{0}", label);
                _ASM.WR("jmp For{0}", label);
                _ASM.WR("ForInc{0}:", label);
                if (_ID == IDTokens.Identificador) {
                    Incremento(_TblAtrib[Match(_Valor)]);
                }
                Match(IDTokens.FinParametros);
                _ASM.WR("jmp ForCon{0}", label);
                _ASM.WR("For{0}:", label);
                Cuerpo();
                _ASM.WR("jmp ForInc{0}", label);
                _ASM.WR("ForFin{0}:\n", label);
            }
        }

        private void If()
        {
            if (IsMatch("if")) {
                int nIf = 0;
                int label = _LabelID;
                _LabelID++;
                Condicion();
                _ASM.WR("pop eax");
                _ASM.WR("cmp eax, 1");
                _ASM.WR("jne If{0}{1}", label, nIf);
                CuerpoOrSentencia();
                _ASM.WR("jmp IfFin{0}", label);
                _ASM.WR("If{0}{1}:", label, nIf++);
                do {
                    if (IsMatch("else")) {
                        if (IsMatch("if")) {
                            Condicion();
                            _ASM.WR("pop eax");
                            _ASM.WR("cmp eax, 1");
                            _ASM.WR("jne If{0}{1}", label, nIf);
                            CuerpoOrSentencia();
                            _ASM.WR("jmp IfFin{0}", label);
                            _ASM.WR("If{0}{1}:", label, nIf++);
                        } else {
                            Sentencia();
                            break;
                        }
                    } else break;
                } while (true);
                _ASM.WR("IfFin{0}:\n", label);
            }
        }
    }
}
