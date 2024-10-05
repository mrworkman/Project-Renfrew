using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

using DiffMatchPatch;

using NUnit.Framework;

namespace GrammarTests.Util {
   internal static class CustomAssert {
      // Characters for "highlighting" differences in black & white "consoles".
      const char CombiningLongStrokeOverlay = '\u0336'; // e.g. A̶
      const char CombiningDiaeresis = '\u0308';         // e.g. 1̈

      public static void ByteArraysAreEqual(
         byte[] expected,
         byte[] actual,
         string message = "",
         params object[] args) {
         Assert.AreEqual(
            expected, 
            actual, 
            $"{message}\r\n{DumpDiffToString(expected, actual)}", 
            args
         );
      }

      static string DumpDiffToString(byte[] expectedBytes, byte[] actualBytes) {
         var expectedHex = ToHexString(expectedBytes);
         var actualHex = ToHexString(actualBytes);

         var diffs = Diff.Compute(expectedHex, actualHex);

         var (expectedDumpHex, deletions) = GetMarkedUpDiff(
            diffs, Operation.Delete
         );
         var (actualDumpHex, insertions) = GetMarkedUpDiff(
            diffs, Operation.Insert
         );

         var expectedDumpStr = HexDumpDiff(
            expectedDumpHex,
            expectedBytes,
            deletions,
            Operation.Delete,
            "Expected"
         );
         var actualDumpStr = HexDumpDiff(
            actualDumpHex,
            actualBytes,
            insertions,
            Operation.Insert,
            "Actual"
         );

         return $"{expectedDumpStr}{actualDumpStr}";
      }

      static (string, HashSet<int>) GetMarkedUpDiff(
         List<Diff> diffs,
         Operation operation
      ) {
         var changes = new HashSet<int>();
         var builder = new StringBuilder();

         var currentIndex = 0;
         var markNext = false;
         var marker = operation == Operation.Delete ?
            CombiningLongStrokeOverlay : CombiningDiaeresis;

         foreach (var diff in diffs) {
            if (diff.Operation == Operation.Equal) {
               currentIndex += diff.Text.Length;

               if (markNext) {
                  builder.Append(diff.Text[0]);
                  builder.Append(marker);
                  builder.Append(string.Join("", diff.Text.Skip(1)));
                  markNext = false;
               } else {
                  builder.Append(diff.Text);
               }

               if (currentIndex % 2 != 0) {
                  builder.Append(marker);
               }
            } else if (diff.Operation == operation) {
               builder.Append(MarkUp(diff.Text, marker));

               markNext = currentIndex % 2 == 0 && diff.Text.Length % 2 != 0;

               foreach (var _ in diff.Text) {
                  changes.Add(
                     (currentIndex % 2 == 0 ? currentIndex : currentIndex - 1) / 2
                  );
                  currentIndex++;
               }
            }
         }

         var expectedDumpHex = builder.ToString();

         return (expectedDumpHex, changes);
      }


      static string MarkUp(string s, char marker) {
         return String.Join(
            "",
            s.Select(c => (c > 31 && c < 127) ? $"{c}{marker}" : $"{c}")
         );
      }

      static string HexDumpDiff(
         string diffText,
         byte[] sourceBytes,
         HashSet<int> differenceIndeces,
         Operation operation,
         string label
      ) {
         var enumerator = StringInfo.GetTextElementEnumerator(diffText);
         var clusters = new List<string>();

         while (enumerator.MoveNext()) {
            clusters.Add((string)enumerator.Current);
         }

         var mainBuilder = new StringBuilder();
         mainBuilder.AppendLine();
         mainBuilder.AppendLine($"{label}:");

         var marker = operation == Operation.Delete ?
            CombiningLongStrokeOverlay : CombiningDiaeresis;

         const int bytesPerLine = 16;

         mainBuilder.AppendLine(
            "         00 01 02 03 04 05 06 07 08 09 0A 0B 0C 0D 0E 0F"
         );

         for (int i = 0; i < clusters.Count; i += (bytesPerLine * 2)) {
            var clusterLine = clusters.Skip(i).Take(bytesPerLine * 2).ToList();

            mainBuilder.Append($"{i / 2:x8} ");

            for (int j = 0; j < clusterLine.Count; j += 2) {
               mainBuilder.Append($"{string.Join("", clusterLine.Skip(j).Take(2))} ");
            }

            if (clusterLine.Count / 2 != bytesPerLine) {
               var padding = bytesPerLine - clusterLine.Count / 2;
               for (int j = 0; j < padding; j++) {
                  mainBuilder.Append("   ");
               }
            }

            mainBuilder.Append("  ");

            for (
               int j = i / 2, k = 0;
               j < sourceBytes.Length && k < bytesPerLine;
               j++, k++
            ) {
               var c = sourceBytes[j];
               var addMarker = differenceIndeces.Contains(j);
               var builder = new StringBuilder();

               if (c is > 31 and < 127) {
                  builder.Append((char)c);
               } else {
                  builder.Append('.');
               }

               if (addMarker) {
                  builder.Append(marker);
               }

               mainBuilder.Append(builder.ToString());
            }

            mainBuilder.AppendLine();
         }

         return mainBuilder.ToString();
      }

      static string ToHexString(byte[] bytes) {
         return BitConverter.ToString(bytes).Replace("-", "");
      }
   }
}
