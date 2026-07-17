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

using System.Collections.Generic;
using System.IO;
using System.Linq;

#pragma warning disable CS0659
namespace Renfrew.Grammar.Serialization.LowLevelTypes {
    internal class SrCfgRule : ISerializableRule {
        public const uint SrCfgRuleSize = 8;
        public const uint SrCfgSymbolSize = 8;

        public uint Size =>
           SrCfgRuleSize + (uint)Symbols.Count * SrCfgSymbolSize;

        public uint UniqueId { get; set; }
        public List<SrCfgSymbol> Symbols { get; set; }

        public void Serialize(BinaryWriter writer) {
            writer.Write(Size);
            writer.Write(UniqueId);
            Symbols.ForEach(symbol => symbol.Serialize(writer));
        }

        public override bool Equals(object obj) {
            var other = obj as SrCfgRule;

            if (other == null) {
                return false;
            }

            return Size == other.Size
                   && UniqueId == other.UniqueId
                   && Symbols.SequenceEqual(other.Symbols);
        }
    }
}
