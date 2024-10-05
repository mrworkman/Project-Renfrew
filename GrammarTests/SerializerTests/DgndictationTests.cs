using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DiffMatchPatch;

using Moq;

using NUnit.Framework;

using Renfrew.Grammar;
using Renfrew.Grammar.FluentApi;
using Renfrew.NatSpeakInterop;

namespace GrammarTests.SerializerTests {
   class SimpleGrammar : Grammar {
      public SimpleGrammar(IGrammarService grammarService, INatSpeak natSpeak) : base(grammarService, natSpeak) {

      }

      public override void Dispose() {
         throw new NotImplementedException();
      }

      public override void Initialize() {
         ImportRule("dgndictation");
         AddRule("naming_scheme_x", rule => rule.Say("ape").WithRule("dgndictation"));

         //Load();

         //ActivateRule("naming_scheme_x");
      }
   }

   [TestFixture]
   class DgndictationTests {

      // Characters for "highlighting" differences in black & white "consoles".
      const char CombiningLongStrokeOverlay = '\u0336'; // e.g. A̶
      const char CombiningDiaeresis = '\u0308';         // e.g. 1̈

      [Test]
      public void SimpleGrammarShouldProduceCorrectBytes() {
         var expectedBytes = new byte[] {
            #region Bytes
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00,
            0x18, 0x00, 0x00, 0x00, 0x18, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00,
            0x6e, 0x61, 0x6d, 0x69, 0x6e, 0x67, 0x5f, 0x73, 0x63, 0x68, 0x65, 0x6d,
            0x65, 0x5f, 0x78, 0x00, 0x05, 0x00, 0x00, 0x00, 0x18, 0x00, 0x00, 0x00,
            0x18, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x64, 0x67, 0x6e, 0x64,
            0x69, 0x63, 0x74, 0x61, 0x74, 0x69, 0x6f, 0x6e, 0x00, 0x00, 0x00, 0x00,
            0x02, 0x00, 0x00, 0x00, 0x0c, 0x00, 0x00, 0x00, 0x0c, 0x00, 0x00, 0x00,
            0x01, 0x00, 0x00, 0x00, 0x61, 0x70, 0x65, 0x00, 0x03, 0x00, 0x00, 0x00,
            0x28, 0x00, 0x00, 0x00, 0x28, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00,
            0x01, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x03, 0x00, 0x00, 0x00,
            0x01, 0x00, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00,
            0x02, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00
            #endregion
         };

         var ser = new GrammarSerializer(false);
         
         var gram = new SimpleGrammar(
            new Mock<IGrammarService>().Object,
            new Mock<INatSpeak>().Object
         );
         gram.Initialize();
         
         var actualBytes = ser.Serialize(gram);

         DumpDiffToConsole(expectedBytes, actualBytes);

         Assert.AreEqual(expectedBytes, actualBytes);
      }

      void DumpDiffToConsole(byte[] expectedBytes, byte[] actualBytes) {
         var expectedHex = ToHexString(expectedBytes);
         var actualHex = ToHexString(actualBytes);

         var diffs = Diff.Compute(expectedHex, actualHex);

         var (expectedDumpHex, deletions) = GetMarkedUpDiff(diffs, Operation.Delete);
         var (actualDumpHex, insertions) = GetMarkedUpDiff(diffs, Operation.Insert);

         HexDumpDiffToConsole(expectedDumpHex, expectedBytes, deletions, "Expected");
         HexDumpDiffToConsole(actualDumpHex, actualBytes, insertions, "Actual");

         Console.WriteLine();
      }

      (string, HashSet<int>) GetMarkedUpDiff(List<Diff> diffs, Operation operation) {
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
                  changes.Add((currentIndex % 2 == 0 ? currentIndex : currentIndex - 1) / 2);
                  currentIndex++;
               }
            }
         }

         var expectedDumpHex = builder.ToString();

         return (expectedDumpHex, changes);
      }


      string MarkUp(string s, char marker) {
         return String.Join(
            "", 
            s.Select(c => (c > 31 && c < 127) ?  $"{c}{marker}" : $"{c}")
         );
      }

      void HexDumpDiffToConsole(
         string diffText, 
         byte[] sourceBytes, 
         HashSet<int> differenceIndeces, 
         string label
      ) {
         var enumerator = StringInfo.GetTextElementEnumerator(diffText);
         var clusters = new List<string>();

         while (enumerator.MoveNext()) {
            clusters.Add((string)enumerator.Current);
         }

         Console.WriteLine();
         Console.WriteLine(label);

         const int BytesPerLine = 16;

         for (int i = 0; i < clusters.Count; i += (BytesPerLine * 2)) {
            var clusterLine = clusters.Skip(i).Take(BytesPerLine * 2).ToList();

            Console.Write($"{i / 2:x4} ");

            for (int j = 0; j < clusterLine.Count; j += 2) {
               var hex = string.Join("", clusterLine.Skip(j).Take(2));

               Console.Write($"{hex} ");
            }

            Console.Write("  ");

            for (
               int j = i / 2, k = 0; 
               j < sourceBytes.Length && k < BytesPerLine; 
               j++, k++
            ) {
               var c = sourceBytes[j];

               if (c is > 31 and < 127) {
                  Console.Write((char)c);
               } else {
                  Console.Write('.');
               }
            }

            Console.WriteLine();
         }
      }

      string ToHexString(byte[] bytes) {
         return BitConverter.ToString(bytes).Replace("-", "");
      }
   }


}
