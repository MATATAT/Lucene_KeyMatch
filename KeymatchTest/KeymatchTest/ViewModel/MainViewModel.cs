using GalaSoft.MvvmLight;
using KeymatchTest.Model;
using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using GalaSoft.MvvmLight.Command;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;

namespace KeymatchTest.ViewModel
{
    public sealed class MainViewModel : ViewModelBase, IDisposable
    {
        public TestLuceneIndex2 TestIndex { get; private set; }

        public RelayCommand OnSelectFile { get; private set; }
        public RelayCommand OnSearch { get; private set; }
        public RelayCommand OnShowAll { get; private set; }

        public MainViewModel()
        {
            TestIndex = new TestLuceneIndex2();

            OnSelectFile = new RelayCommand(SelectFile);
            OnSearch = new RelayCommand(Search);
            OnShowAll = new RelayCommand(ShowAll);

            SearchRuntime = new Stopwatch();

        }

        private void ShowAll()
        {
            Keymatches = TestIndex.GetAllKeyMatches();
        }

        public void SelectFile()
        {
            var dialog = new OpenFileDialog() { Filter = "CSV Files (.csv)|*.csv" };

            if (dialog.ShowDialog().Value)
            {
                OpenFile = dialog.FileName;
                TestIndex.ReadKeyMatchData(dialog.FileName);

                ShowAll();
            }
        }

        public void Search()
        {
            SearchRuntime = Stopwatch.StartNew();

            Keymatches = TestIndex.Query(QueryString);

            SearchRuntime.Stop();
        }

        private string queryString;
        public string QueryString
        {
            get { return queryString; }
            set 
            { 
                queryString = value;
                RaisePropertyChanged("QueryString");
            }
        }

        private List<KeyMatch> keymatches;
        public List<KeyMatch> Keymatches
        {
            get { return keymatches; }
            set
            {
                keymatches = value;
                RaisePropertyChanged("Keymatches");
            }
        }

        private string openFile;
        public string OpenFile
        {
            get { return openFile; }
            set 
            { 
                openFile = value;
                RaisePropertyChanged("OpenFile");
            }
        }

        private Stopwatch searchRuntime;
        public Stopwatch SearchRuntime
        {
            get { return searchRuntime; }
            set 
            { 
                searchRuntime = value;
                RaisePropertyChanged("SearchRuntime");
            }
        }
        

        public void Dispose()
        {
            TestIndex.Dispose();
        }
    }
}