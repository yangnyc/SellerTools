// <copyright file="KillingState.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp.States
{
    using System;
    using System.Threading.Tasks;

    internal class KillingState : State
    {
        public KillingState(StateManager stateManager)
            : base(stateManager)
        {
        }

        public override async Task EnterFromAsync(LauncherBase launcher, State fromState, TimeSpan timeout)
        {
            if (!this.StateManager.TryEnter(launcher, fromState, this))
            {
                // Delegate KillAsync to current state, because it has already changed since
                // transition to this state was initiated.
                await this.StateManager.CurrentState.KillAsync(launcher).ConfigureAwait(false);
            }

            try
            {
                if (!launcher.Process.HasExited)
                {
                    launcher.Process.Kill();
                }
            }
            catch (InvalidOperationException)
            {
                // Ignore
                return;
            }

            await this.WaitForExitAsync(launcher).ConfigureAwait(false);
        }

        public override Task ExitAsync(LauncherBase p, TimeSpan timeout) => this.WaitForExitAsync(p);

        public override Task KillAsync(LauncherBase p) => this.WaitForExitAsync(p);
    }
}
