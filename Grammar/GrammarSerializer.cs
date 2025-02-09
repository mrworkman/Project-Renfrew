// Project Renfrew
// Copyright(C) 2017 Stephen Workman (workman.stephen@gmail.com)
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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NLog;
using Renfrew.Grammar.Dragon.SpeechRecognition;
using Renfrew.Grammar.FluentApi;
using Renfrew.NatSpeakInterop;

namespace Renfrew.Grammar {
   public class GrammarSerializer : IGrammarSerializer {
      private static Logger _logger = LogManager.GetCurrentClassLogger();

      #region Speech Recognition Constants

      private enum HeaderTypes : uint {
         Cfg = 0,
         Dictation = 2,
      }

      private enum NameEncoding : uint {
         Ascii = 0,
         Unicode = 1,
      }

      private enum ChunkType : uint {
         Language = 1,
         Words = 2,
         Rules = 3,
         ExportRules = 4,
         ImportRules = 5,
      }

      #endregion

      private readonly NameEncoding _encoding;

      public GrammarSerializer() : this(useUnicode: true) { }

      public GrammarSerializer(bool useUnicode) {
         _encoding = useUnicode ? NameEncoding.Unicode : NameEncoding.Ascii;
      }

      private int GetPaddedStringLength(string s) {
         var numBytes = _encoding == NameEncoding.Unicode ?
            Encoding.Unicode.GetByteCount(s) + 2 :
            Encoding.ASCII.GetByteCount(s) + 1;

         // Pad to 4-byte boundary.
         return (numBytes + 3) & ~3;
      }

      private void SerializeExportRules(
         Grammar grammar,
         BinaryWriter stream
      ) {
         SerializeStrings(
            ChunkType.ExportRules,
            grammar.ExportedRules,
            stream
         );
      }

      private void SerializeImportRules(
         Grammar grammar,
         BinaryWriter stream
      ) {
         SerializeStrings(
            ChunkType.ImportRules,
            grammar.ImportedRules,
            stream
         );
      }

      private void SerializeWords(Grammar grammar, BinaryWriter stream) {
         SerializeStrings(ChunkType.Words, grammar.Words, stream);
      }

      private void SerializeStrings<T>(
         ChunkType chunkType,
         IReadOnlyList<T> names,
         BinaryWriter stream
      )
         where T: IIdString {
         if (chunkType == ChunkType.Rules) {
            throw new ArgumentException(
               "Cannot use the Rules chunk type here."
            );
         }

         if (!names.Any()) {
            return;
         }

         var bytes = SerializeStrings(names);

         // Write SRCHUNK struct.
         stream.Write((uint) chunkType); // dwChunkId
         stream.Write(bytes.Length); // dwChunkSize
         stream.Write(bytes); // avInfo
      }

      private byte[] SerializeStrings<T>(IReadOnlyList<T> idStrings)
         where T: IIdString {
         var memoryStream = new MemoryStream();
         var stream = new BinaryWriter(memoryStream);

         foreach (var idString in idStrings) {
            var name = idString.String;
            var id = idString.Id;

            var length = GetPaddedStringLength(name);

            var nameBytes = _encoding == NameEncoding.Unicode ?
               Encoding.Unicode.GetBytes(name) :
               Encoding.ASCII.GetBytes(name);

            // Write SRCFGXRULE struct.
            stream.Write(length + 8); // dwSize + sizeof(SRCFGXRULE)
            stream.Write(id); // dwRuleNum
            stream.Write(nameBytes); // szString

            // Make sure that the word/rule name is padded to a 4-byte boundary.
            stream.Write(new byte[length - nameBytes.Length]);
         }

         stream.Flush();

         try {
            return memoryStream.ToArray();
         } finally {
            stream.Dispose();
            memoryStream.Dispose();
         }
      }

      private void SerializeRules(Grammar grammar, BinaryWriter stream) {
         if (!grammar.AllRules.Any()) {
            return;
         }

         var bytes = SerializeRules(grammar);

         // Write SRCHUNK struct.
         stream.Write((uint) ChunkType.Rules); // dwChunkId
         stream.Write(bytes.Length); // dwChunkSize
         stream.Write(bytes); // avInfo
      }

      private byte[] SerializeRules(Grammar grammar) {
         var memoryStream = new MemoryStream();
         var stream = new BinaryWriter(memoryStream);

         // TODO:
         var ruleData = new GrammarRuleConverter(grammar).Convert();

         foreach (var rule in ruleData) {
            // The SRCFGRULE struct is 8 bytes long
            const int srCfgRuleSize = 8;

            var length = ruleData.Count * srCfgRuleSize;
            stream.Write(length + srCfgRuleSize); // dwSize
            stream.Write(rule.Id); // dwRuleNum

            // Serialize the rule's symbols.
         }

         // TODO: Refactor
         //var definitionFactory = new RuleDefinitionFactory(new RuleDirectiveFactory());

         //// One "table" per rule...
         //var tables = definitionFactory.CreateDefinitionTables(grammar);

         //Int32 ruleNumber = 1;
         //foreach (var table in tables) {

         //   // The SRCFGRULE struct is 8 bytes long
         //   const int srCfgRuleSize = 8;

         //   var length = table.Count() * srCfgRuleSize;

         //   stream.Write(length + srCfgRuleSize);
         //   stream.Write(ruleNumber);

         //   foreach (var row in table) {
         //      _logger.Trace(row);
         //      Console.WriteLine(row);

         //      stream.Write((UInt16) row.DirectiveType);
         //      stream.Write((UInt16) 0); // Assume probability of Zero

         //      if (row.ElementGrouping == ElementGroupings.NOT_APPLICABLE) {
         //         stream.Write((UInt32) row.Id);
         //      } else {
         //         stream.Write((UInt32) row.ElementGrouping);
         //      }
         //   }

         //   ruleNumber++;
         //}

         try {
            return memoryStream.ToArray();
         } finally {
            stream.Dispose();
            memoryStream.Dispose();
         }
      }

      public byte[] Serialize(IGrammar iGrammar) {
         var grammar = (Grammar) iGrammar; // FIXME: This doesn't feel right.

         var memoryStream = new MemoryStream();
         var stream = new BinaryWriter(memoryStream);

         // Start off with the necessary header and flags
         stream.Write((uint) HeaderTypes.Cfg);
         stream.Write((uint) _encoding);

         SerializeExportRules(grammar, stream);
         SerializeImportRules(grammar, stream);
         SerializeWords(grammar, stream);
         SerializeRules(grammar, stream);

         stream.Flush();

         try {
            return memoryStream.ToArray();
         } finally {
            stream.Dispose();
            memoryStream.Dispose();
         }
      }
   }
}
