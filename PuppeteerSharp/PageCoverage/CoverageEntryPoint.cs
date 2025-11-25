// <copyright file="CoverageEntryPoint.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp.PageCoverage
{
    using System;

    internal class CoverageEntryPoint : IComparable<CoverageEntryPoint>
    {
        public int Offset { get; internal set; }

        public int Type { get; internal set; }

        public CoverageRange Range { get; internal set; }

        public int CompareTo(CoverageEntryPoint other)
        {
            // Sort with increasing offsets.
            if (this.Offset != other.Offset)
            {
                return this.Offset - other.Offset;
            }

            // All "end" points should go before "start" points.
            if (this.Type != other.Type)
            {
                return this.Type - other.Type;
            }

            var aLength = this.Range.EndOffset - this.Range.StartOffset;
            var bLength = other.Range.EndOffset - other.Range.StartOffset;

            // For two "start" points, the one with longer range goes first.
            if (this.Type == 0)
            {
                return bLength - aLength;
            }

            // For two "end" points, the one with shorter range goes first.
            return aLength - bLength;
        }
    }
}
