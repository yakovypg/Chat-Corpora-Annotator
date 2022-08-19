using Microsoft.Recognizers.Text;
using Microsoft.Recognizers.Text.DateTime;
using System.Text;
using System.Text.RegularExpressions;

namespace IndexEngine.Parsers
{
    internal class DateTimeParser
    {
        public static Culture[] SupportedCultures => Culture.SupportedCultures;
        public static string[] SupportedCultureCodes => SupportedCultures.Select(t => t.CultureCode).ToArray();

        protected static readonly string[] DateTypes = new string[]
        {
            "datetime",
            "date",
            "time",
            "datetimerange",
            "daterange",
            "timerange"
        };

        private string _cultureCode = Culture.English;
        public string CultureCode
        {
            get => _cultureCode;
            set
            {
                if (!SupportedCultureCodes.Contains(value))
                    throw new CultureNotSupportedException(value);

                _cultureCode = value;
            }
        }

        public DateTimeParser() : this(Culture.English)
        {
        }

        public DateTimeParser(Culture culture) : this(culture.CultureCode)
        {
        }

        public DateTimeParser(string cultureCode)
        {
            CultureCode = cultureCode;
        }

        public DateTime Parse(string source)
        {
            string correctedSource = CorrectSourceString(source);
            return Recognise(correctedSource);
        }

        public DateTime Recognise(string source)
        {
            ModelResult recognizeResult = DateTimeRecognizer.RecognizeDateTime(source, CultureCode)[0];
            Dictionary<string, string> resultInfo = GetModelResultValues(recognizeResult);

            string type = resultInfo["type"];

            if (!DateTypes.Contains(type))
                throw new DateTimeFormatException(source, source);

            string part = Regex.IsMatch(type, @"range$")
                ? resultInfo["start"]
                : resultInfo["value"];

            return DateTime.TryParse(part, out DateTime dateTime)
                ? dateTime
                : throw new DateTimeFormatException(source, source);
        }

        private string CorrectSourceString(string source)
        {
            StringBuilder correctedSource = new();
            List<ModelResult> recognizeResults = DateTimeRecognizer.RecognizeDateTime(source, CultureCode);

            foreach (ModelResult res in recognizeResults)
            {
                var resInfo = GetModelResultValues(res);
                string type = resInfo["type"];

                if (!DateTypes.Contains(type))
                    continue;

                correctedSource.Append(res.Text + ' ');
            }

            return correctedSource.ToString().TrimEnd();
        }

        private static Dictionary<string, string> GetModelResultValues(ModelResult model)
        {
            object obj = model.Resolution["values"];

            return obj is not List<Dictionary<string, string>> values || values.Count == 0
                ? throw new ApplicationException($"The {nameof(ModelResult)} class has changed its structure.")
                : values[0];
        }
    }
}
