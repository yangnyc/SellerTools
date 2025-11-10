using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System.IO;

namespace SQLDBApp.Data
{
    /// <summary>
    /// Entity Framework database context for development SQL database.
    /// </summary>
    public class DevSqlDBContext : DbContext
    {
        public string ConnString { get; set; }

        /// <summary>
        /// Initializes a new instance of <see cref="DevSqlDBContext"/> using provided options.
        /// </summary>
        /// <param name="options">The <see cref="DbContextOptions{TContext}"/> to configure the context.</param>
        public DevSqlDBContext(DbContextOptions<DevSqlDBContext> options) : base(options) { }

        /// <summary>
        /// Initializes a new instance of <see cref="DevSqlDBContext"/> and assigns the connection string from configuration.
        /// </summary>
        public DevSqlDBContext() { AssignConnString(); }

        /// <summary>
        /// Initializes a new instance of <see cref="DevSqlDBContext"/> using an explicit connection string.
        /// </summary>
        /// <param name="ConnectionString">Connection string to use for the context.</param>
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
