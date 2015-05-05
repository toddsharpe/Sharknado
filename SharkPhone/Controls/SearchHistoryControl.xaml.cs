using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using SharkPhone.Models;

namespace SharkPhone.Controls
{
    public partial class SearchHistoryControl : UserControl
    {
        private const string SearchHistoryFileName = "SearchHistory.xml";
        private const int MaxHistorySize = 10;

        private ObservableCollection<Search> _history;

        //public static readonly DependencyProperty SearchControlProperty = DependencyProperty.Register("SearchControl", typeof(SearchControl), typeof(SearchHistoryControl), new PropertyMetadata(null));
        private SearchControl _searchControl;
        public SearchControl SearchControl
        {
            get
            {
                return _searchControl;
            }
            set
            {
                if (_searchControl == value)
                    return;

                if (_searchControl != null)
                    _searchControl.OnSearch -= searchControl_OnSearch;
                _searchControl = value;
                _searchControl.OnSearch += searchControl_OnSearch;
            }
        }

        public SearchHistoryControl()
        {
            InitializeComponent();
        }

        void searchControl_OnSearch(object sender, Search e)
        {
            int index = _history.IndexOf(e);
            if (index >= 0)
                _history.RemoveAt(index);

            _history.Insert(0, e);

            if (_history.Count > MaxHistorySize)
                _history.RemoveAt(MaxHistorySize);
            
            Utilities.SaveObject(SearchHistoryFileName, _history);
        }

        private void SearchHistoryControl_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (_history == null)
            {
                _history = Utilities.LoadObject<ObservableCollection<Search>>(SearchHistoryFileName, true);
                LongListSelector.ItemsSource = _history;
            }
        }

        private void LongListSelector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0)
                return;

            Search selected = e.AddedItems[0] as Search;
            if (selected == null)
                return;

            SearchControl.ExecuteSearch(selected.Query, selected.Type);
            LongListSelector.SelectedItem = null;
        }
    }
}
