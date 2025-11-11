using SQLDBApp.Data;
using SQLDBApp.Models.DataItems;
using System;
using System.Linq;

namespace SQLDBApp
{
    public class SeedData
    {
        public static void GenerateSampleProducts(int count = 1000)
        {
            using var context = new DevSqlDBContext();
            
            var random = new Random();
            var categories = new[] { "Electronics", "Home & Garden", "Sports", "Toys", "Books", "Clothing", "Tools", "Kitchen" };
            var brands = new[] { "TechPro", "HomeMax", "SportElite", "KidZone", "ReadWell", "StyleFit", "PowerTool", "CookMaster" };
            var adjectives = new[] { "Premium", "Deluxe", "Professional", "Standard", "Economy", "Ultra", "Advanced", "Basic" };
            
            Console.WriteLine($"Generating {count} sample products...");
            
            for (int i = 1; i <= count; i++)
            {
                var category = categories[random.Next(categories.Length)];
                var brand = brands[random.Next(categories.Length)];
                var adjective = adjectives[random.Next(adjectives.Length)];
                var productNum = 10000 + i;
                
                var product = new DataItemProduct
                {
                    Id = i,
                    Product = productNum,
                    Title = $"{adjective} {brand} {category} Item #{i}",
                    Url = $"https://example.com/products/{productNum}",
                    ImgMain = $"https://example.com/images/{productNum}_main.jpg",
                    ImgsGalleryHtml = $"<div class='gallery'>Gallery for product {productNum}</div>",
                    Item = $"ITEM-{productNum}",
                    Model = $"MODEL-{brand.ToUpper()}-{i:D4}",
                    HighlightsHtml = $"<ul><li>Feature 1</li><li>Feature 2</li><li>Feature 3</li></ul>",
                    DetailsHtml = $"<p>Detailed description for {adjective} {brand} {category}. High quality product with excellent features.</p>",
                    SpecificationsHtml = $"<table><tr><td>Weight</td><td>{random.Next(1, 50)} lbs</td></tr><tr><td>Dimensions</td><td>{random.Next(5, 30)}x{random.Next(5, 30)}x{random.Next(5, 30)} inches</td></tr></table>",
                    PriceBuyDef = Math.Round(random.NextDouble() * 500 + 50, 2),
                    PriceBuyCurrent = Math.Round(random.NextDouble() * 500 + 50, 2),
                    PriceBuyDefAdv = Math.Round(random.NextDouble() * 600 + 60, 2),
                    PriceBuyCurrentAdv = Math.Round(random.NextDouble() * 600 + 60, 2),
                    PriceSellMin = Math.Round(random.NextDouble() * 700 + 70, 2),
                    PriceSellMax = Math.Round(random.NextDouble() * 1000 + 100, 2),
                    IsAllBackOrdered = random.Next(0, 10) > 8,
                    DateLastAvail = DateTime.Now.AddDays(-random.Next(0, 365)).ToString("yyyy-MM-dd"),
                    DateOfferExp = DateTime.Now.AddDays(random.Next(1, 90)).ToString("yyyy-MM-dd"),
                    OfferInfoHtml = random.Next(0, 3) == 0 ? $"<p>Special offer: Save {random.Next(5, 30)}%!</p>" : null,
                    UnitOfMeas = new[] { "Each", "Pair", "Set", "Box", "Pack" }[random.Next(5)],
                    ProductOptionsHtml = $"<select><option>Option A</option><option>Option B</option></select>",
                    HowManyStars = random.Next(1, 6),
                    HowManyRatings = random.Next(0, 500),
                    IsCollectedFull = random.Next(0, 2) == 1,
                    IsActive = random.Next(0, 10) > 1,
                    DateLastUpdated = DateTime.Now.AddDays(-random.Next(0, 30)),
                    ShortProdUrl = $"/p/{productNum}"
                };
                
                context.DataItemProduct.Add(product);
                
                if (i % 100 == 0)
                {
                    Console.WriteLine($"Added {i} products...");
                    context.SaveChanges();
                }
            }
            
            context.SaveChanges();
            Console.WriteLine($"Successfully added {count} sample products to the database!");
        }
        
        public static void Main(string[] args)
        {
            int count = 1000;
            if (args.Length > 0 && int.TryParse(args[0], out int parsedCount))
            {
                count = parsedCount;
            }
            
            GenerateSampleProducts(count);
        }
    }
}
