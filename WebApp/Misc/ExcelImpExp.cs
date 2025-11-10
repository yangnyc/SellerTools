using System;
using Excel = Microsoft.Office.Interop.Excel;

namespace WebApp.Misc
{
    public class ExcelImpExp
    {
        public ExcelImpExp() { }

        public bool WriteDataTableToExcel(System.Data.DataTable dataTable, string worksheetName, string saveAsLocation)
        {
            Excel.Application excel;
            Excel.Workbook excelworkBook;
            Excel.Worksheet excelSheet;
            //Range excelCellrange;
            try
            {
                excel = new Excel.Application();

                excel.Visible = false;
                excel.DisplayAlerts = false;
                excelworkBook = excel.Workbooks.Add(Type.Missing);
                excelSheet = (Excel.Worksheet)excelworkBook.ActiveSheet;
                excelSheet.Name = worksheetName;

                excelSheet.Cells[1, 2] = "Date : " + DateTime.Now.ToShortDateString();
                //int rowcount = 2;
                for (int Idx = 0; Idx < dataTable.Columns.Count; Idx++)
                {
                    excelSheet.Range["A2"].Offset[0, Idx].Value = dataTable.Columns[Idx].ColumnName;
                }
                for (int Idx = 0; Idx < dataTable.Rows.Count; Idx++)
                {
                    excelSheet.Range["A3"].Offset[Idx].Resize[1, dataTable.Columns.Count].Value =
                    dataTable.Rows[Idx].ItemArray;
                }
                excelworkBook.SaveAs(saveAsLocation); ;
                excelworkBook.Close();
                excel.Quit();
                return true;
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message);
                return false;
            }
        }

        /*public System.Data.DataTable GetTable()
    {
        SQLiteConnection.CreateFile("MyDatabase.sqlite");
        SQLiteConnection m_dbConnection = new SQLiteConnection("Data Source=MyDatabase.sqlite");
        m_dbConnection.Open();
        string sql = "create table MyData ( ID varchar(20), Date varchar(20),  Time varchar(20) , Year varchar(20), Month varchar(20),Day varchar(20),Hour varchar(20),DayOfWeek varchar(20),  OrderLineOid varchar(20) ,Product varchar(20) )";

        SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
        command.ExecuteNonQuery();

        sql = "INSERT INTO MyData(ID, Date,Time,Year,Month,Day,Hour,DayOfWeek,OrderLineOid,Product) VALUES('1', '01/12/2022','14:34','2022','12','1','14','4','1','9×13' )";
        command = new SQLiteCommand(sql, m_dbConnection);
        command.ExecuteNonQuery();

        sql = "INSERT INTO MyData(ID, Date,Time,Year,Month,Day,Hour,DayOfWeek,OrderLineOid,Product) VALUES('2', '01/12/2022','14:34','2022','12','1','14','4','2','10×10' )";
        command = new SQLiteCommand(sql, m_dbConnection);
        command.ExecuteNonQuery();

        SQLiteCommand sqlCom = new SQLiteCommand("Select * From MyData", m_dbConnection);
        SQLiteDataAdapter sda = new SQLiteDataAdapter(sqlCom);
        System.Data.DataTable dt = new System.Data.DataTable();
        sda.Fill(dt);
        m_dbConnection.Close();
        return dt;
    }*/
    }
}