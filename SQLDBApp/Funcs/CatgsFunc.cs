using Microsoft.Data.SqlClient;
using SQLDBApp.Data;
using SQLDBApp.Models.DataItems;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace SQLDBApp.Funcs
{
    public class CatgsFunc
    {
        private readonly DevSqlDBContext db;

        public CatgsFunc() => db = new DevSqlDBContext();

        public List<DataItemCatg> GetAll()
        {
            return db.DataItemCatg.ToList();
        }

        public DataItemCatg GetCatgById(int id)
        {
            return db.DataItemCatg.FirstOrDefault(x => x.Id == id);
        }

        public void AddUrl(String url)
        {
            if (IsExist(url))
                return;
            Add(new DataItemCatg() { Url = url });
        }

        public bool AddUrl(string[] dataItemcatgArr)
        {
            List<DataItemCatg> dataItemCatgsList = new List<DataItemCatg>();

            foreach (string dataItemcatg in dataItemcatgArr)
                if (!IsExist(dataItemcatg))
                    dataItemCatgsList.Add(new DataItemCatg() { Url = dataItemcatg });
            Add(dataItemCatgsList);
            return true;
        }

        public bool AddSidUrl(List<string[]> strList)
        {
            List<DataItemCatg> dataItemCatgList = new List<DataItemCatg>();

            foreach (string[] str in strList)
                if (!IsExist(str[1]))
                    dataItemCatgList.Add(new DataItemCatg() { Id = int.Parse(str[0]), Url = str[1] });
            return Add(dataItemCatgList);
        }

        public bool IsExist(String url)
        {
            return db.DataItemCatg.FirstOrDefault(x => x.Url.Equals(url)) != null;
        }

        public bool SetIsCollectedHRefToTrue(long id)
        {
            try
            {
                db.DataItemCatg.FirstOrDefault(x => x.Id == id).IsCollectedHRef = true;
                db.SaveChanges();
            }
            catch (Exception e) { return false; }
            return true;
        }

        public DataItemCatg GetNextIsCollectedHRefFalse()
        {
            //////TEMP FOR TEST
            DataItemCatg test1 = new DataItemCatg();
            test1.Url = @"https://www.gmail.com/";
            return test1;
            /////END TEMP
           // return db.DataItemCatg.FirstOrDefault(x => x.IsCollectedHRef == false);
        }

        public bool Add(List<DataItemCatg> dataItemCatgList)
        {
            try
            {
                db.DataItemCatg.AddRange(dataItemCatgList);
                db.SaveChanges();
            }
            catch (Exception e) { return false; }
            return true;
        }

        public bool Add(DataItemCatg dataItemCatg)
        {
            try
            {
                db.DataItemCatg.Add(dataItemCatg);
                db.SaveChanges();
            }
            catch (Exception e) { return false; }
            return true;
        }

        public bool Set(DataItemCatg dataItemCatg)
        {
            try
            {
                db.DataItemCatg.Update(dataItemCatg);
                db.SaveChanges();
            }
            catch (Exception e) { return false; }
            return true;
        }

        public DataTable GetDataTable()
        {
            DataTable dataTable = new DataTable();

            using (SqlDataAdapter sqlDataAdapter = new SqlDataAdapter())
            {
                sqlDataAdapter.SelectCommand = new SqlCommand("Select * From DataItemCatg", new SqlConnection(db.ConnString));
                sqlDataAdapter.Fill(dataTable);
            }
            return dataTable;
        }
    }
}
