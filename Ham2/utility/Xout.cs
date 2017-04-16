using System;
using System.IO;
using System.Linq;

namespace Ham2.utility
{
    public static class Xout
    {
        public static void LogInf(string contents) => File.AppendAllText(FilePath, contents + "\r\n");

        public static string FilePath
        {
            get; set;
        }
    }
}