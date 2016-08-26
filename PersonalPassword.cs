using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PayStub
{
    class PersonalPassword
    {
        private Dictionary<string, string> _passwordDict;
        private Dictionary<string, string> _mailDict;
        public int Count { 
            get
            {
                return _passwordDict.Count;
            } 
        }
        public string this[string name]
        {
            get
            {
                if(_passwordDict.ContainsKey(name))
                {
                    return _passwordDict[name];
                }
                return string.Empty;
            }
        }

        public string this[int index]
        {
            get
            {
                return _passwordDict.ElementAt(index).Key;                
            }
        }

        public string GetMail(string name)
        {
            if (_mailDict.ContainsKey(name))
            {
                return _mailDict[name];
            }
            return string.Empty;
        }
        internal void Initialize(object data, int rowCount, int colCount)
        {
            object[,] valueArray = (object[,])data;
            _passwordDict = new Dictionary<string, string>();
            _mailDict = new Dictionary<string, string>();
            for (int row = 1; row <= rowCount; row++)
            {
                string name = (string)valueArray[row, 1];
                string password = (string)valueArray[row, 2];
                string mailaddress = (string)valueArray[row, 3];
                if (!string.IsNullOrEmpty(name) &&
                    !string.IsNullOrEmpty(password))
                {
                    name.Trim();
                    password.Trim();
                    _passwordDict.Add(name, password);
                }
                if (!string.IsNullOrEmpty(mailaddress))
                {
                    mailaddress.Trim();
                    _mailDict.Add(name, mailaddress);
                }
            }
        }

    }
}
