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
using NLog;
using Renfrew.Grammar.FluentApi;
using Renfrew.Grammar.FluentApi.ExpressionParts.SequenceMembers;
using Renfrew.Grammar.FluentApi.Interfaces;
using Renfrew.Grammar.Serialization.LowLevelTypes;
using Renfrew.Grammar.Types;
using Renfrew.NatSpeakInterop;

namespace Renfrew.Grammar.Serialization {
   public class Serializer : IGrammarSerializer {
      private static Logger _logger = LogManager.GetCurrentClassLogger();

      #region Speech Recognition Constants

      internal enum HeaderTypes : uint {
         Cfg = 0,
         Dictation = 2,
      }

      internal enum SrHeaderFlags : uint {
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

      public Serializer() { }

      internal SrChunk CreateExportRulesNamesChunk(
         IReadOnlyList<IRule> exportRules
      ) {
         return CreateStringChunk(ChunkType.ExportRules, exportRules);
      }

      internal SrChunk CreateImportRulesNamesChunk(
         IReadOnlyList<IRule> importRules
      ) {
         return CreateStringChunk(ChunkType.ImportRules, importRules);
      }

      internal SrChunk CreateWordsChunk(IReadOnlyList<Word> words) {
         return CreateStringChunk(ChunkType.Words, words);
      }

      internal SrChunk CreateStringChunk<T>(
         ChunkType chunkType,
         IReadOnlyList<T> idStrings
      )
         where T : IIdString {
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
                     RuleNumber = idString.Id,
                     String = idString.String,
                  }
               )
               .ToList()
         };
      }

      internal SrChunk CreateRulesChunk(IReadOnlyList<IRule> exportedRules) {
         if (!exportedRules.Any()) {
            return null;
         }

         var ruleData = new RuleConverter().Convert(exportedRules);

         return new SrChunk {
            ChunkId = (uint) ChunkType.Rules,
            Rules = ruleData.Select(
                  rule => (ISerializableRule) new SrCfgRule {
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
            CreateExportRulesNamesChunk(grammar.ExportedRules),
            CreateImportRulesNamesChunk(grammar.ImportedRules),
            CreateWordsChunk(grammar.Words),
            CreateRulesChunk(grammar.ExportedRules)
         };

         return (
            new SrHeader {
               Type = (uint) HeaderTypes.Cfg,
               Flags = (uint) SrHeaderFlags.Unicode,
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
