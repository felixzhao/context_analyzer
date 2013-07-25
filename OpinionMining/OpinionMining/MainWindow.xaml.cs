using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Configuration;
using Work;
using System.Windows.Threading;
using System.Threading;
using System.Data;
using OpinionMining.BLL;
using OpinionMining.Common;

namespace OpinionMining
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        //是否导入义原
        private bool IsImportYiYuan = false;
        //是否导入义项
        private bool IsImportYiXiang = false;
        //是否导入情感词典
        private bool IsImportDictionary = false;
        //义原
        private Primitive YiYuan;
        //义项
        private WordSimilarity YiXiang;
        //褒义情感词库
        private List<string> PosSentiment;
        //贬义情感词库
        private List<string> NegSentiment;

        //分析出的情感詞結果

        private DataTable dtResult;

        public MainWindow()
        {
            InitializeComponent();
            txtYiYuanPath.Text= ConfigurationSettings.AppSettings["YiYuanPath"];
            txtYiXiangPath.Text = ConfigurationSettings.AppSettings["YiXiangPath"];
            //txtPosSentimentPath.Text = ConfigurationSettings.AppSettings["PosSentimentPath"];
            //txtNegSentimentPath.Text = ConfigurationSettings.AppSettings["NegSentimentPath"];
            txtSentimentPath.Text = ConfigurationSettings.AppSettings["SentimentFolder"];
            txtRawDataPath.Text = ConfigurationSettings.AppSettings["RawDataFolder"];
            txtMasterDataPath.Text = ConfigurationSettings.AppSettings["MasterDataFolder"];
            txtExportPath.Text = ConfigurationSettings.AppSettings["ExportResult"];
        }

        private void MessageInit()
        {
            txtAlertMessage.Text = "";
        }
        private void MessageSuccess(string message)
        {
            txtAlertMessage.Text = txtAlertMessage.Text + message + "; ";
            txtAlertMessage.Foreground = Brushes.Green;
        }
        private void MessageFail(string message)
        {
            txtAlertMessage.Text = txtAlertMessage.Text + message + "; ";
            txtAlertMessage.Foreground = Brushes.Red;
        }

        private void btnImportYiYuan_Click(object sender, RoutedEventArgs e)
        {
            YiYuan = new Primitive();
            IsImportYiYuan = false;
            MessageInit();

            txtYiYuanMessage.Text = "义原导入中...";
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate { }));

            YiYuan.Path = txtYiYuanPath.Text;
            YiYuan.loadGlossary();
            if (YiYuan.PRIMITIVESID.Count > 0)
            {
                IsImportYiYuan = true;
                txtYiYuanMessage.Text = "成功导入义原" + YiYuan.ALLPRIMITIVES.Count.ToString() + "条";
                txtYiYuanMessage.Foreground = Brushes.Green;
            }
            else
            {
                IsImportYiYuan = false;
                txtYiYuanMessage.Text = "义原尚未导入";
                txtYiYuanMessage.Foreground = Brushes.Red;
                MessageFail("义原导入失败，请去日志查看原因");
            }
        }

        private void btnImportYiXiang_Click(object sender, RoutedEventArgs e)
        {
            IsImportYiXiang = false;
            MessageInit();

            if (IsImportYiYuan)
            {
                YiXiang = new WordSimilarity();
                txtYiXiangMessage.Text = "义项导入中...";
                Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate { }));

                YiXiang.Path = txtYiXiangPath.Text;
                YiXiang.loadGlossary();
                YiXiang.primitive = YiYuan;
                if (YiXiang.ALLWORDS.Count > 0)
                {
                    IsImportYiXiang = true;
                    txtYiXiangMessage.Text = "成功导入义项" + YiXiang.ALLWORDS.Count.ToString() + "条";
                    txtYiXiangMessage.Foreground = Brushes.Green;
                }
                else
                {
                    IsImportYiXiang = false;
                    txtYiXiangMessage.Text = "义项尚未导入";
                    txtYiXiangMessage.Foreground = Brushes.Red;
                    MessageFail("义项导入失败，请去日志查看原因");
                }
            }
            else
            {
                MessageFail("请先导入义原表");    
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            MessageInit();
            txtWordYiYuan.Text = "";
            txtWordYiXiang.Text = "";
            txtAlertMessage.Text = "";

            if (txtSearchWord.Text.Trim().ToString() == String.Empty)
            {
                MessageBox.Show("输入不能为空");
                txtSearchWord.Focus();
                return;
            }
            if (IsImportYiXiang)
            {
                List<int> plist = YiYuan.getparents(txtSearchWord.Text.Trim().ToString());
                int[] lst = plist.ToArray();
                if (lst.Length > 0)
                {
                    for (int i = 0; i < lst.Length; i++)
                    {
                        txtWordYiYuan.AppendText(lst[i].ToString() + " " + YiYuan.ALLPRIMITIVES[lst[i]].getprimitiveEnglish() + "|" + YiYuan.ALLPRIMITIVES[lst[i]].getPrimitiveChinese() + "  " + YiYuan.ALLPRIMITIVES[lst[i]].getParentId() + "\n");
                    }
                }
                else
                {
                    MessageFail("不是义原");

                }
            }
            else
            {
                MessageFail("请先导入义原表");
            }

            if (YiXiang != null)
            {
                Word yxt = new Word();
                List<Word> tlist = null;
                if (YiXiang.ALLWORDS.ContainsKey(txtSearchWord.Text.Trim().ToString()))
                {
                    tlist = YiXiang.ALLWORDS[txtSearchWord.Text.Trim().ToString()];
                }
                else
                {
                    MessageFail("义项不存在");
                    return;
                }

                Word[] get = tlist.ToArray();
                for (int i = 0; i < get.Length; i++)
                {
                    string tempOut = get[i].getType() + " " + get[i].getRelated();
                    if (!txtWordYiXiang.Text.Trim().Contains(tempOut.Trim()))
                        txtWordYiXiang.AppendText(tempOut + "\n");
                    //textBox2.AppendText(get[i].getType() + " " + get[i].getRelated() + "\n");
                }
            }
            else {
               MessageFail("请先导入义项表");   
            }
        }

        private void btnImportSentiment_Click(object sender, RoutedEventArgs e)
        {
            MessageInit();
            txtPosMessage.Text = "褒义词库尚未导入";
            txtNegMessage.Text = "贬义词库尚未导入";
            txtPosMessage.Foreground = Brushes.Red;
            txtNegMessage.Foreground = Brushes.Red;

            PosSentiment = Common.Common.ImportPosSemtidic(txtSentimentPath.Text);
            NegSentiment = Common.Common.ImportNegSemtidic(txtSentimentPath.Text);
            if (PosSentiment.Count > 0 && NegSentiment.Count >0 )
            { 
                IsImportDictionary = true;
                txtPosMessage.Foreground = Brushes.Green;
                txtNegMessage.Foreground = Brushes.Green;
                txtPosMessage.Text = "导入 " + PosSentiment.Count.ToString() + " 条褒义词";
                txtNegMessage.Text = "导入 " + NegSentiment.Count.ToString() + " 条贬义词";
            }
            else
            {
                IsImportDictionary = false;
                if (PosSentiment.Count == 0)
                {
                    MessageFail("褒义词库导入失败");
                }
                if (NegSentiment.Count == 0)
                {
                    MessageFail("贬义词库导入失败");
                }
            }
        }

        //分词
        private void btnSeparateWord_Click(object sender, RoutedEventArgs e)
        {
            MessageInit();
            WordSeparation wordSeparation = new WordSeparation( txtRawDataPath.Text);
            string result = wordSeparation.SeparateWord();

            if (result == "")
            {
                MessageSuccess("分词成功");
            }
            else {
                MessageFail(result);   
            }
        }

        //通过比较语料和情感词典，找出情感词
        private void btnCompare_Click(object sender, RoutedEventArgs e)
        {
            MessageInit();
            double confidence;
            try
            {
               confidence = double.Parse(txtConfidence.Text);
               if (confidence >= 1 || confidence <= 0)
               {
                   MessageBox.Show("置信度是0－1之间的小数");
                   return;
               }
            }
            catch {
                MessageBox.Show("置信度是0－1之间的小数");
                return;

            }
            if (IsImportYiXiang && IsImportYiYuan & IsImportDictionary)
            {
                MasterDataBL masterDataBL = new MasterDataBL(txtMasterDataPath.Text, confidence, YiYuan, YiXiang, PosSentiment, NegSentiment);

                MessageSuccess("时间可能较长，请耐心等待");
                Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate { }));
                string result = masterDataBL.CompareMasterWithDic();
                if (result == "")
                {

                    dtResult = masterDataBL.MasterDataTable;
                    EnumerableRowCollection<DataRow> query = from order in dtResult.AsEnumerable()
                                                             orderby order.Field<double>("confidence-score") descending
                                                             select order;

                    DataView dv = query.AsDataView();
                    dgdSentimentWord.ItemsSource = dv;
                    MessageInit();
                    MessageSuccess("操作成功");
                }
                else
                {
                    MessageInit();
                    MessageFail(result);   
                
                }

            }
            else {
                if (!IsImportYiXiang)
                {
                    MessageFail("必须导入义项");
                }
                if (!IsImportYiYuan)
                {
                    MessageFail("必须导入义原");
                }
                if (!IsImportDictionary)
                {
                    MessageFail("必须导入情感词典");
                }
            
            }
        }

        //比较两个单词的相似度（用于测试义项库）
        private void btnCompareTwoWords_Click(object sender, RoutedEventArgs e)
        {
            MessageInit();
            if (txtWord1.Text.Trim() == "")
            {
                MessageBox.Show("单词1必须填写");
                return;
            }
            if (txtWord2.Text.Trim() == "")
            {
                MessageBox.Show("单词2必须填写");
                return;
            }
            if (IsImportYiXiang)
            {
                double sim = YiXiang.simWord(txtWord1.Text.Trim(), txtWord2.Text.Trim());
                MessageSuccess("相似度：" + sim.ToString());
            }
            else
            {
                MessageFail("必须导入义项");

            }
        }

        //到處到exploerfolder中
        private void btnExport_Click(object sender, RoutedEventArgs e)
        {
            MessageInit();
            if (dtResult != null)
            {
               string result = Common.Common.ExportDataTableToTXT(dtResult, txtExportPath.Text);
               if (result == "")
               {
                   MessageSuccess("操作成功");
               }
               else
               {
                   MessageFail(result);
               }

            }
            else {
                MessageBox.Show("沒有結果");
            
            }

        }

        
    }
}
