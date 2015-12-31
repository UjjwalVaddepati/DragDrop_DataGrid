using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BindableColumn.Model;

namespace BindableColumn.Model
{
    public class Column
    {
        public int SupplierId { get; set; }
        public string SupplierName { get; set; }
        public string Currency { get; set; }
        public string Location { get; set; }
    }
}
