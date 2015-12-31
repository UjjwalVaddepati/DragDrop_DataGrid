using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;

namespace BindableColumn
{
    public class MappedValue : ViewModelBase
    {
        CellData value;

        public object ColumnBinding { get; set; }

        public object RowBinding { get; set; }

        public CellData Value
        {
            get
            {
                return value;
            }
            set
            {
                if (this.value != value)
                {
                    this.value = value;
                    base.RaisePropertyChanged(() => Value);
                }
            }
        }
    }

    public class CellData : ViewModelBase
    {
        public int SampleID { get; set; }

        public bool IsSelected { get; set; }

        public string Deviation1 { get; set; }

        public string Deviation2 { get; set; }

        public string RawValue { get; set; }

        public string CalculatedValue { get; set; }


        private string _colorName;

        public string ColorName
        {
            get { return _colorName; }
            set
            {
                _colorName = value;
                base.RaisePropertyChanged(() => this.ColorName);
            }
        }


        private bool _isCellHit;

        public bool IsCellHit
        {
            get { return _isCellHit; }
            set
            {
                _isCellHit = value;
                base.RaisePropertyChanged(() => this.IsCellHit);
            }
        }

    }
}
