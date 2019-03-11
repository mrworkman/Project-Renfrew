// pch.cpp: source file corresponding to pre-compiled header; necessary for compilation to succeed

#include "pch.h"

namespace std {
   #ifdef UNICODE
   wostream& tcout = wcout;
   #else
   ostream& tcout = cout;
   #endif // UNICODE
}