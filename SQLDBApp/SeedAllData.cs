using SQLDBApp.Data;
using SQLDBApp.Models.DataItems;
using System;
using System.Linq;

namespace SQLDBApp
{
    public class SeedAllData
    {
        public static void Main(string[] args)
        {
            using var context = new DevSqlDBContext();
            var random = new Random();

            // Get existing product IDs
            var productIds = context.DataItemProduct.Select(p => p.Id).ToList();
            Console.WriteLine($"Found {productIds.Count} existing products.");

            if (productIds.Count == 0)
            {
                Console.WriteLine("No products found! Please seed DataItemProduct first.");
                return;
            }

            // 1. Seed DataItemCatg (Categories) - 1000 items
            Console.WriteLine("\n=== Seeding DataItemCatg (Categories) ===");
            var categoryNames = new[] 
            { 
                "Electronics", "Home & Garden", "Sports & Outdoors", "Toys & Games", 
                "Books & Media", "Clothing & Accessories", "Tools & Hardware", "Kitchen & Dining",
                "Health & Beauty", "Automotive", "Office Supplies", "Pet Supplies"
            };
            
            for (int i = 1; i <= 1000; i++)
            {
                var catgCL = random.Next(1, 6); // Category level 1-5
                var category = new DataItemCatg
                {
                    Id = i,
                    CatgID = $"CATG-{i:D5}",
                    CatgCL = catgCL,
                    Url = $"https://example.com/category/{i}",
                    PrvId = catgCL > 1 ? random.Next(1, i) : 0,
                    FullPathId = i * 1000 + catgCL,
                    Name = $"{categoryNames[random.Next(categoryNames.Length)]} - Level {catgCL} - {i}",
                    C1 = random.Next(0, 3) == 0 ? $"Custom1-{i}" : null,
                    C2 = random.Next(0, 3) == 0 ? $"Custom2-{i}" : null,
                    C3 = random.Next(0, 3) == 0 ? $"Custom3-{i}" : null,
                    C4 = random.Next(0, 3) == 0 ? $"Custom4-{i}" : null,
                    C5 = random.Next(0, 3) == 0 ? $"Custom5-{i}" : null,
                    C6 = random.Next(0, 3) == 0 ? $"Custom6-{i}" : null,
                    C7 = random.Next(0, 3) == 0 ? $"Custom7-{i}" : null,
                    C8 = random.Next(0, 3) == 0 ? $"Custom8-{i}" : null,
                    C9 = random.Next(0, 3) == 0 ? $"Custom9-{i}" : null,
                    C10 = random.Next(0, 3) == 0 ? $"Custom10-{i}" : null,
                    IsCollectedHRef = random.Next(0, 10) > 2
                };
                
                context.DataItemCatg.Add(category);
                
                if (i % 100 == 0)
                {
                    Console.WriteLine($"Added {i} categories...");
                    context.SaveChanges();
                }
            }
            context.SaveChanges();
            Console.WriteLine("✓ Completed seeding DataItemCatg");

            // 2. Seed DataItemCatgPerProd (Category per Product) - 1000 items
            Console.WriteLine("\n=== Seeding DataItemCatgPerProd (Category Breadcrumbs) ===");
            var breadcrumbLevels = new[] { "Home", "Department", "Category", "Subcategory", "Type", "Brand", "Model" };
            
            for (int i = 1; i <= 1000; i++)
            {
                var productId = productIds[i - 1]; // Use each product once
                var numLevels = random.Next(2, 8); // 2-7 breadcrumb levels
                
                var catgPerProd = new DataItemCatgPerProd
                {
                    Id = productId,
                    DataItemCatgPerProdId = i,
                    Name1 = numLevels >= 1 ? breadcrumbLevels[0] : null,
                    Name2 = numLevels >= 2 ? $"{categoryNames[random.Next(categoryNames.Length)]}" : null,
                    Name3 = numLevels >= 3 ? $"Category-{random.Next(1, 100)}" : null,
                    Name4 = numLevels >= 4 ? $"Subcategory-{random.Next(1, 50)}" : null,
                    Name5 = numLevels >= 5 ? $"Type-{random.Next(1, 30)}" : null,
                    Name6 = numLevels >= 6 ? $"Brand-{random.Next(1, 20)}" : null,
                    Name7 = numLevels >= 7 ? $"Model-{random.Next(1, 10)}" : null,
                    Href1 = numLevels >= 1 ? "/" : null,
                    Href2 = numLevels >= 2 ? $"/dept/{random.Next(1, 20)}" : null,
                    Href3 = numLevels >= 3 ? $"/cat/{random.Next(1, 100)}" : null,
                    Href4 = numLevels >= 4 ? $"/subcat/{random.Next(1, 50)}" : null,
                    Href5 = numLevels >= 5 ? $"/type/{random.Next(1, 30)}" : null,
                    Href6 = numLevels >= 6 ? $"/brand/{random.Next(1, 20)}" : null,
                    Href7 = numLevels >= 7 ? $"/model/{random.Next(1, 10)}" : null
                };
                
                context.DataItemCatgPerProd.Add(catgPerProd);
                
                if (i % 100 == 0)
                {
                    Console.WriteLine($"Added {i} category breadcrumbs...");
                    context.SaveChanges();
                }
            }
            context.SaveChanges();
            Console.WriteLine("✓ Completed seeding DataItemCatgPerProd");

            // 3. Seed DataItemPics (Product Pictures) - ~3000 items (3 per product on average)
            Console.WriteLine("\n=== Seeding DataItemPics (Product Pictures) ===");
            int picId = 1;
            
            foreach (var productId in productIds.Take(1000))
            {
                int numPics = random.Next(1, 6); // 1-5 pictures per product
                
                for (int j = 0; j < numPics; j++)
                {
                    var pic = new DataItemPics
                    {
                        Id = productId,
                        DataItemPicsId = picId++,
                        OrderNum = j + 1,
                        Url = $"https://example.com/images/product-{productId}-pic-{j + 1}.jpg",
                        UrlHtml = $"<img src='https://example.com/images/product-{productId}-pic-{j + 1}.jpg' alt='Product {productId} Image {j + 1}' />",
                        DateLastUpdated = DateTime.Now.AddDays(-random.Next(0, 30))
                    };
                    
                    context.DataItemPics.Add(pic);
                }
                
                if (picId % 300 == 0)
                {
                    Console.WriteLine($"Added {picId} pictures...");
                    context.SaveChanges();
                }
            }
            context.SaveChanges();
            Console.WriteLine($"✓ Completed seeding DataItemPics - Total: {picId - 1} pictures");

            // 4. Seed DataItemSpecs (Product Specifications) - ~5000 items (5 per product on average)
            Console.WriteLine("\n=== Seeding DataItemSpecs (Product Specifications) ===");
            var specNames = new[] 
            { 
                "Weight", "Dimensions", "Color", "Material", "Warranty", 
                "Power Source", "Voltage", "Capacity", "Brand", "Model Number",
                "Country of Origin", "Assembly Required", "Battery Included", "Age Range", "Care Instructions"
            };
            int specId = 1;
            
            foreach (var productId in productIds.Take(1000))
            {
                int numSpecs = random.Next(3, 8); // 3-7 specs per product
                var usedSpecs = new System.Collections.Generic.HashSet<string>();
                
                for (int j = 0; j < numSpecs; j++)
                {
                    string specName;
                    do
                    {
                        specName = specNames[random.Next(specNames.Length)];
                    } while (usedSpecs.Contains(specName) && usedSpecs.Count < specNames.Length);
                    
                    usedSpecs.Add(specName);
                    
                    var spec = new DataItemSpecs
                    {
                        Id = productId,
                        DataItemSpecsId = specId++,
                        OrderNum = j + 1,
                        Name = specName,
                        Info = GenerateSpecValue(specName, random),
                        Html = $"<tr><td><strong>{specName}</strong></td><td>{GenerateSpecValue(specName, random)}</td></tr>",
                        DateLastUpdated = DateTime.Now.AddDays(-random.Next(0, 30))
                    };
                    
                    context.DataItemSpecs.Add(spec);
                }
                
                if (specId % 500 == 0)
                {
                    Console.WriteLine($"Added {specId} specifications...");
                    context.SaveChanges();
                }
            }
            context.SaveChanges();
            Console.WriteLine($"✓ Completed seeding DataItemSpecs - Total: {specId - 1} specifications");

            // 5. Seed DataItemPrices (Product Prices) - ~2000 items (2 per product on average)
            Console.WriteLine("\n=== Seeding DataItemPrices (Product Prices by Location) ===");
            var cities = new[] { "New York", "Los Angeles", "Chicago", "Houston", "Phoenix", "Philadelphia", "San Antonio", "San Diego", "Dallas", "San Jose" };
            var states = new[] { "NY", "CA", "IL", "TX", "AZ", "PA", "TX", "CA", "TX", "CA" };
            int priceId = 1;
            
            foreach (var productId in productIds.Take(1000))
            {
                int numPrices = random.Next(1, 4); // 1-3 price records per product
                
                for (int j = 0; j < numPrices; j++)
                {
                    var cityIndex = random.Next(cities.Length);
                    var isOutOfStock = random.Next(0, 10) > 7;
                    var isBackOrdered = !isOutOfStock && random.Next(0, 10) > 7;
                    
                    var price = new DataItemPrices
                    {
                        Id = productId,
                        DataItemPricesId = priceId++,
                        ZipCodeTo = 10000 + random.Next(1, 90000),
                        CityTo = cities[cityIndex],
                        StateTo = states[cityIndex],
                        IsOutOfStock = isOutOfStock,
                        DeliveredBy = isOutOfStock ? null : DateTime.Now.AddDays(random.Next(3, 30)).ToString("yyyy-MM-dd"),
                        IsBackOrdered = isBackOrdered,
                        BackOrderTill = isBackOrdered ? DateTime.Now.AddDays(random.Next(7, 60)).ToString("yyyy-MM-dd") : null,
                        PriceBuyDef = Math.Round(random.NextDouble() * 500 + 50, 2),
                        PriceBuyCurrent = Math.Round(random.NextDouble() * 500 + 50, 2),
                        DateOfferExp = DateTime.Now.AddDays(random.Next(1, 90)).ToString("yyyy-MM-dd"),
                        PriceSellDef = Math.Round(random.NextDouble() * 700 + 70, 2),
                        PriceSellCurrent = Math.Round(random.NextDouble() * 700 + 70, 2),
                        PriceSellMin = Math.Round(random.NextDouble() * 600 + 60, 2),
                        PriceSellMax = Math.Round(random.NextDouble() * 1000 + 100, 2),
                        DateLastInStock = isOutOfStock ? DateTime.Now.AddDays(-random.Next(1, 365)).ToString("yyyy-MM-dd") : null,
                        OfferInfoHtml = random.Next(0, 3) == 0 ? $"<p>Special: Save {random.Next(10, 40)}% on orders to {cities[cityIndex]}!</p>" : null,
                        DateLastUpdated = DateTime.Now.AddDays(-random.Next(0, 30))
                    };
                    
                    context.DataItemPrices.Add(price);
                }
                
                if (priceId % 200 == 0)
                {
                    Console.WriteLine($"Added {priceId} price records...");
                    context.SaveChanges();
                }
            }
            context.SaveChanges();
            Console.WriteLine($"✓ Completed seeding DataItemPrices - Total: {priceId - 1} price records");

            Console.WriteLine("\n========================================");
            Console.WriteLine("✓✓✓ ALL DATA SEEDING COMPLETED! ✓✓✓");
            Console.WriteLine("========================================");
            Console.WriteLine($"Categories: 1000");
            Console.WriteLine($"Category Breadcrumbs: 1000");
            Console.WriteLine($"Product Pictures: {picId - 1}");
            Console.WriteLine($"Product Specifications: {specId - 1}");
            Console.WriteLine($"Price Records: {priceId - 1}");
        }

