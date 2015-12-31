using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using BindableColumn.Model;

namespace BindableColumn
{
    public class RowAndColumnMultiValueConverter : IMultiValueConverter
    { 
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            SupplierCostData dummyData = new SupplierCostData();

            //double? val = dummyData.GetCost((int)RowValue, (int)ColumnValue);

            return "";//string.Format("{0}", val.Value);   
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }        
    }
}
