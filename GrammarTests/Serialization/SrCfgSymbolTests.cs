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

using System.IO;
using Moq;
using NUnit.Framework;
using Renfrew.Grammar.Serialization.LowLevelTypes;

namespace GrammarTests.Serialization {
    [TestFixture]
    internal class SrCfgSymbolTests {
        [Test]
        public void ShouldWriteCorrectBytes() {
            var symbol = new SrCfgSymbol {
                Type = 1,
                Probability = 2,
                Value = 3
            };

            var mockWriter = new Mock<BinaryWriter>();

            symbol.Serialize(mockWriter.Object);

            mockWriter.Verify(
               writer => writer.Write((ushort)1),
               Times.Once,
               "Expected an unsigned short == 1"
            );

            mockWriter.Verify(
               writer => writer.Write((ushort)2),
               Times.Once,
               "Expected an unsigned short == 2"
            );

            mockWriter.Verify(
               writer => writer.Write((uint)3),
               Times.Once,
               "Expected an unsigned int == 1"
            );
        }
    }
}
