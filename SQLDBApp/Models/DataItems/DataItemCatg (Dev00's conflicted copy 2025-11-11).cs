using System.ComponentModel.DataAnnotations.Schema;

namespace SQLDBApp.Models.DataItems
{
    /// <summary>
    /// Represents a product category entity in the database.
    /// Stores category hierarchy information, URLs, and collection status.
    /// </summary>
    public class DataItemCatg
    {
        /// <summary>
        /// Gets or sets the unique identifier for the category.
        /// Database-generated option is disabled for manual ID assignment.
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long Id { get; set; }

        /// <summary>
        /// Gets or sets the category identifier string.
        /// </summary>
        public string CatgID { get; set; }

        /// <summary>
        /// Gets or sets the category classification level.
        /// </summary>
        public System.Nullable<int> CatgCL { get; set; } = 0;

        /// <summary>
        /// Gets or sets the URL of the category page.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Gets or sets the parent category identifier.
        /// </summary>
        public System.Nullable<int> PrvId { get; set; } = 0;

        /// <summary>
        /// Gets or sets the full path identifier in the category hierarchy.
        /// </summary>
        public System.Nullable<long> FullPathId { get; set; } = 0;

        /// <summary>
        /// Gets or sets the category name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets category custom field 1.
        /// </summary>
        public string C1 { get; set; }

        /// <summary>
        /// Gets or sets category custom field 2.
        /// </summary>
        public string C2 { get; set; }

        /// <summary>
        /// Gets or sets category custom field 3.
        /// </summary>
        public string C3 { get; set; }

        /// <summary>
        /// Gets or sets category custom field 4.
        /// </summary>
        public string C4 { get; set; }

        /// <summary>
        /// Gets or sets category custom field 5.
        /// </summary>
        public string C5 { get; set; }

        /// <summary>
        /// Gets or sets category custom field 6.
        /// </summary>
        public string C6 { get; set; }

        /// <summary>
        /// Gets or sets category custom field 7.
        /// </summary>
        public string C7 { get; set; }

        /// <summary>
        /// Gets or sets category custom field 8.
        /// </summary>
        public string C8 { get; set; }

        /// <summary>
        /// Gets or sets category custom field 9.
        /// </summary>
        public string C9 { get; set; }

        /// <summary>
        /// Gets or sets category custom field 10.
        /// </summary>
        public string C10 { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether product hyperlinks have been collected from this category.
        /// </summary>
        public bool IsCollectedHRef { get; set; } = false;
    }
}