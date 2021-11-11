namespace ChatCorporaAnnotator.Services.Csv
{
    internal interface ICsvWriteService
    {
        char Delimiter { get; set; }
        char QuoteCharacter { get; set; }
        bool IsWriterOpen { get; }

        void CloseWriter();
        void OpenWriter(string path);
        void WriteRow(params string[] items);
    }
}
