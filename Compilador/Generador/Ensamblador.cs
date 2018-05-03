using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Compilador.Analizadores.Semantica;

namespace Compilador.Generador
{
    public class Ensamblador
    {
        private StreamWriter _Ensamblado;
        private string _InBuff;
        private string _OutBuff;
        private string _KeyBuff;
        private string _InHand;
        private string _OutHand;
        private string _BytesWr;

        public Ensamblador(StreamWriter stream)
        {
            _Ensamblado = stream;
            _InBuff = "InBuff";
            _OutBuff = "OutBuff";
            _KeyBuff = "KeyBuff";
            _InHand = "ConsoleInHand";
            _OutHand = "ConsoleOutHand";
            _BytesWr = "OutBWr";
            InitASM();
        }

        public void InitASM()
        {
            WR(".386");
            WR(".model flat, stdcall");
            WR("option casemap:none");
            WR(@"includelib \masm32\lib\masm32.lib");
            WR(@"include \masm32\include\windows.inc");
            WR(@"include \masm32\include\masm32.inc");
            //WR(@"include \masm32\include\msvcrt.inc");
            //WR(@"include \masm32\macros\macros.asm");
            WR("");

            WR(".stack 4096");                              //funciones
            WR("STD_INPUT_HANDLE EQU -10");
            WR("STD_OUTPUT_HANDLE EQU -11");
            WR("ExitProcess PROTO, dwExitCode: DWORD ");
            WR("dwtoa PROTO, siq: DWORD, soq: dword");
            WR("GetStdHandle PROTO, nStdHandle: DWORD ");
            WR("SetConsoleMode PROTO HANDLE: DWORD, dwMode: DWORD");
            WR("ReadConsoleInputA PROTO, " +
                "handle: DWORD, " +
                "lpBuffer: PTR BYTE, " +
                "nlen:DWORD, " +
                "lpNumberOfEvents:PTR DWORD");
            WR("ReadConsoleA PROTO, " +
                "handle: DWORD, " +
                "lpBuffer:PTR BYTE, " +
                "nNumberOfBytesToWrite:DWORD, " +
                "lpNumberOfBytesWritten:PTR DWORD, " +
                "lpReserved:DWORD");
            WR("WriteConsoleA PROTO, " +
                "handle: DWORD, " +
                "lpBuffer:PTR BYTE, " +
                "nNumberOfBytesToWrite:DWORD, " +
                "lpNumberOfBytesWritten:PTR DWORD, " +
                "lpReserved:DWORD");
            WR("");
            WR(".code");
            WR("main PROC");
            WR("INVOKE GetStdHandle, STD_INPUT_HANDLE");
            WR($"mov {_InHand}, eax ");
            WR("INVOKE GetStdHandle, STD_OUTPUT_HANDLE");
            WR($"mov {_OutHand}, eax ");
            WR("");
            
        }

        public void EndASM()
        {
            WR("INVOKE ExitProcess, 0");
            WR("main ENDP");
            WR("END main");
            WR("");
        }

        public void WR(string codigo, params object[] parametros)
        {
            _Ensamblado.WriteLine(codigo, parametros);
        }

        public void Add(string left = "eax", string right = "ebx")
        {
            WR($"add {left}, {right}");
        }
        public void Sub(string left = "eax", string right = "ebx")
        {
            WR($"sub {left}, {right}");
        }
        public void Mul(string data = "ebx")
        {
            WR($"mul {data}");
        }
        public void Div(string data = "ebx")
        {
            WR($"div {data}");
        }
        public void Mod(string data = "ebx")
        {
            WR("xor edx, edx");
            WR($"div {data}");
            WR("mov eax, edx");
        }
        public void And(string left = "eax", string right = "ebx")
        {
            WR($"and {left}, {right}");
        }
        public void Or(string left = "eax", string right = "ebx")
        {
            WR($"or {left}, {right}");
        }
        public void AritLog(string op)
        {
            WR("pop ebx");
            WR("pop eax");
            switch (op)
            {
                case "+": Add(); break;
                case "-": Sub(); break;
                case "*": Mul(); break;
                case "/": Div(); break;
                case "%": Mod(); break;
                case "||": And(); break;
                case "&&": Or(); break;
                default:
                    throw new FormatException($"Asm no reconoce el operador {op}");
            }
            WR("push eax");
        }

        public void Comp(string op)
        {
            WR("pop ebx");
            WR("pop eax");
            WR("cmp ax, bx");
            switch (op)
            {
                case "==": WR("je $+6"); break;
                case "!=": WR("jne $+6"); break;
                case ">=": WR("jea $+6"); break;
                case "<=": WR("jeb $+6"); break;
                case ">": WR("ja $+6"); break;
                case "<": WR("jb $+6"); break;
                default:
                    throw new FormatException($"Asm no reconoce el operador de comparacion {op}");
            }
            WR("push 0");
            WR("jmp $+4");
            WR("push 1");
        }

        public void Not()
        {
            WR("pop eax");
            WR("cmp eax, 1");
            WR("je $+6");
            WR("push 1");
            WR("jmp $+4");
            WR("push 0");
        }

        public string OutBuff { get => _OutBuff; }
        public string InHand { get => _InHand; }
        public string OutHand { get => _OutHand; }
        public string BytesWr { get => _BytesWr; }
        public string KeyBuff { get => _KeyBuff; }
        public string InBuff { get => _InBuff; }

    }
}
