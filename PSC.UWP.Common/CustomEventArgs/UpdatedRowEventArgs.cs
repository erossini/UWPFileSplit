using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSC.UWP.Common.CustomEventArgs
{
    public class UpdatedRowEventArgs : EventArgs
    {
        public int CurrentRow { get; set; }
        public string Message { get; set; }
        public int TotalRows { get; set; }
    }
}
