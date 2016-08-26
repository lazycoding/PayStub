using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PayStub
{
    class Payment
    {
        private List<Record> _zqEmpoly = new List<Record>();
        private List<object> _tableHead;
        public int ColCount 
        { 
            get { return _tableHead.Count; } 
        }
        private void ParseTableHead(object[,] valueArray, int row, int colCount)
        {
            for (int i = 0; i < colCount; i++)
            {
                if (3 <= i && i <= 5)
                {
                    continue;
                }
                _tableHead.Add(valueArray[row, i + 1]);
            }
        }

        private void ProcessValue(object[,] valueArray, int rowCount, int colCount)
        {
            for (int row = 3; row <= rowCount; row++)
            {
                string name = string.Empty;
                List<object> sal = new List<object>();
                for (int col = 1; col <= colCount; col++)
                {
                    var value = valueArray[row, col];
                    if (4 <= col && col <= 6)
                    {
                        continue;
                    }
                    else if (col == 3)
                    {
                        name = (string)value;
                    }
                   
                    sal.Add(value);
                }
                if (!string.IsNullOrEmpty(name) && sal.Count != 0)
                {
                    var rec = new Record() { Name = name };
                    rec.Sal = sal;
                    _zqEmpoly.Add(rec);
                }
            }
        }

        internal void Initialize(object data, int rowCount, int colCount)
        {
            object[,] valueArray =(object[,]) data;
            _tableHead = new List<object>();
            ParseTableHead(valueArray, 2, 30);
            ProcessValue(valueArray, rowCount, 30);
        }

        internal object[] GetValueByName(string name)
        {
            var value = from empoly in _zqEmpoly where empoly.Name == name select empoly;
            var finded = value.FirstOrDefault();
            if (finded == null)
            {
                //throw new Exception("没有找到名为[" + name + "]的工资信息");
                return null;
            }
            object[] result = new object[2];
            result[0] = _tableHead.ToArray();
            result[1] = finded.Sal.ToArray();
            return result;
        }
    }
}
