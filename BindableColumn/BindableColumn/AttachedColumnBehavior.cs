using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using BindableColumn.ViewModel;
using GalaSoft.MvvmLight;

namespace BindableColumn
{
    public class AttachedColumnBehavior
    {        
        public static readonly DependencyProperty AttachedColumnsProperty = 
                DependencyProperty.RegisterAttached("AttachedColumns",
                typeof(IEnumerable),
                typeof(AttachedColumnBehavior),
                new UIPropertyMetadata(null, OnAttachedColumnsPropertyChanged));

        public static readonly DependencyProperty MappedValuesProperty =
                DependencyProperty.RegisterAttached("MappedValues",
                typeof(MappedValueCollection),
                typeof(AttachedColumnBehavior),
                new UIPropertyMetadata(null, OnMappedValuesPropertyChanged));

        public static readonly DependencyProperty SelectedCellsProperty =
                DependencyProperty.RegisterAttached("SelectedCells",
                typeof(MappedValueCollection),
                typeof(AttachedColumnBehavior),
                new UIPropertyMetadata(null, OnMappedValuesPropertyChanged));

        public static readonly DependencyProperty HeaderTemplateProperty = 
                DependencyProperty.RegisterAttached("HeaderTemplate", 
                typeof(DataTemplate),
                typeof(AttachedColumnBehavior),
                new UIPropertyMetadata(null, OnHeaderTemplatePropertyChanged));

        public static readonly DependencyProperty AttachedCellTemplateProperty = 
                DependencyProperty.RegisterAttached("AttachedCellTemplate", 
                typeof(DataTemplate),
                typeof(AttachedColumnBehavior),
                new UIPropertyMetadata(null, OnCellTemplatePropertyChanged));

        public static readonly DependencyProperty AttachedCellEditingTemplateProperty =
        DependencyProperty.RegisterAttached("AttachedCellEditingTemplate",
        typeof(DataTemplate),
        typeof(DataGrid),
        new UIPropertyMetadata(null, OnCellEditingTemplatePropertyChanged));

        private static void OnAttachedColumnsPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            var dataGrid = dependencyObject as DataGrid;
            if (dataGrid == null) return;
            var columns = e.NewValue as INotifyCollectionChanged;
            if (columns != null)
            {
                columns.CollectionChanged += (sender, args) => 
                    {
                        if (args.Action == NotifyCollectionChangedAction.Remove)
                            RemoveColumns(dataGrid, args.OldItems);
                        else if(args.Action == NotifyCollectionChangedAction.Add)
                            AddColumns(dataGrid, args.NewItems);
                    };
                dataGrid.Loaded += (sender, args) => AddColumns(dataGrid, GetAttachedColumns(dataGrid)); 
                var items = dataGrid.ItemsSource as INotifyCollectionChanged;
                if (items != null)
                    items.CollectionChanged += (sender, args) => 
                        {
                            if (args.Action == NotifyCollectionChangedAction.Remove)
                                RemoveMappingByRow(dataGrid, args.NewItems);
                        };
            }           
        }   

        private static void AddColumns(DataGrid dataGrid, IEnumerable columns)
        {
            foreach (var column in columns)
            {
                CustomBoundColumn customBoundColumn = new CustomBoundColumn()
                {
                    Header = column,
                    HeaderTemplate = GetHeaderTemplate(dataGrid),
                    CellTemplate = GetAttachedCellTemplate(dataGrid),                     
                    CellEditingTemplate = GetAttachedCellEditingTemplate(dataGrid), 
                    MappedValueCollection = GetMappedValues(dataGrid)
                };

                dataGrid.Columns.Add(customBoundColumn);
            }
        }

        private static void RemoveColumns(DataGrid dataGrid, IEnumerable columns)
        {
            foreach (var column in columns)
            {
                DataGridColumn col = dataGrid.Columns.Where(x => x.Header == column).Single();
                GetMappedValues(dataGrid).RemoveByColumn(column);
                dataGrid.Columns.Remove(col);                
            }
        }

        private static void RemoveMappingByRow(DataGrid dataGrid, IEnumerable rows)
        {
            if (rows != null)
            {
                foreach (var row in rows)
                {
                    GetMappedValues(dataGrid).RemoveByRow(row);
                }
            }
        }

        #region OnChange handlers
        private static void OnCellTemplatePropertyChanged(DependencyObject depObj, DependencyPropertyChangedEventArgs e)
        {

        }
        private static void OnHeaderTemplatePropertyChanged(DependencyObject depObj, DependencyPropertyChangedEventArgs e)
        {

        }

        private static void OnCellEditingTemplatePropertyChanged(DependencyObject depObj, DependencyPropertyChangedEventArgs e)
        {

        }
        private static void OnMappedValuesPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {

        }
        #endregion


        public static IEnumerable GetAttachedColumns(DependencyObject dataGrid)
        {
            return (IEnumerable)dataGrid.GetValue(AttachedColumnsProperty);
        }

        public static void SetAttachedColumns(DependencyObject dataGrid, IEnumerable value)
        {
            dataGrid.SetValue(AttachedColumnsProperty, value);
        }

        public static MappedValueCollection GetMappedValues(DependencyObject dataGrid)
        {
            return (MappedValueCollection)dataGrid.GetValue(MappedValuesProperty);
        }

        public static void SetMappedValues(DependencyObject dataGrid, MappedValueCollection value)
        {
            dataGrid.SetValue(MappedValuesProperty, value);
        }

        public static MappedValueCollection GetSelectedCells(DependencyObject dataGrid)
        {
            return (MappedValueCollection)dataGrid.GetValue(SelectedCellsProperty);
        }

        public static void SetSelectedCells(DependencyObject dataGrid, MappedValueCollection value)
        {
            dataGrid.SetValue(SelectedCellsProperty, value);
        }

        public static DataTemplate GetHeaderTemplate(DependencyObject dataGrid)
        {
            return (DataTemplate)dataGrid.GetValue(HeaderTemplateProperty);
        }

        public static void SetHeaderTemplate(DependencyObject dataGrid, DataTemplate value)
        {
            dataGrid.SetValue(HeaderTemplateProperty, value);
        }

        public static DataTemplate GetAttachedCellTemplate(DependencyObject dataGrid)
        {
            return (DataTemplate)dataGrid.GetValue(AttachedCellTemplateProperty);
        }

        public static void SetAttachedCellTemplate(DependencyObject dataGrid, DataTemplate value)
        {
            dataGrid.SetValue(AttachedCellTemplateProperty, value);
        }

        public static DataTemplate GetAttachedCellEditingTemplate(DependencyObject dataGrid)
        {
            return (DataTemplate)dataGrid.GetValue(AttachedCellEditingTemplateProperty);
        }

        public static void SetAttachedCellEditingTemplate(DependencyObject dataGrid, DataTemplate value)
        {
            dataGrid.SetValue(AttachedCellEditingTemplateProperty, value);
        }
    }
}
