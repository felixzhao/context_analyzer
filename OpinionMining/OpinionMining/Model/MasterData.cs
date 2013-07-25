using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpinionMining.Model
{
    public class MasterData
    {
        public string DocName { get; set; } //文档名
        public string SentenceId { get; set; } //段落号
        public string WordValue { get; set; }  //词语
        public string WordOrder { get; set; }  //词语在段落中的序号
        public string Property { get; set; }   //词行
        public double Weight { get; set; } //情感词相似度权值
        public string Polarity { get; set; } //极性 pos 或者 neg
    }
}
