﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileSplit.Excel.Exceptions
{
    /// <summary>
    /// Class ImportException.
    /// Implements the <see cref="System.Exception" />
    /// </summary>
    /// <seealso cref="System.Exception" />
    public class ImportException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ImportException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="ex">The ex.</param>
        public ImportException(string message, Exception ex)
            : base(message, ex)
        {
        }
    }
}