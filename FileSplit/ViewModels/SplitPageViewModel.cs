using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using FileSplit.Code;
using FileSplit.Excel;
using FileSplit.Excel.Models;
using PSC.UWP.Common.CustomEventArgs;
using PSC.UWP.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Xaml;

namespace FileSplit.ViewModels
{
    public class SplitPageViewModel : INotifyPropertyChanged
    {
        private DataGrid grid;
        public ObservableCollection<ListItemData> ListItemLeft { get; set; } = new ObservableCollection<ListItemData>();
        public ObservableCollection<ListItemData> ListItemRight { get; set; } = new ObservableCollection<ListItemData>();

        public SplitPageViewModel(Windows.UI.Core.CoreDispatcher dispatcher)
        {
            _dispatcher = dispatcher;

            MoveLeftCommand = new RelayCommand(new Action(MoveLeft), CanExecuteMoveLeftCommand);
            MoveRightCommand = new RelayCommand(new Action(MoveRight), CanExecuteMoveRightCommand);
        }

        #region IContext
        private readonly Windows.UI.Core.CoreDispatcher _dispatcher;
        #endregion

        private string _baseFileName = "output";
        public string BaseFileName
        {
            get { return _baseFileName; }
            set
            {
                if (_baseFileName != value)
                {
                    _baseFileName = value;
                    OnPropertyChanged(nameof(BaseFileName));
                }
            }
        }

        private int _currentStep = 1;

        public int CurrentStep
        {
            get { return _currentStep; }
            set
            {
                if (_currentStep != value)
                {
                    _currentStep = value;
                    OnPropertyChanged(nameof(CurrentStep));
                }
            }
        }

        private int _currentRow;

        public int CurrentRow
        {
            get { return _currentRow; }
            set
            {
                if (_currentRow != value)
                {
                    _currentRow = value;
                    OnPropertyChanged(nameof(CurrentRow));
                }
            }
        }

        private int _totalRows;

        public int TotalRows
        {
            get { return _totalRows; }
            set
            {
                if (_totalRows != value)
                {
                    _totalRows = value;
                    OnPropertyChanged(nameof(TotalRows));
                }
            }
        }

        private string _filename = "";

        public string FileName
        {
            get { return _filename; }
            set
            {
                if (_filename != value)
                {
                    _filename = value;
                    OnPropertyChanged(nameof(FileName));
                }
            }
        }

        private string _folder = "";

        public string Folder
        {
            get { return _folder; }
            set
            {
                if (_folder != value)
                {
                    _folder = value;
                    OnPropertyChanged(nameof(Folder));
                }
            }
        }

        private string _message = "Starting reading file...";

        public string Message
        {
            get { return _message; }
            set
            {
                if (_message != value)
                {
                    _message = value;
                    OnPropertyChanged(nameof(Message));
                }
            }
        }

        private List<ListItemData> _selectedItemLeft;

        public List<ListItemData> SelectedItemLeft
        {
            get { return _selectedItemLeft; }
            set
            {
                if (_selectedItemLeft != value)
                {
                    _selectedItemLeft = value;
                    OnPropertyChanged(nameof(SelectedItemLeft));
                }
            }
        }

        private List<ListItemData> _selectedItemRight;

        public List<ListItemData> SelectedItemRight
        {
            get { return _selectedItemRight; }
            set
            {
                if (_selectedItemRight != value)
                {
                    _selectedItemRight = value;
                    OnPropertyChanged(nameof(SelectedItemRight));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected async void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventArgs e = new PropertyChangedEventArgs(propertyName);
            //this.PropertyChanged?.Invoke(this, e);

            await _dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            });
        }

        #region Read Excel

        public async Task ValidateAndStart()
        {
            await DecodeFile();
        }

