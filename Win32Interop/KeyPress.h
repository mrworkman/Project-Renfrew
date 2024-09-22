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

using namespace System;

namespace Renfrew::Win32::Interop {
   public ref class KeyPress abstract : IEquatable<KeyPress^> {
      internal: virtual property DWORD KeyFlags {
         DWORD get();
      }

      internal: virtual property WORD ScanCode {
         WORD get();
      }

      internal: virtual property WORD VirtualKeyCode {
         WORD get();
      }

      internal: virtual void KeyDown(LPINPUT input) sealed;
      internal: virtual void KeyUp(LPINPUT input) sealed;

      public: virtual bool Equals(KeyPress^ other);
      public: String^ ToString() override;
   };

   public ref class ExtendedKeyPress abstract : KeyPress {
      internal: virtual property DWORD KeyFlags {
         DWORD get() override;
      }
   };

   public ref class UnicodeKeyPress abstract : KeyPress {
      internal: virtual property DWORD KeyFlags {
         DWORD get() override;
      }
   };
}
