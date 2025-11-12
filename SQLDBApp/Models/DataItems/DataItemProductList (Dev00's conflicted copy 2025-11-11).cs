using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace SQLDBApp.Models.DataItems
{
    /// <summary>
    /// Container class that aggregates a product with all its related data.
    /// Used for batch operations where product, prices, pictures, specs, and category
    /// information need to be handled together.
    /// </summary>
    public class DataItemProductList
    {
        /// <summary>
        /// Gets or sets the core product information.
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public DataItemProduct dataItemProduct;

        /// <summary>
        /// Gets or sets the list of price records associated with the product.
        /// </summary>
        public List<DataItemPrices> dataItemPricesList;

        /// <summary>
        /// Gets or sets the list of picture records associated with the product.
        /// </summary>
        public List<DataItemPics> dataItemPicsList;

        /// <summary>
        /// Gets or sets the list of specification records associated with the product.
        /// </summary>
        public List<DataItemSpecs> dataItemSpecsList;

        /// <summary>
        /// Gets or sets the category navigation breadcrumb for the product.
        /// </summary>
        public DataItemCatgPerProd dataItemCatgPerProd;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataItemProductList"/> class.
        /// Creates empty product, prices, pictures, and specifications collections.
        /// </summary>
        public DataItemProductList()
        {
            dataItemProduct = new DataItemProduct();
            dataItemPricesList = new List<DataItemPrices>();
            dataItemPicsList = new List<DataItemPics>();
            dataItemSpecsList = new List<DataItemSpecs>();
        }
    }
}
