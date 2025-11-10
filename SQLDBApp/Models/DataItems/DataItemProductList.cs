using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace SQLDBApp.Models.DataItems
{
    /// <summary>
    /// Container class for a product and its related data including prices, pictures, specs, and categories.
    /// </summary>
    public class DataItemProductList
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public DataItemProduct dataItemProduct;
        public List<DataItemPrices> dataItemPricesList;
        public List<DataItemPics> dataItemPicsList;
        public List<DataItemSpecs> dataItemSpecsList;
        public DataItemCatgPerProd dataItemCatgPerProd;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataItemProductList"/> class with empty collections.
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
