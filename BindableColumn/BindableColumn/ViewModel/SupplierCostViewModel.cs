using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BindableColumn.Model;
using GalaSoft.MvvmLight;

namespace BindableColumn.ViewModel
{
    public class SupplierCostViewModel :ViewModelBase
    {
        SupplierCost model;

        public SupplierCostViewModel(SupplierCost model)
        {
            this.model = model;
        }

        public int SupplierId
        {
            get
            {
                return model.SupplierId; 
            }
            set
            {
                model.SupplierId = value;
                base.RaisePropertyChanged(() => this.SupplierId);
            }
        }

        public int CostId
        {
            get
            {
                return model.CostId;
            }
            set
            {
                model.CostId = value;
                base.RaisePropertyChanged(() => this.CostId);
            }
        }

        public double? Value
        {
            get
            {
                return model.Value;
            }
            set
            {
                model.Value = value;
                base.RaisePropertyChanged(() => this.Value);
            }
        }
    }
}
