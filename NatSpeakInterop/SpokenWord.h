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
        SpokenWord(String^ word, const DWORD wordId, const DWORD ruleId) {
            Word = word;
            WordId = wordId;
            RuleId = ruleId;
        }

        property String^ Word;
        property DWORD WordId;
        property DWORD RuleId;

        virtual String^ ToString() override {
            return String::Format(
                "{{ Word = {0}, Word ID: {1}, Rule ID: {2} }}",
                Word,
                WordId,
                RuleId
            );
        }
};
