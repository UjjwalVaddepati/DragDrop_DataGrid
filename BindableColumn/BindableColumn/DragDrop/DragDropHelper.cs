using BindableColumn.ViewModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace BindableColumn.DragDrop
{
    public class DragDropHelper
    {
        // source and target
        private readonly DataFormat _format = DataFormats.GetDataFormat("DragDropItemsControl");
        private string targetCell = string.Empty;
        private string sourceCell = string.Empty;
        List<string> cellsHovered = new List<string>();
        private bool _isMultipleCells = false;
        private Point _initialMousePosition;
        private Vector _initialMouseOffset;
        private object _draggedData;
        private Dictionary<string, object> _multipleDraggedData = new Dictionary<string, object>();
        private DraggedAdorner _draggedAdorner;
        private InsertionAdorner _insertionAdorner;
        private Window _topWindow;
        // source
        private ItemsControl _sourceItemsControl;
        private object _sourceDataContext;
        private FrameworkElement _sourceItemContainer;
        //private FrameworkElement _sourceCellContainer;
        // target
        private ItemsControl _targetItemsControl;
        private object _targetDataContext;
        private FrameworkElement _targetItemContainer;
        //private FrameworkElement _targetCellContainer;

        //view models
        object _columnsModel;
        object _rowModel;
        DataGridCell _sourceCell;

        private bool _hasVerticalOrientation;
        private int _insertionIndex;
        private bool _isInFirstHalf;
        //private double _scrollHorizontalOffset;
        //private double _scrollVerticalOffset;
        //private double _targetTopMargin;
        // private double _targetLeftMargin;

        // singleton
        private static DragDropHelper _instance;
        private static DragDropHelper Instance
        {
            get { return _instance ?? (_instance = new DragDropHelper()); }
        }

        #region Highlight Column
        public static bool GetHighlightColumn(DependencyObject obj)
        {
            return (bool)obj.GetValue(HighlightColumnProperty);
        }

        public static void SetHighlightColumn(DependencyObject obj, bool value)
        {
            obj.SetValue(HighlightColumnProperty, value);
        }

        // Using a DependencyProperty as the backing store for HighlightColumn.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HighlightColumnProperty =
            DependencyProperty.RegisterAttached("HighlightColumn", typeof(bool),
            typeof(DragDropHelper), new FrameworkPropertyMetadata(false, OnHighlightColumnPropertyChanged));

        private static void OnHighlightColumnPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            DataGridCell cell = sender as DataGridCell;

            if (cell != null && GetHighlightColumn(sender))
            {
                Instance.DataGrid_CellDrop(sender, null);

                SetHighlightColumn(sender, false);
            }
        }

        private void DataGrid_CellDrop(object sender, DragEventArgs e)
        {
            DataGridCell dgc = sender as DataGridCell;

            if (_draggedData != null && !_isMultipleCells)
            {
                MappedValue selectedCell = _draggedData as MappedValue;

                RowViewModel sourceRowModel = (selectedCell.RowBinding as RowViewModel);
                ColumnsViewModel sourceColumnModel = (selectedCell.ColumnBinding as ColumnsViewModel);


                RowViewModel targetRowModel = ((BindableColumn.ViewModel.RowViewModel)(dgc.DataContext));
                ColumnsViewModel targetColumnModel = ((BindableColumn.ViewModel.ColumnsViewModel)((dgc.Column).Header));

                targetCell = targetRowModel.Name + targetColumnModel.Currency;
                sourceCell = sourceRowModel.Name + sourceColumnModel.Currency;


                if (sourceCell == targetCell)
                {
                    RemoveDraggedAdorner();
                    RemoveInsertionAdorner();
                    (this._sourceItemsControl as DataGrid).UnselectAllCells();
                    dgc.IsSelected = false;
                    return;
                }

                MappedValueCollection mappedValueCollection = this._mappedValueCollection;
                var movingCellData = mappedValueCollection.ReturnIfExistAddIfNot(sourceColumnModel, sourceRowModel);

                if (mappedValueCollection.AddCellValue(targetRowModel, targetColumnModel, movingCellData))
                {
                    if (movingCellData.Value != null)
                        dgc.IsSelected = true;

                    mappedValueCollection.EmptyCellValue(sourceRowModel, sourceColumnModel);
                }
            }
            else if (_draggedData != null && _isMultipleCells)
            {
                RowViewModel targetRowModel = ((BindableColumn.ViewModel.RowViewModel)(dgc.DataContext));
                ColumnsViewModel targetColumnModel = ((BindableColumn.ViewModel.ColumnsViewModel)((dgc.Column).Header));

                targetCell = targetRowModel.Name + targetColumnModel.Currency;

                if (MoveMultipleCells(targetCell, this._multipleDraggedData.Count))                
                    this._multipleDraggedData.Clear();                    
                
            }

            RemoveDraggedAdorner();
            RemoveInsertionAdorner();
            this._multipleDraggedData.Clear();
            (this._sourceItemsControl as DataGrid).UnselectAllCells();
        }

        private bool MoveMultipleCells(string targetCell, int countOfCells)
        {
            Dictionary<string, object> availableCells = GetNextAvailableCells(targetCell, countOfCells);

            if (availableCells.Count == 0)
                MessageBox.Show("No Available Cells, please drop the cells in the available space");
            else if (availableCells.Count < _multipleDraggedData.Count &&
                MessageBox.Show("Only Some cells will be moved are you ok ?? ", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                return ExchangeData(availableCells);
            else if (availableCells.Count.Equals(_multipleDraggedData.Count))
                return ExchangeData(availableCells);

            return false;
        }

        private bool ExchangeData(Dictionary<string, object> availableCells)
        {
            if (availableCells.Count > 1)
            {
                foreach (KeyValuePair<string, object> movingCellData in availableCells)
                {
                    MappedValueCollection mappedValueCollection = this._mappedValueCollection;
                    var CellData = mappedValueCollection.ReturnIfExistAddIfNot((movingCellData.Value as MappedValue).ColumnBinding, (movingCellData.Value as MappedValue).RowBinding);
                    var bindingData = GetDataForBinding();

                    if (bindingData != null)
                    {
                        CellData.Value = (bindingData as MappedValue).Value;
                        MappedValue value = this._mappedValueCollection.ReturnIfExistAddIfNot((bindingData as MappedValue).ColumnBinding, (bindingData as MappedValue).RowBinding);
                        value.Value = null;
                    }
                }
                return true;
            }
            return false;
        }

        private object GetDataForBinding()
        {
            if (_multipleDraggedData.Count >= 1)
            {
                var key = this._multipleDraggedData.First();
                MappedValue dataToSend = this._multipleDraggedData[key.Key] as MappedValue;
                _multipleDraggedData.Remove(key.Key);
                
                return dataToSend;
            }

            return null;
        }

        private Dictionary<string, object> GetNextAvailableCells(string targetCell, int countOfCells)
        {
            string rowPosition;
            string columnPosition;

            Dictionary<string, object> _availableCells = new Dictionary<string, object>();

            char[] arrayofAlphabets = new char[] { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P' };
            string[] arrayofNumbers = new string[] { "01", "02", "03", "04", "05", "06", "07", "08", "09", "10", "11", "12" };

            bool isRowAlphabet = !String.IsNullOrEmpty(targetCell) && Char.IsLetter(targetCell.ToCharArray()[0]);
            GetPlate_RowColumnPosition(isRowAlphabet, targetCell, out rowPosition, out columnPosition);


            int rowCount = isRowAlphabet ? Array.IndexOf(arrayofAlphabets, Convert.ToChar(rowPosition)) : Convert.ToInt32(rowPosition);
            int columnCount = !isRowAlphabet ? Array.IndexOf(arrayofNumbers, Convert.ToChar(columnPosition)) : Convert.ToInt32(columnPosition);

            ObservableCollection<RowViewModel> RowCollection = (this._sourceDataContext as MainWindowViewModel).RowCollection;
            ObservableCollection<ColumnsViewModel> ColumnsCollection = (this._sourceDataContext as MainWindowViewModel).ColumnsCollection;
            MappedValueCollection RowColumnValues = (this._sourceDataContext as MainWindowViewModel).RowColumnValues;


            for (int currentRow = rowCount; currentRow < RowCollection.Count; currentRow++)
            {
                string row = isRowAlphabet ? Convert.ToString(arrayofAlphabets[currentRow]) : string.Format("{0:D2}", currentRow);
                var tempRowBinding = RowCollection.Where(q => q.Name.Equals(row)).FirstOrDefault();

                for (int currentColumn = columnCount; currentColumn < ColumnsCollection.Count; currentColumn++)
                {
                    if (countOfCells == _availableCells.Count)
                    {
                        break;
                    }
                    else
                    {
                        string column = !isRowAlphabet ? Convert.ToString(arrayofAlphabets[currentColumn]) : string.Format("{0:D2}", currentColumn);
                        var tempColumnBinding = ColumnsCollection.Where(p => p.Currency.Equals(column)).FirstOrDefault();
                        string currentCell = row + column;
                        MappedValue tempRowColumn = RowColumnValues.Where(Z => Z.RowBinding == tempRowBinding && Z.ColumnBinding == tempColumnBinding).FirstOrDefault();

                        if (tempRowColumn.Value == null || (tempRowColumn.Value != null && (tempRowColumn.Value as CellData).ColorName == null))
                            _availableCells.Add(currentCell, tempRowColumn);
                    }
                }

                if (countOfCells == _availableCells.Count)
                {
                    break;
                }
            }


            return _availableCells;
        }

        private static void GetPlate_RowColumnPosition(bool isRowAlphabet, string t, out string rowPosition, out string columnPosition)
        {
            if (string.IsNullOrWhiteSpace(t))
            {
                rowPosition = null;
                columnPosition = null;
                return;
            }

            Regex re;
            Match result;
            StringBuilder number = new StringBuilder();

            if (isRowAlphabet)
            {
                re = new Regex(@"([a-zA-Z]+)(\d+)");
                result = re.Match(t);
            }
            else
            {
                re = new Regex(@"(\d+)([a-zA-Z]+)");
                result = re.Match(t);
            }

            rowPosition = isRowAlphabet ? result.Groups[1].Value : String.Format("{0:D2}", Convert.ToInt32(result.Groups[1].Value));
            columnPosition = !isRowAlphabet ? result.Groups[2].Value : String.Format("{0:D2}", Convert.ToInt32(result.Groups[2].Value));

        }

        #endregion

        #region IsDropRejectFromOthers
        public static bool GetIsDropRejectFromOthers(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsDropRejectFromOthers);
        }

        public static void SetIsDropRejectFromOthers(DependencyObject obj, bool value)
        {
            obj.SetValue(IsDropRejectFromOthers, value);
        }

        public static readonly DependencyProperty IsDropRejectFromOthers =
            DependencyProperty.RegisterAttached("IsDropRejectFromOthers", typeof(bool), typeof(DragDropHelper), new UIPropertyMetadata(null));
        #endregion

        #region IsDragSource
        //---
        public static bool GetIsDragSource(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsDragSourceProperty);
        }

        public static void SetIsDragSource(DependencyObject obj, bool value)
        {
            obj.SetValue(IsDragSourceProperty, value);
        }

        public static readonly DependencyProperty IsDragSourceProperty =
            DependencyProperty.RegisterAttached("IsDragSource", typeof(bool), typeof(DragDropHelper), new UIPropertyMetadata(false, IsDragSourceChanged));

        private static void IsDragSourceChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var dragSource = obj as DataGrid;
            if (dragSource != null)
            {
                if (Equals(e.NewValue, true))
                {
                    dragSource.PreviewMouseLeftButtonDown += Instance.DragSource_PreviewMouseLeftButtonDown;
                    dragSource.PreviewMouseLeftButtonUp += Instance.DragSource_PreviewMouseLeftButtonUp;
                    dragSource.PreviewMouseMove += Instance.DragSource_PreviewMouseMove;
                    dragSource.SelectedCellsChanged += Instance.DragSource_SelectedCellsChanged;
                }
                else
                {
                    dragSource.PreviewMouseLeftButtonDown -= Instance.DragSource_PreviewMouseLeftButtonDown;
                    dragSource.PreviewMouseLeftButtonUp -= Instance.DragSource_PreviewMouseLeftButtonUp;
                    dragSource.PreviewMouseMove -= Instance.DragSource_PreviewMouseMove;
                    dragSource.SelectedCellsChanged -= Instance.DragSource_SelectedCellsChanged;
                }
            }
        }

        private void DragSource_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            IList<DataGridCellInfo> selectedCells = (sender as DataGrid).SelectedCells;
            this._multipleDraggedData.Clear();

            selectedCells.ToList().ForEach(x =>
            {
                RowViewModel rowModel = x.Item as RowViewModel;
                ColumnsViewModel columnModel = x.Column.Header as ColumnsViewModel;
                MappedValue value = AttachedColumnBehavior.GetMappedValues(sender as DependencyObject).ReturnIfExistAddIfNot(columnModel, rowModel);

                var selectedCell = rowModel.Name + columnModel.Currency;

                if (value.Value != null && selectedCells.Count > 1 && !_multipleDraggedData.ContainsKey(selectedCell))
                    this._multipleDraggedData.Add(selectedCell, value);
            });

            _isMultipleCells = selectedCells.Count > 1;
        }

        #endregion

        #region IsCellDrop
        //---
        public static bool GetIsCellDrop(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsCellDropProperty);
        }

        public static void SetIsCellDrop(DependencyObject obj, bool value)
        {
            obj.SetValue(IsCellDropProperty, value);
        }

        public static readonly DependencyProperty IsCellDropProperty =
            DependencyProperty.RegisterAttached("IsCellDrop", typeof(bool), typeof(DragDropHelper), new UIPropertyMetadata(false, IsCellDropChanged));

        private static void IsCellDropChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region IsDropTarget
        //
        public static bool GetIsDropTarget(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsDropTargetProperty);
        }

        public static void SetIsDropTarget(DependencyObject obj, bool value)
        {
            obj.SetValue(IsDropTargetProperty, value);
        }

        public static readonly DependencyProperty IsDropTargetProperty =
            DependencyProperty.RegisterAttached("IsDropTarget", typeof(bool), typeof(DragDropHelper), new UIPropertyMetadata(false, IsDropTargetChanged));

        private static void IsDropTargetChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var dropTarget = obj as DataGrid;
            if (dropTarget != null)
            {
                if (Equals(e.NewValue, true))
                {
                    dropTarget.AllowDrop = true;
                    //dropTarget.PreviewDrop += Instance.DropTarget_PreviewDrop;
                    dropTarget.PreviewDragEnter += Instance.DropTarget_PreviewDragEnter;
                    dropTarget.PreviewDragOver += Instance.DropTarget_PreviewDragOver;
                    dropTarget.PreviewDragLeave += Instance.DropTarget_PreviewDragLeave;
                }
                else
                {
                    dropTarget.AllowDrop = false;
                    //dropTarget.PreviewDrop -= Instance.DropTarget_PreviewDrop;
                    dropTarget.PreviewDragEnter -= Instance.DropTarget_PreviewDragEnter;
                    dropTarget.PreviewDragOver -= Instance.DropTarget_PreviewDragOver;
                    dropTarget.PreviewDragLeave -= Instance.DropTarget_PreviewDragLeave;
                }
            }
        }
        //
        #endregion

        #region DragDropTemplate
        public static DataTemplate GetDragDropTemplate(DependencyObject obj)
        {
            return (DataTemplate)obj.GetValue(DragDropTemplateProperty);
        }

        public static void SetDragDropTemplate(DependencyObject obj, DataTemplate value)
        {
            obj.SetValue(DragDropTemplateProperty, value);
        }

        public static readonly DependencyProperty DragDropTemplateProperty =
            DependencyProperty.RegisterAttached("DragDropTemplate", typeof(DataTemplate), typeof(DragDropHelper), new UIPropertyMetadata(null));
        //

        #endregion

        #region Drag Source Events
        // DragSource        

        private void DragSource_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this._sourceItemsControl = (ItemsControl)sender;
            var visual = e.OriginalSource as Visual;

            this._sourceDataContext = (sender as DataGrid).DataContext;
            this._targetDataContext = null;

            this._mappedValueCollection = AttachedColumnBehavior.GetMappedValues(sender as DependencyObject);

            this._topWindow = Window.GetWindow(this._sourceItemsControl);
            this._initialMousePosition = e.GetPosition(this._topWindow);

            this._sourceItemContainer = _sourceItemsControl.ContainerFromElement(visual) as FrameworkElement;

            _columnsModel = GetColumnModel(sender, e, out _sourceCell);

            if (_columnsModel == null)
                return;

            if (this._sourceItemContainer != null)
            {
                _rowModel = this._sourceItemContainer.DataContext;
                this._draggedData = AttachedColumnBehavior.GetMappedValues(sender as DependencyObject).ReturnIfExistAddIfNot(_columnsModel, _rowModel);
            }
        }

        private object GetColumnModel(object sender, MouseButtonEventArgs e, out DataGridCell cell)
        {
            DependencyObject dep = (DependencyObject)e.OriginalSource;
            cell = new DataGridCell();
            // iteratively traverse the visual tree
            while ((dep != null) &&
                    !(dep is DataGridCell) &&
                    !(dep is DataGridColumnHeader))
            {
                dep = VisualTreeHelper.GetParent(dep);
            }


            if (dep is DataGridColumnHeader)
            {
                DataGridColumnHeader columnHeader = dep as DataGridColumnHeader;
                // do something
            }

            if (dep is DataGridCell)
            {
                cell = dep as DataGridCell;
                this._sourceItemContainer = cell;
            }

            if (cell.Column != null)
                return (sender as DataGrid).Columns[cell.Column.DisplayIndex].Header;

            return null;
        }

        // Drag = mouse down + move by a certain amount
        private void DragSource_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (this._draggedData != null)
            {
                // Only drag when user moved the mouse by a reasonable amount.
                if (IsMovementBigEnough(this._initialMousePosition, e.GetPosition(this._topWindow)))
                {
                    this._initialMouseOffset = this._initialMousePosition - this._sourceItemContainer.TranslatePoint(new Point(0, 0), this._topWindow);

                    var data = new DataObject(this._format.Name, this._draggedData);

                    _sourceCell.IsSelected = true;

                    // Adding events to the window to make sure dragged adorner comes up when mouse is not over a drop target.
                    bool previousAllowDrop = this._topWindow.AllowDrop;
                    this._topWindow.AllowDrop = true;
                    this._topWindow.DragEnter += TopWindow_DragEnter;
                    this._topWindow.DragOver += TopWindow_DragOver;
                    this._topWindow.DragLeave += TopWindow_DragLeave;

                    DragDropEffects effects = System.Windows.DragDrop.DoDragDrop((DependencyObject)sender, data, DragDropEffects.Move);

                    // Without this call, there would be a problem in the following scenario: Click on a data item, and drag
                    // the mouse very fast outside of the window. When doing this really fast, for some reason I don't get 
                    // the Window leave event, and the dragged adorner is left behind.
                    // With this call, the dragged adorner will disappear when we release the mouse outside of the window,
                    // which is when the DoDragDrop synchronous method returns.
                    RemoveDraggedAdorner();

                    this._topWindow.AllowDrop = previousAllowDrop;
                    this._topWindow.DragEnter -= TopWindow_DragEnter;
                    this._topWindow.DragOver -= TopWindow_DragOver;
                    this._topWindow.DragLeave -= TopWindow_DragLeave;

                    this._draggedData = null;
                    //   this._multipleDraggedData.Clear();
                }
            }
        }

        private void DragSource_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            this._draggedData = null;
            //this._multipleDraggedData.Clear();
            //this._multipleDraggedData = new List<object>();
        }

        #endregion

        #region Drop Target Events
        // DropTarget

        private void DropTarget_PreviewDragEnter(object sender, DragEventArgs e)
        {
            this._targetItemsControl = (ItemsControl)sender;
            //var margin = SumMargins(_targetItemsControl);
            //_targetTopMargin = margin.Top;
            //_targetLeftMargin = margin.Left;
            object draggedItem = e.Data.GetData(this._format.Name);

            DecideDropTarget(e);
            if (draggedItem != null)
            {
                var position = e.GetPosition(this._topWindow);
                //ScrollIntoView(this._targetItemsControl, position);
                // Dragged Adorner is created on the first enter only.
                ShowDraggedAdorner(position);
                CreateInsertionAdorner();
            }
            e.Handled = true;
        }

        private void DropTarget_PreviewDragOver(object sender, DragEventArgs e)
        {
            this._targetItemsControl = (ItemsControl)sender;
            object draggedItem = e.Data.GetData(this._format.Name);

            DecideDropTarget(e);
            if (draggedItem != null)
            {
                // Dragged Adorner is only updated here - it has already been created in DragEnter.
                var position = e.GetPosition(this._topWindow);
                //ScrollIntoView(this._targetItemsControl, position);

                ShowDraggedAdorner(position);
                UpdateInsertionAdornerPosition();

            }
            e.Handled = true;
        }

        private void DropTarget_PreviewDrop(object sender, DragEventArgs e)
        {
            object draggedItem = e.Data.GetData(this._format.Name);

            int indexRemoved = -1;

            if (draggedItem != null)
            {
                if ((e.Effects & DragDropEffects.Move) != 0 && (this._sourceItemsControl == this._targetItemsControl || GetIsDropRejectFromOthers(this._targetItemsControl)))
                {
                    this._targetDataContext = (sender as DataGrid).DataContext;

                    // Remove the selected Cell here from Mapped Values
                    DataGrid dgc = sender as DataGrid;

                    RowViewModel sourceRowHeader = ((RowViewModel)(dgc.CurrentItem));
                    ColumnsViewModel sourceColumnHeader = ((ColumnsViewModel)((dgc.CurrentColumn).Header));
                    string srcCell = sourceRowHeader.Name + sourceColumnHeader.Currency;


                    MappedValueCollection sourceCell = AttachedColumnBehavior.GetMappedValues(sender as DependencyObject);
                    MappedValue mappedValue = sourceCell.ReturnIfExistAddIfNot(sourceColumnHeader, sourceRowHeader);
                    int getIndex = sourceCell.IndexOf(mappedValue);

                    //var hitTestInfo = (sender as DataGrid).InputHitTest(new Point{ X = e.})

                    sourceCell[getIndex].Value = (draggedItem as MappedValue).Value;


                    //indexRemoved = RemoveItemFromItemsControl(this._sourceItemsControl, draggedItem);
                }
                // This happens when we drag an item to a later position within the same ItemsControl.
                if (indexRemoved != -1 && this._sourceItemsControl == this._targetItemsControl && indexRemoved < this._insertionIndex)
                {
                    this._insertionIndex--;
                }
                if (!GetIsDropRejectFromOthers(this._targetItemsControl))
                {
                    IEnumerable itemsSource = _targetItemsControl.ItemsSource;
                    Type type = itemsSource.GetType();
                    Type genericIListType = type.GetInterface("IList`1");
                    int elementsCount = 0;
                    if (genericIListType != null)
                    {
                        elementsCount = (int)type.GetProperty("Count").GetValue(itemsSource, null);
                    }
                    if (elementsCount > this._insertionIndex)
                    {
                        RemoveItemFromItemsControlByIndex(this._targetItemsControl, this._insertionIndex);
                        InsertItemInItemsControl(this._targetItemsControl, draggedItem, this._insertionIndex);

                        if (this._sourceItemsControl == this._targetItemsControl)
                        {
                            Type draggedType = draggedItem.GetType();
                            object newitem = Activator.CreateInstance(draggedType);
                            InsertItemInItemsControl(this._sourceItemsControl, newitem, indexRemoved);
                        }
                    }
                }
                else
                {
                    Type draggedType = draggedItem.GetType();
                    object newitem = Activator.CreateInstance(draggedType);
                    //InsertItemInItemsControl(this._sourceItemsControl, newitem, indexRemoved);
                }

                RemoveDraggedAdorner();
                RemoveInsertionAdorner();
            }
            e.Handled = true;
        }

        private void DropTarget_PreviewDragLeave(object sender, DragEventArgs e)
        {
            this._targetItemsControl = (ItemsControl)sender;
            // Dragged Adorner is only created once on DragEnter + every time we enter the window. 
            // It's only removed once on the DragDrop, and every time we leave the window. (so no need to remove it here)
            object draggedItem = e.Data.GetData(this._format.Name);

            if (draggedItem != null)
            {
                RemoveInsertionAdorner();
            }

            DataGrid dgc = sender as DataGrid;

            var rowHeader = ((BindableColumn.ViewModel.RowViewModel)((sender as DataGrid).CurrentItem)).Name;
            var ColumnHeader = ((BindableColumn.ViewModel.ColumnsViewModel)(((sender as DataGrid).CurrentColumn).Header)).Currency;

            string cellSelected = rowHeader + ColumnHeader;

            if (cellSelected != null)
                cellsHovered.Add(cellSelected);


            e.Handled = true;
        }

        // If the types of the dragged data and ItemsControl's source are compatible, 
        // there are 3 situations to have into account when deciding the drop target:
        // 1. mouse is over an items container
        // 2. mouse is over the empty part of an ItemsControl, but ItemsControl is not empty
        // 3. mouse is over an empty ItemsControl.
        // The goal of this method is to decide on the values of the following properties: 
        // targetItemContainer, insertionIndex and isInFirstHalf.
        private void DecideDropTarget(DragEventArgs e)
        {
            int targetItemsControlCount = this._targetItemsControl.Items.Count;
            object draggedItem = e.Data.GetData(this._format.Name);

            if (IsDropDataTypeAllowed(draggedItem))
            {
                if (targetItemsControlCount > 0)
                {
                    this._hasVerticalOrientation = HasVerticalOrientation(this._targetItemsControl.ItemContainerGenerator.ContainerFromIndex(0) as FrameworkElement);
                    this._targetItemContainer = _targetItemsControl.ContainerFromElement((DependencyObject)e.OriginalSource) as FrameworkElement;

                    if (this._targetItemContainer != null)
                    {
                        Point positionRelativeToItemContainer = e.GetPosition(this._targetItemContainer);
                        this._isInFirstHalf = IsInFirstHalf(this._targetItemContainer, positionRelativeToItemContainer, this._hasVerticalOrientation);
                        this._insertionIndex = this._targetItemsControl.ItemContainerGenerator.IndexFromContainer(this._targetItemContainer);

                        if (!this._isInFirstHalf)
                        {
                            this._insertionIndex++;
                        }
                    }
                    else
                    {
                        this._targetItemContainer = this._targetItemsControl.ItemContainerGenerator.ContainerFromIndex(targetItemsControlCount - 1) as FrameworkElement;
                        this._isInFirstHalf = false;
                        this._insertionIndex = targetItemsControlCount;
                    }
                }
                else
                {
                    this._targetItemContainer = null;
                    this._insertionIndex = 0;
                }
            }
            else
            {
                this._targetItemContainer = null;
                this._insertionIndex = -1;
                e.Effects = DragDropEffects.None;
            }
        }

        // Can the dragged data be added to the destination collection?
        // It can if destination is bound to IList<allowed type>, IList or not data bound.
        private bool IsDropDataTypeAllowed(object draggedItem)
        {
            return draggedItem != null;
        }

        #endregion

        #region Window Events
        // Window

        private void TopWindow_DragEnter(object sender, DragEventArgs e)
        {
            ShowDraggedAdorner(e.GetPosition(this._topWindow));
            e.Effects = DragDropEffects.None;
            e.Handled = true;
        }

        private void TopWindow_DragOver(object sender, DragEventArgs e)
        {
            ShowDraggedAdorner(e.GetPosition(this._topWindow));
            e.Effects = DragDropEffects.None;
            e.Handled = true;
        }

        private void TopWindow_DragLeave(object sender, DragEventArgs e)
        {
            RemoveDraggedAdorner();
            e.Handled = true;
        }

        #endregion

        #region Adorners and Others

        // Adorners

        // Creates or updates the dragged Adorner. 
        private void ShowDraggedAdorner(Point currentPosition)
        {
            if (this._draggedAdorner == null)
            {
                var adornerLayer = AdornerLayer.GetAdornerLayer(this._sourceItemsControl);
                this._draggedAdorner = new DraggedAdorner(this._draggedData, GetDragDropTemplate(this._sourceItemsControl), this._sourceItemContainer, adornerLayer);
            }

            double left = currentPosition.X - this._initialMousePosition.X + this._initialMouseOffset.X - 25;
            Debug.WriteLine("Adorner Left: " + left);
            double top = currentPosition.Y - this._initialMousePosition.Y + this._initialMouseOffset.Y - 50;
            Debug.WriteLine("Adorner Top: " + top);

            this._draggedAdorner.SetPosition(left, top);
        }

        private void RemoveDraggedAdorner()
        {
            if (this._draggedAdorner != null)
            {
                this._draggedAdorner.Detach();
                this._draggedAdorner = null;
            }
        }

        private void CreateInsertionAdorner()
        {
            if (this._targetItemContainer != null)
            {
                // Here, I need to get adorner layer from targetItemContainer and not targetItemsControl. 
                // This way I get the AdornerLayer within ScrollContentPresenter, and not the one under AdornerDecorator (Snoop is awesome).
                // If I used targetItemsControl, the adorner would hang out of ItemsControl when there's a horizontal scroll bar.
                var adornerLayer = AdornerLayer.GetAdornerLayer(this._targetItemContainer);
                this._insertionAdorner = new InsertionAdorner(this._hasVerticalOrientation, this._isInFirstHalf, this._targetItemContainer, adornerLayer);
            }
        }

        private void UpdateInsertionAdornerPosition()
        {
            if (this._insertionAdorner != null)
            {
                this._insertionAdorner.IsInFirstHalf = this._isInFirstHalf;
                this._insertionAdorner.InvalidateVisual();
            }
        }

        private void RemoveInsertionAdorner()
        {
            if (this._insertionAdorner != null)
            {
                this._insertionAdorner.Detach();
                this._insertionAdorner = null;
            }
        }

        // Finds the orientation of the panel of the ItemsControl that contains the itemContainer passed as a parameter.
        // The orientation is needed to figure out where to draw the adorner that indicates where the item will be dropped.
        private static bool HasVerticalOrientation(FrameworkElement itemContainer)
        {
            var hasVerticalOrientation = true;

            if (itemContainer != null)
            {
                var panel = VisualTreeHelper.GetParent(itemContainer) as Panel;
                StackPanel stackPanel;
                WrapPanel wrapPanel;

                if ((stackPanel = panel as StackPanel) != null)
                {
                    hasVerticalOrientation = (stackPanel.Orientation == Orientation.Vertical);
                }
                else if ((wrapPanel = panel as WrapPanel) != null)
                {
                    hasVerticalOrientation = (wrapPanel.Orientation == Orientation.Vertical);
                }
                // You can add support for more panel types here.
            }
            return hasVerticalOrientation;
        }

        private static void InsertItemInItemsControl(ItemsControl itemsControl, object itemToInsert, int insertionIndex)
        {
            try
            {
                if (itemToInsert != null)
                {
                    IEnumerable itemsSource = itemsControl.ItemsSource;

                    Type typeItem = itemToInsert.GetType();
                    object itemcopy = typeItem.GetMethod("Copy").Invoke(itemToInsert, null);
                    itemToInsert = itemcopy;

                    if (itemsSource == null)
                    {
                        //if(!itemsControl.Items.Contains(itemToInsert))
                        itemsControl.Items.Insert(insertionIndex, itemToInsert);
                    }
                    // Is the ItemsSource IList or IList<T>? If so, insert the dragged item in the list.
                    else if (itemsSource is IList)
                    {
                        //if (!((IList)itemsSource).Contains(itemToInsert))
                        ((IList)itemsSource).Insert(insertionIndex, itemToInsert);
                    }
                    else
                    {
                        Type type = itemsSource.GetType();
                        Type genericIListType = type.GetInterface("IList`1");
                        if (genericIListType != null)
                        {
                            type.GetMethod("Insert").Invoke(itemsSource, new[] { insertionIndex, itemToInsert });
                        }
                    }
                }
            }
            catch
            {

            }
        }

        private static int RemoveItemFromItemsControl(ItemsControl itemsControl, object itemToRemove)
        {
            int indexToBeRemoved = -1;
            if (itemToRemove != null)
            {
                indexToBeRemoved = itemsControl.Items.IndexOf(itemToRemove);

                if (indexToBeRemoved != -1)
                {
                    IEnumerable itemsSource = itemsControl.ItemsSource;
                    if (itemsSource == null)
                    {
                        if (indexToBeRemoved >= 0 && indexToBeRemoved < itemsControl.Items.Count)
                            itemsControl.Items.RemoveAt(indexToBeRemoved);
                    }
                    // Is the ItemsSource IList or IList<T>? If so, remove the item from the list.
                    else if (itemsSource is IList)
                    {
                        var list = ((IList)itemsSource);
                        if (indexToBeRemoved >= 0 && indexToBeRemoved < list.Count)
                            list.RemoveAt(indexToBeRemoved);
                    }
                    else
                    {
                        Type type = itemsSource.GetType();
                        Type genericIListType = type.GetInterface("IList`1");
                        if (genericIListType != null)
                        {
                            type.GetMethod("RemoveAt").Invoke(itemsSource, new object[] { indexToBeRemoved });
                        }
                    }
                }
            }
            return indexToBeRemoved;
        }

        private static void RemoveItemFromItemsControlByIndex(ItemsControl itemsControl, int indexToBeRemoved)
        {
            IEnumerable itemsSource = itemsControl.ItemsSource;
            Type type = itemsSource.GetType();
            Type genericIListType = type.GetInterface("IList`1");
            if (genericIListType != null)
            {
                type.GetMethod("RemoveAt").Invoke(itemsSource, new object[] { indexToBeRemoved });
            }
        }

        private static bool IsInFirstHalf(FrameworkElement container, Point clickedPoint, bool hasVerticalOrientation)
        {
            if (hasVerticalOrientation)
            {
                return clickedPoint.Y < container.ActualHeight / 2;
            }
            return clickedPoint.X < container.ActualWidth / 2;
        }

        private static bool IsMovementBigEnough(Point initialMousePosition, Point currentPosition)
        {
            return (Math.Abs(currentPosition.X - initialMousePosition.X) >= SystemParameters.MinimumHorizontalDragDistance ||
                    Math.Abs(currentPosition.Y - initialMousePosition.Y) >= SystemParameters.MinimumVerticalDragDistance);
        }
        #endregion

        public MappedValueCollection _mappedValueCollection { get; set; }
    }
}

