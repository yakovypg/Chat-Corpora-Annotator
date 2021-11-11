using SoftCircuits.CsvParser;
using System;

namespace ChatCorporaAnnotator.Services.Csv
{
    internal class CsvWriteService : ICsvWriteService
    {
        private CsvWriter _csvWriter;

        public char Delimiter { get; set; }
        public char QuoteCharacter { get; set; }
        public bool IsWriterOpen { get; private set; }

        public CsvWriteService(char delimiter, char quoteCharacter)
        {
            Delimiter = delimiter;
            QuoteCharacter = quoteCharacter;
        }

        public void OpenWriter(string path)
        {
            if (IsWriterOpen)
                CloseWriter();

            var settings = new CsvSettings()
            {
                ColumnDelimiter = Delimiter,
                QuoteCharacter = QuoteCharacter,
                InvalidDataRaisesException = true
            };

            IsWriterOpen = true;
            _csvWriter = new CsvWriter(path, settings);
        }

        public void CloseWriter()
        {
            if (!IsWriterOpen)
                return;

            _csvWriter.Close();
            _csvWriter.Dispose();

            IsWriterOpen = false;
            _csvWriter = null;
        }

        public void WriteRow(params string[] items)
        {
            if (!IsWriterOpen)
                throw new Exception("Writer not open.");

            _csvWriter.WriteRow(items);
        }
    }
}
