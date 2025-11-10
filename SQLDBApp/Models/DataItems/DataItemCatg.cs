using System.ComponentModel.DataAnnotations.Schema;

namespace SQLDBApp.Models.DataItems
{
    public class DataItemCatg
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long Id { get; set; }

        public string CatgID { get; set; }

        public System.Nullable<int> CatgCL { get; set; } = 0;

        public string Url { get; set; }

        public System.Nullable<int> PrvId { get; set; } = 0;

        public System.Nullable<long> FullPathId { get; set; } = 0;

        public string Name { get; set; }

        public string C1 { get; set; }

        public string C2 { get; set; }

        public string C3 { get; set; }

        public string C4 { get; set; }

        public string C5 { get; set; }

        public string C6 { get; set; }

        public string C7 { get; set; }

        public string C8 { get; set; }

        public string C9 { get; set; }

        public string C10 { get; set; }

        public bool IsCollectedHRef { get; set; } = false;
    }
}