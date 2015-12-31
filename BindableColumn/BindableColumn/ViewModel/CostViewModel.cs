using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BindableColumn.Model;
using GalaSoft.MvvmLight;

namespace BindableColumn.ViewModel
{
    public class RowViewModel : ViewModelBase
    {
        Row model; 

        public RowViewModel(Row model)
        {
            this.model = model;
        }

        public int CostId
        {
            get { return model.CostId; }
            set
            {
                if (model.CostId != value)
                {
                    model.CostId = value;
                    base.RaisePropertyChanged(() => this.CostId);
                }
            }
        }      
                  
        public string Name
        {
            get { return model.Name; }
            set
            {
                if (model.Name != value)
                {
                    model.Name = value;
                    base.RaisePropertyChanged(() => this.Name);
                }
            }
        } 
        
   
    }
}
