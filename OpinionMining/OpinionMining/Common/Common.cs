using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.IO;
using OpinionMining.Model;
using System.Xml;
using System.Collections;


namespace OpinionMining.Common
{
    public static class Common
    {
        public static DataTable MasterDataToDataTime(Dictionary<string, MasterData> dic)
        {

            foreach (MasterData va in dic.Values)
            {

            }

            return new DataTable();
        }

        #region "导在语料里标注的情感词"

        public static List<string> ImportPosSemtidic(string path)
        {
            List<string> lst = new List<string>();

            var fileList = Directory.GetFiles(path, "*.txt");
            foreach (var file in fileList)
            {
                StreamReader read = null;
                try
                {
                    string line = "";
                    FileStream fs = new FileStream(file, FileMode.OpenOrCreate, FileAccess.Read);
                    read = new StreamReader(fs, Encoding.GetEncoding("GB2312"));
                    read.BaseStream.Seek(0, SeekOrigin.Begin);
                    line = read.ReadLine();
                    line = line.Trim().ToString();
                    while (line != null)
                    {
                        string[] strs = System.Text.RegularExpressions.Regex.Split(line, @"\s+");
                        if (strs[3] == "p")
                        {
                            if (!lst.Contains(strs[0]))
                            {
                                lst.Add(strs[0]);
                            }
                        }

                        line = read.ReadLine();
                        if (line != null)
                        {
                            line = line.Trim().ToString();
                        }
                    }
                }
                catch
                {

                }
                finally
                {
                    read.Close();
                }
            }


            return lst;

        }

        public static List<string> ImportNegSemtidic(string path)
        {
            List<string> lst = new List<string>();

            var fileList = Directory.GetFiles(path, "*.txt");
            foreach (var file in fileList)
            {
                StreamReader read = null;
                try
                {
                    string line = "";
                    FileStream fs = new FileStream(file, FileMode.OpenOrCreate, FileAccess.Read);
                    read = new StreamReader(fs, Encoding.GetEncoding("GB2312"));
                    read.BaseStream.Seek(0, SeekOrigin.Begin);
                    line = read.ReadLine();
                    line = line.Trim().ToString();
                    while (line != null)
                    {
                        string[] strs = System.Text.RegularExpressions.Regex.Split(line, @"\s+");
                        if (strs[3] == "n")
                        {
                            if (!lst.Contains(strs[0]))
                            {
                                lst.Add(strs[0]);
                            }
                        }

                        line = read.ReadLine();
                        if (line != null)
                        {
                            line = line.Trim().ToString();
                        }
                    }
                }
                catch
                {

                }
                finally
                {
                    read.Close();
                }
            }


            return lst;

        }

        #endregion

        public static List<string> ImportTXTtoList(string path)
        {
            List<string> lst = new List<string>();
            StreamReader read = null;
            try
            {

                string line = "";
                FileStream fs = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Read);
                read = new StreamReader(fs, Encoding.GetEncoding("GB2312"));
                read.BaseStream.Seek(0, SeekOrigin.Begin);
                line = read.ReadLine();
                line = line.Trim().ToString();
                while (line != null)
                {
                    if (!lst.Contains(line))
                    {
                        lst.Add(line);
                    }
                    line = read.ReadLine();
                    if (line != null)
                    {
                        line = line.Trim().ToString();
                    }
                }
            }
            catch
            {

            }
            finally
            {
                read.Close();
            }

            return lst;
        }


        public static string ExportDataTableToTXT(DataTable dt, string filePath)
        { 
            string result = "";

            int i = 0;
            StreamWriter sw = null;

            try
            {
                
                sw = new StreamWriter(filePath, false);

               
                for (i = 0; i < dt.Columns.Count - 1; i++)
                {

                    sw.Write(dt.Columns[i].ColumnName + " | ");

                }
                sw.Write(dt.Columns[i].ColumnName);
                sw.WriteLine();

              
                foreach (DataRow row in dt.Rows)
                {
                    object[] array = row.ItemArray;

                    for (i = 0; i < array.Length - 1; i++)
                    {
                        sw.Write(array[i].ToString() + " | ");
                    }
                    sw.Write(array[i].ToString());
                    sw.WriteLine();

                }

                sw.Close();
            }

            catch (Exception ex)
            {
                result = ex.ToString();
            }


            return result;
        

        }
    
    }
}
