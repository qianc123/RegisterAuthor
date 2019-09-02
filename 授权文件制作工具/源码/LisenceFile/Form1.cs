using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LisenceFile
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            this.Load += Form1_Load;
        }

        void Form1_Load(object sender, EventArgs e)
        {   
            cbxVersions.DataSource = VersionSettingsManager.Instance.List();
            cbxAuthoTypes.DataSource = AuthorizeManager.Instance.LoadItems();
        }

        /// <summary>
        /// 版本管理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnVisionSettings_Click(object sender, EventArgs e)
        {
            FormVersionSettings fs = new FormVersionSettings();
            fs.ShowDialog(this);

            cbxVersions.DataSource = VersionSettingsManager.Instance.List();
        }

        
        private void cbxAuthoTypes_SelectedIndexChanged(object sender, EventArgs e)
        {
            AuthorizeItem ai = cbxAuthoTypes.SelectedItem as AuthorizeItem;
            if (ai == null)
                return;

            lblMemo.Text = ai.Memo;
        }

        private void btnClose_Click(object sender, EventArgs e)
        {

            this.Close();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            

            AuthorizeFile af = new AuthorizeFile();
            af.Version = cbxVersions.Text;
            
            af.AuthorizeType = cbxAuthoTypes.Text;
            af.AuthorizeContent = txtContent.Text;

            if (string.IsNullOrWhiteSpace(af.Version))
            {   
                MessageBox.Show("版本类型不可为空");
                return;
            }

            if (string.IsNullOrWhiteSpace(af.AuthorizeContent))
            {
                MessageBox.Show("授权类型中的内容不可为空");
                return;
            }

            string localFilePath = String.Empty;
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();

            //设置文件类型  
            saveFileDialog1.Filter = "授权文件(*.dat)|*.dat|所有文件(*.*)|*.*";
            //设置文件名称：
            saveFileDialog1.FileName = "授权文件["+ af.Version +"+"+ af.AuthorizeType +"]"+ DateTime.Now.ToString("yyyyMMddHHmm") +".dat";

            //设置默认文件类型显示顺序  
            saveFileDialog1.FilterIndex = 1;

            //保存对话框是否记忆上次打开的目录  
            saveFileDialog1.RestoreDirectory = true;

            //点了保存按钮进入  
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                //获得文件路径  
                localFilePath = saveFileDialog1.FileName.ToString();

                string json = af.ToJson();
                //读取公钥内容
                string publicKeyXml = VersionSettingsManager.Instance.ReadPublicKey(af.Version);
                //读取私钥内容
                string privateKeyXml = VersionSettingsManager.Instance.ReadPrivateKey(af.Version);

                LisenceSign signer = new LisenceSign();
                var sign = signer.Sign(json, privateKeyXml);

                System.IO.File.WriteAllText(localFilePath, json + "+++++=====+++++" + sign + "+++++=====+++++" + publicKeyXml);
            }

        }

    }
}
