using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SQLDBApp.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DataItemCatg",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    CatgID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CatgCL = table.Column<int>(type: "int", nullable: true),
                    Url = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PrvId = table.Column<int>(type: "int", nullable: true),
                    FullPathId = table.Column<long>(type: "bigint", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    C1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    C2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    C3 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    C4 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    C5 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    C6 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    C7 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    C8 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    C9 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    C10 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsCollectedHRef = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataItemCatg", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DataItemCatgPerProd",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    DataItemCatgPerProdId = table.Column<long>(type: "bigint", nullable: false),
                    Name1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name3 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name4 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name5 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name6 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name7 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Href1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Href2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Href3 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Href4 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Href5 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Href6 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Href7 = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataItemCatgPerProd", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DataItemPics",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    DataItemPicsId = table.Column<long>(type: "bigint", nullable: false),
                    OrderNum = table.Column<int>(type: "int", nullable: false),
                    Url = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UrlHtml = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateLastUpdated = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataItemPics", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DataItemPrices",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    DataItemPricesId = table.Column<long>(type: "bigint", nullable: false),
                    ZipCodeTo = table.Column<int>(type: "int", nullable: true),
                    CityTo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StateTo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsOutOfStock = table.Column<bool>(type: "bit", nullable: true),
                    DeliveredBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsBackOrdered = table.Column<bool>(type: "bit", nullable: true),
                    BackOrderTill = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PriceBuyDef = table.Column<double>(type: "float", nullable: true),
                    PriceBuyCurrent = table.Column<double>(type: "float", nullable: true),
                    DateOfferExp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PriceSellDef = table.Column<double>(type: "float", nullable: true),
                    PriceSellCurrent = table.Column<double>(type: "float", nullable: true),
                    PriceSellMin = table.Column<double>(type: "float", nullable: true),
                    PriceSellMax = table.Column<double>(type: "float", nullable: true),
                    DateLastInStock = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OfferInfoHtml = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateLastUpdated = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataItemPrices", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DataItemProduct",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    Product = table.Column<long>(type: "bigint", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Url = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ImgMain = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ImgsGalleryHtml = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Item = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Model = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HighlightsHtml = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DetailsHtml = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SpecificationsHtml = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PriceBuyDef = table.Column<double>(type: "float", nullable: true),
                    PriceBuyCurrent = table.Column<double>(type: "float", nullable: true),
                    PriceBuyDefAdv = table.Column<double>(type: "float", nullable: true),
                    PriceBuyCurrentAdv = table.Column<double>(type: "float", nullable: true),
                    PriceSellMin = table.Column<double>(type: "float", nullable: true),
                    PriceSellMax = table.Column<double>(type: "float", nullable: true),
                    IsAllBackOrdered = table.Column<bool>(type: "bit", nullable: true),
                    DateLastAvail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateOfferExp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OfferInfoHtml = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UnitOfMeas = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProductOptionsHtml = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HowManyStars = table.Column<int>(type: "int", nullable: false),
                    HowManyRatings = table.Column<int>(type: "int", nullable: false),
                    IsCollectedFull = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    DateLastUpdated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ShortProdUrl = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataItemProduct", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DataItemSpecs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    DataItemSpecsId = table.Column<long>(type: "bigint", nullable: false),
                    OrderNum = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Info = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Html = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateLastUpdated = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataItemSpecs", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DataItemCatg");

            migrationBuilder.DropTable(
                name: "DataItemCatgPerProd");

            migrationBuilder.DropTable(
                name: "DataItemPics");

            migrationBuilder.DropTable(
                name: "DataItemPrices");

            migrationBuilder.DropTable(
                name: "DataItemProduct");

            migrationBuilder.DropTable(
                name: "DataItemSpecs");
        }
    }
}
