// Project Renfrew
// Copyright(C) 2024 Stephen Workman (workman.stephen@gmail.com)
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

#pragma once

#include "Keys.h"

using namespace System;
using namespace System::Collections::Generic;
using namespace System::Text;

namespace Renfrew::Win32::Interop {
   public ref class KeyChord sealed : IEquatable<KeyChord^> {
      KeyChord(IEnumerable<KeyPress^>^ keys);

   internal:
      property List<KeyPress^>^ _keys;

   public:
      static KeyChord^ Keys(... array<KeyPress^>^ keys);
      static KeyChord^ Keys(IEnumerable<KeyPress^>^ keys);

      virtual bool Equals(KeyChord^ other);

      String^ ToString() override;
   };
}
