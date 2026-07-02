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

using System;

#pragma warning disable CS0659
namespace Renfrew.Grammar.Serialization.HighLevelTypes {
    public class Symbol {
        public SymbolType Type { get; set; }
        public ushort Probability { get; } = 0;
        public OperationType? OperationType { get; set; }
        public uint? Id { get; set; }

        public uint Value {
            get {
                if (OperationType != null) {
                    return (uint) OperationType;
                }

                if (Id != null) {
                    return (uint) Id;
                }

                throw new ArgumentNullException();
            }
        }

        public override bool Equals(object obj) {
            var other = (Symbol) obj;

            if (other == null) {
                return false;
            }

            return Type == other.Type
                   && Probability == other.Probability
                   && OperationType == other.OperationType
                   && Id == other.Id;
        }
    }
}
