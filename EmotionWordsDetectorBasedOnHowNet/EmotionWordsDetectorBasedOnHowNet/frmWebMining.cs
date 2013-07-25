using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Collections; 

namespace WebMining
{
    public partial class frmWebMining : Form
    {
        private string selectedDirectory = "";   

        public frmWebMining()
        {
            InitializeComponent();
            Common.loadHowNetEmotionalDictionary();  
        }



        private void btnSelect_Click(object sender, EventArgs e)
        {
            this.folderBrowserDialog1.Description = "Select the directory that you want to load flat file";
            this.folderBrowserDialog1.RootFolder = Environment.SpecialFolder.Desktop; 
            if (this.folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                this.txtDirectory.Text = this.folderBrowserDialog1.SelectedPath; 
            }
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            if (!String.IsNullOrEmpty(this.txtDirectory.Text))
            {
                if (Directory.Exists(this.txtDirectory.Text))
                {
                    selectedDirectory = this.txtDirectory.Text;

                    //Scan flat files located in selected directory
                    String[] filenames = Directory.GetFiles(selectedDirectory, "*.txt", SearchOption.TopDirectoryOnly);
                    for (int i = 0; i < filenames.Length; i++)
                    {
                        string filename = filenames[i].Substring(selectedDirectory.Length + 1, filenames[i].Length - selectedDirectory.Length - 1);
                        this.chkListFiles.Items.Add(filename,true);
                        this.chkSelectAll.Checked = true;  
                    }                    
                }
                else
                {
                    MessageBox.Show("Selected directory does not exist","Warning",MessageBoxButtons.OK,MessageBoxIcon.Warning);    
                }
            }
            else
            {
                MessageBox.Show("Please select directory that you want to load flat file.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);    
            }
        }

        private void chkSelectAll_CheckedChanged(object sender, EventArgs e)
        {
            for (int i = 0; i < this.chkListFiles.Items.Count; i++)
            {
                this.chkListFiles.SetItemChecked(i, this.chkSelectAll.Checked);   
            }
        }

        private void btnAnalyze_Click(object sender, EventArgs e)
        {
            this.txtContent.Text = "开始进行分析，这需要几分钟时间，请耐心等待";

            if (!ICTCLAS.initialize())
            {
                MessageBox.Show("Split Word Tools does not initialize!","Error",MessageBoxButtons.OK,MessageBoxIcon.Error); 
                return;
            }

            
            System.Data.DataTable dt = new DataTable();
            dt.Columns.Add(new DataColumn("emotion", typeof(string)));
            dt.Columns.Add(new DataColumn("confidence", typeof(double)));
            dt.Columns.Add(new DataColumn("word",typeof(string)));
            dt.Columns.Add(new DataColumn("wordtype",typeof(string)));  
            dt.Columns.Add(new DataColumn("sentence",typeof(string)));
  

            foreach (var item in this.chkListFiles.CheckedItems)
            {
                string filename = item.ToString();                
                StreamReader rd = new StreamReader(selectedDirectory+"\\"+filename, Encoding.GetEncoding("gb2312"), true);
                string filedata = rd.ReadToEnd().Trim().Replace("\r", "");
                filedata = filedata.Replace("\n", "");
                char[] delimiterChars = { '。', '！', '?' };
                string[] seArray = filedata.Split(delimiterChars,StringSplitOptions.RemoveEmptyEntries);
                
                for (int i = 0; i < seArray.Length; i++)
                {
                    seArray[i] = seArray[i].Trim();
                     
                    if (!String.IsNullOrEmpty(seArray[i]))
                    {
                        ArrayList wordList = new ArrayList();
                        ICTCLAS.splitword(seArray[i], wordList);

                        foreach (SegWord term in wordList)
                        {
                            if (Common.isEmotionWordType(term.szWord.Trim(), term.wordType) && term.szWord.Trim().Length>1)
                            {
                                WordType wordtype = WordType.UNSET;
                                if (term.wordType[0] == 'n')
                                {
                                    wordtype = WordType.N;  
                                }
                                else if (term.wordType[0] == 'v')
                                {
                                    wordtype = WordType.V;
                                }
                                else if (term.wordType[0] == 'a')
                                {
                                    wordtype = WordType.ADJ;
                                }
                                else if (term.wordType[0] == 'd')
                                {
                                    wordtype = WordType.ADV;
                                }

                                Word word = new Word(term.szWord.Trim(), wordtype);
                                word=Common.checkEmotion(word, double.Parse(this.txtThreshold.Text));   

                                if(word.Emotional!=Emotion.NA)
                                {
                                    string sentence = this.outputEmotionSentence(Common.number, term.szWord.Trim(), term.Number, word.Emotional, wordList, item.ToString(), word.Confidential);
                                    DataRow row = dt.NewRow();
                                    row["emotion"] = word.Emotional.ToString();
                                    row["confidence"] = word.Confidential;
                                    row["word"] = word.szTerm;
                                    row["wordtype"] = word.szWordType.ToString();
                                    row["sentence"] = sentence;
                                    dt.Rows.Add(row);                              
                                }                                
                            }
                        }
                    }
                }
            }

            DataView dv = new DataView(dt);
            Hashtable ht = new Hashtable(); 
            dv.Sort = "confidence desc";
            int id = 0;
            StringBuilder sb = new StringBuilder();
            foreach (DataRowView row in dv)
            {
                if (!ht.ContainsKey((string)row["word"]))
                {
                    if (id < 100)
                    {
                        sb.AppendLine((id + 1).ToString() + "\t" + (string)row["sentence"]);
                        ht.Add((string)row["word"], (string)row["word"]); 
                    }
                    id++;
                }
            }

            if (sb.Length > 0)
            {
                this.txtContent.Text = sb.ToString();
            }
            else
            {
                this.txtContent.Text = "分析结束，没有任何情感次"; 
            }
            ICTCLAS.uninitialize();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            this.saveFileDialog1.Filter = "Text File(*.txt)|*.txt";
            if (this.saveFileDialog1.ShowDialog()==DialogResult.OK)
            {
                if (File.Exists(this.saveFileDialog1.FileName))
                {
                    File.Delete(this.saveFileDialog1.FileName);
                }

                StreamWriter sw = new StreamWriter(this.saveFileDialog1.FileName, false, Encoding.GetEncoding("gb2312"));//File.CreateText(this.saveFileDialog1.FileName);
                sw.Write(this.txtContent.Text);
                sw.Flush(); 
                sw.Close();

                MessageBox.Show("Write Successfully","Success",MessageBoxButtons.OK,MessageBoxIcon.Information);  
            }
        }

        private string outputEmotionSentence(string studentID,string emotionWord, int pos, Emotion em, ArrayList wordList, string docid,double orientation)
        {
            //在HowNet情感词中存在，输出学号，情感词，文档编号，方向，前后取20个词
            string tmp = studentID + "\t" + emotionWord + "\t" + em.ToString() + "\t" + docid + "\t"+string.Format("{0:0.000}",orientation)+"\t";
            int start = pos - 10;
            if (start < 0) { start = 0; }
            int end = pos + 10;
            if (end > wordList.Count - 1) { end = wordList.Count - 1; }

            string content = "";

            for (int j = start; j <= pos; j++)
            {
                content += ((SegWord)wordList[j]).szWord;
            }


            for (int j = pos + 1; j <= end; j++)
            {
                content += ((SegWord)wordList[j]).szWord;
            }

            tmp += content + "\r\n";
            return tmp;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //Similarity similarity = new Similarity();
            //double bbb = similarity.calOrientation("警惕", WordType.N);  
            
        }


    }
}
