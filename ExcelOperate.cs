using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Office.Interop.Excel;

namespace PayStub
{
    class ExcelOperate
    {
        private static Microsoft.Office.Interop.Excel.Application _xlApp;
        private Workbooks _workBooks;
        private Workbook _workBook;
        private Worksheet _workSheet;
        public bool Open(string fileName)
        {
            if (_xlApp == null)
            {
                _xlApp = new ApplicationClass();
                _xlApp.Visible = false;
            }
            try
            {
                _workBooks = _xlApp.Workbooks;
                _workBook = _workBooks.Open(fileName, Type.Missing, Type.Missing,
                    Type.Missing, Type.Missing, Type.Missing, Type.Missing,
                    Type.Missing, Type.Missing, Type.Missing, Type.Missing,
                    Type.Missing, Type.Missing, Type.Missing, Type.Missing);
                _workSheet = (Worksheet)(_workBook.Worksheets.get_Item(1));
                return _workSheet != null ? true : false;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void Save(string fileName, string password)
        {
            _workBook.SaveAs(fileName, XlFileFormat.xlExcel8, password, Type.Missing, Type.Missing,
                Type.Missing, XlSaveAsAccessMode.xlNoChange, XlSaveConflictResolution.xlLocalSessionChanges);            
        }

        public void Read(out object content, out int rowCount, out int colCount)
        {
            var usedRange = (Range)_workSheet.UsedRange;
            colCount = usedRange.Columns.Count;
            rowCount = usedRange.Rows.Count;
            content = usedRange.Rows.get_Value(XlRangeValueDataType.xlRangeValueDefault);
        }

        public bool NewFile()
        {
            if (_xlApp == null)
            {
                _xlApp = new ApplicationClass();
                _xlApp.Visible = false;
            }
            if(_workBooks==null)
            {
                _workBooks = _xlApp.Workbooks;
            }
            _workBook = _workBooks.Add(Type.Missing);
            _workSheet = (Worksheet)_workBook.Worksheets.get_Item(1);
            return _workSheet != null ? true : false;
        }

        public void Write(object content, int row, int col)
        {
            object[] data = (object[])content;
            for (int i = 0; i < row; i++)
            {
                object[] rowData = (object[])data[i];
                for (int j = 0; j < col; j++)
                {
                    _workSheet.Cells[i + 1, j + 1] = rowData[j];                    
                }
            }
        }

        public void Close()
        {           
            _workBook.Close(true, Type.Missing, Type.Missing);
            _workBook = null;
        }

        public void Quit()
        {            
            _xlApp.Quit();
        }
    }
}
