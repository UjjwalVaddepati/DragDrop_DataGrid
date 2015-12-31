using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using BindableColumn.Model;
using BindableColumn.ViewModel;

namespace BindableColumn
{
    public class CustomBoundColumn : DataGridTemplateColumn
    {
        public DataTemplate AttachedCellTemplate { get; set; }
        public DataTemplate AttachedCellEditingTemplate { get; set; }
        public MappedValueCollection MappedValueCollection { get; set; }
       
        protected override FrameworkElement GenerateElement(DataGridCell cell, object dataItem)
        {
            var content = new ContentControl();
            MappedValue context = MappedValueCollection.ReturnIfExistAddIfNot(cell.Column.Header, dataItem);
            var binding = new Binding() { Source = context };
            content.ContentTemplate = cell.IsEditing ? CellTemplate : CellTemplate;
            content.SetBinding(ContentControl.ContentProperty, binding);           
            return content;
        }

        protected override FrameworkElement GenerateEditingElement(DataGridCell cell, object dataItem)
        {
            return GenerateElement(cell, dataItem);
        }
    }
}
