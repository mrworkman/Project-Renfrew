// Tips for Getting Started:
//   1. Use the Solution Explorer window to add/manage files
//   2. Use the Team Explorer window to connect to source control
//   3. Use the Output window to see build output and other messages
//   4. Use the Error List window to view errors
//   5. Go to Project > Add New Item to create new code files, or Project > Add Existing Item to add existing code files to the project
//   6. In the future, to open this project again, go to File > Open > Project and select the .sln file

#ifndef PCH_H
#define PCH_H

#include <windows.h>
#include <iostream>
#include <thread>

#include <tchar.h>

// Make things easier...
// \ref https://stackoverflow.com/a/48768008
namespace std {
   #ifdef UNICODE
      extern wostream& tcout;
   #else
      extern ostream& tcout;
   #endif
}

#ifdef UNICODE
   #define RPC_TSTR RPC_WSTR
#else
   #define RPC_TSTR RPC_CSTR
#endif

#endif //PCH_H
