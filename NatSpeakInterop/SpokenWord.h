// Project Renfrew
// Copyright(C) 2025 Stephen Workman (workman.stephen@gmail.com)
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
// along with this program. If not, see<http://www.gnu.org/licenses/>.
//

#pragma once

public ref class SpokenWord {
   public:
      SpokenWord(String^ word, const DWORD wordNumber, const DWORD ruleNumber) {
         Word = word;
         WordNumber = wordNumber;
         RuleNumber = ruleNumber;
      }

      property String^ Word;
      property DWORD WordNumber;
      property DWORD RuleNumber;

      virtual String^ ToString() override {
         return String::Format(
            "{{ Word = {0}, Word Number: {1}, Rule Number: {2} }}",
            Word, WordNumber, RuleNumber
         );
      }
};
