using Microsoft.Data.SqlClient;
using System;

namespace SQLDBApp
{
    public class RecreateDatabase
    {
        public static void Main(string[] args)
        {
            var connectionString = "Data Source=192.168.105.1;Initial Catalog=master;Persist Security Info=True;User ID=sa;Password=PasGitSQL111;TrustServerCertificate=true";
            
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                
                try
                {
                    Console.WriteLine("Dropping database DB2 if exists...");
                    var dropCmd = new SqlCommand(@"
                        IF EXISTS (SELECT name FROM sys.databases WHERE name = 'DB2')
                        BEGIN
                            ALTER DATABASE DB2 SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                            DROP DATABASE DB2;
                        END", connection);
                    dropCmd.ExecuteNonQuery();
                    Console.WriteLine("Database DB2 dropped successfully.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error dropping database: {ex.Message}");
                }
                
                try
                {
                    Console.WriteLine("Deleting orphaned files...");
                    var deleteFilesCmd = new SqlCommand(@"
                        EXEC sp_detach_db 'DB2', 'true';
                        ", connection);
                    deleteFilesCmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"No detach needed: {ex.Message}");
                }
                
                try
                {
                    Console.WriteLine("Creating database DB2...");
                    var createCmd = new SqlCommand(@"
                        CREATE DATABASE DB2
                        ON PRIMARY 
                        (NAME = DB2_Data, FILENAME = '/var/opt/mssql/data/DB2_New.mdf')
                        LOG ON 
                        (NAME = DB2_Log, FILENAME = '/var/opt/mssql/data/DB2_New.ldf')
                        ", connection);
                    createCmd.ExecuteNonQuery();
                    Console.WriteLine("Database DB2 created successfully.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error creating database: {ex.Message}");
                }
            }
        }
    }
}
