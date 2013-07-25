using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

namespace Work
{
    //相似度计算
    public class WordSimilarity
    {
        public string Path { get; set; }
        public Dictionary<string, List<Word>> ALLWORDS = new Dictionary<string, List<Word>>();
        // sim(p1,p2)=alpha/(d+alpha)
        private double alpha = 1.6;
        private double beta1 = 0.5;
        private double beta2 = 0.2;
        private double beta3 = 0.17;
        private double beta4 = 0.13;
        //具体词与义原的相似度一律处理为一个比较小的常数. 具体词和具体词的相似度，如果两个词相同，则为1，否则为0
        private double gamma = 0.2;
        // 将任一非空值与空值的相似度定义为一个比较小的常数
        private double delta = 0.2;
        //两个无关义原之间的默认距离
        private int DEFAULT_PRIMITIVE_DIS = 20;
        //知网中的逻辑符号
        private static String LOGICAL_SYMBOL = ",~^";
        //知网中的关系符号, 如果含有下面的符号，说明为义项的关系义原
        private String RELATIONAL_SYMBOL = "#%$*+&@?!";
        //知网中的特殊符号，虚词，或具体词,知网中的虚词都是用{}括起来的
        private string SPECIAL_SYMBOL = "{";
        public WordSimilarity()
        {
            //  loadGlossary();
        }

        //义原表
        public Primitive primitive { get; set; }

        //义项
        public void loadGlossary()
        {
            StreamReader read = null;
            string strLine = "";
            try
            {
                FileStream fs = new FileStream(Path, FileMode.OpenOrCreate, FileAccess.Read);
                read = new StreamReader(fs, Encoding.GetEncoding("GB2312"));
                read.BaseStream.Seek(0, SeekOrigin.Begin);
                strLine = read.ReadLine();
                //strLine = read.ReadLine();
                strLine = strLine.Trim().ToString();
                int jj = 0;
                while (strLine != null)
                {

                    /* #region regexStr
                   string matchStr = @"\w+\s\s+\d";
                   Regex r = new Regex(matchStr);
                   Match m = r.Match(strLine, 0, strLine.Length);
                   string tpline = strLine;
                   string tp = null;
                   while (m.Success)
                   {
                       tp = m.Value.ToString();
                       tp = tp.Remove(tp.Length - 1);
                       string tp2 = tp.Trim().ToString() + " ";
                       tpline = tpline.Replace(tp, tp2);
                       m = m.NextMatch();
                   }

                   strLine = tpline;
                  
                      string matchStrT = @"\w+\s+";
                      Regex rT = new Regex(matchStrT);
                      Match mT = rT.Match(strLine, 0, strLine.Length);
                      string tplineT = strLine;
                      string tpT = null;
                      while (mT.Success)
                      {
                          tpT = mT.Value.ToString();
                          string tp2T = tpT.Trim().ToString() + " ";
                          tplineT = tplineT.Replace(tpT, tp2T);
                          mT = mT.NextMatch();
                      }
                      strLine = tplineT;
                      #endregion regexStr */
                    string[] strs = System.Text.RegularExpressions.Regex.Split(strLine, @"\s+");
                    string word = strs[0];//第一个是词语
                    string type = strs[1];//第二个是类型
                    string related = strs[2];
                    //Log.Write2File("---" + jj.ToString()+  ", " + word + ", " + type + ", " + related + "---");

                    /*   //因为是按空格划分，最后一部分的加回去--解释：因为后面的每组词语之间可能存在空格。
                    for (int i = 3; i < strs.Length; i++)
                    {
                        related += (" " + strs[i]);

                    }*/
                    //jj++;
                    //if (jj == 3800)
                    //jj = 0;
                    //对前面两部分创建一个新词
                    Word w = new Word();
                    w.setWord(word);
                    w.setType(type);
                    w.setRelated(related);

                    parseDetail(related, w);//知网中的关键部分在于这个函数中是如何解析后面的义原的。
                    // save this word.


                    addWord(w);
                    //Write2File(outpath, "-success--");
                    // read the next line

                    strLine = read.ReadLine();
                    if (strLine != null)
                    {
                        strLine = strLine.Trim().ToString();
                    }

                }
            }
            catch (Exception ex)
            {
                Log.Write2File("-YiXiang fail--" + ex.Message);
            }
            finally
            {
                try
                {
                    read.Close();
                }
                catch (Exception ex)
                {

                }
            }

        }

