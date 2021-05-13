using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using FileSplit.Excel.CustomEventArgs;
using FileSplit.Excel.Models;
using Microsoft.Win32.SafeHandles;
using PSC.UWP.Common.CustomEventArgs;
using PSC.UWP.Common.Files;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Storage;

namespace FileSplit.Excel
{
    /// <summary>
    /// Class ImportExcel.
    /// </summary>
    public class ImportExcel
    {
        public event EventHandler<ReadCompletedEventArgs> ReadCompleted;
        public event EventHandler<ReadErrorEventArgs> ReadError;
        public event EventHandler<ReadHeaderEventArgs> ReadHeader;
        public event EventHandler<UpdatedRowEventArgs> UpdatedRow;

        protected virtual void OnReadCompleted(ReadCompletedEventArgs e)
        {
            EventHandler<ReadCompletedEventArgs> handler = ReadCompleted;
            if (handler != null) handler(this, e);

            Debug.WriteLine($"Read completed");
        }

        protected virtual void OnReadError(ReadErrorEventArgs e)
        {
            EventHandler<ReadErrorEventArgs> handler = ReadError;
            if (handler != null) handler(this, e);

            Debug.WriteLine($"Read error");
        }

        protected virtual void OnReadHeader(ReadHeaderEventArgs e)
        {
            EventHandler<ReadHeaderEventArgs> handler = ReadHeader;
            if (handler != null) handler(this, e);

            Debug.WriteLine($"Header has been read. It contains {e.HeaderNumber} columns");
        }

        protected virtual void OnUpdatedRow(UpdatedRowEventArgs e)
        {
            EventHandler<UpdatedRowEventArgs> handler = UpdatedRow;
            if (handler != null) handler(this, e);

            Debug.WriteLine($"Read record {e.CurrentRow}/{e.TotalRows}");
        }

        private void UpdateRowEvent(int rowIndex, int totalRows)
        {
            UpdatedRowEventArgs args = new UpdatedRowEventArgs();
            args.CurrentRow = rowIndex;
            args.TotalRows = totalRows;
            OnUpdatedRow(args);
        }

        /// <summary>
        /// Validates the header.
        /// </summary>
        /// <param name="dataGrid">The data grid.</param>
        /// <param name="rowDef">The row definition.</param>
        /// <returns>System.ValueTuple&lt;System.Boolean, List&lt;System.String&gt;&gt;.</returns>
        public (bool IsValid, List<string> Errors) ValidateHeader(DataGrid dataGrid, RowDef rowDef)
        {
            bool rtn = true;
            List<string> errs = new List<string>();

            foreach (CellDef cell in rowDef.Cells)
            {
                if (dataGrid.Headers.Where(h => h == cell.CellValue.Text).Count() == 0)
                {
                    rtn = false;
                    errs.Add($"The field {cell.CellValue.Text} is not present in the Excel file");
                }
            }

            return (rtn, errs);
        }

