;Fecha de compilacion: 03/05/2018 09:35:39 a. m.
;Angel Emmanuel Ruiz Alcaraz
.386
.model flat, stdcall
option casemap:none
includelib \masm32\lib\masm32.lib
include \masm32\include\windows.inc
include \masm32\include\masm32.inc

.data
t1 dd ?
i dw ?
t2 dw ?
ConsoleInHand dd ?
ConsoleOutHand dd ?
InBuff db "0"
OutBuff db "0"
KeyBuff db "0"
OutBWr dd ?
NewL db 13,10

.stack 4096
STD_INPUT_HANDLE EQU -10
STD_OUTPUT_HANDLE EQU -11
ExitProcess PROTO, dwExitCode: DWORD 
dwtoa PROTO, siq: DWORD, soq: dword
GetStdHandle PROTO, nStdHandle: DWORD 
SetConsoleMode PROTO HANDLE: DWORD, dwMode: DWORD
ReadConsoleInputA PROTO, handle: DWORD, lpBuffer: PTR BYTE, nlen:DWORD, lpNumberOfEvents:PTR DWORD
ReadConsoleA PROTO, handle: DWORD, lpBuffer:PTR BYTE, nNumberOfBytesToWrite:DWORD, lpNumberOfBytesWritten:PTR DWORD, lpReserved:DWORD
WriteConsoleA PROTO, handle: DWORD, lpBuffer:PTR BYTE, nNumberOfBytesToWrite:DWORD, lpNumberOfBytesWritten:PTR DWORD, lpReserved:DWORD

.code
main PROC
INVOKE GetStdHandle, STD_INPUT_HANDLE
mov ConsoleInHand, eax 
INVOKE GetStdHandle, STD_OUTPUT_HANDLE
mov ConsoleOutHand, eax 

push 4
pop t1

push 0
pop i

ForCon0:
push i
push 5
pop ebx
pop eax
cmp ax, bx
jb $+6
push 0
jmp $+4
push 1
pop eax
cmp eax, 1
jne ForFin0
jmp For0
ForInc0:
add i, 1
jmp ForCon0
For0:
push i
pop t1
INVOKE dwtoa, i, ADDR OutBuff
lea ebx, OutBuff
INVOKE WriteConsoleA, ConsoleOutHand, ebx, 1, offset OutBWr, 0
jmp ForInc0
ForFin0:

push 0
pop eax
cmp eax, 1
jne If10
jmp IfFin1
If10:
push 0
pop eax
cmp eax, 1
je $+6
push 1
jmp $+4
push 0
pop eax
cmp eax, 1
jne If11
push 4
push 3
pop ebx
pop eax
xor edx, edx
div ebx
mov eax, edx
push eax
pop t2

INVOKE dwtoa, t2, ADDR OutBuff
lea ebx, OutBuff
INVOKE WriteConsoleA, ConsoleOutHand, ebx, 1, offset OutBWr, 0
lea ebx, NewL
INVOKE WriteConsoleA, ConsoleOutHand, ebx, 2, offset OutBWr, 0
INVOKE ReadConsoleA, ConsoleInHand, offset InBuff, 10, offset OutBWr, 0
jmp IfFin1
If11:
INVOKE dwtoa, t1, ADDR OutBuff
lea ebx, OutBuff
INVOKE WriteConsoleA, ConsoleOutHand, ebx, 1, offset OutBWr, 0
lea ebx, NewL
INVOKE WriteConsoleA, ConsoleOutHand, ebx, 2, offset OutBWr, 0
IfFin1:

INVOKE SetConsoleMode, ConsoleInHand, not 2
INVOKE ReadConsoleA, ConsoleInHand, offset KeyBuff, 1, offset OutBWr, 0
INVOKE SetConsoleMode, ConsoleInHand, not 6
INVOKE ExitProcess, 0
main ENDP
END main


