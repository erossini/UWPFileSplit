using CsvHelper;
using CsvHelper.Configuration;
using FileSplit.Interfaces;
using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;

namespace FileSplit.CSV
{
    public class CSVSplit : ISplit
    {
        public async Task ReadFile(string file, bool HasHeaderRecord)
        {
        }

        public void ReadHeaders(string file, bool HasHeaderRecord)
        {
            dynamic obj = ParseCsv(file, HasHeaderRecord);
            var r = obj;
        }

        public async Task<dynamic> ParseCsv(string file, bool HasHeaderRecord)
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = HasHeaderRecord,
            };

            StorageFile fl = await StorageFile.GetFileFromPathAsync(file);
            var fStream = await fl.OpenAsync(FileAccessMode.Read);

            var reader = new DataReader(fStream.GetInputStreamAt(0));
            var bytes = new byte[fStream.Size];
            await reader.LoadAsync((uint)fStream.Size);
            reader.ReadBytes(bytes);

            var stream = new MemoryStream(bytes);

            string text = Encoding.UTF8.GetString(bytes, 0, bytes.Length);

            var readFile = await Windows.Storage.FileIO.ReadLinesAsync(fl, Windows.Storage.Streams.UnicodeEncoding.Utf8);

            return null;
        }
    }
}