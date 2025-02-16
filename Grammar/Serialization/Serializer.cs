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
using System.IO;
using System.Linq;
using System.Text;
using NLog;
using Renfrew.Grammar.FluentApi;
using Renfrew.Grammar.Serialization.LowLevelTypes;
using Renfrew.NatSpeakInterop;

namespace Renfrew.Grammar.Serialization {
   public class Serializer : IGrammarSerializer {
      private static Logger _logger = LogManager.GetCurrentClassLogger();

      #region Speech Recognition Constants

      internal enum HeaderTypes : uint {
         Cfg = 0,
         Dictation = 2,
      }

      internal enum NameEncoding : uint {
         Ascii = 0,
         Unicode = 1,
      }

      internal enum ChunkType : uint {
         Language = 1,
         Words = 2,
         Rules = 3,
         ExportRules = 4,
         ImportRules = 5,
      }

      #endregion

      private readonly NameEncoding _encoding;

      public Serializer() : this(useUnicode: true) { }

      public Serializer(bool useUnicode) {
         _encoding = useUnicode ? NameEncoding.Unicode : NameEncoding.Ascii;
      }

      internal SrChunk SerializeExportRules(Grammar grammar) {
         return SerializeStrings(ChunkType.ExportRules, grammar.ExportedRules);
      }

      internal SrChunk SerializeImportRules(Grammar grammar) {
         return SerializeStrings(ChunkType.ImportRules, grammar.ImportedRules);
      }

      internal SrChunk SerializeWords(Grammar grammar) {
         return SerializeStrings(ChunkType.Words, grammar.Words);
      }

      internal SrChunk SerializeStrings<T>(
         ChunkType chunkType,
         IReadOnlyList<T> idStrings
      )
         where T: IIdString {
         if (chunkType == ChunkType.Rules) {
            throw new ArgumentException(
               "Cannot use the Rules chunk type here."
            );
         }

         if (!idStrings.Any()) {
            return null;
         }

         return new SrChunk {
            ChunkId = (uint) chunkType,
            Rules = idStrings.Select(
                  idString => (ISerializableRule) new SrCfgXRule {
                     //Size =
                     //   GetPaddedStringLength(idString.String) + SrCfgXRuleSize, // TODO: Infer value
                     RuleNumber = idString.Id,
                     String = idString.String,
                     EncodeAsUnicode = _encoding == NameEncoding.Unicode,
                  }
               )
               .ToList()
         };
      }

      internal SrChunk SerializeRules(Grammar grammar) {
         if (!grammar.AllRules.Any()) {
            return null;
         }

         var ruleData = new RuleConverter().Convert(grammar);

         return new SrChunk {
            ChunkId = (uint) ChunkType.Rules,
            Rules = ruleData.Select(
                  rule => (ISerializableRule) new SrCfgRule {
                     //Size = (uint) rule.Symbols.Count * SrCfgRuleSize, TODO: Infer value
                     UniqueId = rule.Id,
                     Symbols = rule.Symbols.Select(
                           symbol => new SrCfgSymbol {
                              Type = (ushort) symbol.Type,
                              Probability = symbol.Probability,
                              Value = symbol.Value
                           }
                        )
                        .ToList(),
                  }
               )
               .ToList()
         };
      }

      internal (SrHeader Header, List<SrChunk> Chunks)
         CreateDataStructures(Grammar grammar) {
         var chunks = new List<SrChunk> {
            SerializeExportRules(grammar),
            SerializeImportRules(grammar),
            SerializeWords(grammar),
            SerializeRules(grammar)
         };

         return (
            new SrHeader {
               Type = (uint) HeaderTypes.Cfg,
               Flags = (uint) _encoding,
            },
            chunks.Where(chunk => chunk != null).ToList()
         );
      }

      public byte[] Serialize(IGrammar iGrammar) {
         var grammar = (Grammar) iGrammar; // FIXME: This doesn't feel right.

         var dataStructures = CreateDataStructures(grammar);

         var writer = new BinaryWriter(new MemoryStream());

         dataStructures.Header.Serialize(writer);
         dataStructures.Chunks.ForEach(chunk => chunk.Serialize(writer));

         writer.Flush();

         try {
            return (writer.BaseStream as MemoryStream)!.ToArray();
         } finally {
            writer.Dispose();
         }
      }
   }
}
