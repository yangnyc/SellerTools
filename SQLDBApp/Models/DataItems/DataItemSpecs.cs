using System.ComponentModel.DataAnnotations.Schema;

namespace SQLDBApp.Models.DataItems
{
    public class DataItemSpecs
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long Id { get; set; }

        public long DataItemSpecsId { get; set; }

        public int OrderNum { get; set; }

        public string Name { get; set; }

        public string Info { get; set; }

        public string Html { get; set; }

        public System.DateTime DateLastUpdated { get; set; }
    }
}