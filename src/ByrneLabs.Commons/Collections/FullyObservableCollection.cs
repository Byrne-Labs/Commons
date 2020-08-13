using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace ByrneLabs.Commons.Collections
{
    public class FullyObservableCollection<T> : ObservableCollection<T> where T : INotifyPropertyChanged
    {
        public FullyObservableCollection(IEnumerable<T> items) : base(items)
        {
            InitializeCollection();
        }

        public FullyObservableCollection()
        {
            InitializeCollection();
        }

        private void InitializeCollection()
        {
            CollectionChanged += (sender, args) =>
            {
                if (args.NewItems != null)
                {
                    foreach (var item in args.NewItems.Cast<INotifyPropertyChanged>())
                    {
                        item.PropertyChanged += ItemOnPropertyChanged;
                    }
                }

                if (args.OldItems != null)
                {
                    foreach (var item in args.OldItems.Cast<INotifyPropertyChanged>())
                    {
                        item.PropertyChanged -= ItemOnPropertyChanged;
                    }
                }
            };

            foreach (var item in this)
            {
                item.PropertyChanged += ItemOnPropertyChanged;
            }
        }

        private void ItemOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, sender, sender, IndexOf((T) sender));
            OnCollectionChanged(args);
        }
    }
}