        public async Task DecodeFile()
        {
            ImportExcel excel = new ImportExcel();
            excel.ReadCompleted += Excel_ReadCompleted;
            excel.ReadHeader += Excel_ReadHeader;
            excel.UpdatedRow += Excel_UpdatedRow;

            await excel.ReadToGrid(FileName);
        }

        private void Excel_ReadCompleted(object sender, Excel.CustomEventArgs.ReadCompletedEventArgs e)
        {
            grid = e.DataGrid;

            for (int i = 0; i < e.DataGrid.Headers.Count; i++)
                ListItemLeft.Add(new ListItemData() { Index = i, ListItemText = e.DataGrid.Headers[i] });

            OnPropertyChanged(nameof(ListItemLeft));
            MoveRightCommand.RaiseCanExecuteChanged();
            MoveLeftCommand.RaiseCanExecuteChanged();
        }

        private void Excel_UpdatedRow(object sender, UpdatedRowEventArgs e)
        {
            if (e.CurrentRow == 0)
            {
                CurrentRow = 1;
                TotalRows = e.TotalRows;
                Message = $"Starting to read all {e.TotalRows} records...";
            }
            else
            {
                CurrentRow = e.CurrentRow;
                Message = $"Read record {e.CurrentRow}/{e.TotalRows}";
            }
        }

        private void Excel_ReadHeader(object sender, Excel.CustomEventArgs.ReadHeaderEventArgs e)
        {
            Message = $"Header has been read. It contains {e.HeaderNumber} columns";
        }

        #endregion Read Excel

        public event EventHandler<UpdatedRowEventArgs> UpdatedRow;
        protected virtual void OnUpdatedRow(UpdatedRowEventArgs e)
        {
            EventHandler<UpdatedRowEventArgs> handler = UpdatedRow;
            if (handler != null) handler(this, e);
        }

        #region Commands

        public RelayCommand MoveLeftCommand { get; private set; }
        public RelayCommand MoveRightCommand { get; private set; }

        /// <summary>
        /// Move left command valid when items present in the list on right.
        /// </summary>
        /// <returns>True, if count is greater than 0.</returns>
        private bool CanExecuteMoveLeftCommand()
        {
            return ListItemRight.Count > 0;
        }

        /// <summary>
        /// Move right command valid when items present in the list on left.
        /// </summary>
        /// <returns>True, if count is greater than 0.</returns>
        private bool CanExecuteMoveRightCommand()
        {
            return ListItemLeft.Count > 0;
        }

        public void MoveRight()
        {
            if (ListItemLeft.Count > 0 && SelectedItemLeft != null)
            {
                var list = ListItemLeft;
                foreach (var item in _selectedItemLeft)
                {
                    ListItemRight.Add(item);
                    list.Remove(item);
                }

                var orderList = ListItemRight.ToList().OrderBy(l => l.Index);
                ListItemRight = new ObservableCollection<ListItemData>(orderList);
                OnPropertyChanged(nameof(ListItemRight));

                ListItemLeft = new ObservableCollection<ListItemData>(list);
                OnPropertyChanged(nameof(ListItemLeft));

                SelectedItemLeft = new List<ListItemData>();

                MoveRightCommand.RaiseCanExecuteChanged();
                MoveLeftCommand.RaiseCanExecuteChanged();
            }
        }

        /// <summary>
        /// The command implementation to execute when the Move item left button is pressed.
        /// </summary>
        public void MoveLeft()
        {
            if (ListItemRight.Count > 0 && SelectedItemRight != null)
            {
                var list = ListItemRight;
                foreach (var item in _selectedItemRight)
                {
                    ListItemLeft.Add(item);
                    list.Remove(item);
                }

                var orderList = ListItemLeft.ToList().OrderBy(l => l.Index);
                ListItemLeft = new ObservableCollection<ListItemData>(orderList);
                OnPropertyChanged(nameof(ListItemLeft));

                ListItemRight = new ObservableCollection<ListItemData>(list);
                OnPropertyChanged(nameof(ListItemRight));

                SelectedItemLeft = new List<ListItemData>();

                MoveRightCommand.RaiseCanExecuteChanged();
                MoveLeftCommand.RaiseCanExecuteChanged();
            }
        }

