using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BindableColumn.Model;
using GalaSoft.MvvmLight;

namespace BindableColumn.ViewModel
{
    public class ColumnsViewModel : ViewModelBase
    {
        Column model; 

        public ColumnsViewModel(Column model)
        {
            this.model = model; 
        }

        public int SupplierId
        {
            get { return model.SupplierId; }
            set
            {
                if (model.SupplierId != value)
                {
                    model.SupplierId = value;
                    base.RaisePropertyChanged(() => this.SupplierId);
                }
            }
        } 

        public string SupplierName
        {
            get { return model.SupplierName; }
            set
            {
                if (model.SupplierName != value)
                {
                    model.SupplierName = value;
                    base.RaisePropertyChanged(() => this.SupplierName);
                }
            }
        }
        
        public string Currency
        {
            get { return model.Currency; }
            set
            {
                if (model.Currency != value)
                {
                    model.Currency = value;
                    base.RaisePropertyChanged(() => this.Currency);
                }
            }
        }

        public string Location
        {
            get
            {
                return model.Location; 
            }
            set
            {
                model.Location = value;
                base.RaisePropertyChanged(() => this.Location);
            }
        }
    }
}