        //解析具体概念部分，将解析的结果存入<code>Word word</code>
        public void parseDetail(string related, Word word)
        {
            bool isSpecial = false; // 是否虚词
            //首先判断是否虚词，虚词例子{firstPerson|我,mass|众}
            if (related.StartsWith(SPECIAL_SYMBOL))
            {
                isSpecial = true;//是虚词
                related = related.Substring(1, related.Length - 2);
            }

            //按照,分开 例子：aValue|属性值,attachment|归属,#country|国家,ProperName|专
            string[] parts = related.Split(',');
            bool isFirst = true;//是否为第一个，主要是为了获取第一义原而设置的参数
            bool isRelational = false;//是否是关系义原
            bool isSimbol = false;//是否符号义原
            string chinese = null;//是否是中文
            string relationalPrimitiveKey = null;//关系义原的KEY值
            string simbolKey = null;//符号的KEY
            for (int i = 0; i < parts.Length; i++)
            {
                if (isSpecial == true)
                {
                    string[] strs = parts[i].Split('|');
                    if (strs.Length > 1)
                    {
                        // |后面的是中文的值VALUE
                        word.addStructruralWord(strs[1]);
                    }
                    else
                    {
                        //如果没有中文部分，则使用英文词
                        word.addStructruralWord(strs[0]);
                    }
                    continue;
                }
                //如果是具体词，则以括号开始和结尾: (Afghanistan|阿富汗)
                if (parts[i].StartsWith("("))
                {
                    //去掉具体词的左右两个括号，剩下的parts[i]存放的是具体词本身
                    parts[i] = parts[i].Substring(1, parts[i].Length - 1);
                    // parts[i] = parts[i].Replace("\\s+", "");
                }
                //关系义原，之后的都是关系义原
                if (parts[i].Contains("="))
                {
                    isRelational = true;//关系义原标志TRUE
                    // format: content=fact|事情
                    string[] strs = parts[i].Split('=');
                    relationalPrimitiveKey = strs[0];//等号前面的是KEY
                    string[] values = strs[1].Split('|');
                    if (values.Length > 1)
                    {  //等号后面的|后面的是中文的值VALUE
                        if (relationalPrimitiveKey != string.Empty)
                        {
                            word.addRelationalPrimitive(relationalPrimitiveKey, values[1]);//添加关系义原
                        }
                    }
                    //继续下一个循环
                    continue;
                }
                //获取中文部分chinese
                string[] strss = parts[i].Split('|');
                // 其中中文部分的词语,部分虚词没有中文解释
                if (strss.Length > 1)
                {
                    //获取|后面的中文部分
                    chinese = strss[1];
                }
                //下面中文部分，还有可能存在是 具体词
                if (chinese != null && (chinese.EndsWith(")") || chinese.EndsWith("}")))
                {
                    //小标从0开始是因为：(Europe|欧洲)
                    chinese = chinese.Substring(0, chinese.Length - 1);

                }
                //开始的第一个字符，确定 这个义原的类别
                int type = getPrimitiveType(strss[0]);
                //如果这个是义原
                if (type == 0)
                {
                    // 之前有一个关系义原，后面的就都加入到关系义原的链表中。
                    if (isRelational)
                    {
                        word.addRelationalPrimitive(relationalPrimitiveKey, chinese);
                        continue;
                    }
                    // 之前有一个是符号义原，后面的就都加入到符号义原中。
                    if (isSimbol)
                    {
                        word.addRelationSimbolPrimitive(simbolKey, chinese);
                        continue;
                    }
                    //如果是第一个，加入到基本义原列表，否则，加入到其他义原列表。
                    if (isFirst)
                    {
                        word.setFirstPrimitive(chinese);
                        isFirst = false;
                        continue;
                    }
                    else
                    {
                        word.addOtherPrimitive(chinese);
                        continue;
                    }

                }
                // 关系符号表
                if (type == 1)
                {
                    isSimbol = true;
                    isRelational = false;
                    //取出前面的第一个符号，即关系义原的符号!@#$%*等
                    simbolKey = (strss[0].ToCharArray()[0]).ToString();
                    //添加到关系符号义原的链表中
                    word.addRelationSimbolPrimitive(simbolKey, chinese);
                    continue;
                }

            }

        }
        //从英文部分确定这个义原的类别。
        //return 一个代表类别的整数，其值为0,1
        public int getPrimitiveType(string str)
        {
            string first = (str.ToCharArray()[0]).ToString();
            if (RELATIONAL_SYMBOL.Contains(first))
            {
                return 1;//符号义原
            }
            /* 这里去掉虚词的判断，虚词判断改在parseDetail里判断
            if (SPECIAL_SYMBOL.Contains(first))
            {
                return 2;//虚词
            }*/
            return 0;//基本义原
        }


