// <copyright file="PaperFormat.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp.Media
{
    /// <summary>
    /// Paper format.
    /// </summary>
    /// <seealso cref="PdfOptions.Format"/>
    public record PaperFormat
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PaperFormat"/> class.
        /// Page width and height in inches.
        /// </summary>
        /// <param name="width">Page width in inches.</param>
        /// <param name="height">Page height in inches.</param>
        public PaperFormat(decimal width, decimal height)
        {
            this.Width = width;
            this.Height = height;
        }

        /// <summary>
        /// Gets letter: 8.5 inches x 11 inches.
        /// </summary>
        public static PaperFormat Letter => new PaperFormat(8.5m, 11);

        /// <summary>
        /// Gets legal: 8.5 inches by 14 inches.
        /// </summary>
        public static PaperFormat Legal => new PaperFormat(8.5m, 14);

        /// <summary>
        /// Gets tabloid: 11 inches by 17 inches.
        /// </summary>
        public static PaperFormat Tabloid => new PaperFormat(11, 17);

        /// <summary>
        /// Gets ledger: 17 inches by 11 inches.
        /// </summary>
        public static PaperFormat Ledger => new PaperFormat(17, 11);

        /// <summary>
        /// Gets a0: 33.1102 inches by 46.811 inches.
        /// </summary>
        public static PaperFormat A0 => new PaperFormat(33.1102m, 46.811m);

        /// <summary>
        /// Gets a1: 23.3858 inches by 33.1102 inches.
        /// </summary>
        public static PaperFormat A1 => new PaperFormat(23.3858m, 33.1102m);

        /// <summary>
        /// Gets a2: 16.5354 inches by 23.3858 inches.
        /// </summary>
        public static PaperFormat A2 => new PaperFormat(16.5354m, 23.3858m);

        /// <summary>
        /// Gets a3: 11.6929 inches by 16.5354 inches.
        /// </summary>
        public static PaperFormat A3 => new PaperFormat(11.6929m, 16.5354m);

        /// <summary>
        /// Gets a4: 8.2677 inches by 11.6929 inches.
        /// </summary>
        public static PaperFormat A4 => new PaperFormat(8.2677m, 11.6929m);

        /// <summary>
        /// Gets a5: 5.8268 inches by 8.2677 inches.
        /// </summary>
        public static PaperFormat A5 => new PaperFormat(5.8268m, 8.2677m);

        /// <summary>
        /// Gets a6: 4.1339 inches by 5.8268 inches.
        /// </summary>
        public static PaperFormat A6 => new PaperFormat(4.1339m, 5.8268m);

        /// <summary>
        /// Gets or sets page width in inches.
        /// </summary>
        /// <value>The width.</value>
        public decimal Width { get; set; }

        /// <summary>
        /// Gets or sets page height in inches.
        /// </summary>
        /// <value>The Height.</value>
        public decimal Height { get; set; }
    }
}
