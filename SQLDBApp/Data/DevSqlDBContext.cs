using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System.IO;

namespace SQLDBApp.Data
{
    public class DevSqlDBContext : DbContext
    {
        public string ConnString { get; set; }

        public DevSqlDBContext(DbContextOptions<DevSqlDBContext> options) : base(options) { }

        public DevSqlDBContext() { AssignConnString(); }

        public DevSqlDBContext(string ConnectionString)
        {
            ConnString = ConnectionString;
        }

        private void AssignConnString()
        {
            if (ConnString == null)
                ConnString = (string)(JObject.Parse(System.IO.File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json"))))["ConnectionStrings"]["DevSqDBContext"];
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            AssignConnString();
            if (!optionsBuilder.IsConfigured)
                optionsBuilder.UseSqlServer(ConnString);
        }

        public DbSet<SQLDBApp.Models.DataItems.DataItemProduct> DataItemProduct { get; set; }
        public DbSet<SQLDBApp.Models.DataItems.DataItemPics> DataItemPics { get; set; }
        public DbSet<SQLDBApp.Models.DataItems.DataItemSpecs> DataItemSpecs { get; set; }
        public DbSet<SQLDBApp.Models.DataItems.DataItemPrices> DataItemPrices { get; set; }
        public DbSet<SQLDBApp.Models.DataItems.DataItemCatgPerProd> DataItemCatgPerProd { get; set; }
        public DbSet<SQLDBApp.Models.DataItems.DataItemCatg> DataItemCatg { get; set; }
    }
}
