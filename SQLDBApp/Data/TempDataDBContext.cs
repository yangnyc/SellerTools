using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System.IO;

namespace SQLDBApp.Data
{
    public class TempDataDBContext : DbContext
    {
        public string ConnString { get; set; }

        public TempDataDBContext(DbContextOptions<TempDataDBContext> options) : base(options) { }

        public TempDataDBContext() { AssignConnString(); }

        public TempDataDBContext(string ConnectionString)
        {
            ConnString = ConnectionString;
        }

        private void AssignConnString()
        {
            if (ConnString == null)
                ConnString = (string)(JObject.Parse(System.IO.File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json"))))["ConnectionStrings"]["TempDataDBContext"];
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            AssignConnString();
            if (!optionsBuilder.IsConfigured)
                optionsBuilder.UseSqlServer(ConnString);
        }

    }
}
