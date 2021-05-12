using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileSplit.Excel.CustomEventArgs
{
    public class ReadErrorEventArgs : EventArgs
    {
        public string Message { get; set; }
    }
}
