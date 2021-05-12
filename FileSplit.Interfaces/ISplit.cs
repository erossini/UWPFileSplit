using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileSplit.Interfaces
{
    public interface ISplit
    {
        void ReadHeaders(string file, bool HasHeaderRecord);
        Task ReadFile(string file, bool HasHeaderRecord);
    }
}
