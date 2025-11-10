using System.ComponentModel.DataAnnotations.Schema;

namespace SQLDBApp.Models.DataItems
{
    public class DataItemPics
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long Id { get; set; }

        public long DataItemPicsId { get; set; }

        public int OrderNum { get; set; }

        public string Url { get; set; }

        public string UrlHtml { get; set; }

        public System.DateTime DateLastUpdated { get; set; }
    }
}