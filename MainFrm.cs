using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net.Configuration;
using System.Text;
using System.Threading;
using System.Windows.Forms;
namespace PayStub
{
    public partial class MainFrm : Form
    {
        public MainFrm()
        {
            InitializeComponent();

            InitializeMailContent();
        }

        private string bodystr;
        private string titlestr;

        private void InitializeMailContent()
        {
            FileStream fs = new FileStream("content.txt", FileMode.Open);
            byte[] buffer = new byte[fs.Length];
            fs.Read(buffer, 0, buffer.Length);
            var bodystr_templete = Encoding.UTF8.GetString(buffer);
            fs.Close();

            var smtpSection = (SmtpSection)ConfigurationManager.GetSection("system.net/mailSettings/smtp");
            string username = smtpSection.Network.UserName;

            int month = DateTime.Now.Month - 1;
            titlestr = string.Format("{0}月工资条", month);
            bodystr = string.Format(bodystr_templete, month, username);

            sendFromText.Text = username;
            bodyText.Text = bodystr;
            subjectText.Text = titlestr;
        }

        private string _salaryPath;
        private string _encrtyPath;
        private string _savePath;
        private MailSender _mailsend;
        private void btnBrowse_Click(object sender, EventArgs e)
        {
            FileDialog dlg = new OpenFileDialog();
            var result = dlg.ShowDialog();
            if (result == DialogResult.OK)
            {
                salTextbox.Text = dlg.FileName;
                _salaryPath = salTextbox.Text;
            }
        }

        private void btnBrowseEncrty_Click(object sender, EventArgs e)
        {
            FileDialog dlg = new OpenFileDialog();
            var result = dlg.ShowDialog();
            if (result == DialogResult.OK)
            {
                passwordTextbox.Text = dlg.FileName;
                _encrtyPath = dlg.FileName;
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dlg = new FolderBrowserDialog();
            var result = dlg.ShowDialog();
            if (result == DialogResult.OK)
            {
                pathTextbox.Text = dlg.SelectedPath + "\\";
                _savePath = pathTextbox.Text;
            }
        }

        private void saveBtn_Click(object sender, EventArgs e)
        {
            if (_mailsend == null)
            {
                _mailsend = new MailSender();
            }
            _mailsend.Body = bodyText.Text;
            _mailsend.Subject = subjectText.Text;
            _mailsend.User = sendFromText.Text;
            _mailsend.SendCompleted += OnSendCompleted;

            if (string.IsNullOrEmpty(_mailsend.Body) ||
                string.IsNullOrEmpty(_mailsend.Subject) ||
                string.IsNullOrEmpty(_mailsend.User))
            {
                MessageBox.Show("请完善邮件发送设置!");
                return;
            }

            var trd = new Thread(new ThreadStart(Process));

            saveBtn.Enabled = false;
            progressBar1.Visible = true;
            progressBar1.Minimum = 0;
            progressBar1.Maximum = 4;
            progressBar1.Step = 1;
            progressBar1.Value = 0;
            trd.Start();
        }
        private delegate void showProgressBar();
        private void ShowProgressBar()
        {
            if (progressBar1.InvokeRequired)
            {
                var dele = new showProgressBar(() => { ShowProgressBar(); });
                this.BeginInvoke(dele);
            }
            else
            {
                progressBar1.PerformStep();
                if (progressBar1.Value == 3)
                {
                    saveBtn.Enabled = true;
                    progressBar1.Visible = false;
                }
            }

        }

        private void Process()
        {
            ExcelOperate excelOp = null;
            try
            {
                excelOp = new ExcelOperate();
                object data = new object();
                int row = 0;
                int col = 0;

                excelOp.Open(_encrtyPath);
                excelOp.Read(out data, out row, out col);
                var pp = new PersonalPassword();
                var ml = new List<MailReceiver>();
                pp.Initialize(data, row, col);
                excelOp.Close();
                ShowProgressBar();

                data = null;
                row = 0;
                col = 0;
                excelOp.Open(_salaryPath);
                excelOp.Read(out data, out row, out col);
                var payment = new Payment();
                payment.Initialize(data, row, col);
                ShowProgressBar();

                string folderPath = _savePath;

                for (int i = 0; i < pp.Count; i++)
                {
                    var name = pp[i];
                    var passWord = pp[name];
                    string fileName = folderPath + name + ".xls";
                    excelOp.NewFile();
                    object[] value = payment.GetValueByName(name);
                    if (value == null)
                    {
                        string log = "没有找到名为[" + name + "]的工资信息";
                        OnSendCompleted(log);
                        continue;
                    }
                    excelOp.Write(value, 2, payment.ColCount);
                    excelOp.Save(fileName, passWord);
                    excelOp.Close();

                    var ms = new MailReceiver();
                    ms.MailAddress = pp.GetMail(name);
                    ms.AppendixPath = fileName;
                    ml.Add(ms);
                }
                ShowProgressBar();

                if (MessageBox.Show("即将发送邮件，请确认!", "确认",
                    MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                {
                    _mailsend.Send(ml);
                }

                ShowProgressBar();
            }
            catch (Exception ex)
            {
                OnSendCompleted(ex.Message);
                OnSendCompleted(ex.StackTrace);
            }
            finally
            {
                excelOp.Quit();
            }
        }

        private delegate void ShowLogHandler(string args);
        private void OnSendCompleted(string args)
        {
            if (logListBox.InvokeRequired)
            {
                logListBox.BeginInvoke(new ShowLogHandler(
                    (log) => { OnSendCompleted(log); }
                    ), args);
            }
            else
            {
                logListBox.Items.Add(args);
                var size = logListBox.Size;
                var itemCount = size.Height / logListBox.ItemHeight;
                if (logListBox.Items.Count > itemCount)
                {
                    logListBox.TopIndex = logListBox.Items.Count - itemCount;
                }
            }
        }

        private void btnTest_Click(object sender, EventArgs e)
        {
            if (_mailsend == null)
            {
                _mailsend = new MailSender();
            }
            _mailsend.Body = "Test";
            _mailsend.Subject = "Test";
            _mailsend.User = "Test";
            _mailsend.SendCompleted += OnSendCompleted;
            var ml = new List<MailReceiver>();
            var ms = new MailReceiver();
            ms.MailAddress = @"heshasha@zqvideo.com";
            ms.AppendixPath = @"d:\1.txt";
            ml.Add(ms);
            _mailsend.Send(ml);
        }
    }
}
