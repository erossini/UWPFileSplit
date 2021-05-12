using FileSplit.Excel.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileSplit.Excel.CustomEventArgs
{
    public class ReadCompletedEventArgs : EventArgs
    {
        public DataGrid DataGrid { get; set; }
    }
}
