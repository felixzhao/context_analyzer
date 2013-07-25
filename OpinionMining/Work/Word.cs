using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Work
{
    //相似度、分析词语
    public class Word
    {
        private string word;//词语本身
        private string type;//词语类型ADJ NUM N PREP等
        private string related;

        //第一基本义原。
        private string firstPrimitive;
        //其他基本义原。  --除了基本义原之外的，如果前面没有符号#$@!等，也不含有=的义原。
        private List<string> otherPrimitives = new List<string>();
        //如果该list非空，则该词是一个虚词。 列表里存放的是该虚词的一个义原，部分虚词无中文虚词解释
        private List<string> structruralWords = new List<string>();
        //该词的关系义原。key: 关系义原。 value： 基本义原|(具体词)的一个列表
        private Dictionary<string, List<string>> relationalPrimitives = new Dictionary<string, List<string>>();
        //该词的关系符号义原。Key: 关系符号。 value: 属于该挂系符号的一组基本义原|(具体词)
        private Dictionary<string, List<string>> relationSimbolPrimitives = new Dictionary<string, List<string>>();
        //获取词语本身
        public string getWord()
        {
            return word;
        }
        public string getRelated()
        {
            return related;
        }
        public void setRelated(string related)
        {
            this.related = related;
        }
        //设置词语
        public void setWord(string word)
        {
            this.word = word;
        }
        //获取词语类型
        public string getType()
        {
            return type;
        }
        //设置词语类型--词性--Part of Speech
        public void setType(string type)
        {
            this.type = type;
        }
        //获取第一个义原
        public string getFirstPrimitive()
        {
            return firstPrimitive;
        }
        //设置第一个义原
        public void setFirstPrimitive(string firstPrimitive)
        {
            this.firstPrimitive = firstPrimitive;
        }
        //获取其他义原
        public List<string> getOtherPrimitives()
        {
            return otherPrimitives;
        }
        //设置其他义原
        public void setOtherPrimitives(List<string> otherPrimitives)
        {
            this.otherPrimitives = otherPrimitives;
        }
        //添加其他义原，往List<string>里面增加string类型的 <<义原>>
        public void addOtherPrimitive(string otherPrimitive)
        {
            this.otherPrimitives.Add(otherPrimitive);
        }
        //获取结构义原
        public List<string> getStructruralWords()
        {
            return structruralWords;
        }
        //是否为虚词--如果是虚词，该structruralWords非空。
        public bool isStructruralWord()
        {
            return !(structruralWords.Count == 0);
        }
        //设置结构义原
        public void setStructruralWords(List<string> structruralWords)
        {
            this.structruralWords = structruralWords;
        }
        //添加结构义原
        public void addStructruralWord(string structruralWord)
        {
            this.structruralWords.Add(structruralWord);
        }
        //获取关系义原
        public Dictionary<string, List<string>> getRelationalPrimitives()
        {
            return relationalPrimitives;
        }
        //获取关系符号义原
        public Dictionary<string, List<string>> getRelationSimbolPrimitives()
        {
            return relationSimbolPrimitives;
        }
        //添加关系义原
        //如果关系义原的key对应的List为空，就新建一个，增加value。
        //否则，就直接在关系义原的key对应的List里面直接增加value。
        public void addRelationalPrimitive(string key, string value)
        {
            List<string> list = null;
            if (relationalPrimitives.ContainsKey(key))
            {
                list = relationalPrimitives[key];
                list.Add(value);
            }
            else {
                list = new List<string>();
                list.Add(value);
                relationalPrimitives.Add(key, list);
            }
        }
        //添加结构符号义原
        public void addRelationSimbolPrimitive(string key, string value)
        {
            List<string> list = null;
            if (relationSimbolPrimitives.ContainsKey(key))
            {
                list = relationSimbolPrimitives[key];
                list.Add(value);
            }
            else
            {
                list = new List<string>();
                list.Add(value);
                relationSimbolPrimitives.Add(key, list);
            }
           
        }

    }
}
