// <copyright file="ExitingState.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp.States
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using PuppeteerSharp.Helpers;

    internal class ExitingState : State
    {
        public ExitingState(StateManager stateManager)
            : base(stateManager)
        {
        }

        public override Task EnterFromAsync(LauncherBase launcher, State fromState, TimeSpan timeout)
            => !this.StateManager.TryEnter(launcher, fromState, this) ? this.StateManager.CurrentState.ExitAsync(launcher, timeout) : this.ExitAsync(launcher, timeout);

        public override async Task ExitAsync(LauncherBase p, TimeSpan timeout)
        {
            var waitForExitTask = this.WaitForExitAsync(p);
            await waitForExitTask.WithTimeout(
                async () =>
                {
                    await this.StateManager.Killing.EnterFromAsync(p, this, timeout).ConfigureAwait(false);
                    await waitForExitTask.ConfigureAwait(false);
                },
                timeout,
                CancellationToken.None).ConfigureAwait(false);
        }

        public override Task KillAsync(LauncherBase p) => this.StateManager.Killing.EnterFromAsync(p, this);
    }
}
