using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Mail;
namespace PayStub
{
    public class MailReceiver
    {
        public string MailAddress { get; set; }
        public string AppendixPath { get; set; }
    }

    public class MailSender
    {
        public string User { get; set; }
        public string Password { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        
        public delegate void SendCompletedHandeler(string args);
        public event SendCompletedHandeler SendCompleted;

        public void Send(IEnumerable<MailReceiver> mailReceivers)
        {
            foreach (var item in mailReceivers)
            {
                Send(item);
            }
        }

        private void Send(MailReceiver mr)
        {
            SmtpClient client = new SmtpClient();
            var mail = new MailMessage();
            mail.To.Add(mr.MailAddress);
            string from = client.Credentials.GetCredential(client.Host, client.Port, "Basic").UserName;
            mail.From = new MailAddress(from, User, Encoding.UTF8);
            mail.Subject = Subject;
            mail.SubjectEncoding = Encoding.UTF8;
            mail.Body = Body;
            mail.BodyEncoding = Encoding.UTF8;
            mail.IsBodyHtml = false;
            mail.Attachments.Add(new Attachment(mr.AppendixPath));

            
            client.Timeout = 5000;
            object userToken = mail;
            try
            {
                client.SendAsync(mail, userToken);
                client.SendCompleted += client_SendCompleted;
            }
            catch (Exception ex)
            {
                if (SendCompleted != null)
                {
                    string log = string.Format("发送异常:{0}", ex.Message);
                    SendCompleted(log);                    
                }
                mail.Dispose();
            }
        }

        void client_SendCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            string log = string.Empty;
            var client = (SmtpClient)sender;
            var userdata = (MailMessage)e.UserState;
            if (e.Cancelled)
            {
                client.SendAsyncCancel();
                log = string.Format("发送被取消：{0}", userdata.To[0].Address);
            }
            else if (e.Error != null)
            {
                log = string.Format("发送失败：{0}.原因:{1}", userdata.To[0].Address, e.Error.Message);
            }
            else
            {
                log = string.Format("发送成功：{0}", userdata.To[0].Address);
            }
            if (SendCompleted != null)
            {
                SendCompleted(log);  
            }
            userdata.Dispose();
        }
    }
}
