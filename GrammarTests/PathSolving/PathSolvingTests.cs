using System.Collections.Generic;
using System.IO;
using System.Linq;

using GrammarTests.PathSolving.Yaml;
using GrammarTests.Util;

using Moq;
using NUnit.Framework;
using Renfrew.Grammar;
using Renfrew.Grammar.Collections;
using Renfrew.Grammar.FluentApi;
using Renfrew.Grammar.FluentApi.Interfaces;
using Renfrew.Grammar.Solving;
using Renfrew.NatSpeakInterop;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace GrammarTests.PathSolving {
   public class TestCase {
      public TestGrammar TestGrammar { get; set; }
      public int TestCaseIndex { get; set; }
      public List<SpokenWord> TestCaseWords { get; set; }

      public bool ExpectedSuccess { get; set; }
   }

   public class TestGrammar : Grammar {
      public TestGrammar(
         string name,
         IGrammarService grammarService,
         INatSpeak natSpeak
      ) : base(grammarService, natSpeak) {
         Name = name;
      }

      public TestGrammar(
         string name,
         RuleFactory ruleFactory,
         IIdGenerator idGenerator,
         IGrammarService grammarService,
         INatSpeak natSpeak
      ) : base(ruleFactory, idGenerator, grammarService, natSpeak) {
         Name = name;
      }

      // TODO: Add this to `Grammar` or `IGrammar`.
      public string Name { get; }

      public override void Dispose() { }

      public override void Initialize() { }

      public void AddRuleSpecs(List<TestRuleSpec> ruleSpecs) {
         foreach (var ruleSpec in ruleSpecs) {
            var parser = new RuleStringParser();

            AddRule(ruleSpec.Name, parser.ParseExpression(ruleSpec.Rule));
         }
      }
   }

   [TestFixture]
   public class PathSolvingTests {
      private static readonly List<TestCase> TestCases = LoadTestCases();

      private static IEnumerable<TestCaseData> TestGrammarCases() {
         return TestCases.Select(testCase => {
            var name = testCase.TestGrammar.Name;
            var kind = testCase.ExpectedSuccess ? "good" : "bad ";
            var index = testCase.TestCaseIndex;
            var words = string.Join(
               " ",
               testCase.TestCaseWords.Select(w => $"{w.Word}")
            );

            return new TestCaseData(testCase).SetName(
               $"{name}: {kind} [{index}] {words}"
            );
         }
         );
      }

      private static List<TestCase> LoadTestCases() {
         var deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .Build();

         var directory = Path.Combine(
            Path.GetDirectoryName(typeof(PathSolvingTests).Assembly.Location)
            ?? string.Empty,
            "PathSolving",
            "config"
         );

         var testCases = new List<TestCase>();

         var files = Directory.EnumerateFiles(directory, "*.yaml");

         foreach (var file in files) {
            TestContext.WriteLine($"Loading test data from {file}...");

            var grammarSpecs =
               deserializer.Deserialize<List<TestGrammarSpec>>(
                  File.ReadAllText(file)
               )
               ?? Enumerable.Empty<TestGrammarSpec>();

            // For each grammar spec, create a new grammar                
            foreach (var grammarSpec in grammarSpecs) {
               var grammarServiceMock =
                  new Mock<IGrammarService>(MockBehavior.Loose);
               var natSpeakMock = new Mock<INatSpeak>(MockBehavior.Loose);

               var testGrammar = new TestGrammar(
                  grammarSpec.Name,
                  grammarServiceMock.Object,
                  natSpeakMock.Object
               );

               testGrammar.AddRuleSpecs(grammarSpec.Rules);

               testCases.AddRange(
                  ParseTestCases(grammarSpec.Tests.Good)
                     .Select((testCaseWords, index) => new TestCase {
                        TestGrammar = testGrammar,
                        TestCaseIndex = index,
                        TestCaseWords = testCaseWords,
                        ExpectedSuccess = true
                     }
                     )
               );

               testCases.AddRange(
                  ParseTestCases(grammarSpec.Tests.Bad)
                     .Select((spokenWords, index) => new TestCase {
                        TestGrammar = testGrammar,
                        TestCaseIndex = index,
                        TestCaseWords = spokenWords,
                        ExpectedSuccess = false
                     }
                     )
               );
            }
         }

         return testCases;
      }

      private static List<List<SpokenWord>> ParseTestCases(
         List<string> testCaseStrings
      ) {
         return testCaseStrings.Select(s =>
               s.Split(',')
                  .Select(token => token.Trim())
                  .Where(token => token.Length > 0)
                  .Select(token => token.Split(':', '@'))
                  .Select(parts => new SpokenWord(
                        parts[0],
                        uint.Parse(parts[1]),
                        uint.Parse(parts[2])
                     )
                  )
                  .ToList()
            )
            .ToList();
      }

      [Test]
      [TestCaseSource(nameof(TestGrammarCases))]
      public void PathSolvingTest(TestCase testCase) {
         var phrase = new ListWalker<SpokenWord>(testCase.TestCaseWords);

         var result = Solver.VisitSequence(
            null,
            isTrunkSequence: true,
            phrase: phrase,
            grammar: testCase.TestGrammar
         );

         // var result = solver.Solve(phrase: phrase);

         if (testCase.ExpectedSuccess) {
            Assert.IsInstanceOf<SolveResult.Success>(result);
         } else {
            Assert.IsInstanceOf<SolveResult.Failure>(result);
         }
      }
   }
}
