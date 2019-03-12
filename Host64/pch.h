// Project Renfrew
// Copyright(C) 2019 Stephen Workman (workman.stephen@gmail.com)
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.If not, see<http://www.gnu.org/licenses/>.
//

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
