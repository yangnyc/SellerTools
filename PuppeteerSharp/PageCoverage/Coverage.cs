// <copyright file="Coverage.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp.PageCoverage
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    /// <inheritdoc/>
    public class Coverage : ICoverage
    {
        private readonly JSCoverage jsCoverage;
        private readonly CSSCoverage cssCoverage;

        internal Coverage(CDPSession client)
        {
            this.jsCoverage = new JSCoverage(client);
            this.cssCoverage = new CSSCoverage(client);
        }

        /// <inheritdoc/>
        public Task StartJSCoverageAsync(CoverageStartOptions options = null)
            => this.jsCoverage.StartAsync(options ?? new CoverageStartOptions());

        /// <inheritdoc/>
        public Task<JSCoverageEntry[]> StopJSCoverageAsync() => this.jsCoverage.StopAsync();

        /// <inheritdoc/>
        public Task StartCSSCoverageAsync(CoverageStartOptions options = null)
            => this.cssCoverage.StartAsync(options ?? new CoverageStartOptions());

        /// <inheritdoc/>
        public Task<CoverageEntry[]> StopCSSCoverageAsync() => this.cssCoverage.StopAsync();

        internal static CoverageEntryRange[] ConvertToDisjointRanges(IEnumerable<CoverageRange> nestedRanges)
        {
            var points = new List<CoverageEntryPoint>();
            foreach (var range in nestedRanges)
            {
                points.Add(new CoverageEntryPoint
                {
                    Offset = range.StartOffset,
                    Type = 0,
                    Range = range,
                });

                points.Add(new CoverageEntryPoint
                {
                    Offset = range.EndOffset,
                    Type = 1,
                    Range = range,
                });
            }

            points.Sort();

            var hitCountStack = new List<int>();
            var results = new List<CoverageEntryRange>();
            var lastOffset = 0;

            // Run scanning line to intersect all ranges.
            foreach (var point in points)
            {
                if (hitCountStack.Count > 0 && lastOffset < point.Offset && hitCountStack[hitCountStack.Count - 1] > 0)
                {
                    var lastResult = results.Count > 0 ? results[results.Count - 1] : null;
                    if (lastResult != null && lastResult.End == lastOffset)
                    {
                        lastResult.End = point.Offset;
                    }
                    else
                    {
                        results.Add(new CoverageEntryRange
                        {
                            Start = lastOffset,
                            End = point.Offset,
                        });
                    }
                }

                lastOffset = point.Offset;
                if (point.Type == 0)
                {
                    hitCountStack.Add(point.Range.Count);
                }
                else
                {
                    hitCountStack.RemoveAt(hitCountStack.Count - 1);
                }
            }

            // Filter out empty ranges.
            return results.Where(range => range.End - range.Start > 0).ToArray();
        }

        internal void UpdateClient(CDPSession client)
        {
            this.jsCoverage.UpdateClient(client);
            this.cssCoverage.UpdateClient(client);
        }
    }
}
