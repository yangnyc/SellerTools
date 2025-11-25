// <copyright file="StartedState.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp.States
{
    using System;
    using System.Threading.Tasks;

    internal class StartedState : State
    {
        public StartedState(StateManager stateManager)
            : base(stateManager)
        {
        }

        public override Task EnterFromAsync(LauncherBase launcher, State fromState, TimeSpan timeout)
        {
            this.StateManager.TryEnter(launcher, fromState, this);
            return Task.CompletedTask;
        }

        public override Task StartAsync(LauncherBase p) => Task.CompletedTask;

        public override Task ExitAsync(LauncherBase p, TimeSpan timeout) => new ExitingState(this.StateManager).EnterFromAsync(p, this, timeout);

        public override Task KillAsync(LauncherBase p) => new KillingState(this.StateManager).EnterFromAsync(p, this);
    }
}
