using System.Collections.Generic;

namespace GrammarTests.Parsing.Yaml {
    public class TestGrammarSpec {
        public string Name { get; set; }
        public List<TestRuleSpec> Rules { get; set; }
        public TestRuleTests Tests { get; set; }
    }
}
