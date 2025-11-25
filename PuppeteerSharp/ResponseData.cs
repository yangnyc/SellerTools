// <copyright file="ResponseData.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp
{
    using System.Collections.Generic;
    using System.Net;
    using System.Text;

    /// <summary>
    /// Response that will fulfill a request.
    /// </summary>
    public class ResponseData
    {
        /// <summary>
        /// Gets or sets response body (text content).
        /// </summary>
        /// <value>Body as text.</value>
        public string Body
        {
            get => Encoding.UTF8.GetString(this.BodyData);
            set => this.BodyData = Encoding.UTF8.GetBytes(value);
        }

        /// <summary>
        /// Gets or sets response body (binary content).
        /// </summary>
        /// <value>The body as binary.</value>
        public byte[] BodyData { get; set; }

        /// <summary>
        /// Gets or sets response headers. Header values will be converted to strings. Headers with null values will be
        /// ignored. When multiple headers values are required use an <see cref="System.Collections.ICollection"/>
        /// to add multiple values for the Header key.
        /// </summary>
        /// <value>Headers.</value>
        public Dictionary<string, object> Headers { get; set; }

        /// <summary>
        /// Gets or sets if set, equals to setting <see href="https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Content-Type"/> response header.
        /// </summary>
        /// <value>The Content-Type.</value>
        public string ContentType { get; set; }

        /// <summary>
        /// Gets or sets response status code.
        /// </summary>
        /// <value>Status Code.</value>
        public HttpStatusCode? Status { get; set; }
    }
}
