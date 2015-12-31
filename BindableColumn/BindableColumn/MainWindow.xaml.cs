using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BindableColumn
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<object> _selected = new List<object>();
        List<string> cellsHovered = new List<string>();
        Point startPoint;

        public MainWindow()
        {
            InitializeComponent();
            this.myGrid.BeginningEdit += myGrid_BeginningEdit;
        }

        void myGrid_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            //throw new NotImplementedException();
        }

        private void myGrid_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Store the mouse position
            startPoint = e.GetPosition(null);
        }

        private void myGrid_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            // Get the current mouse position
            Point mousePos = e.GetPosition(null);
            Vector diff = startPoint - mousePos;

            if (e.LeftButton == MouseButtonState.Pressed &&
                Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)
            {
                // Get the dragged ListViewItem                
                // Find the data behind the ListViewItem
                var RowColumnValues = AttachedColumnBehavior.GetMappedValues(sender as DependencyObject);

                //var draggedItems = RowColumnValues.Where(x => (x.Value as PlateResultUi).IsSelected == true && (x.Value as PlateResultUi).IsCellDataAvailable == true);

                // Initialize the drag & drop operation
                DataObject dragData = new DataObject("myFormat", RowColumnValues);

                System.Windows.DragDrop.DoDragDrop(sender as DependencyObject, dragData, DragDropEffects.Move);
            }
        }

        //Cut
        private void CommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var dg = sender as DataGrid;
            if (dg == null) return;
            _selected.Clear();
            _selected.AddRange(dg.SelectedItems.Cast<object>());
        }

        //Copy
        private void CommandBinding_Executed_1(object sender, ExecutedRoutedEventArgs e)
        {

        }

        //Paste
        private void CommandBinding_Executed_2(object sender, ExecutedRoutedEventArgs e)
        {

        }

        private void myGrid_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var dataGrid = sender as DataGrid;


            //var visualcontainer = this.dataGrid.GetVisualContainer();
        }

        private void myGrid_DragLeave(object sender, DragEventArgs e)
        {

        }

        private void myGrid_Drop(object sender, DragEventArgs e)
        {
            //DataGridCell dgc = sender as DataGridCell;

            //var rowHeader = ((BindableColumn.ViewModel.RowViewModel)(dgc.DataContext)).Name;
            //var ColumnHeader = ((BindableColumn.ViewModel.ColumnsViewModel)((dgc.Column).Header)).Currency;

            //string cellSelected = rowHeader + ColumnHeader;

            //if (cellSelected != null)
            //    cellsHovered.Add(cellSelected);
        }

        private void myGrid_PreviewDragLeave(object sender, DragEventArgs e)
        {

        }

        private void EventSetter_OnHandler(object sender, DragEventArgs e)
        {            
            //DragDrop.DragDropHelper.SetHighlightColumn(sender as DependencyObject, true);
        }

        private void myGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var dataGrid = e.Source;
        }
    }
}
