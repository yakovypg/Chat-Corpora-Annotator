// The code is based on the work of Antonio Miras: https://github.com/AMArostegui/CoreNLPClient.Net

/*
MIT License

Copyright (c) 2022 Antonio Miras

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

namespace CoreNLPClientDotNet
{
    using System;
    using System.IO;
    using System.Text;     
    using Authlete.Util;
    using Newtonsoft.Json.Linq;

    public static class PropertiesExt
    {
        public static void Update(this JObject thisProps, JObject properties)
        {
            foreach (var prop in properties)
                thisProps[prop.Key] = prop.Value;
        }

        public static void ReadCoreNlpProps(this JObject properties, string path)
        {
            using (var sr = new StreamReader(path))
            {
                var dictProp = PropertiesLoader.Load(sr);
                foreach (var prop in dictProp)
                    properties.Add(prop.Key, prop.Value);
            }
        }

        public static string WriteCoreNlpProps(this JObject properties, string path = "")
        {
            if (string.IsNullOrEmpty(path))
            {
                var strUid = Guid.NewGuid().ToString();
                strUid = strUid.Replace("-", string.Empty).Substring(0, 16);
                path = $"corenlp_server-{strUid}.props";
            }

            using (var sw = new StreamWriter(File.Open(path, FileMode.Create), Encoding.GetEncoding("iso-8859-1")))
            {
                foreach (var prop in properties)
                {
                    sw.WriteLine(prop.Key + " = " + prop.Value.ToString());
                    sw.WriteLine(string.Empty);
                }

                return path;
            }
        }
    }
}
