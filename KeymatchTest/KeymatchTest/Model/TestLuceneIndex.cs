using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using Lucene.Net.Analysis;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Store;

namespace KeymatchTest.Model
{
    public sealed class TestLuceneIndex : IDisposable
    {
        public TestLuceneIndex() { }

        private IndexWriter indexWriter;

        private const string KEY_FIELD = "key";
        private const string KEYMATCH_FIELD = "keymatch";
        private const string TITLE_FIELD = "title";
        private const string URL_FIELD = "url";

        /// <summary>
        /// Builds Lucene index out of KeyMatch data
        /// </summary>
        /// <param name="path">Path to CSV for KeyMatch data</param>
        public void BuildLuceneIndex(string path)
        {
            if (indexWriter != null)
            {
                indexWriter.Dispose();
            }
            indexWriter = new IndexWriter(new RAMDirectory(), null, IndexWriter.MaxFieldLength.UNLIMITED);

            var csv = new CsvReader(new StreamReader(path));
            csv.Configuration.RegisterClassMap<KeyMatchClassMap>();

            foreach (var keymatch in csv.GetRecords<KeyMatch>())
            {
                var doc = new Document();
                // Key
                doc.Add(new Field(KEY_FIELD, keymatch.Key, Field.Store.YES, Field.Index.ANALYZED));
                // Keymatch Type
                doc.Add(new Field(KEYMATCH_FIELD, keymatch.KeyMatchType.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED_NO_NORMS));
                // Title
                doc.Add(new Field(TITLE_FIELD, keymatch.Title, Field.Store.YES, Field.Index.NOT_ANALYZED_NO_NORMS));
                // Url
                doc.Add(new Field(URL_FIELD, keymatch.Url, Field.Store.YES, Field.Index.NOT_ANALYZED_NO_NORMS));

                // Need to add analyzer
                indexWriter.AddDocument(doc, ((keymatch.KeyMatchType == KeyMatchType.ExactMatch) ? (Analyzer)new ExactMatchAnalyzer() : new SimpleAnalyzer()));
            }
        }

        public List<KeyMatch> Query(string query)
        { 
            // Needs to return something but not sure what
            return null;
        }

        public List<KeyMatch> GetAllDocs()
        {
            var searcher = new IndexSearcher(indexWriter.GetReader());
            var results = searcher.Search(new MatchAllDocsQuery(), null, int.MaxValue);
            var docs = results.ScoreDocs.Select(doc => searcher.Doc(doc.Doc));

            return (from doc in docs
                    select new KeyMatch
                    {
                        Key = doc.Get(KEY_FIELD),
                        KeyMatchType = (KeyMatchType)Enum.Parse(typeof(KeyMatchType), doc.Get(KEYMATCH_FIELD)),
                        Title = doc.Get(TITLE_FIELD),
                        Url = doc.Get(URL_FIELD)
                    }).ToList();
        }

        public void Dispose()
        {
            indexWriter.Dispose();
        }
    }

    /// <summary>
    /// Analyzer that only lowercases streams, no tokenizing.
    /// </summary>
    public class ExactMatchAnalyzer : Analyzer
    {
        public override TokenStream TokenStream(string fieldName, TextReader reader)
        {
            return new LowerCaseTokenizer(reader);
        }
    }
}
