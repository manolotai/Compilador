.386
.model flat, stdcall
option casemap:none
include \masm32\include\windows.inc
include \masm32\include\masm32.inc
include \masm32\include\msvcrt.inc
include \masm32\macros\macros.asm
;include \masm32\Irvine\Examples\Irvine32.inc
      includelib \masm32\lib\masm32.lib
	  ;includelib \masm32\Irvine\Examples\Irvine32.lib
;include \masm32\include\kernel32.inc
;iclude \masm32\include\user32.inc

;ncludelib \masm32\lib\kernel32.lib
;ncludelib \masm32\lib\user32.lib
.stack 4096

 STD_OUTPUT_HANDLE EQU -11 
 STD_INPUT_HANDLE EQU -10
 GetStdHandle PROTO, nStdHandle: DWORD 
 WriteConsoleA PROTO, handle: DWORD, lpBuffer:PTR BYTE, nNumberOfBytesToWrite:DWORD, lpNumberOfBytesWritten:PTR DWORD, lpReserved:DWORD
 ReadConsoleA PROTO, handle: DWORD, lpBuffer:PTR BYTE, nNumberOfBytesToWrite:DWORD, lpNumberOfBytesWritten:PTR DWORD, lpReserved:DWORD
 ReadConsoleInputA PROTO, handle: DWORD, lpBuffer: PTR BYTE, nlen:DWORD, lpNumberOfEvents:PTR DWORD
 ;InKey PROTO, han: DWORD
 ReadChar Proto

 dwtoa PROTO, siq: DWORD, soq: dword
 ExitProcess PROTO, dwExitCode: DWORD 
 GetConsoleMode PROTO, nConsoleHandle: DWORD, lpMode: PTR DWORD
 SetConsoleMode proto, hConsoleHandle: DWORD, dwMode: DWORD

 .data


buffer1 byte 20 dup (35) 
NumberOfChars DWORD ?

 CONSOLE_READCONSOLE_CONTROL struct
	nLength ULONG sizeof CONSOLE_READCONSOLE_CONTROL
	nInitialChars ULONG 0
	dwCtrlWakeupMask ULONG 0x08
	dwControlKeyState ULONG 0x0008
CONSOLE_READCONSOLE_CONTROL ends

 consoleOutHandle dd ?
 consoleInHandle dd ?
 bytesWritten dd ?
 bytesWritten2 dd ? 
 message db "Hello World",13,10
 lmessage dd 13
 ma dd 72
 newl db 13, 10
 hConInput HANDLE ?

 .code

 main PROC
 ;INVOKE SetConsoleMode, 0, 1

  INVOKE GetStdHandle, STD_INPUT_HANDLE
  mov consoleInHandle, eax 
  ;INVOKE ReadConsoleInputA, consoleInHandle, ADDR buffer1, 1, ADDR NumberOfChars

  mov edx,offset message 
  
  pushad    
  mov eax, lmessage
  INVOKE ReadConsoleA, consoleInHandle, ebx, 10, offset bytesWritten, 0
  INVOKE GetStdHandle, STD_OUTPUT_HANDLE
  mov consoleOutHandle, eax
  
  ;mov ma, 72
  ;mov ebx, offset ma
  ;mov message, ma
  ;INVOKE dwtoa, ma, ADDR message;funcionaaaa
  ;lea ebx, message
  ;INVOKE WriteConsoleA, consoleOutHandle, ebx, 4 , offset bytesWritten2, 0
  ;lea ebx, newl
  ;INVOKE WriteConsoleA, consoleOutHandle, ebx, 2 , offset bytesWritten2, 0
  INVOKE ReadConsoleA, consoleInHandle, ebx, 10, offset bytesWritten, 0

  popad
  INVOKE ExitProcess,0 
main ENDP
END main