using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Linq;
using OpinionMining.Model;
using Work;
using System.Data;

namespace OpinionMining.BLL
{
    public class MasterDataBL
    {
        public string Path { get; set; }

        public double Confredence { get; set; }
        //义原
        private Primitive YiYuan;
        //义项
        private WordSimilarity YiXiang;
        //褒义情感词库
        private List<string> PosSentiment;
        //贬义情感词库
        private List<string> NegSentiment;

        //比对情感词库后的MasterData列表
        public Dictionary<string, MasterData> MasterDatas;
        public DataTable MasterDataTable;


        public MasterDataBL(string path, double confredence, Primitive yiYuan, WordSimilarity yiXiang, List<string> posSentiment, List<string> negSentiment)
        {
            YiYuan = yiYuan;
            YiXiang = yiXiang;
            PosSentiment = posSentiment;
            NegSentiment = negSentiment;
            MasterDatas = new Dictionary<string, MasterData>();
            Path = path;
            Confredence = confredence;
            MasterDataTable = new DataTable();
            MasterDataTable.Columns.Add("id");
            MasterDataTable.Columns.Add("docid");
           // MasterDataTable.Columns.Add("SentenceId");
            MasterDataTable.Columns.Add("word-string");
            //MasterDataTable.Columns.Add("WordOrder");
            MasterDataTable.Columns.Add("confidence-score", typeof(double));
            MasterDataTable.Columns.Add("word-polarity");
            MasterDataTable.Columns.Add("context-string");
        }



        public string CompareMasterWithDic()
        {

            var fileList = Directory.GetFiles(Path, "*.xml");

            foreach (var file in fileList)
            {
                GetPracticeDocString(file);
            }

            return "";
        }

        public string LoadMasterData()
        {

            return "";
        }


        private void GetPracticeDocString(string sourcefile)
        {
            XDocument xdoc = XDocument.Load(sourcefile);
            foreach (XElement document in xdoc.Descendants("document"))
            {
                var docName = document.Attribute("name");
                foreach (XElement sentence in xdoc.Descendants("sentence"))
                {
                    var sentenceId = sentence.Attribute("id");
                    foreach (XElement word in sentence.Descendants("word"))
                    {
                        var property = word.Attribute("property");
                        if (property.Value == "n" || property.Value == "nl" || property.Value.ToUpper() == "ng" || property.Value.ToUpper() == "v" || property.Value.ToUpper() == "a")
                        {
                            if (!MasterDatas.ContainsKey(word.Value))
                            {
                                CompareReturn compareReturn = CompareWordWithSentiment(word.Value);

                                MasterData masterdata = new MasterData();
                                masterdata.DocName = docName.Value;
                                //masterdata.SentenceId = sentenceId.Value;
                                masterdata.WordValue = word.Value;
                                //masterdata.WordOrder = wordorder.Value;
                                masterdata.Weight = compareReturn.Weight;
                                masterdata.Polarity = compareReturn.Polarity;
                                MasterDatas.Add(word.Value, masterdata);

                                if (compareReturn.Polarity != "")
                                {
                                    var emotion = word.Attribute("emotion");
                                    var wordorder = word.Attribute("wordOrder");
                                    int order = Int32.Parse(wordorder.Value);
                                    int begin = order - 10 > 0 ? order - 10 : 1;
                                    int end = order + 10;

                                    IEnumerable<string> list1 = from el in sentence.Descendants("word")
                                                                  where (int)el.Attribute("wordOrder")>=begin && (int)el.Attribute("wordOrder")<=end 
                                                                  select el.Value;
                                    DataRow dr = MasterDataTable.NewRow();
                                    dr["docid"] = docName.Value;
                                    //dr["SentenceId"] = sentenceId.Value;
                                    dr["word-string"] = word.Value;
                                    //dr["WordOrder"] = wordorder.Value;
                                    dr["confidence-score"] = compareReturn.Weight;
                                    dr["word-polarity"] = compareReturn.Polarity;
                                    dr["context-string"] = String.Concat(list1);
                                    MasterDataTable.Rows.Add(dr);
                                }


                            }
                        }
                    }
                }
            }
        }
        public class CompareReturn
        {
            public string Polarity { get; set; }
            public double Weight { get; set; }
        }

        private CompareReturn CompareWordWithSentiment(string word)
        {
            CompareReturn compareReturn = new CompareReturn();

            if (PosSentiment.Contains(word))
            {
                compareReturn.Polarity = "pos";
                compareReturn.Weight = 1;
                return compareReturn;
            }
            if (NegSentiment.Contains(word))
            {
                compareReturn.Polarity = "neg";
                compareReturn.Weight = 1;
                return compareReturn;
            }

            double possim = 0;
            double negsim = 0;
            double Weight;

            foreach (string posword in PosSentiment)
            {
                possim = YiXiang.simWord(word, posword) > possim ? YiXiang.simWord(word, posword) : possim;
                if (possim == 1)
                    break;
            }
            foreach (string negword in NegSentiment)
            {
                negsim = YiXiang.simWord(word, negword) > negsim ? YiXiang.simWord(word, negword) : negsim;
                if (negsim == 1)
                    break;
            }
            Weight = possim - negsim;
            if (Math.Abs(Weight) > Confredence)
            {
                if (Weight > 0)
                {
                    compareReturn.Polarity = "neg";
                }
                else if (Weight < 0)
                {
                    compareReturn.Polarity = "pos";
                }
                else
                {
                    compareReturn.Polarity = "";
                }
            }
            else {
                compareReturn.Polarity = "";
            }

            compareReturn.Weight = Math.Abs(Weight);
            return compareReturn;
        }
    }
}