        //计算两个词语的相似度
        //计算两个词语之间的相似度，取两个词语的所有义项之间的最大值作为两个词语的相似度
        public double simWord(string word1, string word2)
        {
            if (ALLWORDS.ContainsKey(word1) && ALLWORDS.ContainsKey(word2))
            {
                List<Word> list1 = ALLWORDS[word1];//同一个词语，有多少行，就存在多少个概念(义项)
                List<Word> list2 = ALLWORDS[word2];
                double max = 0;
                Word[] lst1 = new Word[list1.Count];
                list1.CopyTo(lst1, 0);
                Word[] lst2 = new Word[list2.Count];
                list2.CopyTo(lst2, 0);
                for (int i = 0; i < lst1.Length; i++)
                {
                    Word w1 = lst1[i];
                    for (int j = 0; j < lst2.Length; j++)
                    {
                        Word w2 = lst2[j];
                        double sim = simWord(w1, w2);
                        max = (sim > max) ? sim : max;
                    }
                }
                return max;
            }
            return 0;

        }
        //输入2个词语
        public double simWord(Word w1, Word w2)
        {
            // 虚词和实词的相似度为零
            if (w1.isStructruralWord() != w2.isStructruralWord())
            {
                return 0;
            }
            //虚词
            //由于虚词概念总是用“{句法义原}”或“{关系义原}”这两种方式进行描述，所以，虚词概念的相似度计算非常简单，只需要计算其对应的句法义原或关系义原之间的相似度即可。
            if (w1.isStructruralWord() && w2.isStructruralWord())
            {
                List<string> list1 = w1.getStructruralWords();
                List<string> list2 = w2.getStructruralWords();
                return simList(list1, list2);
            }
            //实词
            if (!w1.isStructruralWord() && !w2.isStructruralWord())
            {
                // 实词的相似度分为4个部分
                // 基本义原相似度
                string firstPrimitive1 = w1.getFirstPrimitive();
                string firstPrimitive2 = w2.getFirstPrimitive();
                double sim1 = simPrimitive(primitive, firstPrimitive1, firstPrimitive2);
                // 其他基本义原相似度
                List<string> list1 = w1.getOtherPrimitives();
                List<string> list2 = w2.getOtherPrimitives();
                double sim2 = simList(list1, list2);
                // 关系义原相似度
                Dictionary<string, List<string>> dic1 = w1.getRelationalPrimitives();
                Dictionary<string, List<string>> dic2 = w2.getRelationalPrimitives();
                double sim3 = simDictionary(dic1, dic2);
                // 关系符号相似度
                dic1 = w1.getRelationSimbolPrimitives();
                dic2 = w2.getRelationSimbolPrimitives();
                double sim4 = simDictionary(dic1, dic2);

                double product = sim1;
                double sum = beta1 * product;
                product *= sim2;
                sum += beta2 * product;
                product *= sim3;
                sum += beta3 * product;
                product *= sim4;
                sum += beta4 * product;
                return sum;
            }
            return 0;
        }

