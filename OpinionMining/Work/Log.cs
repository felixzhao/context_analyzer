using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Work
{
    class Log
    {
        private static StreamWriter logWriter;
        private static string outpath = @"c:\sourceInfo.txt";

        public static void Write2File(string outString)
        {
            FileInfo logFile = new FileInfo(outpath);
            logWriter = logFile.AppendText();
            logWriter.WriteLine(outString);
            logWriter.Flush();
            logWriter.Close();
            logWriter.Dispose();
        }
    }
}
