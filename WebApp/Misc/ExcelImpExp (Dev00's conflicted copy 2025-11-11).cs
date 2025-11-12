using System;
using Excel = Microsoft.Office.Interop.Excel;

namespace WebApp.Misc
{
    /// <summary>
    /// Provides functionality for importing and exporting data to Excel files.
    /// Uses Microsoft Office Interop to create and manipulate Excel workbooks.
    /// </summary>
    public class ExcelImpExp
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExcelImpExp"/> class.
        /// </summary>
        public ExcelImpExp() { }

        /// <summary>
        /// Writes a DataTable to an Excel file.
        /// Creates a new Excel workbook with the data formatted in a worksheet.
        /// </summary>
        /// <param name="dataTable">The DataTable containing data to export.</param>
        /// <param name="worksheetName">The name for the worksheet.</param>
        /// <param name="saveAsLocation">The file path where the Excel file should be saved.</param>
        /// <returns>True if successful; false if an exception occurred.</returns>
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