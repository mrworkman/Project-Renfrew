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

namespace Renfrew.Grammar.Serialization.LowLevelTypes {
   internal class SrCfgXRule : ISerializableRule {
      public const uint SrCfgXRuleSize = 8;

      public uint Size => SrCfgXRuleSize
                          + GetPaddedStringLength(String, EncodeAsUnicode);

      public uint RuleNumber { get; set; }
      public string String { get; set; }
      public bool EncodeAsUnicode { get; set; }

      internal static uint GetPaddedStringLength(
         string s,
         bool encodeAsUnicode
      ) {
         var numBytes = encodeAsUnicode ?
            (uint) Encoding.Unicode.GetByteCount(s) + 2u :
            (uint) Encoding.ASCII.GetByteCount(s) + 1u;

         // Pad to 4-byte boundary.
         return (numBytes + 3u) & ~3u;
      }

      public void Serialize(BinaryWriter writer) {
         writer.Write(Size);
         writer.Write(RuleNumber);

         var stringLength = GetPaddedStringLength(String, EncodeAsUnicode);

         var encodedString = EncodeAsUnicode ?
            Encoding.Unicode.GetBytes(String) :
            Encoding.ASCII.GetBytes(String);

         writer.Write(encodedString);

         writer.Write(new byte[stringLength - encodedString.Length]);
      }
   }
}
