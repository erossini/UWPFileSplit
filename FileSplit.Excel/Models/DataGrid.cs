﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileSplit.Excel.Models
{
    /// <summary>
    /// Class DataGrid.
    /// </summary>
    public class DataGrid
    {
        public bool Success { get; set; } = false;

        /// <summary>
        /// Gets the headers.
        /// </summary>
        /// <value>The headers.</value>
        public List<string> Headers { get; } = new List<string>();

        /// <summary>
        /// Gets the types.
        /// </summary>
        /// <value>The types.</value>
        public Dictionary<string, string> Types { get; } = new Dictionary<string, string>();

        /// <summary>
        /// Gets the rows.
        /// </summary>
        /// <value>The rows.</value>
        public List<Dictionary<string, string>> Rows { get; } = new List<Dictionary<string, string>>();
    }
}