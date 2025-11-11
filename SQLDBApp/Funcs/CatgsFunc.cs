using Microsoft.Data.SqlClient;
using SQLDBApp.Data;
using SQLDBApp.Models.DataItems;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace SQLDBApp.Funcs
{
    /// <summary>
    /// Provides data access layer functions for managing product categories.
    /// Handles CRUD operations for DataItemCatg entities including URL management,
    /// existence checks, and collection status tracking.
    /// </summary>
    public class CatgsFunc
    {
        /// <summary>
        /// Database context for Entity Framework operations.
        /// </summary>
        private readonly DevSqlDBContext db;

        /// <summary>
        /// Initializes a new instance of the <see cref="CatgsFunc"/> class.
        /// Creates a new <see cref="DevSqlDBContext"/> for DB operations.
        /// </summary>
        public CatgsFunc() => db = new DevSqlDBContext();

        /// <summary>
        /// Retrieves all category items from the database.
        /// </summary>
        /// <returns>List of <see cref="DataItemCatg"/> from the database.</returns>
        public List<DataItemCatg> GetAll()
        {
            return db.DataItemCatg.ToList();
        }

        /// <summary>
        /// Retrieves a category item by its identifier.
        /// </summary>
        /// <param name="id">The identifier of the category item.</param>
        /// <returns>The matching <see cref="DataItemCatg"/> or null if not found.</returns>
        public DataItemCatg GetCatgById(int id)
        {
            return db.DataItemCatg.FirstOrDefault(x => x.Id == id);
        }

        /// <summary>
        /// Adds a category URL if it does not already exist.
        /// </summary>
        /// <param name="url">The category URL to add.</param>
        public void AddUrl(String url)
        {
            if (IsExist(url))
                return;
            Add(new DataItemCatg() { Url = url });
        }

        /// <summary>
        /// Adds multiple category URLs.
        /// Skips URLs that already exist.
        /// </summary>
        /// <param name="dataItemcatgArr">Array of category URLs.</param>
        /// <returns>True if operation attempted; false only on unexpected failure.</returns>
        public bool AddUrl(string[] dataItemcatgArr)
        {
            List<DataItemCatg> dataItemCatgsList = new List<DataItemCatg>();

            foreach (string dataItemcatg in dataItemcatgArr)
                if (!IsExist(dataItemcatg))
                    dataItemCatgsList.Add(new DataItemCatg() { Url = dataItemcatg });
            Add(dataItemCatgsList);
            return true;
        }

        /// <summary>
        /// Adds category entries using id/url pairs if they don't already exist.
        /// </summary>
        /// <param name="strList">List of string arrays where each array contains at least id and url.</param>
        /// <returns>True if the add succeeded; false otherwise.</returns>
        public bool AddSidUrl(List<string[]> strList)
        {
            List<DataItemCatg> dataItemCatgList = new List<DataItemCatg>();

            foreach (string[] str in strList)
                if (!IsExist(str[1]))
                    dataItemCatgList.Add(new DataItemCatg() { Id = int.Parse(str[0]), Url = str[1] });
            return Add(dataItemCatgList);
        }

        /// <summary>
        /// Determines whether a category URL already exists in the database.
        /// </summary>
        /// <param name="url">The category URL to check.</param>
        /// <returns>True if the URL exists; otherwise false.</returns>
        public bool IsExist(String url)
        {
            return db.DataItemCatg.FirstOrDefault(x => x.Url.Equals(url)) != null;
        }

        /// <summary>
        /// Sets the <c>IsCollectedHRef</c> flag to true for the specified category id.
        /// </summary>
        /// <param name="id">The category identifier.</param>
        /// <returns>True if update succeeded; false on exception.</returns>
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

        /// <summary>
        /// Gets the next category whose <c>IsCollectedHRef</c> is false.
        /// Note: currently returns a temporary test item (see in-method TEMP comment).
        /// </summary>
        /// <returns>A <see cref="DataItemCatg"/> that needs href collection, or null.</returns>
        public DataItemCatg GetNextIsCollectedHRefFalse()
        {
            //////TEMP FOR TEST
            DataItemCatg test1 = new DataItemCatg();
            test1.Url = @"https://www.gmail.com/";
            return test1;
            /////END TEMP
           // return db.DataItemCatg.FirstOrDefault(x => x.IsCollectedHRef == false);
        }

        /// <summary>
        /// Adds a list of <see cref="DataItemCatg"/> to the database.
        /// </summary>
        /// <param name="dataItemCatgList">List of category items to add.</param>
        /// <returns>True if add succeeded; false on exception.</returns>
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

        /// <summary>
        /// Adds a single <see cref="DataItemCatg"/> to the database.
        /// </summary>
        /// <param name="dataItemCatg">The category item to add.</param>
        /// <returns>True if add succeeded; false on exception.</returns>
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

        /// <summary>
        /// Updates an existing <see cref="DataItemCatg"/> in the database.
        /// </summary>
        /// <param name="dataItemCatg">The category item with updated values.</param>
        /// <returns>True if update succeeded; false on exception.</returns>
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

        /// <summary>
        /// Retrieves all category items as a <see cref="DataTable"/>.
        /// </summary>
        /// <returns>A <see cref="DataTable"/> filled with category records.</returns>
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