        private static string GenerateSpecValue(string specName, Random random)
        {
            return specName switch
            {
                "Weight" => $"{random.Next(1, 100)} lbs",
                "Dimensions" => $"{random.Next(5, 50)}\" x {random.Next(5, 50)}\" x {random.Next(5, 50)}\"",
                "Color" => new[] { "Black", "White", "Silver", "Gray", "Blue", "Red", "Green", "Yellow" }[random.Next(8)],
                "Material" => new[] { "Plastic", "Metal", "Wood", "Glass", "Fabric", "Composite" }[random.Next(6)],
                "Warranty" => $"{random.Next(1, 5)} Year(s)",
                "Power Source" => new[] { "Battery", "AC", "USB", "Solar", "Manual" }[random.Next(5)],
                "Voltage" => $"{random.Next(5, 240)}V",
                "Capacity" => $"{random.Next(1, 500)} {new[] { "oz", "lbs", "gal", "L" }[random.Next(4)]}",
                "Brand" => $"Brand-{random.Next(1, 100)}",
                "Model Number" => $"MDL-{random.Next(1000, 9999)}",
                "Country of Origin" => new[] { "USA", "China", "Germany", "Japan", "Mexico", "Canada" }[random.Next(6)],
                "Assembly Required" => random.Next(0, 2) == 0 ? "Yes" : "No",
                "Battery Included" => random.Next(0, 2) == 0 ? "Yes" : "No",
                "Age Range" => $"{random.Next(1, 18)}+ years",
                "Care Instructions" => new[] { "Machine washable", "Wipe clean with damp cloth", "Hand wash only" }[random.Next(3)],
                _ => $"Value-{random.Next(1, 100)}"
            };
        }
    }
}
