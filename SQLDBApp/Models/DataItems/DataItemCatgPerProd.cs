using System.ComponentModel.DataAnnotations.Schema;

namespace SQLDBApp.Models.DataItems
{
    public class DataItemCatgPerProd
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long Id { get; set; }
        public long DataItemCatgPerProdId { get; set; }
        public string Name1 { get; set; }
        public string Name2 { get; set; }
        public string Name3 { get; set; }
        public string Name4 { get; set; }
        public string Name5 { get; set; }
        public string Name6 { get; set; }
        public string Name7 { get; set; }
        public string Href1 { get; set; }
        public string Href2 { get; set; }
        public string Href3 { get; set; }
        public string Href4 { get; set; }
        public string Href5 { get; set; }
        public string Href6 { get; set; }
        public string Href7 { get; set; }
    }
}
