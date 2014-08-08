using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CsvHelper;
using Lucene.Net.Analysis;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Lucene.Net.Store;

using System.Timers;
using System.Windows;
using System.Diagnostics;

namespace KeymatchTest.Model
{
    public class TestLuceneIndex2 : DependencyObject, IDisposable
    {
        public TestLuceneIndex2() 
        {
            keymatchEntries = new List<KeyMatch>();
            indexWriter = new IndexWriter(new RAMDirectory(), new KeyMatchAnalyzer(), IndexWriter.MaxFieldLength.UNLIMITED);
        }

        IndexWriter indexWriter;
        private List<KeyMatch> keymatchEntries;

        private const string QUERY_FIELD = "query";

        /// <summary>
        /// Reads the KeyMatch data from the given path.
        /// </summary>
        /// <param name="path">Path to CSV for KeyMatch data</param>
        public void ReadKeyMatchData(string path)
        {
            var csv = new CsvReader(new StreamReader(path));
            csv.Configuration.RegisterClassMap<KeyMatchClassMap>();
            keymatchEntries = csv.GetRecords<KeyMatch>().ToList();
        }

        public List<KeyMatch> Query(string query)
        {
            Runtime = Stopwatch.StartNew();

            var matchList = new List<KeyMatch>();
            
            var document = new Document();
            document.Add(new Field(QUERY_FIELD, query, Field.Store.YES, Field.Index.ANALYZED_NO_NORMS));
            indexWriter.AddDocument(document);

            var searcher = new IndexSearcher(indexWriter.GetReader());

            foreach (var keymatchEntry in keymatchEntries)
            {
                bool matchFound = false;

                switch (keymatchEntry.KeyMatchType)
                {
                    case KeyMatchType.KeywordMatch:
                        matchFound = checkForMatch(searcher, makeKeywordTypeQuery(keymatchEntry.Key));
                        break;
                    case KeyMatchType.PhraseMatch:
                        matchFound = checkForMatch(searcher, makePhraseTypeQuery(keymatchEntry.Key));
                        break;
                    case KeyMatchType.ExactMatch:
                        // Since it must be the exact string then just do a string compare
                        matchFound = keymatchEntry.Key.Equals(query, StringComparison.InvariantCultureIgnoreCase);
                        break;
                }

                if (matchFound)
                {
                    matchList.Add(keymatchEntry);
                }
            }

            indexWriter.DeleteAll();
            
            Runtime.Stop();

            return matchList;
        }

        private BooleanQuery makeKeywordTypeQuery(string queryString)
        {
            var query = new BooleanQuery();
            var splitQuery = queryString.ToLower().Split(' ');
            foreach (var queryPiece in splitQuery)
            {
                query.Add(new BooleanClause(new TermQuery(new Term(QUERY_FIELD, queryPiece)), Occur.MUST));
            }
            return query;
        }

        private PhraseQuery makePhraseTypeQuery(string queryString)
        {
            // slop = 0 must be exact phrase
            var query = new PhraseQuery() { Slop = 0 };
            var splitQuery = queryString.ToLower().Split(' ');
            foreach (var queryPiece in splitQuery)
            {
                query.Add(new Term(QUERY_FIELD, queryPiece));
            }
            return query;
        }

        private bool checkForMatch(IndexSearcher searcher, Query query)
        {
            return searcher.Search(query, null, int.MaxValue).ScoreDocs.Any();
        }

        public List<KeyMatch> GetAllKeyMatches()
        {
            return keymatchEntries;
        }

        public Stopwatch Runtime
        {
            get { return (Stopwatch)GetValue(RuntimeProperty); }
            set { SetValue(RuntimeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Runtime.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RuntimeProperty =
            DependencyProperty.Register("Runtime", typeof(Stopwatch), typeof(TestLuceneIndex2), new PropertyMetadata(new Stopwatch()));

        public void Dispose()
        {
            if (indexWriter != null)
            {
                indexWriter.Dispose();
            }

            GC.SuppressFinalize(this);
        }
    }
}