        #endregion Commands

        public byte[] ExportData()
        {
            var stream = new MemoryStream();

            using (var document = SpreadsheetDocument.Create(stream, SpreadsheetDocumentType.Workbook))
            {
                var workbookPart = document.AddWorkbookPart();
                workbookPart.Workbook = new Workbook();

                var worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
                worksheetPart.Worksheet = new Worksheet();

                var sheets = workbookPart.Workbook.AppendChild(new Sheets());
                var sheet = new Sheet() { Id = workbookPart.GetIdOfPart(worksheetPart), SheetId = 1, Name = "TableData" };
                sheets.Append(sheet);

                workbookPart.Workbook.Save();

                var sheetData = worksheetPart.Worksheet.AppendChild(new SheetData());

                // Constructing header
                var row = new Row();

                string headers = string.Join(',', ListItemRight.Select(i => i.ListItemText));

                foreach (var item in ListItemRight)
                    row.Append(CreateCell(item.ListItemText, CellValues.String));

                sheetData.AppendChild(row);

                foreach (var item in grid.Rows)
                {
                    row = new Row();

                    foreach (var itm in ListItemRight)
                    {
                        string vl = "";
                        item.TryGetValue(itm.ListItemText, out vl);

                        row.Append(CreateCell(vl, CellValues.String));
                    }

                    sheetData.AppendChild(row);
                }

                worksheetPart.Worksheet.Save();
                document.Close();
            }

            stream.Position = 0;
            return stream.ToArray();
        }

        private static Cell CreateCell(string value, CellValues dataType)
        {
            return new Cell()
            {
                CellValue = new CellValue(value),
                DataType = new EnumValue<CellValues>(dataType)
            };
        }

        public async Task SaveExport()
        {
            DataGrid copy = grid;

            byte[] export = ExportData();

            Windows.Storage.StorageFolder storageFolder = await StorageFolder.GetFolderFromPathAsync(Folder);

            Windows.Storage.StorageFile excelFile = await storageFolder.CreateFileAsync($"{_baseFileName}.xls",
                Windows.Storage.CreationCollisionOption.ReplaceExisting);
            var streamExport = await excelFile.OpenAsync(Windows.Storage.FileAccessMode.ReadWrite);
            using (var ouputExcel = streamExport.GetOutputStreamAt(0))
            {
                using (var dataWriterExcel = new Windows.Storage.Streams.DataWriter(ouputExcel))
                {
                    dataWriterExcel.WriteBytes(export);
                    await dataWriterExcel.StoreAsync();
                }
            }
            streamExport.Dispose();

            string headers = string.Join(',', ListItemRight.Select(i => i.ListItemText));

            Windows.Storage.StorageFile csvFile = await storageFolder.CreateFileAsync($"{_baseFileName}.csv",
                Windows.Storage.CreationCollisionOption.ReplaceExisting);
            var streamCSV = await csvFile.OpenAsync(Windows.Storage.FileAccessMode.ReadWrite);

            using (var outputStreamCSV = streamCSV.GetOutputStreamAt(0))
            {
                using (var dataWriterCSV = new Windows.Storage.Streams.DataWriter(outputStreamCSV))
                {
                    dataWriterCSV.WriteString(headers + Environment.NewLine);

                    foreach (var item in grid.Rows)
                    {
                        List<string> record = new List<string>();
                        foreach (var itm in ListItemRight)
                        {
                            string vl = "";
                            item.TryGetValue(itm.ListItemText, out vl);

                            record.Add(vl);
                        }

                        dataWriterCSV.WriteString(string.Join(',', record) + Environment.NewLine);
                    }
                    await dataWriterCSV.StoreAsync();
                }
            }
            streamCSV.Dispose();
        }
    }
}