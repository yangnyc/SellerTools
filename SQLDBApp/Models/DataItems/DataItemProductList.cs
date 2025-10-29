using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace SQLDBApp.Models.DataItems
{
    public class DataItemProductList
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public DataItemProduct dataItemProduct;
        public List<DataItemPrices> dataItemPricesList;
        public List<DataItemPics> dataItemPicsList;
        public List<DataItemSpecs> dataItemSpecsList;
        public DataItemCatgPerProd dataItemCatgPerProd;

        public DataItemProductList()
        {
            dataItemProduct = new DataItemProduct();
            dataItemPricesList = new List<DataItemPrices>();
            dataItemPicsList = new List<DataItemPics>();
            dataItemSpecsList = new List<DataItemSpecs>();
        }
    }
}
