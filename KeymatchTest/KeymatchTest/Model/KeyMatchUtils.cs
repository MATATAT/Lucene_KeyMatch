using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CsvHelper.Configuration;
using Lucene.Net.Analysis;

namespace KeymatchTest.Model
{
    public class KeyMatch
    {
        public string Key { get; set; }

        public KeyMatchType KeyMatchType { get; set; }

        public string Url { get; set; }

        public string Title { get; set; }
    }

    public class KeyMatchClassMap : CsvClassMap<KeyMatch>
    {
        public KeyMatchClassMap()
        {
            Map(m => m.Key).Index(0);
            Map(m => m.KeyMatchType).Index(1).ConvertUsing(row => (KeyMatchType)Enum.Parse(typeof(KeyMatchType), row.GetField(1)));
            Map(m => m.Url).Index(2);
            Map(m => m.Title).Index(3);
        }
    }

    public enum KeyMatchType
    {
        KeywordMatch,
        PhraseMatch,
        ExactMatch
    }

    public class KeyMatchAnalyzer : Analyzer
    {
        public override TokenStream TokenStream(string fieldName, System.IO.TextReader reader)
        {
            return new LowerCaseFilter(new WhitespaceTokenizer(reader));
        }
    }

}
