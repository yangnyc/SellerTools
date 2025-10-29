using System.ComponentModel.DataAnnotations.Schema;

namespace SQLDBApp.Models.DataItems
{
    public class DataItemPrices
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long Id { get; set; }

        public long DataItemPricesId { get; set; }

        public System.Nullable<int> ZipCodeTo { get; set; }

        public string CityTo { get; set; }

        public string StateTo { get; set; }

        public System.Nullable<bool> IsOutOfStock { get; set; } = true;

        public string DeliveredBy { get; set; }

        public System.Nullable<bool> IsBackOrdered { get; set; }

        public string BackOrderTill { get; set; }

        public System.Nullable<double> PriceBuyDef { get; set; } = 0;

        public System.Nullable<double> PriceBuyCurrent { get; set; } = 0;

        public string DateOfferExp { get; set; }

        public System.Nullable<double> PriceSellDef { get; set; } = 0;

        public System.Nullable<double> PriceSellCurrent { get; set; } = 0;

        public System.Nullable<double> PriceSellMin { get; set; } = 0;

        public System.Nullable<double> PriceSellMax { get; set; } = 0;

        public string DateLastInStock { get; set; }

        public string OfferInfoHtml { get; set; }

        public System.DateTime DateLastUpdated { get; set; }
    }
}