using Devweb.Poco;
using System;

namespace Devweb.Crawler
{
    public class CrawlArgs : EventArgs
    {
        public CrawlContext CrawlContext { get; set; }

        public CrawlArgs(CrawlContext crawlContext)
        {
            CrawlContext = crawlContext ?? throw new ArgumentNullException(nameof(crawlContext));
        }
    }
}
