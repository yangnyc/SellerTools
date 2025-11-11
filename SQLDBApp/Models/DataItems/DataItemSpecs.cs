using System.ComponentModel.DataAnnotations.Schema;

namespace SQLDBApp.Models.DataItems
{
    /// <summary>
    /// Represents a product specification or technical detail entry.
    /// Stores specification name, information, and HTML content in display order.
    /// </summary>
    public class DataItemSpecs
    {
        /// <summary>
        /// Gets or sets the product identifier this specification belongs to.
        /// Database-generated option is disabled for manual ID assignment.
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long Id { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier for this specification entry.
        /// </summary>
        public long DataItemSpecsId { get; set; }

        /// <summary>
        /// Gets or sets the display order number for this specification.
        /// </summary>
        public int OrderNum { get; set; }

        /// <summary>
        /// Gets or sets the name or label of the specification (e.g., "Weight", "Dimensions").
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the specification value or information (e.g., "10 lbs", "5x3x2 inches").
        /// </summary>
        public string Info { get; set; }

        /// <summary>
        /// Gets or sets the HTML markup for displaying this specification.
        /// </summary>
        public string Html { get; set; }

        /// <summary>
        /// Gets or sets the date and time when this specification record was last updated.
        /// </summary>
        public System.DateTime DateLastUpdated { get; set; }
    }
}