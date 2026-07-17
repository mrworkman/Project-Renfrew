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
using System.Text;

#pragma warning disable CS0659
namespace Renfrew.Grammar.Serialization.LowLevelTypes {
    internal class SrCfgXRule : ISerializableRule {
        public const uint SrCfgXRuleSize = 8;

        public uint RuleNumber { get; set; }
        public string String { get; set; }

        public uint Size => SrCfgXRuleSize + GetPaddedStringLength(String);

        public void Serialize(BinaryWriter writer) {
            writer.Write(Size);
            writer.Write(RuleNumber);

            var stringLength = GetPaddedStringLength(String);

            var encodedString = Encoding.Unicode.GetBytes(String);

            writer.Write(encodedString);

            if (stringLength - encodedString.Length != 0) {
                writer.Write(new byte[stringLength - encodedString.Length]);
            }
        }

        internal static uint GetPaddedStringLength(string s) {
            var numBytes = (uint)Encoding.Unicode.GetByteCount(s) + 2u;

            // Pad to 4-byte boundary.
            return (numBytes + 3u) & ~3u;
        }

        public override bool Equals(object obj) {
            var other = obj as SrCfgXRule;

            if (other == null) {
                return false;
            }

            return Size == other.Size
                   && RuleNumber == other.RuleNumber
                   && String == other.String;
        }
    }
}
