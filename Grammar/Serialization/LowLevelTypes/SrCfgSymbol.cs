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

namespace Renfrew.Grammar.Serialization.LowLevelTypes {
   internal struct SrCfgSymbol {
      public ushort Type { get; set; }
      public ushort Probability { get; set; }
      public uint Value { get; set; }

      public void Serialize(BinaryWriter writer) {
         writer.Write(Type);
         writer.Write(Probability);
         writer.Write(Value);
      }
   }
}
