using System.ComponentModel.DataAnnotations.Schema;

namespace SQLDBApp.Models.DataItems
{
    /// <summary>
    /// Represents product pricing and availability data for specific locations.
    /// Tracks buy/sell prices, stock status, delivery information, and promotional offers.
    /// </summary>
    public class DataItemPrices
    {
        /// <summary>
        /// Gets or sets the product identifier this price record belongs to.
        /// Database-generated option is disabled for manual ID assignment.
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long Id { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier for this price entry.
        /// </summary>
        public long DataItemPricesId { get; set; }

        /// <summary>
        /// Gets or sets the destination ZIP code for shipping/pricing calculations.
        /// </summary>
        public System.Nullable<int> ZipCodeTo { get; set; }

        /// <summary>
        /// Gets or sets the destination city for shipping/pricing calculations.
        /// </summary>
        public string CityTo { get; set; }

        /// <summary>
        /// Gets or sets the destination state for shipping/pricing calculations.
        /// </summary>
        public string StateTo { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the product is out of stock.
        /// </summary>
        public System.Nullable<bool> IsOutOfStock { get; set; } = true;

        /// <summary>
        /// Gets or sets the expected delivery date or timeframe.
        /// </summary>
        public string DeliveredBy { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the product is on back order.
        /// </summary>
        public System.Nullable<bool> IsBackOrdered { get; set; }

        /// <summary>
        /// Gets or sets the expected date when back-ordered items will be available.
        /// </summary>
        public string BackOrderTill { get; set; }

        /// <summary>
        /// Gets or sets the default purchase price.
        /// </summary>
        public System.Nullable<double> PriceBuyDef { get; set; } = 0;

        /// <summary>
        /// Gets or sets the current purchase price (may include discounts or promotions).
        /// </summary>
        public System.Nullable<double> PriceBuyCurrent { get; set; } = 0;

        /// <summary>
        /// Gets or sets the date when the current price offer expires.
        /// </summary>
        public string DateOfferExp { get; set; }

        /// <summary>
        /// Gets or sets the default selling price.
        /// </summary>
        public System.Nullable<double> PriceSellDef { get; set; } = 0;

        /// <summary>
        /// Gets or sets the current selling price.
        /// </summary>
        public System.Nullable<double> PriceSellCurrent { get; set; } = 0;

        /// <summary>
        /// Gets or sets the minimum selling price for this product.
        /// </summary>
        public System.Nullable<double> PriceSellMin { get; set; } = 0;

        /// <summary>
        /// Gets or sets the maximum selling price for this product.
        /// </summary>
        public System.Nullable<double> PriceSellMax { get; set; } = 0;

        /// <summary>
        /// Gets or sets the date the product was last in stock.
        /// </summary>
        public string DateLastInStock { get; set; }

        /// <summary>
        /// Gets or sets HTML content with offer or promotional information.
        /// </summary>
        public string OfferInfoHtml { get; set; }

        /// <summary>
        /// Gets or sets the date and time when this price record was last updated.
        /// </summary>
        public System.DateTime DateLastUpdated { get; set; }
    }
}