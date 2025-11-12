using System.ComponentModel.DataAnnotations.Schema;

namespace SQLDBApp.Models.DataItems
{
    /// <summary>
    /// Represents product image data.
    /// Stores URLs and HTML for product pictures in their display order.
    /// </summary>
    public class DataItemPics
    {
        /// <summary>
        /// Gets or sets the product identifier this picture belongs to.
        /// Database-generated option is disabled for manual ID assignment.
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long Id { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier for this picture entry.
        /// </summary>
        public long DataItemPicsId { get; set; }

        /// <summary>
        /// Gets or sets the display order number for this picture in the product gallery.
        /// </summary>
        public int OrderNum { get; set; }

        /// <summary>
        /// Gets or sets the direct URL to the picture file.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Gets or sets the HTML markup associated with this picture.
        /// </summary>
        public string UrlHtml { get; set; }

        /// <summary>
        /// Gets or sets the date and time when this picture record was last updated.
        /// </summary>
        public System.DateTime DateLastUpdated { get; set; }
    }
}