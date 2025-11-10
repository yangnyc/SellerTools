using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System.IO;

namespace SQLDBApp.Data
{
    public class TempDataDBContext : DbContext
    {
        /// <summary>
        /// Gets or sets the connection string used by this context.
        /// </summary>
        public string ConnString { get; set; }

        /// <summary>
        /// Initializes a new instance of <see cref="TempDataDBContext"/> using provided options.
        /// </summary>
        /// <param name="options">The <see cref="DbContextOptions{TempDataDBContext}"/>.</param>
        public TempDataDBContext(DbContextOptions<TempDataDBContext> options) : base(options) { }

        /// <summary>
        /// Initializes a new instance of <see cref="TempDataDBContext"/> and assigns the connection string from configuration.
        /// </summary>
        public TempDataDBContext() { AssignConnString(); }

        /// <summary>
        /// Initializes a new instance of <see cref="TempDataDBContext"/> with an explicit connection string.
        /// </summary>
        /// <param name="ConnectionString">Connection string to use for this context.</param>
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