        //具体计算
        //比较两个集合的相似度
        public double simList(List<string> list1, List<string> list2)
        {
            if (list1.Count == 0 && list2.Count == 0)
                return 1;
            int m = list1.Count;//第一个列表的个数
            int n = list2.Count;//第二个列表的个数
            int big = m > n ? m : n;//较大的
            int N = (m < n) ? m : n;//较小的
            int count = 0;
            int index1 = 0, index2 = 0;//索引
            double sum = 0;
            double max = 0;
            while (count < N)
            {
                max = 0;
                for (int i = 0; i < list1.Count; i++)
                {
                    for (int j = 0; j < list2.Count; j++)
                    {
                        //计算两个义原的相似度，找到最大相似度的进行配对。
                        //如果是义原，就计算义原的距离
                        //如果是具体词，相同为1，不同为0。
                        double sim = innerSimWord(primitive, list1[i], list2[j]);
                        if (sim > max)
                        {
                            index1 = i;
                            index2 = j;
                            max = sim;
                        }
                    }

                }
                //累加值计算
                sum += max;
                //移除配对成功的数据
                list1.RemoveAt(index1);
                list2.RemoveAt(index2);
                count++;//配对成功的个数。--其实肯定是N个（链表个数的最小值）
            }
            return (sum + delta * (big - N)) / big;
        }

        //内部比较两个词，可能是为具体词，也可能是义原
        private double innerSimWord(Primitive p, string word1, string word2)
        {
            //Primitive p = new Primitive();
            bool isPrimitive1 = p.isPrimitive(word1);
            bool isPrimitive2 = p.isPrimitive(word2);
            //两个义原
            if (isPrimitive1 && isPrimitive2)
                return simPrimitive(p, word1, word2);
            if (!isPrimitive1 && !isPrimitive2)
            {
                if (word1.Equals(word2))
                    return 1;
                else
                    return 0;
            }
            // 义原和具体词的相似度, 默认为gamma=0.2
            return gamma;
        }

        //计算两个义原之间的相似度
        public double simPrimitive(Primitive p, string primitive1, string primitive2)
        {
            int dis = disPrimitive(p, primitive1, primitive2);
            return alpha / (dis + alpha);

        }
        //计算两个义原之间的距离，如果两个义原层次没有共同节点，则设置他们的距离为20。
        public int disPrimitive(Primitive p, string primitive1, string primitive2)
        {
            //Primitive p = new Primitive();
            List<int> list1 = p.getparents(primitive1);
            List<int> list2 = p.getparents(primitive2);
            for (int i = 0; i < list1.Count; i++)
            {
                int id1 = list1[i];
                if (list2.Contains(id1))
                {
                    int index = list2.IndexOf(id1);
                    return index + i;//两个义原在树上的节点的路径数
                }
            }
            return DEFAULT_PRIMITIVE_DIS;
        }
        //特征结构Dictionary的相似度
        public double simDictionary(Dictionary<string, List<string>> dic1, Dictionary<string, List<string>> dic2)
        {
            if (dic1.Count == 0 && dic2.Count == 0)//若两个结构都为空相似度为1；
            {
                return 1;
            }
            int total = dic1.Count + dic2.Count;
            double sim = 0;
            int count = 0;

            foreach (string key in dic1.Keys)
            {
                System.Console.WriteLine(key);

                if (dic2.ContainsKey(key))
                {
                    //如果两个Map中的key相同，就计算两个Map的相似度。
                    List<string> list1 = dic1[key];
                    List<string> list2 = dic2[key];
                    sim += simList(list1, list2);
                    count++;
                }
            }
            return (sim + delta * (total - 2 * count)) / (total - count);

        }

        //加入一个词语
        public void addWord(Word word)
        {
            if (ALLWORDS.ContainsKey(word.getWord()))
            {
                List<Word> list = ALLWORDS[word.getWord()];
                list.Add(word);
            }
            else
            {
                List<Word> list = new List<Word>();
                list.Add(word);
                ALLWORDS.Add(word.getWord(), list);
            }

        }
    }
}
