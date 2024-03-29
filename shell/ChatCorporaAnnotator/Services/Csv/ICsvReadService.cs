﻿namespace ChatCorporaAnnotator.Services.Csv
{
    internal interface ICsvReadService
    {
        string[] GetFields(string path, string delimiter);
        int GetLineCount(string path, bool header);
    }
}
