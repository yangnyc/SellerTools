// <copyright file="LauncherBase.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using PuppeteerSharp.Helpers;
    using PuppeteerSharp.States;

    /// <summary>
    /// Represents a Base process and any associated temporary user data directory that have created
    /// by Puppeteer and therefore must be cleaned up when no longer needed.
    /// </summary>
    public abstract class LauncherBase : IDisposable
    {
        private readonly StateManager stateManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="LauncherBase"/> class.
        /// </summary>
        /// <param name="executable">Full path of executable.</param>
        /// <param name="options">Options for launching Base.</param>
        protected LauncherBase(string executable, LaunchOptions options)
        {
            this.stateManager = new StateManager();
            this.stateManager.Starting = new ProcessStartingState(this.stateManager);

            this.Options = options ?? throw new ArgumentNullException(nameof(options));

            this.Process = new Process
            {
                EnableRaisingEvents = true,
            };
            this.Process.StartInfo.UseShellExecute = false;
            this.Process.StartInfo.FileName = executable;
            this.Process.StartInfo.RedirectStandardError = true;

            SetEnvVariables(this.Process.StartInfo.Environment, options.Env, Environment.GetEnvironmentVariables());

            if (options.DumpIO)
            {
                this.Process.ErrorDataReceived += (_, e) => Console.Error.WriteLine(e.Data);
            }
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="LauncherBase"/> class.
        /// </summary>
        ~LauncherBase()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// Gets Base process details.
        /// </summary>
        public Process Process { get; }

        /// <summary>
        /// Gets Base endpoint.
        /// </summary>
        public string EndPoint => this.StartCompletionSource.Task.IsCompleted
            ? this.StartCompletionSource.Task.Result
            : null;

        /// <summary>
        /// Gets a value indicating whether indicates whether Base process is exiting.
        /// </summary>
        public bool IsExiting => this.stateManager.CurrentState.IsExiting;

        /// <summary>
        /// Gets a value indicating whether indicates whether Base process has exited.
        /// </summary>
        public bool HasExited => this.stateManager.CurrentState.IsExited;

        internal TaskCompletionSource<bool> ExitCompletionSource { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        internal TaskCompletionSource<string> StartCompletionSource { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        internal LaunchOptions Options { get; }

        internal TempDirectory TempUserDataDir { get; init; }

        /// <summary>
        /// Gets Base process current state.
        /// </summary>
        internal State CurrentState => this.stateManager.CurrentState;

        /// <summary>
        /// Default build.
        /// </summary>
        /// <returns>A tasks that resolves when the build is obtained.</returns>
        public abstract Task<string> GetDefaultBuildIdAsync();

        /// <inheritdoc />
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Asynchronously starts Base process.
        /// </summary>
        /// <returns>Task which resolves when after start process begins.</returns>
        public Task StartAsync() => this.stateManager.CurrentState.StartAsync(this);

        /// <summary>
        /// Asynchronously waits for graceful Base process exit within a given timeout period.
        /// Kills the Base process if it has not exited within this period.
        /// </summary>
        /// <param name="timeout">The maximum waiting time for a graceful process exit.</param>
        /// <returns>Task which resolves when the process is exited or killed.</returns>
        public Task EnsureExitAsync(TimeSpan? timeout) => timeout.HasValue
            ? this.stateManager.CurrentState.ExitAsync(this, timeout.Value)
            : this.stateManager.CurrentState.KillAsync(this);

        /// <summary>
        /// Asynchronously kills Base process.
        /// </summary>
        /// <returns>Task which resolves when the process is killed.</returns>
        public Task KillAsync() => this.stateManager.CurrentState.KillAsync(this);

        /// <summary>
        /// Waits for Base process exit within a given timeout.
        /// </summary>
        /// <param name="timeout">The maximum wait period.</param>
        /// <returns><c>true</c> if Base process has exited within the given <paramref name="timeout"/>,
        /// or <c>false</c> otherwise.</returns>
        public async Task<bool> WaitForExitAsync(TimeSpan? timeout)
        {
            if (timeout.HasValue)
            {
                var taskCompleted = true;
                await this.ExitCompletionSource.Task.WithTimeout(
                    () =>
                    {
                        taskCompleted = false;
                    },
                    timeout.Value).ConfigureAwait(false);
                return taskCompleted;
            }

            await this.ExitCompletionSource.Task.ConfigureAwait(false);
            return true;
        }

        /// <summary>
        /// Cleans up temporary user data directory.
        /// </summary>
        internal virtual void OnExit()
        {
            if (this.TempUserDataDir is { } tempUserDataDir)
            {
                tempUserDataDir
                    .DeleteAsync()
                    .ContinueWith(
                        t => this.ExitCompletionSource.TrySetResult(true),
                        TaskScheduler.Default);
            }
            else
            {
                this.ExitCompletionSource.TrySetResult(true);
            }
        }

        /// <summary>
        /// Set Env Variables.
        /// </summary>
        /// <param name="environment">The environment.</param>
        /// <param name="customEnv">The customEnv.</param>
        /// <param name="realEnv">The realEnv.</param>
        protected static void SetEnvVariables(IDictionary<string, string> environment, IDictionary<string, string> customEnv, IDictionary realEnv)
        {
            if (environment == null)
            {
                throw new ArgumentNullException(nameof(environment));
            }

            if (realEnv == null)
            {
                throw new ArgumentNullException(nameof(realEnv));
            }

            foreach (DictionaryEntry item in realEnv)
            {
                environment[item.Key.ToString()] = item.Value.ToString();
            }

            if (customEnv != null)
            {
                foreach (var item in customEnv)
                {
                    environment[item.Key] = item.Value;
                }
            }
        }

        /// <summary>
        /// Disposes Base process and any temporary user directory.
        /// </summary>
        /// <param name="disposing">Indicates whether disposal was initiated by <see cref="Dispose()"/> operation.</param>
        protected virtual void Dispose(bool disposing) => this.stateManager.CurrentState.Dispose(this);
    }
}
