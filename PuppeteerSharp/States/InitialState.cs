// <copyright file="InitialState.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp.States
{
    using System;
    using System.Threading.Tasks;

    internal class InitialState : State
    {
        public InitialState(StateManager stateManager)
            : base(stateManager)
        {
        }

        public override Task StartAsync(LauncherBase p) => this.StateManager.Starting.EnterFromAsync(p, this, TimeSpan.Zero);

        public override Task ExitAsync(LauncherBase p, TimeSpan timeout)
        {
            this.StateManager.Exited.EnterFromAsync(p, this);
            return Task.CompletedTask;
        }

        public override Task KillAsync(LauncherBase p)
        {
            this.StateManager.Exited.EnterFromAsync(p, this);
            return Task.CompletedTask;
        }

        public override Task WaitForExitAsync(LauncherBase p) => Task.FromException(this.InvalidOperation("wait for exit"));

        private Exception InvalidOperation(string v)
        {
            throw new NotImplementedException();
        }
    }
}
