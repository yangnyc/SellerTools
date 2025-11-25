// <copyright file="Touchscreen.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp.Input
{
    using System.Threading.Tasks;

    /// <inheritdoc/>
    public abstract class Touchscreen : ITouchscreen
    {
        /// <inheritdoc />
        public async Task TapAsync(decimal x, decimal y)
        {
            await this.TouchStartAsync(x, y).ConfigureAwait(false);
            await this.TouchEndAsync().ConfigureAwait(false);
        }

        /// <inheritdoc />
        public abstract Task TouchStartAsync(decimal x, decimal y);

        /// <inheritdoc />
        public abstract Task TouchMoveAsync(decimal x, decimal y);

        /// <inheritdoc />
        public abstract Task TouchEndAsync();
    }
}
