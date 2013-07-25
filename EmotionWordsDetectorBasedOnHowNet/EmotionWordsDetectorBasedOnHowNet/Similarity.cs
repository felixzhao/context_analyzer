using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace WebMining
{
    public class Semanteme
    {
        public Semanteme(int id, string sema, int parentid)
        {
            ID = id;
            Sema = sema;
            ParentID = parentid;
        }

        public int ID { get; set; }
        public string Sema { get; set; }
        public int ParentID { get; set; }
    }

    public enum WordType
    {
        ADJ,
        ADV,
        CLAS,
        CONJ,
        ECHO,
        EXPR,
        N,
        NUM,
        PREP,
        PRON,
        STRU,
        UNSET,
        V,                 
        W     
    }

    public class Similarity
    {
        private const string _RelationalSymbol = "~^#%$*+&@?!";

        private const double _alpha = 1.6;     //计算基本义原相似度时的参数 
        private const double _beta1 = 0.5;     //4种描述式相似度的权值 
        private const double _beta2 = 0.2;
        private const double _beta3 = 0.17;
        private const double _beta4 = 0.13;

        private const double _delta = 0.2;
        private const double _gama = 0.2;

        private const int _Default_Distance = 20;
        private const string _SemantemeFileName = @"HowNet\WHOLE.DAT";
        private const string _ConceptFileName = @"HowNet\glossary.dat";
        private const string _ConceptIndexFileName = @"HowNet\glossary.idx";
        private const string _BasePos = @"HowNet\褒义基准词.txt";
        private const string _BaseNeg = @"HowNet\贬义基准词.txt";

        private System.Collections.Hashtable _ConceptIndex;
        private System.Collections.Hashtable _Semantems;
        private System.Collections.Hashtable _IndexSemantems;

        public Dictionary<String, String> basePosEmotionDic;
        public Dictionary<String, String> baseNegEmotionDic;
        
        public System.Collections.Hashtable Semantems
        {
            get { return _Semantems; }
            set { _Semantems = value; }
        }

        public System.Collections.Hashtable IndexSemantmes
        {
            get { return _IndexSemantems; }
            set { _IndexSemantems = value; }
        }

        public System.Collections.Hashtable ConceptIndex
        {
            get { return _ConceptIndex; }
            set { _ConceptIndex = value; }
        }

        public Similarity()
        {
            initialize();
        }

        public bool initialize()
        {
            bool flag = false;
            flag = this.ConstructSemantems();
            if (!flag) { return flag; }

            flag = this.ConstructConceptIndex();
            if (!flag) { return flag; }

           

            flag = this.ConstructBasePositiveWords();
            if (!flag) { return flag; }

            flag = this.ConstructBaseNegativeWords();
            if (!flag) { return flag; }

            return flag;
        }

       
        /// <summary>
        /// 加载褒义基准词库
        /// </summary>
        /// <returns></returns>
        private bool ConstructBasePositiveWords()
        {
            if (!File.Exists(_BasePos))
            {
                return false;
            }

            basePosEmotionDic = new Dictionary<string, string>();

            StreamReader sr = new StreamReader(_BasePos, Encoding.GetEncoding("gb2312"), true);
            string line = "";
            while ((line = sr.ReadLine()) != null)
            {
                line = line.Trim();
                this.basePosEmotionDic.Add(line, line);
            }
            return true;
        }

        /// <summary>
        /// 加载贬义基准词库
        /// </summary>
        /// <returns></returns>
        private bool ConstructBaseNegativeWords()
        {
            if (!File.Exists(_BaseNeg))
            {
                return false;
            }

            baseNegEmotionDic = new Dictionary<string, string>();

            StreamReader sr = new StreamReader(_BaseNeg, Encoding.GetEncoding("gb2312"), true);
            string line = "";
            while ((line = sr.ReadLine()) != null)
            {
                line = line.Trim();
                this.baseNegEmotionDic.Add(line, line);
            }
            return true;
        }

        /// <summary>
        /// 加载HowNet概念库索引
        /// </summary>
        /// <returns></returns>
        private bool ConstructConceptIndex()
        {
            if (!File.Exists(_ConceptIndexFileName))
            {
                return false;
            }

            this.ConceptIndex = new System.Collections.Hashtable();

            StreamReader sr = new StreamReader(_ConceptIndexFileName, Encoding.GetEncoding("gb2312"), true);
            string str = "";
            while ((str = sr.ReadLine()) != null)
            {
                char[] separator = { ' ' };
                string[] words = str.Split(separator, StringSplitOptions.RemoveEmptyEntries);
                if (words.Length >= 2)
                {
                    if (ConceptIndex.ContainsKey(words[0].Trim()))
                    {
                        ConceptIndex[words[0]] = ConceptIndex[words[0]] + " " + words[1].Trim();
                    }
                    else
                    {
                        ConceptIndex.Add(words[0].Trim(), words[1].Trim());
                    }
                }
                else
                {
                    continue;
                }
            }
            return true;
        }

        /// <summary>
        /// 加载HowNet义原树
        /// </summary>
        /// <returns></returns>
        private bool ConstructSemantems()
        {
            if (!File.Exists(_SemantemeFileName))
            {
                return false;
            }

            this.Semantems = new System.Collections.Hashtable();
            this.IndexSemantmes = new System.Collections.Hashtable();

            StreamReader sr = new StreamReader(_SemantemeFileName, Encoding.GetEncoding("gb2312"), true);
            string str = "";
            while ((str = sr.ReadLine()) != null)
            {
                char[] separator = { ' ' };
                string[] words = str.Split(separator, StringSplitOptions.RemoveEmptyEntries);
                if (words.Length >= 3)
                {
                    Semanteme item = new Semanteme(int.Parse(words[0]), words[1].Trim(), int.Parse(words[2]));
                    if (this.Semantems.ContainsKey(item.Sema))
                    {
                        this.Semantems[item.Sema] = item;
                    }
                    else
                    {
                        this.Semantems.Add(item.Sema, item);
                    }
                    this.IndexSemantmes.Add(item.ID, item);
                }
                else
                {
                    continue;
                }
            }
            return true;
        }

        /// <summary>
        /// 获取义原
        /// </summary>
        /// <param name="sema"></param>
        /// <returns></returns>
        public Semanteme GetSemantems(string sema)
        {
            if (this.Semantems != null && this.Semantems.ContainsKey(sema.Trim()))
            {
                Semanteme result = Semantems[sema.Trim()] as Semanteme;
                return (this.IndexSemantmes[result.ID] as Semanteme);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 获取父义原
        /// </summary>
        /// <param name="sem"></param>
        /// <returns></returns>
        public Semanteme GetParentSemantem(Semanteme sem)
        {
            if (this.IndexSemantmes != null)
            {
                return this.IndexSemantmes[sem.ParentID] as Semanteme;
            }
            else
            {
                return null;
            }
        }

        //比较2个义原之间的距离
        public int Distance(string s1, string s2)
        {
            Semanteme sem1 = GetSemantems(s1);
            Semanteme sem2 = GetSemantems(s2);

            if (sem1 == null || sem2 == null)
            {
                return _Default_Distance;
            }

            Stack<Semanteme> stk1 = new Stack<Semanteme>();
            while (sem1.ID != sem1.ParentID)
            {
                stk1.Push(sem1);
                sem1 = GetParentSemantem(sem1);
            }
            stk1.Push(sem1);

            Stack<Semanteme> stk2 = new Stack<Semanteme>();
            while (sem2.ID != sem2.ParentID)
            {
                stk2.Push(sem2);
                sem2 = GetParentSemantem(sem2);
            }
            stk2.Push(sem2);

            if (sem1.ID != sem2.ID)
            {
                return _Default_Distance;
            }

            while (stk1.Count > 0 && stk2.Count > 0)
            {
                if (stk1.Peek().ID == stk2.Peek().ID)
                {
                    stk1.Pop();
                    stk2.Pop();
                }
                else
                {
                    break;
                }
            }

            return stk1.Count + stk2.Count;
        }

        /// <summary>
        /// 根据词获取在HowNet中的所有概念
        /// </summary>
        /// <param name="word"></param>
        /// <returns></returns>
        public string[] getConcepts(string word)
        {
            if (!File.Exists(_ConceptFileName))
            {
                return null;
            }

            if (this.ConceptIndex.ContainsKey(word))
            {
                char[] separator = { ' ' };
                string[] posArray = ((string)this.ConceptIndex[word]).Split(separator, StringSplitOptions.RemoveEmptyEntries);
                if (posArray.Length == 0)
                {
                    return null;
                }

                string[] concepts = new string[posArray.Length];
                FileStream fs = new FileStream(_ConceptFileName, FileMode.Open, FileAccess.Read);
                for (int i = 0; i < posArray.Length; i++)
                {
                    long pos = long.Parse(posArray[i]);
                    fs.Seek(pos, SeekOrigin.Begin);
                    StreamReader sr = new StreamReader(fs, Encoding.GetEncoding("gb2312"), true);
                    string str = sr.ReadLine();
                    string[] tmp = str.Split('\t');
                    concepts[i] = tmp[2].Trim();
                }
                fs.Close();

                return concepts;

            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 根据词和词的类型获取在HowNet中的所有概念
        /// </summary>
        /// <param name="word"></param>
        /// <param name="wordtypes"></param>
        /// <returns></returns>
        public string[] getConcepts(string word,ref string[] wordtypes)
        {
            if (!File.Exists(_ConceptFileName))
            {
                return null;
            }

            if (this.ConceptIndex.ContainsKey(word))
            {
                char[] separator = { ' ' };
                string[] posArray = ((string)this.ConceptIndex[word]).Split(separator, StringSplitOptions.RemoveEmptyEntries);
                if (posArray.Length == 0)
                {
                    return null;
                }

                string[] concepts = new string[posArray.Length];
                wordtypes = new string[posArray.Length]; 
                FileStream fs = new FileStream(_ConceptFileName, FileMode.Open, FileAccess.Read);
                for (int i = 0; i < posArray.Length; i++)
                {
                    long pos = long.Parse(posArray[i]);
                    fs.Seek(pos, SeekOrigin.Begin);
                    StreamReader sr = new StreamReader(fs, Encoding.GetEncoding("gb2312"), true);
                    string str = sr.ReadLine();
                    string[] tmp = str.Split('\t');
                    wordtypes[i] = tmp[1].Trim(); 
                    concepts[i] = tmp[2].Trim();
                }
                fs.Close();

                return concepts;

            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 计算两个基本义原的相似度
        /// </summary>
        /// <param name="sem1"></param>
        /// <param name="sem2"></param>
        /// <returns></returns>
        private double calSimBase(string sem1, string sem2)
        {
            //有一个是具体词，而另一个不是
            if (sem1[0] == '(' ^ sem2[0] == '(')
            {
                return 0d;
            }

            if (sem1[0] == '(' && sem2[0] == '(')
            {
                if (sem1 != sem2)
                {
                    return 0d;
                }
            }

            if (sem1 == sem2)
            {
                return 1d;
            }

            int dist = this.Distance(sem1, sem2);
            double result = _alpha / (_alpha + dist);

            return result;
        }

        /// <summary>
        /// 计算两个基本关系义原的相似度
        /// </summary>
        /// <param name="sem1"></param>
        /// <param name="sem2"></param>
        /// <returns></returns>
        private double calSimReal(string sem1, string sem2)
        {
            if (sem1[0] == '(')
            {
                sem1 = sem1.Substring(1, sem1.Length - 2);
            }

            if (sem2[0] == '(')
            {
                sem2 = sem2.Substring(1, sem2.Length - 2);
            }

            int pos1 = sem1.IndexOf("=");
            string rela1 = sem1.Substring(0, pos1);

            int pos2 = sem2.IndexOf("=");
            string rela2 = sem2.Substring(0, pos2);

            if (rela1 == rela2)
            {
                string base1 = sem1.Substring(pos1 + 1);
                string base2 = sem2.Substring(pos2 + 1);
                return calSimBase(base1, base2);
            }
            else
            {
                return 0d;
            }
        }

        /// <summary>
        /// 计算第一独立义原描述式的相似度
        /// </summary>
        /// <param name="line1"></param>
        /// <param name="line2"></param>
        /// <returns></returns>
        private double calSim1(string line1, string line2)
        {
            if (line1 == "" && line2 == "")
            {
                return 1d;
            }

            if (line1 == "" || line2 == "")
            {
                return 0d;
            }

            string[] sems1 = this.splitSemanetes(line1);
            string[] sems2 = this.splitSemanetes(line2);

            if (sems1.Length >= 1 && sems2.Length >= 1)
            {
                return this.calSimBase(sems1[0], sems2[0]);
            }
            else
            {
                return 0d;
            }
        }

        /// <summary>
        /// 计算其他独立义原描述式的相似度 
        /// </summary>
        /// <param name="line1"></param>
        /// <param name="line2"></param>
        /// <returns></returns>
        private double calSim2(string line1, string line2)
        {
            if (line1 == "" && line2 == "")
            {
                return 1d;
            }

            if (line1 == "" || line2 == "")
            {
                return 0d;
            }

            string[] sems1 = this.splitSemanetes(line1);
            System.Collections.ArrayList array_sems1 = new System.Collections.ArrayList();
            for (int i = 0; i < sems1.Length; i++)
            {
                array_sems1.Add(sems1[i]);
            }

            string[] sems2 = this.splitSemanetes(line2);
            System.Collections.ArrayList array_sems2 = new System.Collections.ArrayList();
            for (int j = 0; j < sems2.Length; j++)
            {
                array_sems2.Add(sems2[j]);
            }

            Stack<double> stk_max_sim = new Stack<double>();

            int len1 = array_sems1.Count;
            int len2 = array_sems2.Count;

            while (len1 != 0 && len2 != 0)
            {
                int m = -1, n = -1;
                double max_sim = 0d;
                for (int i = 0; i < len1; i++)
                {
                    for (int j = 0; j < len2; j++)
                    {
                        double simil = calSimBase((string)array_sems1[i], (string)array_sems2[j]);
                        if (simil > max_sim)
                        {
                            m = i;
                            n = j;
                            max_sim = simil;
                        }
                    }
                }

                if (max_sim == 0d)
                {
                    break;
                }

                stk_max_sim.Push(max_sim);

                if (m != -1)
                {
                    array_sems1.RemoveAt(m);
                }

                if (n != -1)
                {
                    array_sems2.RemoveAt(n);
                }

                len1 = array_sems1.Count;
                len2 = array_sems2.Count;
            }

            //把整体相似度还原为部分相似度的加权平均,这里权值取一样，即计算算术平均 
            if (stk_max_sim.Count == 0)
            {
                return 0d;
            }

            double sum = 0.0;
            int count = stk_max_sim.Count;
            while (stk_max_sim.Count > 0)
            {
                sum += stk_max_sim.Pop();
            }

            return sum / count;
        }

        /// <summary>
        /// 计算关系义原描述式的相似度
        /// </summary>
        /// <param name="line1"></param>
        /// <param name="line2"></param>
        /// <returns></returns>
        private double calSim3(string line1, string line2)
        {
            if (line1 == "" && line2 == "")
            {
                return 1d;
            }

            if (line1 == "" || line2 == "")
            {
                return 0d;
            }

            string[] sems1 = splitSemanetes(line1);
            System.Collections.ArrayList array_sems1 = new System.Collections.ArrayList();
            for (int i = 0; i < sems1.Length; i++)
            {
                array_sems1.Add(sems1[i]);
            }


            string[] sems2 = splitSemanetes(line2);
            System.Collections.ArrayList array_sems2 = new System.Collections.ArrayList();
            for (int j = 0; j < sems2.Length; j++)
            {
                array_sems2.Add(sems2[j]);
            }

            Stack<double> stk_sim = new Stack<double>();

            int len1 = array_sems1.Count;
            int len2 = array_sems2.Count;

            while (len1 != 0 && len2 != 0)
            {
                for (int j = 0; j < len2; j++)
                {
                    double ss = calSimReal((string)array_sems1[len1 - 1], (string)array_sems2[j]);
                    if (ss != 0)
                    {
                        stk_sim.Push(ss);
                        array_sems2.RemoveAt(j);
                        break;
                    }
                }

                array_sems1.RemoveAt(len1 - 1);

                len1 = array_sems1.Count;
                len2 = array_sems2.Count;
            }

            if (stk_sim.Count == 0)
            {
                return 0d;
            }

            double sum = 0d;

            int count = stk_sim.Count;
            while (stk_sim.Count > 0)
            {
                sum += stk_sim.Pop();
            }
            return sum / count;
        }

        /// <summary>
        /// 计算符号义原描述式的相似度 
        /// </summary>
        /// <param name="line1"></param>
        /// <param name="line2"></param>
        /// <returns></returns>
        private double calSim4(string line1, string line2)
        {
            if (line1 == "" && line2 == "")
            {
                return 1d;
            }

            if (line1 == "" || line2 == "")
            {
                return 0d;
            }

            string[] sems1 = splitSemanetes(line1);
            System.Collections.ArrayList array_sems1 = new System.Collections.ArrayList();
            for (int i = 0; i < sems1.Length; i++)
            {
                array_sems1.Add(sems1[i]);
            }


            string[] sems2 = splitSemanetes(line2);
            System.Collections.ArrayList array_sems2 = new System.Collections.ArrayList();
            for (int j = 0; j < sems2.Length; j++)
            {
                array_sems2.Add(sems2[j]);
            }

            Stack<double> stk_sim = new Stack<double>();

            int len1 = array_sems1.Count;
            int len2 = array_sems2.Count;

            while (len1 != 0 && len2 != 0)
            {
                char sym1 = ((string)array_sems1[len1 - 1])[0];
                for (int j = 0; j < len2; j++)
                {
                    char sym2 = ((string)array_sems2[j])[0];
                    if (sym1 == sym2)
                    {
                        string base1 = ((string)array_sems1[len1 - 1]).Substring(1);
                        string base2 = ((string)array_sems2[j]).Substring(1);
                        double ss = calSimBase(base1, base2);
                        stk_sim.Push(ss);
                        array_sems2.RemoveAt(j);
                        break;
                    }
                }

                array_sems1.RemoveAt(len1 - 1);

                len1 = array_sems1.Count;
                len2 = array_sems2.Count;
            }

            if (stk_sim.Count == 0)
            {
                return 0d;
            }

            double sum = 0d;

            int count = stk_sim.Count;
            while (stk_sim.Count > 0)
            {
                sum += stk_sim.Pop();
            }
            return sum / count;
        }

        /// <summary>
        /// 计算两个“概念”的相似度
        /// </summary>
        /// <param name="concept1"></param>
        /// <param name="concept2"></param>
        /// <returns></returns>
        private double calConceptSim(string concept1, string concept2)
        {
            if (concept1[0] == '{')//概念1是虚词
            {
                if (concept2[0] != '{')////概念2是实词 
                {
                    return 0;
                }
                else //概念2是虚词 
                {
                    ////去掉"{" and "}" 
                    string sem1 = concept1.Substring(1, concept1.Length - 2);
                    string sem2 = concept2.Substring(1, concept2.Length - 2);

                    int pos1 = sem1.IndexOf("=");
                    int pos2 = sem2.IndexOf("=");
                    if (pos1 == -1 ^ pos2 == -1)//一个句法义原，一个是关系义原
                    {
                        return 0;
                    }
                    else if (pos1 == -1 && pos2 == -1)//都是句法义原
                    {
                        return calSimBase(sem1, sem2);
                    }
                    else//都是关系义原 
                    {
                        return calSimReal(sem1, sem2);
                    }
                }
            }
            else //概念1是实词
            {
                if (concept2[0] == '{') //概念2是虚词 
                {
                    return 0d;
                }
                else//概念2是实词 
                {
                    //分别计算4种描述式的相似度 
                    double sim1 = 0.0;
                    double sim2 = 0.0;
                    double sim3 = 0.0;
                    double sim4 = 0.0;

                    char[] separator = { ',' };

                    string[] sems1 = concept1.Split(separator, StringSplitOptions.RemoveEmptyEntries);
                    string[] sems2 = concept2.Split(separator, StringSplitOptions.RemoveEmptyEntries);

                    string BaseSem1 = "";
                    string BaseOtherSem1 = "";
                    string RelSem1 = "";
                    string SymbolSem1 = "";

                    string BaseSem2 = "";
                    string BaseOtherSem2 = "";
                    string RelSem2 = "";
                    string SymbolSem2 = "";

                    bool isfirst = true;

                    for (int i = 0; i < sems1.Length; i++)
                    {
                        string s = sems1[i].Trim();
                        if (s.Contains("="))
                        {
                            if (RelSem1 == "")
                            {
                                RelSem1 += s;
                            }
                            else
                            {
                                RelSem1 += "," + s;
                            }
                        }
                        else if (_RelationalSymbol.IndexOf(s[0]) != -1)
                        {
                            if (SymbolSem1 == "")
                            {
                                SymbolSem1 += s;
                            }
                            else
                            {
                                SymbolSem1 += "," + s;
                            }

                        }
                        else if (isfirst)
                        {
                            BaseSem1 += s;
                            isfirst = false;
                        }
                        else
                        {
                            if (BaseOtherSem1 == "")
                            {
                                BaseOtherSem1 += s;
                            }
                            else
                            {
                                BaseOtherSem1 += "," + s;
                            }
                        }
                    }

                    isfirst = true;
                    for (int i = 0; i < sems2.Length; i++)
                    {
                        string s = sems2[i].Trim();
                        if (s.Contains("="))
                        {
                            if (RelSem2 == "")
                            {
                                RelSem2 += s;
                            }
                            else
                            {
                                RelSem2 += "," + s;
                            }
                        }
                        else if (_RelationalSymbol.IndexOf(s[0]) != -1)
                        {
                            if (SymbolSem2 == "")
                            {
                                SymbolSem2 += s;
                            }
                            else
                            {
                                SymbolSem2 += "," + s;
                            }
                        }
                        else if (isfirst)
                        {
                            BaseSem2 += s;
                            isfirst = false;
                        }
                        else
                        {
                            if (BaseOtherSem2 == "")
                            {
                                BaseOtherSem2 += s;
                            }
                            else
                            {
                                BaseOtherSem2 += "," + s;
                            }
                        }
                    }

                    if (!string.IsNullOrEmpty(BaseSem1) && !string.IsNullOrEmpty(BaseSem2))
                    {
                        sim1 = this.calSim1(BaseSem1, BaseSem2);
                    }
                    else
                    {
                        sim1 = 0.01;
                    }

                    sim2 = this.calSim2(BaseOtherSem1, BaseOtherSem2);
                    sim3 = this.calSim3(RelSem1, RelSem2);
                    sim4 = this.calSim4(SymbolSem1, SymbolSem2);

                    return _beta1 * sim1 +
                            _beta2 * sim1 * sim2 +
                            _beta3 * sim1 * sim2 * sim3 +
                            _beta4 * sim1 * sim2 * sim4 * sim4;
                }
            }
        }

        /// <summary>
        /// 计算两个词语的相似度
        /// </summary>
        /// <param name="word1"></param>
        /// <param name="word2"></param>
        /// <returns></returns>
        public double calWordSim(string word1, string word2)
        {
            string[] concepts1 = this.getConcepts(word1);
            string[] concepts2 = this.getConcepts(word2);

            if (concepts1 == null)
            {
                System.Diagnostics.Trace.WriteLine(word1 + "不在字典中");
                return 0d;
            }

            if (concepts2 == null)
            {
                System.Diagnostics.Trace.WriteLine(word2 + "不在字典中");
                return 0d;
            }

            int len1 = concepts1.Length;
            int len2 = concepts2.Length;

            if (len1 == 0)
            {
                System.Diagnostics.Trace.WriteLine(word1 + "不在字典中");
                return 0d;
            }
            if (len2 == 0)
            {
                System.Diagnostics.Trace.WriteLine(word2 + "不在字典中");
                return 0d;
            }

            double maxsim = 0d;
            for (int i = 0; i < len1; i++)
            {
                for (int j = 0; j < len2; j++)
                {
                    double sim = calConceptSim(concepts1[i], concepts2[j]);
                    if (sim > maxsim) { maxsim = sim; }
                }
            }

            return maxsim;
        }

        /// <summary>
        /// 计算两个词语的相似度
        /// </summary>
        /// <param name="word1"></param>
        /// <param name="word2"></param>
        /// <param name="wordtype"></param>
        /// <returns></returns>
        public double calWordSim(string word1, string word2, WordType wordtype)
        {
            string[] concepts1Type = null;
            string[] concepts1 = this.getConcepts(word1,ref concepts1Type);
            string[] concepts2Type = null;
            string[] concepts2 = this.getConcepts(word2,ref concepts2Type);

            if (concepts1 == null)
            {
                System.Diagnostics.Trace.WriteLine(word1 + "不在字典中");
                return 0d;
            }

            if (concepts2 == null)
            {
                System.Diagnostics.Trace.WriteLine(word2 + "不在字典中");
                return 0d;
            }

            int len1 = concepts1.Length;
            int len2 = concepts2.Length;

            if (len1 == 0)
            {
                System.Diagnostics.Trace.WriteLine(word1 + "不在字典中");
                return 0d;
            }
            if (len2 == 0)
            {
                System.Diagnostics.Trace.WriteLine(word2 + "不在字典中");
                return 0d;
            }

            double maxsim = 0d;
            for (int i = 0; i < len1; i++)
            {
                if (concepts1Type[i].Trim().ToUpper() != wordtype.ToString().ToUpper())
                {
                    continue; 
                }

                for (int j = 0; j < len2; j++)
                {
                    if (concepts1Type[i].Trim().ToUpper() == concepts2Type[j].Trim().ToUpper())
                    {
                        double sim = calConceptSim(concepts1[i], concepts2[j]);
                        if (sim > maxsim) { maxsim = sim; }
                    }
                }
            }
            return maxsim;
        }

        private string[] splitSemanetes(string line)
        {
            char[] separator = { ',' };
            string[] sems = line.Split(separator, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < sems.Length; i++)
            {
                sems[i] = sems[i].Replace("(", "");
                sems[i] = sems[i].Replace(")", "");
            }

            return sems;
        }

        /// <summary>
        /// 计算倾向度
        /// </summary>
        /// <param name="word"></param>
        /// <param name="wordtype"></param>
        /// <returns></returns>
        public double calOrientation(string word,WordType wordtype)
        {
            double max_sim_pos = 0d;
            double max_sim_neg = 0d;

            foreach (KeyValuePair<String, String> item in this.basePosEmotionDic)
            {
                double tmp = this.calWordSim(word, item.Key,wordtype);
                if (tmp > max_sim_pos)
                {
                    max_sim_pos = tmp;
                }
            }

            foreach (KeyValuePair<String, String> item in this.baseNegEmotionDic)
            {
                double tmp = this.calWordSim(word, item.Key,wordtype);
                if (tmp > max_sim_neg)
                {
                    max_sim_neg = tmp;
                }
            }
            return max_sim_pos - max_sim_neg;
        }        
    }
}
