using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using ICTCLASLib;
using System.Xml;
using System.Collections;

namespace OpinionMining.BLL
{
    public class WordSeparation
    {

        public string Path { get; set; }

        public WordSeparation(string path)
        {
            Path = path;      
        }

        public string SeparateWord()
        {
            string result = "";
            if (Directory.Exists(Path))
            {
                string[] fileList;
                DirectoryInfo di = new DirectoryInfo(Path);
                fileList = Directory.GetFiles(Path, "*.txt");
                if (fileList.Length == 0)
                {
                    return "没找到生语料，请检查路径是否准确";
                }

                try
                {
                    if (!ICTCLAS.initialize())
                    {
                        return "无法调用分词接口";
                    }
                    foreach (string fileName in fileList)
                    {
                        StreamReader rd = new StreamReader(fileName, Encoding.Default);
                        string filedata = rd.ReadToEnd().Trim().Replace("\r\n", "");
                        char[] delimiterChars = { '。', '！', '?' };
                        string[] seArray = filedata.Split(delimiterChars);

                        XmlTextWriter xmlWriter = null;
                        int i = 1;
                        int wordOffset = 1;
                        foreach (string s in seArray)
                        {
                            if (s != string.Empty)
                            {
                                s.Replace(" ", string.Empty);
                                ArrayList wordList = new ArrayList();
                                ICTCLAS.splitword(s, wordList);
                                if (xmlWriter == null)
                                    xmlWriter = beginWriteXML(fileName);
                                writeWordsToXML(xmlWriter, wordList, i, ref wordOffset);

                            }
                            i++;
                        }
                        endWriteXML(xmlWriter);
                    }
                    ICTCLAS.uninitialize();
                }
                catch
                {
                    return "分词出错";
                }
                finally
                {
                }
            }


            return result;
        
        }

        #region "分词，根据中科院分词，并生成xml"

        private XmlTextWriter beginWriteXML(string srcFileName)
        {
            FileInfo fileInfo = new FileInfo(srcFileName);
            string name = fileInfo.FullName.TrimEnd(fileInfo.Extension.ToCharArray());
            name += ".xml";
            XmlTextWriter xmlWriter = new XmlTextWriter(name, Encoding.Default);
            xmlWriter.Formatting = Formatting.Indented;
            xmlWriter.WriteStartDocument();
            xmlWriter.WriteStartElement("document");
            xmlWriter.WriteAttributeString("name", fileInfo.Name);
            return xmlWriter;
        }
        private void writeWordsToXML(XmlTextWriter wr, ArrayList wordList, int sentenceId, ref int wordOffset)
        {
            if (wr == null)
                return;
            if (wordList.Count == 0)
                return;
            wr.WriteStartElement("sentence");
            wr.WriteAttributeString("id", sentenceId.ToString());
            wr.WriteAttributeString("emotion", "N/A");
            foreach (ICTCLASLib.SegWord word in wordList)
            {
                wr.WriteStartElement("word");
                wr.WriteAttributeString("id", wordOffset.ToString());
                int order = word.Number + 1;
                wr.WriteAttributeString("wordOrder", order.ToString());
                wr.WriteAttributeString("emotion", "N/A");
                if (word.wordType == "c" || word.wordType == "cc")
                {
                    wr.WriteAttributeString("conjunction", true.ToString());
                    wr.WriteAttributeString("effect", "same");
                }
                wr.WriteAttributeString("property", word.wordType);
                wr.WriteString(word.szWord);
                wr.WriteEndElement();
                wordOffset++;
            }
            wr.WriteEndElement();
        }
        private void endWriteXML(XmlTextWriter wr)
        {
            if (wr == null)
                return;
            wr.WriteEndElement();
            wr.WriteEndDocument();
            wr.Close();
        }


        #endregion

    }
}