        /// <summary>
        /// Reads to grid.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <returns>DataGrid.</returns>
        public async Task<DataGrid> ReadToGrid(string filePath)
        {
            var grid = new DataGrid();
            grid.Success = true;

            try
            {
                StorageFile fl = await StorageFile.GetFileFromPathAsync(filePath);
                SafeFileHandle fileHandle = fl.CreateSafeFileHandle(FileAccess.Read, FileShare.ReadWrite);

                using (FileStream fs = new FileStream(fileHandle, FileAccess.Read))
                {
                    var document = SpreadsheetDocument.Open(fs, false);
                    var sharedStringTable = document.WorkbookPart.SharedStringTablePart.SharedStringTable;
                    var value = string.Empty;

                    bool isheader = true;
                    foreach (var worksheetPart in document.WorkbookPart.WorksheetParts)
                    {
                        foreach (var sheetData in worksheetPart.Worksheet.Elements<SheetData>())
                        {
                            if (sheetData.HasChildren)
                            {
                                int totalRows = sheetData.Elements<Row>().Count();
                                UpdateRowEvent(0, totalRows);

                                int currentRow = 0;
                                foreach (var row in sheetData.Elements<Row>())
                                {
                                    int columnIndex = 0;
                                    var dictionary = new Dictionary<string, string>();

                                    foreach (var cell in row.Elements<Cell>())
                                    {
                                        value = cell.InnerText;

                                        if (value != null && cell.DataType != null && cell.DataType == CellValues.SharedString)
                                            value = sharedStringTable.ElementAt(int.Parse(value)).InnerText;

                                        if (isheader)
                                        {
                                            grid.Headers.Add(value);
                                        }
                                        else
                                        {
                                            int cellRef = CellReferenceToIndex(cell);
                                            var header = grid.Headers[cellRef];
                                            dictionary.Add(header, value);
                                        }

                                        columnIndex++;
                                    }

                                    currentRow++;
                                    if (!isheader)
                                    {
                                        grid.Rows.Add(dictionary);
                                        UpdateRowEvent(currentRow, totalRows);
                                    }
                                    else
                                    {
                                        ReadHeaderEventArgs args = new ReadHeaderEventArgs();
                                        args.HeaderNumber = columnIndex;
                                        OnReadHeader(args);
                                    }

                                    isheader = false;
                                }
                            }
                        }
                    }

                    document.Close();
                }
            }
            catch(Exception ex)
            {
                ReadErrorEventArgs errArgs = new ReadErrorEventArgs();
                errArgs.Message = ex.Message;
                OnReadError(errArgs);
            }

            grid = CheckGrid(grid);

            ReadCompletedEventArgs readArgs = new ReadCompletedEventArgs();
            readArgs.DataGrid = grid;
            OnReadCompleted(readArgs);

            return grid;
        }

        /// <summary>
        /// Checks the grid.
        /// </summary>
        /// <param name="grid">The grid.</param>
        /// <returns>DataGrid.</returns>
        public DataGrid CheckGrid(DataGrid grid)
        {
            DataGrid rtn = new DataGrid();
            rtn.Headers.AddRange(grid.Headers);
            rtn.Success = grid.Success;

            int nColumns = grid.Headers.Count;

            foreach (var item in grid.Rows)
            {
                if (item.Count == nColumns)
                    rtn.Rows.Add(item);
                else
                {
                    var dictionary = new Dictionary<string, string>();

                    foreach (var head in rtn.Headers)
                    {
                        string value = item.ContainsKey(head) ? item[head] : "";
                        dictionary.Add(head, value);
                    }

                    rtn.Rows.Add(dictionary);
                }
            }

            return rtn;
        }

        /// <summary>Cells the index of the reference to.</summary>
        /// <param name="cell">The cell.</param>
        /// <returns>System.Int32.</returns>
        /// <remarks>
        /// The moment you have even a single empty cell in a row then things go haywire.
        /// Essentially we need to figure out the original column index of the cell in case there were empty cells before it.
        /// This function obtains the original/correct index of any cell.
        /// </remarks>
        private static int CellReferenceToIndex(Cell cell)
        {
            //int index = 0;
            string reference = cell.CellReference.ToString().ToUpper();

            //remove digits
            string columnReference = Regex.Replace(reference.ToUpper(), @"[\d]", string.Empty);

            int columnNumber = -1;
            int mulitplier = 1;

            //working from the end of the letters take the ASCII code less 64 (so A = 1, B =2...etc)
            //then multiply that number by our multiplier (which starts at 1)
            //multiply our multiplier by 26 as there are 26 letters
            foreach (char c in columnReference.ToCharArray().Reverse())
            {
                columnNumber += mulitplier * ((int)c - 64);

                mulitplier = mulitplier * 26;
            }

            //the result is zero based so return columnnumber + 1 for a 1 based answer
            //this will match Excel's COLUMN function
            return columnNumber;
        }
    }
}