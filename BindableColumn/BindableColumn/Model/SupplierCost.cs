using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BindableColumn.Model
{
    public class SupplierCost
    {
        public int SupplierId { get; set; }
        public int CostId { get; set; }
        public double? Value { get; set; }
    }
}
