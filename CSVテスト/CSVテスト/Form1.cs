using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

//追加
using System.IO;
using System.Windows.Forms.DataVisualization.Charting;

namespace CsvTest
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            InitializeDataGridView();
            InitializeChartArea();
            txtInputFileName.Text = @"C:\Users\anzhir\Desktop\csv";
        }

        #region イベントハンドラ
        private void BtnRead_Click(object sender, EventArgs e)
        {
            if(File.Exists(txtInputFileName.Text))
            {
                List<string> list = ReadCsv();

                if(dataGridView1.Rows.Count != 0)
                {
                    if (MessageBox.Show("入力されているデータの末端に、新たにファイル内容を追加しますか？",
                        "確認", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.No)
                    {
                        if (MessageBox.Show("現在入力されているデータを削除して新たに取り込みますか？",
                            "確認", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                        {
                            dataGridView1.Rows.Clear();
                            InitializeChartArea();
                        }
                        else
                        {
                            return;
                        }
                    }
                }
                //データグリッドビューへの追加
                string inputLog = string.Empty;
                for(int i = 0;i < list.Count;i++)
                {
                    if (!CheckAndAddPoint(list[i]))
                    {
                        inputLog += i.ToString() + ",";
                    }
                }
                if(inputLog != string.Empty)
                {
                    MessageBox.Show("数値に変換できないデータはスキップされました。\n対象行：" + inputLog,
                        "確認", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                txtAdditionalValue.Focus();
            }
            else
            {
                MessageBox.Show("指定したファイルが見つかりません。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            string saveFileName = string.Empty;

            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.InitialDirectory = @"C:\Users\anzhir\Desktop\csv";
                sfd.FileName = "output.csv";
                sfd.Title = "保存";
                sfd.Filter = "CSVファイル(*.csv)|*.csv|テキストファイル(*.txt)|*.txt|すべてのファイル(*.*)|*.*";
                sfd.RestoreDirectory = true;
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    saveFileName = sfd.FileName;
                }
            }

            //書き込みます。
            if(saveFileName != string.Empty)
            {
                //データグリッドビュー項目からCSV文字列を生成
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < dataGridView1.RowCount - 1; i++)
                {
                    sb.Append(dataGridView1.Rows[i].Cells[1].Value.ToString());
                    sb.Append(",");
                }
                //最後の一行の追加(カンマをつけない)
                sb.Append(dataGridView1.Rows[dataGridView1.RowCount - 1].Cells[1].Value.ToString());

                //書き込み
                WriteCsv(saveFileName,sb.ToString());
            }
        }

        private void BtnEnd_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("プログラムを終了します。よろしいですか？",
                        "確認", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
            {
                this.Close();
            }
        }

        private void BtnInputSearch_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.InitialDirectory = @"C:\Users\anzhir\Desktop\csv";
                ofd.Title = "入力ファイル選択";
                ofd.RestoreDirectory = true;
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    txtInputFileName.Text = ofd.FileName;
                }
            }  
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            txtAdditionalValue.BackColor = Color.White;
            if(!CheckAndAddPoint(txtAdditionalValue.Text))
            {
                MessageBox.Show("入力された値は無効です。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtAdditionalValue.BackColor = Color.Pink;
            }
            else
            {
                txtAdditionalValue.Text = string.Empty;
            }
            txtAdditionalValue.Focus();
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            MessageBox.Show("調整中です。",
                        "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void BtnClear_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("クリアしますか？",
                        "確認", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
            {
                dataGridView1.Rows.Clear();
                InitializeChartArea();
            }
        }

        #endregion

        #region メソッド
        private List<string> ReadCsv()
        {
            //ファイル内容を格納するリスト
            List<string> itemList = new List<string>();

            string fileData = string.Empty;

            //ファイル内容の全取得
            using (StreamReader sr = new System.IO.StreamReader(txtInputFileName.Text))
            {
                fileData = sr.ReadToEnd();
            }

            //ファイル内容をカンマ区切りで配列に格納
            string[] items = fileData.Split(',');

            //配列→リストに変換
            foreach(string item in items)
            {
                itemList.Add(item);
            }

            return itemList;            
        }

        private void WriteCsv(string outputFileName, string text)
        {
            //書き込みます。
            using (StreamWriter sw = new StreamWriter(outputFileName, false, Encoding.UTF8))
            {
                sw.Write(text);
            }
            MessageBox.Show("保存が完了しました。",
                    "確認", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public void InitializeDataGridView()
        {
            dataGridView1.ColumnCount = 2;
            dataGridView1.Columns[0].HeaderText = "No.";
            dataGridView1.Columns[1].HeaderText = "値";
            dataGridView1.Columns[0].Width = 35;
            dataGridView1.Columns[1].Width = 58;
            //dataGridView1.Columns[1].Frozen = true;
        }

        public void InitializeChartArea()
        {
            //初期化
            chart1.ChartAreas.Clear();
            chart1.Series.Clear();
            chart1.Legends.Clear();
            chart1.ChartAreas.Add(new ChartArea("Result"));

            string legend = "Graph";
            chart1.Series.Add(legend);
            chart1.Series[legend].ChartType = SeriesChartType.Column;
            chart1.Series[legend].Color = Color.SlateGray;
            chart1.Series[legend].IsValueShownAsLabel = true;
            chart1.ChartAreas[0].AxisX.Minimum = 0;
            //chart1.ChartAreas[0].AxisX.Maximum = list.Count + 0.99;
            chart1.ChartAreas[0].AxisX.Interval = 1;
            chart1.ChartAreas[0].AxisX.MajorGrid.Enabled = false;

            chart1.ChartAreas[0].AxisY.Maximum = 180;
            chart1.ChartAreas[0].AxisY.Minimum = 0;
            chart1.ChartAreas[0].AxisY.Interval = 30;

            chart1.ChartAreas[0].AxisY.MajorGrid.LineColor = Color.Gray;
            chart1.ChartAreas[0].AxisY.MajorGrid.LineDashStyle = ChartDashStyle.Dot;
            chart1.ChartAreas[0].AxisX.ScaleView.Position = 0.0;
        }

        private bool CheckAndAddPoint(string value)
        {
            //chart1.Series[0].Points.Add(x, y);

            double y = 0.0;

            if (Double.TryParse(value, out y))
            {
                dataGridView1.Rows.Add(dataGridView1.RowCount + 1, y);
                chart1.Series[0].Points.AddXY((double)dataGridView1.RowCount, y);

                //行数を更新
                chart1.ChartAreas[0].AxisX.Maximum = dataGridView1.RowCount + 0.99;
                return true;
            }
            else
            {
                return false;
            }
        }
        #endregion
    }
}
