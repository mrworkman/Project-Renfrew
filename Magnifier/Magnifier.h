// Project Renfrew
// Copyright(C) 2018 Stephen Workman (workman.stephen@gmail.com)
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

namespace Renfrew::Utility {

   public ref class Magnifier : public HwndHost {
      HWND _parentHwnd;
      HWND _magnifierHwnd;
      HINSTANCE _hInstance;

   protected:
      // From HwndHost
      virtual HandleRef BuildWindowCore(HandleRef handleRef) override;
      virtual void DestroyWindowCore(HandleRef handleRef) override;

   public:
      Magnifier();

      // From HwndHost
      property IKeyboardInputSite^ KeyboardInputSite {
         virtual IKeyboardInputSite^ get() {
            throw gcnew NotImplementedException();
         };

         virtual void set(IKeyboardInputSite^ keyboard_input_site) {
            throw gcnew NotImplementedException();
         };
      }

      void Initialize(double scaleMultiplier);
      void SetMagnification(Int32 multiplier);
      void Update(Int32 x, Int32 y, Int32 width, Int32 height);
   };

}
