using System.ComponentModel.DataAnnotations.Schema;

namespace SQLDBApp.Models.DataItems
{
    /// <summary>
    /// Represents a product entity in the database.
    /// Stores comprehensive product information including title, pricing, images,
    /// specifications, availability, and collection status.
    /// </summary>
    public class DataItemProduct
    {
        /// <summary>
        /// Gets or sets the unique identifier for the product.
        /// Database-generated option is disabled for manual ID assignment.
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long Id { get; set; }

        /// <summary>
        /// Gets or sets the product number or SKU.
        /// </summary>
        public long Product { get; set; }

        /// <summary>
        /// Gets or sets the product title.
        /// </summary>
        public string Title { get; set; }
        // { get { return this.Title; } set { this.Title = HttpUtility.HtmlEncode(value); } }

        /// <summary>
        /// Gets or sets the full URL to the product page.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Gets or sets the URL to the main product image.
        /// </summary>
        public string ImgMain { get; set; }

        /// <summary>
        /// Gets or sets the HTML content for the product image gallery.
        /// </summary>
        public string ImgsGalleryHtml { get; set; }

        /// <summary>
        /// Gets or sets the item identifier or code.
        /// </summary>
        public string Item { get; set; }

        /// <summary>
        /// Gets or sets the product model number or name.
        /// </summary>
        public string Model { get; set; }

        /// <summary>
        /// Gets or sets the HTML content for product highlights or key features.
        /// </summary>
        public string HighlightsHtml { get; set; }

        /// <summary>
        /// Gets or sets the HTML content for detailed product description.
        /// </summary>
        public string DetailsHtml { get; set; }

        /// <summary>
        /// Gets or sets the HTML content for product specifications.
        /// </summary>
        public string SpecificationsHtml { get; set; }

        /// <summary>
        /// Gets or sets the default purchase price.
        /// </summary>
        public System.Nullable<double> PriceBuyDef { get; set; } = 0;

        /// <summary>
        /// Gets or sets the current purchase price (may include discounts).
        /// </summary>
        public System.Nullable<double> PriceBuyCurrent { get; set; } = 0;

        /// <summary>
        /// Gets or sets the default advertised purchase price.
        /// </summary>
        public System.Nullable<double> PriceBuyDefAdv { get; set; } = 0;

        /// <summary>
        /// Gets or sets the current advertised purchase price.
        /// </summary>
        public System.Nullable<double> PriceBuyCurrentAdv { get; set; } = 0;

        /// <summary>
        /// Gets or sets the minimum recommended selling price.
        /// </summary>
        public System.Nullable<double> PriceSellMin { get; set; } = 0;

        /// <summary>
        /// Gets or sets the maximum recommended selling price.
        /// </summary>
        public System.Nullable<double> PriceSellMax { get; set; } = 0;

        /// <summary>
        /// Gets or sets a value indicating whether all variants/options are on back order.
        /// </summary>
        public System.Nullable<bool> IsAllBackOrdered { get; set; }

        /// <summary>
        /// Gets or sets the date when the product was last available.
        /// </summary>
        public string DateLastAvail { get; set; }

        /// <summary>
        /// Gets or sets the date when current price offer expires.
        /// </summary>
        public string DateOfferExp { get; set; }

        /// <summary>
        /// Gets or sets the HTML content describing promotional offers.
        /// </summary>
        public string OfferInfoHtml { get; set; }

        /// <summary>
        /// Gets or sets the unit of measurement for the product.
        /// </summary>
        public string UnitOfMeas { get; set; }

        /// <summary>
        /// Gets or sets the HTML content for product options or variations.
        /// </summary>
        public string ProductOptionsHtml { get; set; }

        /// <summary>
        /// Gets or sets the number of stars in the product rating.
        /// </summary>
        public int HowManyStars { get; set; }

        /// <summary>
        /// Gets or sets the total number of customer ratings received.
        /// </summary>
        public int HowManyRatings { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether full product data has been collected.
        /// </summary>
        public bool IsCollectedFull { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether the product is active in the system.
        /// </summary>
        public bool IsActive { get; set; } = false;

        /// <summary>
        /// Gets or sets the date and time when the product record was last updated.
        /// </summary>
        public System.DateTime DateLastUpdated { get; set; }

        /// <summary>
        /// Gets or sets the shortened version of the product URL.
        /// </summary>
        public string ShortProdUrl { get; set; }
    }
}