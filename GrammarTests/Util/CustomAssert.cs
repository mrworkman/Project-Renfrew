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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DiffMatchPatch;
using NUnit.Framework;

namespace GrammarTests.Util {
    internal static class CustomAssert {
        public static void ByteArraysAreEqual(
           byte[] expected,
           byte[] actual,
           string message = "",
           params object[] args
        ) {
            Assert.AreEqual(
               expected,
               actual,
               $"{message}\r\n{DumpDiffToString(expected, actual)}",
               args
            );
        }

        static string DumpDiffToString(
           byte[] expectedBytes,
           byte[] actualBytes
        ) {
            var expectedHex = ToHexString(expectedBytes);
            var actualHex = ToHexString(actualBytes);

            var diffs = Diff.Compute(expectedHex, actualHex);

            var expectedDumpStr = GetMarkedUpDiff(diffs, Operation.Delete);
            var actualDumpStr = GetMarkedUpDiff(diffs, Operation.Insert);

            return $"Expected\r\n{expectedDumpStr}\r\nActual\r\n{actualDumpStr}";
        }

        static string GetMarkedUpDiff(
           List<Diff> diffs,
           Operation operation
        ) {
            var builder = new StringBuilder();

            const int bytesPerLine = 16;

            builder.AppendLine(
               "         00 01 02 03 04 05 06 07 08 09 0A 0B 0C 0D 0E 0F"
            );

            var filteredDiffs = diffs
               .Where(
                  diff => diff.Operation == Operation.Equal
                          || diff.Operation == operation
               );

            var pairs = new List<(string, Operation)>();

            foreach (var diff in filteredDiffs) {
                foreach (var c in diff.Text) {
                    pairs.Add((c.ToString(), diff.Operation));
                }
            }

            for (var i = 0; i < pairs.Count(); i += (bytesPerLine * 2)) {
                var line = pairs.Skip(i)
                   .Take(bytesPerLine * 2)
                   .Select(pair => pair.Item1)
                   .ToList();

                var ops = pairs.Skip(i)
                   .Take(bytesPerLine * 2)
                   .Select(pair => pair.Item2)
                   .ToList();

                builder.Append($"{i / 2:x8} ");

                var decoded = new StringBuilder();

                for (var j = 0; j < line.Count(); j += 2) {
                    var byteDigits = string.Join("", line.Skip(j).Take(2));

                    builder.Append($"{byteDigits} ");

                    var c = Convert.ToByte(byteDigits, 16);

                    if (c is > 31 and < 127) {
                        decoded.Append((char) c);
                    } else {
                        decoded.Append('.');
                    }
                }

                if (line.Count / 2 != bytesPerLine) {
                    var padding = bytesPerLine - line.Count / 2;
                    for (var j = 0; j < padding; j++) {
                        builder.Append("   ");
                    }
                }

                builder.Append("  ");
                builder.Append(string.Join("", decoded));

                if (ops.Any(op => op != Operation.Equal)) {
                    builder.AppendLine();
                    builder.Append(">>>>>>>> ");

                    for (var j = 0;
                         j < ops.Count();
                         j += 2
                        ) {
                        var marks = string.Join(
                           "",
                           ops.Skip(j)
                              .Take(2)
                              .Select(op => op == Operation.Equal ? " " : "^")
                        );
                        builder.Append($"{marks} ");
                    }
                }

                builder.AppendLine();
            }

            builder.AppendLine();

            return builder.ToString();
        }

        static string ToHexString(byte[] bytes) {
            return BitConverter.ToString(bytes).Replace("-", "");
        }
    }
}
