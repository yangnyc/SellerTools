// <copyright file="TempDirectory.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp.Helpers
{
    using System;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using PathHelper = System.IO.Path;

    /// <summary>
    /// Represents a directory that is deleted on disposal.
    /// </summary>
    internal sealed class TempDirectory : IDisposable
    {
        private int disposed;

        public TempDirectory()
            : this(PathHelper.Combine(PathHelper.GetTempPath(), PathHelper.GetRandomFileName()))
        {
        }

        private TempDirectory(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentException("Path must be specified", nameof(path));
            }

            Directory.CreateDirectory(path);
            this.Path = path;
        }

        ~TempDirectory()
        {
            this.DisposeCore();
        }

        public string Path { get; }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            this.DisposeCore();
        }

        public override string ToString() => this.Path;

        public async Task DeleteAsync()
        {
            const int minDelayInMillis = 200;
            const int maxDelayInMillis = 8000;

            var retryDelay = minDelayInMillis;
            while (Directory.Exists(this.Path))
            {
                try
                {
                    Directory.Delete(this.Path, true);
                    return;
                }
                catch
                {
                    await Task.Delay(retryDelay).ConfigureAwait(false);
                    if (retryDelay < maxDelayInMillis)
                    {
                        retryDelay = Math.Min(2 * retryDelay, maxDelayInMillis);
                    }
                }
            }
        }

        private void DisposeCore()
        {
            if (Interlocked.Exchange(ref this.disposed, 1) != 0)
            {
                return;
            }

            _ = this.DeleteAsync();
        }
    }
}
