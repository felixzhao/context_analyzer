using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;

namespace WebMining
{
    public enum Emotion
    {
        Positive=0,
        Negative=1,
        NA=2
    }

    public class Word
    {
        public string szTerm { get; set; }
        public WordType szWordType { get; set; }
        public Emotion Emotional { get; set; }
        public double Confidential { get; set; }

        public Word(string term)
        {
            this.szTerm = term;
            this.szWordType = WordType.UNSET;  
            this.Emotional = Emotion.NA;
            this.Confidential = 0d; 
        }

        public Word(string term, WordType wordtype)
        {
            this.szTerm = term;
            this.szWordType=wordtype;
            this.Emotional = Emotion.NA;
            this.Confidential = 0d; 
        }

        public Word(string term, WordType wordtype,Emotion emotional, double confidential)
        {
            this.szTerm = term;
            this.szWordType = wordtype;  
            this.Emotional = emotional;
            this.Confidential = confidential; 
        }
    }

    public class Common
    {
        public static string number = @"id";
        private static string[] stopWords = {"称","道", "说","是","定","主", "重","轻","热","冷","实","虚","挂","怀","扬", "放","打", "拔", "压","吹",
                                              "安","传","加","减","推","正", "反", "翻", "饰","大", "小","开", "关","抱", "含", "吸", "笃",
                                              "祥", "促", "秀","酷", "患","就","要"};
        private const string _PositiveFileName = @"HowNet\正面情感词语（中文）.txt";
        private const string _NegativeFileName = @"HowNet\负面情感词语（中文）.txt";

        private static Similarity similarity=new Similarity();

        public static Dictionary<String, Emotion> EmotionalDic;
              
         /// <summary>
        /// 加载情感词库
        /// </summary>
        public static void loadHowNetEmotionalDictionary()
        {
            EmotionalDic=new Dictionary<string,Emotion>(); 

            if (!File.Exists(_PositiveFileName))
            {
                throw new Exception("加载错误，正面情感词语（中文）.txt不存在");
            }

            if (!File.Exists(_NegativeFileName))
            {
                throw new Exception("加载错误，负面情感词语（中文）.txt不存在");
            }

            StreamReader sr = new StreamReader(_PositiveFileName, Encoding.GetEncoding("gb2312"), true);
            string strline = sr.ReadLine();
            while (strline != null)
            {
                strline = strline.Trim();
                int pos = strline.IndexOf("\r");
                if (pos != -1)
                {
                    strline = strline.Substring(0, pos);
                }

                pos = strline.IndexOf("\n");
                if (pos != -1)
                {
                    strline = strline.Substring(0, pos);
                }


                if (strline != "中文正面情感词语\t836" && strline != "" && strline.Length > 1)
                {
                    if (!EmotionalDic.ContainsKey(strline))
                    {
                        EmotionalDic.Add(strline, Emotion.Positive);
                    }
                }

                strline = sr.ReadLine();
            }
            sr.Close();

            sr = new StreamReader(_NegativeFileName, Encoding.GetEncoding("gb2312"), true);
            strline = sr.ReadLine();
            while (strline != null)
            {
                strline = strline.Trim();
                int pos = strline.IndexOf("\r");
                if (pos != -1)
                {
                    strline = strline.Substring(0, pos);
                }

                pos = strline.IndexOf("\n");
                if (pos != -1)
                {
                    strline = strline.Substring(0, pos);
                }

                if (strline != "中文负面情感词语\t1254" && strline != "" && strline.Length > 1)
                {
                    if (!EmotionalDic.ContainsKey(strline))
                    {
                        EmotionalDic.Add(strline, Emotion.Negative);
                    }
                }
                strline = sr.ReadLine();
            }
            sr.Close();

            sr = null;
            strline = null;
        }

         /// <summary>
        /// 通过过滤词性，过滤明显不是情感词的
        /// </summary>
        /// <param name="word"></param>
        /// <param name="wordType"></param>
        /// <returns></returns>
        public static bool isEmotionWordType(string word, string wordType)
        {
            foreach (string stopWord in stopWords)
            {
                if (stopWord.Equals(word))
                { return false; }
            }

            if (wordType[0] == 'n')
            {
                if (wordType.Length > 1)
                {
                    if (wordType[1] == 'l' || wordType[1] == 'g')
                    { return true; }
                }
                else
                    return true;
            }
            else if (wordType[0] == 'v')
            {
                if (wordType.Length > 1)
                {
                    if (wordType[1] == 's' || wordType[1] == 'y')
                    { return false; }
                }
                return true;
            }
            else if (wordType[0] == 'a' || wordType[0] == 'd')
            { return true; }
            return false;
        }

        /// <summary>
        /// 词是否包含在情感词库中
        /// </summary>
        /// <param name="word"></param>
        /// <returns></returns>
        public static Emotion SeekEmotionDictionary(string word)
        {
            if (Common.EmotionalDic.ContainsKey(word.Trim()))
            {
                Emotion emotion = Common.EmotionalDic[word.Trim()];
                return emotion; 
            }
            else
            {
                return Emotion.NA; 
            }
        }

        public static Word checkEmotion(Word word,double threshold)
        {
            word.Emotional = Common.SeekEmotionDictionary(word.szTerm.Trim());  
            if (word.Emotional!=Emotion.NA)
            { 
                word.Confidential = 1d;
            }
            else
            {
                double orientation=Common.similarity.calOrientation(word.szTerm, word.szWordType);
                if (orientation > threshold)
                {
                    word.Emotional = Emotion.Positive;
                    word.Confidential = orientation;
                }
                else if (orientation < -threshold)
                {
                    word.Emotional = Emotion.Negative;
                    word.Confidential = orientation;
                }
                else
                {
                    word.Emotional = Emotion.NA;
                    word.Confidential = 0d; 
                }                
            }
            return word;
        }
    }
}
