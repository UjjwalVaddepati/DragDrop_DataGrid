using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BindableColumn.Model
{
    public class SupplierCostData
    {
        List<SupplierCost> costTable;

        public SupplierCostData()
        {
            costTable = new List<SupplierCost>();
            
            costTable.Add(new SupplierCost() { CostId = 1, SupplierId = 1, Value = 105.23}); 
            costTable.Add(new SupplierCost() { CostId = 1, SupplierId = 2, Value = 110.548 }); 
            costTable.Add(new SupplierCost() { CostId = 1, SupplierId = 3, Value = 120.421 });
            costTable.Add(new SupplierCost() { CostId = 1, SupplierId = 4, Value = 100.4 }); 
            
            costTable.Add(new SupplierCost() { CostId = 2, SupplierId = 1, Value = 52.3 });
            costTable.Add(new SupplierCost() { CostId = 2, SupplierId = 2, Value = 87.6 });
            costTable.Add(new SupplierCost() { CostId = 2, SupplierId = 3, Value = 98.5 });
            costTable.Add(new SupplierCost() { CostId = 2, SupplierId = 4, Value = 200 });
            
            costTable.Add(new SupplierCost() { CostId = 3, SupplierId = 1, Value = 89.65 });
            costTable.Add(new SupplierCost() { CostId = 3, SupplierId = 2, Value = 20.6 });
            costTable.Add(new SupplierCost() { CostId = 3, SupplierId = 3, Value = 12 });
            costTable.Add(new SupplierCost() { CostId = 3, SupplierId = 4, Value = 22.5 });
        }

        public double? GetCost(int costId, int supplierId)
        {
            return costTable.Where(x => x.CostId == costId && x.SupplierId == supplierId).SingleOrDefault().Value; 
        }

        public void SetCost(int costId, int supplierId, double value)
        {
            costTable.Where(x => x.CostId == costId && x.SupplierId == supplierId).SingleOrDefault().Value = value; 
        }
    }
}
