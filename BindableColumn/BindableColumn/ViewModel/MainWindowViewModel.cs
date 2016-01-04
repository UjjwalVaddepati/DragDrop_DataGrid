using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using BindableColumn.Model;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

namespace BindableColumn.ViewModel
{
    public class MainWindowViewModel : ViewModelBase
    {
        ObservableCollection<RowViewModel> _rowCollection;
        public ObservableCollection<RowViewModel> RowCollection
        {
            get { return _rowCollection; }
            set
            {
                if (_rowCollection != value)
                {
                    _rowCollection = value;
                    base.RaisePropertyChanged(() => this.RowCollection);
                }
            }
        }

        ObservableCollection<ColumnsViewModel> _columnCollection;
        public ObservableCollection<ColumnsViewModel> ColumnsCollection
        {
            get { return _columnCollection; }
            set
            {
                if (_columnCollection != value)
                {
                    _columnCollection = value;
                    base.RaisePropertyChanged(() => this.ColumnsCollection);
                }
            }
        }

        MappedValueCollection _rowColumnValues;
        public MappedValueCollection RowColumnValues
        {
            get { return _rowColumnValues; }
            set
            {
                if (_rowColumnValues != value)
                {
                    _rowColumnValues = value;
                    base.RaisePropertyChanged(() => this.RowColumnValues);
                }
            }
        }


        object _selectedItem;

        public object SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                _selectedItem = value;
                base.RaisePropertyChanged(() => this.SelectedItem);
            }
        }

        object _selectedCell;

        public object SelectedCell
        {
            get { return _selectedCell; }
            set
            {
                _selectedCell = value;
                base.RaisePropertyChanged(() => this.SelectedCell);
            }
        }

        public MainWindowViewModel()
        {
            this.RowColumnValues = new MappedValueCollection();
            this.ColumnsCollection = new ObservableCollection<ColumnsViewModel>();
            this.RowCollection = new ObservableCollection<RowViewModel>();

            // Columns

            #region Columns
            this.ColumnsCollection.Add(new ColumnsViewModel(new Column() { Currency = "01" }));
            this.ColumnsCollection.Add(new ColumnsViewModel(new Column() { Currency = "02" }));
            this.ColumnsCollection.Add(new ColumnsViewModel(new Column() { Currency = "03" }));
            this.ColumnsCollection.Add(new ColumnsViewModel(new Column() { Currency = "04" }));
            this.ColumnsCollection.Add(new ColumnsViewModel(new Column() { Currency = "05" }));
            this.ColumnsCollection.Add(new ColumnsViewModel(new Column() { Currency = "06" }));
            this.ColumnsCollection.Add(new ColumnsViewModel(new Column() { Currency = "07" }));
            this.ColumnsCollection.Add(new ColumnsViewModel(new Column() { Currency = "08" }));
            this.ColumnsCollection.Add(new ColumnsViewModel(new Column() { Currency = "09" }));
            this.ColumnsCollection.Add(new ColumnsViewModel(new Column() { Currency = "10" }));
            this.ColumnsCollection.Add(new ColumnsViewModel(new Column() { Currency = "11" }));
            this.ColumnsCollection.Add(new ColumnsViewModel(new Column() { Currency = "12" }));
            
            #endregion

            #region Rows
            // Rows
            RowCollection.Add(new RowViewModel(new Row() { Name = "A" }));
            RowCollection.Add(new RowViewModel(new Row() { Name = "B" }));
            RowCollection.Add(new RowViewModel(new Row() { Name = "C" }));
            RowCollection.Add(new RowViewModel(new Row() { Name = "D" }));
            RowCollection.Add(new RowViewModel(new Row() { Name = "E" }));
            RowCollection.Add(new RowViewModel(new Row() { Name = "F" }));
            RowCollection.Add(new RowViewModel(new Row() { Name = "G" }));
            RowCollection.Add(new RowViewModel(new Row() { Name = "H" }));
            RowCollection.Add(new RowViewModel(new Row() { Name = "I" }));
            RowCollection.Add(new RowViewModel(new Row() { Name = "J" }));
            RowCollection.Add(new RowViewModel(new Row() { Name = "K" }));
            #endregion

            #region Cells
            // Cell Values
            RowColumnValues.Add(new MappedValue
            {
                Value = new CellData() { ColorName = "Data 8", IsCellHit = true },
                ColumnBinding = ColumnsCollection.Where(x => x.Currency.Equals("02")).FirstOrDefault(),
                RowBinding = RowCollection.Where(x => x.Name.Equals("B")).FirstOrDefault()
            });

            RowColumnValues.Add(new MappedValue
            {
                Value = new CellData() { ColorName = "Data 1" },
                ColumnBinding = ColumnsCollection.Where(x => x.Currency.Equals("05")).FirstOrDefault(),
                RowBinding = RowCollection.Where(x => x.Name.Equals("C")).FirstOrDefault()
            });


            RowColumnValues.Add(new MappedValue
            {
                Value = new CellData() { ColorName = "Data 2" },
                ColumnBinding = ColumnsCollection.Where(x => x.Currency.Equals("06")).FirstOrDefault(),
                RowBinding = RowCollection.Where(x => x.Name.Equals("D")).FirstOrDefault()
            });

            RowColumnValues.Add(new MappedValue
            {
                Value = new CellData() { ColorName = "Data 3" },
                ColumnBinding = ColumnsCollection.Where(x => x.Currency.Equals("08")).FirstOrDefault(),
                RowBinding = RowCollection.Where(x => x.Name.Equals("E")).FirstOrDefault()
            });

            RowColumnValues.Add(new MappedValue
            {
                Value = new CellData() { ColorName = "Data 4" },
                ColumnBinding = ColumnsCollection.Where(x => x.Currency.Equals("08")).FirstOrDefault(),
                RowBinding = RowCollection.Where(x => x.Name.Equals("C")).FirstOrDefault()
            });

            RowColumnValues.Add(new MappedValue
            {
                Value = new CellData() { ColorName = "Data 5" },
                ColumnBinding = ColumnsCollection.Where(x => x.Currency.Equals("08")).FirstOrDefault(),
                RowBinding = RowCollection.Where(x => x.Name.Equals("H")).FirstOrDefault()
            });

            RowColumnValues.Add(new MappedValue
            {
                Value = new CellData() { ColorName = "Data 6" },
                ColumnBinding = ColumnsCollection.Where(x => x.Currency.Equals("08")).FirstOrDefault(),
                RowBinding = RowCollection.Where(x => x.Name.Equals("F")).FirstOrDefault()
            });

            RowColumnValues.Add(new MappedValue
            {
                Value = new CellData() { ColorName = "Data 7" },
                ColumnBinding = ColumnsCollection.Where(x => x.Currency.Equals("08")).FirstOrDefault(),
                RowBinding = RowCollection.Where(x => x.Name.Equals("A")).FirstOrDefault()
            });

            #endregion

        }

        public ICommand RemoveCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    this.ColumnsCollection.RemoveAt(this.ColumnsCollection.Count - 1);
                });
            }
        }
        public ICommand AddCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    this.ColumnsCollection.Add(new ColumnsViewModel(new Column() { SupplierId = 1, Currency = "PHP", Location = "Philippines", SupplierName = "Bench" }));
                });
            }
        }

    }
}
