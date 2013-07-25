using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

namespace Work
{
    //义原
    public class Primitive
    {
        public string Path { get; set; }
        public Dictionary<int, Primitive> ALLPRIMITIVES = new Dictionary<int, Primitive>();
        public Dictionary<string, int> PRIMITIVESID = new Dictionary<string, int>();

        public Primitive()
        {

        }

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
                int i = 0;
                while (strLine != null)
                {
                    #region test
                    i++;
                    if (i == 999)
                    {
                        string tp5 = "123";
                    }
                    #endregion test
                    //strLine = read.ReadLine();
                    #region regexStr
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
                    #endregion regexStr
                    Log.Write2File("--" + strLine);
                    string[] strs = strLine.Split(' ');
                    int id = int.Parse(strs[0]);
                    string[] words = strs[1].Split('|');
                    string english = words[0];
                    string chinese = strs[1].Split('|')[1];
                    int parentid = int.Parse(strs[2]);
                    //ALLPRIMITIVES.Add(id, new Primitive(id, english, parentid));
                    ALLPRIMITIVES.Add(id, new Primitive(id, english, chinese, parentid));
                    if (!PRIMITIVESID.ContainsKey(chinese))
                        PRIMITIVESID.Add(chinese, id);
                    if (!PRIMITIVESID.ContainsKey(english))
                        PRIMITIVESID.Add(english, id);
                    strLine = read.ReadLine();
                    if (strLine != null)
                    {
                        strLine = strLine.Trim().ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Write2File("-YiYuan fail--" + ex.Message);
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

        //私有变量的定义
        private int id;
        private string primitiveEnglish;
        private string primitiveChinese;
        private int parentid;
        //构造函数 义原（ID,义原，父ID）
        public Primitive(int id, string primitiveEnglish, string primitiveChinese, int parentid)
        {
            this.id = id;
            this.primitiveEnglish = primitiveEnglish;
            this.primitiveChinese = primitiveChinese;
            this.parentid = parentid;
        }
        //获取义原
        public string getPrimitiveChinese()
        {
            return primitiveChinese;
        }
        public string getprimitiveEnglish()
        {
            return primitiveEnglish;
        }
        //ID标志
        public int getId()
        {
            return id;
        }
        //获取父ID
        public int getParentId()
        {
            return parentid;
        }
        //是否是根节点
        public bool isTop()
        {
            return id == parentid;
        }
        //获取一个义原的所有父义原，直到顶层位置
        public List<int> getparents(string primitive)
        {
            List<int> list = new List<int>();
            //获取这个义原的ID
            try
            {
                int id = PRIMITIVESID[primitive];
                //如果存在义原的ID
                if (id >= 0)
                {
                    list.Add(id);//增加义原的ID--换句话说，这个链表包含义原ID本身
                    Primitive parent = ALLPRIMITIVES[id];//获取义原的父亲义原
                    while (!parent.isTop())//判断是否为顶层的节点
                    {
                        list.Add(parent.getParentId());
                        parent = ALLPRIMITIVES[parent.getParentId()];

                    }
                }
            }
            catch (Exception ex)
            {
            }

            //返回逐级存放树的节点的ID，从叶子节点到根节点。
            return list;
        }
        //判断是否为义原
        public bool isPrimitive(string primitive)
        {
            return PRIMITIVESID.ContainsKey(primitive);
        }
    }
}
