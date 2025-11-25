// <copyright file="MarginOptions.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp.Media
{
    using System.Text.Json.Serialization;
    using PuppeteerSharp.Helpers.Json;

    /// <summary>
    /// margin options used in <see cref="PdfOptions"/>.
    /// </summary>
    public record MarginOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PuppeteerSharp.Media.MarginOptions"/> class.
        /// </summary>
        public MarginOptions()
        {
        }

        /// <summary>
        /// Gets or sets top margin, accepts values labeled with units.
        /// </summary>
        [JsonConverter(typeof(PrimitiveTypeConverter))]
        public object Top { get; set; }

        /// <summary>
        /// Gets or sets left margin, accepts values labeled with units.
        /// </summary>
        [JsonConverter(typeof(PrimitiveTypeConverter))]
        public object Left { get; set; }

        /// <summary>
        /// Gets or sets bottom margin, accepts values labeled with units.
        /// </summary>
        [JsonConverter(typeof(PrimitiveTypeConverter))]
        public object Bottom { get; set; }

        /// <summary>
        /// Gets or sets right margin, accepts values labeled with units.
        /// </summary>
        [JsonConverter(typeof(PrimitiveTypeConverter))]
        public object Right { get; set; }
    }
}
