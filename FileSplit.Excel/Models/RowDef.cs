using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileSplit.Excel.Models
{
    /// <summary>
    /// Class Row definition
    /// </summary>
    public class RowDef
    {
        /// <summary>
        /// Gets or sets the cells.
        /// </summary>
        /// <value>The cells.</value>
        public List<CellDef> Cells { get; set; }
    }
}