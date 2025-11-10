using System.ComponentModel.DataAnnotations.Schema;

namespace SQLDBApp.Models.DataItems
{
    public class DataItemProduct
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long Id { get; set; }

        public long Product { get; set; }

        public string Title { get; set; }
        // { get { return this.Title; } set { this.Title = HttpUtility.HtmlEncode(value); } }

        public string Url { get; set; }

        public string ImgMain { get; set; }

        public string ImgsGalleryHtml { get; set; }

        public string Item { get; set; }

        public string Model { get; set; }

        public string HighlightsHtml { get; set; }

        public string DetailsHtml { get; set; }

        public string SpecificationsHtml { get; set; }

        public System.Nullable<double> PriceBuyDef { get; set; } = 0;

        public System.Nullable<double> PriceBuyCurrent { get; set; } = 0;

        public System.Nullable<double> PriceBuyDefAdv { get; set; } = 0;

        public System.Nullable<double> PriceBuyCurrentAdv { get; set; } = 0;

        public System.Nullable<double> PriceSellMin { get; set; } = 0;

        public System.Nullable<double> PriceSellMax { get; set; } = 0;

        public System.Nullable<bool> IsAllBackOrdered { get; set; }

        public string DateLastAvail { get; set; }

        public string DateOfferExp { get; set; }

        public string OfferInfoHtml { get; set; }

        public string UnitOfMeas { get; set; }

        public string ProductOptionsHtml { get; set; }

        public int HowManyStars { get; set; }

        public int HowManyRatings { get; set; }

        public bool IsCollectedFull { get; set; } = false;

        public bool IsActive { get; set; } = false;

        public System.DateTime DateLastUpdated { get; set; }

        public string ShortProdUrl { get; set; }
    }
}