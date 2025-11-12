using System.ComponentModel.DataAnnotations.Schema;

namespace SQLDBApp.Models.DataItems
{
    /// <summary>
    /// Represents the category breadcrumb or navigation path for a specific product.
    /// Maps products to their category hierarchy with names and hyperlinks.
    /// </summary>
    public class DataItemCatgPerProd
    {
        /// <summary>
        /// Gets or sets the product identifier this category mapping belongs to.
        /// Database-generated option is disabled for manual ID assignment.
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long Id { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier for this category-per-product mapping.
        /// </summary>
        public long DataItemCatgPerProdId { get; set; }

        /// <summary>
        /// Gets or sets the first level category name in the breadcrumb.
        /// </summary>
        public string Name1 { get; set; }

        /// <summary>
        /// Gets or sets the second level category name in the breadcrumb.
        /// </summary>
        public string Name2 { get; set; }

        /// <summary>
        /// Gets or sets the third level category name in the breadcrumb.
        /// </summary>
        public string Name3 { get; set; }

        /// <summary>
        /// Gets or sets the fourth level category name in the breadcrumb.
        /// </summary>
        public string Name4 { get; set; }

        /// <summary>
        /// Gets or sets the fifth level category name in the breadcrumb.
        /// </summary>
        public string Name5 { get; set; }

        /// <summary>
        /// Gets or sets the sixth level category name in the breadcrumb.
        /// </summary>
        public string Name6 { get; set; }

        /// <summary>
        /// Gets or sets the seventh level category name in the breadcrumb.
        /// </summary>
        public string Name7 { get; set; }

        /// <summary>
        /// Gets or sets the hyperlink URL for the first category level.
        /// </summary>
        public string Href1 { get; set; }

        /// <summary>
        /// Gets or sets the hyperlink URL for the second category level.
        /// </summary>
        public string Href2 { get; set; }

        /// <summary>
        /// Gets or sets the hyperlink URL for the third category level.
        /// </summary>
        public string Href3 { get; set; }

        /// <summary>
        /// Gets or sets the hyperlink URL for the fourth category level.
        /// </summary>
        public string Href4 { get; set; }

        /// <summary>
        /// Gets or sets the hyperlink URL for the fifth category level.
        /// </summary>
        public string Href5 { get; set; }

        /// <summary>
        /// Gets or sets the hyperlink URL for the sixth category level.
        /// </summary>
        public string Href6 { get; set; }

        /// <summary>
        /// Gets or sets the hyperlink URL for the seventh category level.
        /// </summary>
        public string Href7 { get; set; }
    }
}
