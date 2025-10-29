using SQLDBApp.Data;
using SQLDBApp.Models.DataItems;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SQLDBApp.Funcs
{
    public class ProdsFunc
    {
        private readonly DevSqlDBContext db;
        //public ProdsFunc() => db = new DevSqlDBContext();

        public ProdsFunc()
        {
            if (db == null)
                db = new DevSqlDBContext();
        }

        public List<DataItemProduct> GetAll()
        {
            List<DataItemProduct> dataItemProductsList = new();
            try
            {
                dataItemProductsList = db.DataItemProduct.ToList();
            }
            catch (Exception e)
            {
                System.Console.Out.WriteLine("DB(GetAll()) Error: " + e.Message);
                return null;
            }
            return dataItemProductsList;
        }

        public bool? IsExist(long id)
        {
            bool? result;
            try
            {
                result = db.DataItemProduct.Where(x => x.Id == id).Any();
            }
            catch (Exception e)
            {
                System.Console.Out.WriteLine("DB(GetDataItemProdById(int id)) Error: " + e.Message);
                return null;
            }
            return result;
        }

        public DataItemProduct GetById(int id)
        {
            DataItemProduct dataItemProduct;
            try
            {
                dataItemProduct = db.DataItemProduct.Where(x => x.Id == id).First();
            }
            catch (Exception e)
            {
                System.Console.Out.WriteLine("DB(GetById(int id)) Error: " + e.Message);
                return null;
            }
            return dataItemProduct;
        }

        public bool? Add(DataItemProduct dataItemProduct)
        {
            dataItemProduct.DateLastUpdated = DateTime.Now;
            try
            {
                if (dataItemProduct.Id == 0)
                    db.DataItemProduct.Add(dataItemProduct);
                else
                {
                    bool? check = IsExist(dataItemProduct.Id);
                    if (check != null || check == true)
                        return null;
                }
                db.SaveChanges();
            }
            catch (Exception e)
            {
                System.Console.Out.WriteLine("DB(Add(DataItemProduct dataItemProduct)) Error: " + e.Message);
                return null;
            }
            return true;
        }

        //make changes by cheking the insterted id like in         public bool? Add(StaplesProducts staplesProducts)
        public bool Add(DataItemProductList dataItemProductList)
        {
            if (dataItemProductList.dataItemProduct == null)
                return false;
            dataItemProductList.dataItemProduct.DateLastUpdated = DateTime.Now;

            try
            {
                if (dataItemProductList.dataItemProduct.Id > 0)
                {
                    if (!Set(dataItemProductList.dataItemProduct))
                        return false;
                }
                else
                    db.DataItemProduct.Add(dataItemProductList.dataItemProduct);

                foreach (DataItemPics dataItemPics in dataItemProductList.dataItemPicsList)
                {
                    dataItemPics.Id = dataItemProductList.dataItemProduct.Id;
                    dataItemPics.DateLastUpdated = DateTime.Now;
                    db.DataItemPics.Add(dataItemPics);
                }
                foreach (DataItemPrices dataItemPrices in dataItemProductList.dataItemPricesList)
                {
                    dataItemPrices.Id = dataItemProductList.dataItemProduct.Id;
                    dataItemPrices.DateLastUpdated = DateTime.Now;
                    db.DataItemPrices.Add(dataItemPrices);
                }
                foreach (DataItemSpecs dataItemSpecs in dataItemProductList.dataItemSpecsList)
                {
                    dataItemSpecs.Id = dataItemProductList.dataItemProduct.Id;
                    dataItemSpecs.DateLastUpdated = DateTime.Now;
                    db.DataItemSpecs.Add(dataItemSpecs);
                }
                if (dataItemProductList.dataItemCatgPerProd != null)
                {
                    dataItemProductList.dataItemCatgPerProd.Id = dataItemProductList.dataItemProduct.Id;
                    db.DataItemCatgPerProd.Add(dataItemProductList.dataItemCatgPerProd);
                }
                db.SaveChanges();
            }
            catch (Exception e)
            {
                System.Console.Out.WriteLine("DB(Add(DataItemProductList dataItemProduct)) Error: " + e.Message);
                return false;
            }
            return true;
        }

        public bool Add(List<DataItemProduct> dataItemProductsList)
        {
            foreach (DataItemProduct dataItemProduct in dataItemProductsList)
                dataItemProduct.DateLastUpdated = DateTime.Now;
            try
            {
                db.DataItemProduct.AddRange(dataItemProductsList);
                db.SaveChanges();
            }
            catch (Exception e)
            {
                System.Console.Out.WriteLine("DB(Add(List<DataItemProduct> dataItemProductsList)) Error: " + e.Message);
                return false;
            }
            return true;
        }

        public bool Set(DataItemProduct dataItemProductToSet)
        {
            DataItemProduct dataItemProduct;
            try
            {
                dataItemProduct = db.DataItemProduct.Where(x => x.Id == dataItemProductToSet.Id).First();
                dataItemProduct.Title = dataItemProductToSet.Title;
                dataItemProduct.Url = dataItemProductToSet.Url;
                dataItemProduct.ImgMain = dataItemProductToSet.ImgMain;
                dataItemProduct.ImgsGalleryHtml = dataItemProductToSet.ImgsGalleryHtml;
                dataItemProduct.Item = dataItemProductToSet.Item;
                dataItemProduct.Model = dataItemProductToSet.Model;
                dataItemProduct.HighlightsHtml = dataItemProductToSet.HighlightsHtml;
                dataItemProduct.DetailsHtml = dataItemProductToSet.DetailsHtml;
                dataItemProduct.SpecificationsHtml = dataItemProductToSet.SpecificationsHtml;
                dataItemProduct.PriceBuyDef = dataItemProductToSet.PriceBuyDef;
                dataItemProduct.PriceBuyCurrent = dataItemProductToSet.PriceBuyCurrent;
                dataItemProduct.PriceBuyDefAdv = dataItemProductToSet.PriceBuyDefAdv;
                dataItemProduct.PriceBuyCurrentAdv = dataItemProductToSet.PriceBuyCurrentAdv;
                dataItemProduct.PriceSellMin = dataItemProductToSet.PriceSellMin;
                dataItemProduct.PriceSellMax = dataItemProductToSet.PriceSellMax;
                dataItemProduct.IsAllBackOrdered = dataItemProductToSet.IsAllBackOrdered;
                dataItemProduct.DateLastAvail = dataItemProductToSet.DateLastAvail;
                dataItemProduct.DateOfferExp = dataItemProductToSet.DateOfferExp;
                dataItemProduct.OfferInfoHtml = dataItemProductToSet.OfferInfoHtml;
                dataItemProduct.UnitOfMeas = dataItemProductToSet.UnitOfMeas;
                dataItemProduct.ProductOptionsHtml = dataItemProductToSet.ProductOptionsHtml;
                dataItemProduct.HowManyStars = dataItemProductToSet.HowManyStars;
                dataItemProduct.HowManyRatings = dataItemProductToSet.HowManyRatings;
                dataItemProduct.IsCollectedFull = dataItemProductToSet.IsCollectedFull;
                dataItemProduct.IsActive = dataItemProductToSet.IsActive;
                dataItemProduct.DateLastUpdated = DateTime.Now;
                dataItemProduct.ShortProdUrl = dataItemProductToSet.ShortProdUrl;
                db.SaveChanges();
            }
            catch (Exception e)
            {
                System.Console.Out.WriteLine("DB(Set(DataItemProduct DataItemProductToSet)) Error: " + e.Message);
                return false;
            }
            return true;
        }

        public bool SetIsCollectedFullTrue(long id)
        {
            try
            {
                db.DataItemProduct.Where(x => x.Id == id).Single().IsCollectedFull = true;
                db.SaveChanges();
            }
            catch (Exception e)
            {
                System.Console.Out.WriteLine("DB(SetIsCollectedFullTrue(long id)) Error: " + e.Message);
                return false;
            }
            return true;
        }

        public DataItemProduct getNextIsCollectedFullFalse()
        {
            DataItemProduct dataItemProduct;
            try
            {
                dataItemProduct = db.DataItemProduct.Where(x => x.IsCollectedFull == false).First();
                if (dataItemProduct == null)
                    return null;
                db.SaveChanges();
            }
            catch (Exception e)
            {
                System.Console.Out.WriteLine("DB(getNextIsCollectedFullFalse()) Error: " + e.Message);
                return null;
            }
            return dataItemProduct;
        }

        public DataItemProduct getNextIsCollectedFullFalse(long fromId)
        {
            DataItemProduct dataItemProduct = new();
            try
            {
                dataItemProduct = db.DataItemProduct.Where(x => x.IsCollectedFull == false && x.Id > fromId).First();
            }
            catch (Exception e)
            {
                System.Console.Out.WriteLine("DB(getNextIsCollectedFullFalse(long fromId)) Error: " + e.Message);
                return null;
            }
            if (dataItemProduct == null)
                return null;
            return dataItemProduct;
        }

        public DataItemProduct getByUrl(string url)
        {
            if (string.IsNullOrEmpty(url)) return null;
            DataItemProduct dataItemProduct;
            try
            {
                dataItemProduct = db.DataItemProduct.Where(x => x.Url.ToLower().Equals(url.ToLower())).First();
            }
            catch (Exception e)
            {
                System.Console.Out.WriteLine("DB(getByUrl(string url)) Error: " + e.Message);
                return null;
            }
            return dataItemProduct;
        }

        public bool isExistByShortProdUrl(string shortProdUrl)
        {
            return db.DataItemProduct.Where(x => x.ShortProdUrl.ToLower().Equals(shortProdUrl.ToLower())).Any();
        }

        public DataItemProduct getByShortProdUrl(string shortProdUrl)
        {
            if (string.IsNullOrEmpty(shortProdUrl)) return null;
            DataItemProduct dataItemProduct;
            try
            {
                dataItemProduct = db.DataItemProduct.Where(x => x.ShortProdUrl.ToLower().Equals(shortProdUrl.ToLower())).First();
            }
            catch (Exception e)
            {
                System.Console.Out.WriteLine("DB(getByShortProdUrl(string shortProdUrl)) Error: " + e.Message);
                return null;
            }
            return dataItemProduct;
        }
    }
}