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
// along with this program.If not, see<http://www.gnu.org/licenses/>.
//

using System.IO;

#pragma warning disable CS0659
namespace Renfrew.Grammar.Serialization.LowLevelTypes {
    internal class SrHeader {
        public uint Type { get; set; }
        public uint Flags { get; set; }

        public void Serialize(BinaryWriter writer) {
            writer.Write(Type);
            writer.Write(Flags);
        }

        public override bool Equals(object obj) {
            var other = obj as SrHeader;

            if (other == null) {
                return false;
            }

            return Type == other.Type
                   && Flags == other.Flags;
        }
    }
}
