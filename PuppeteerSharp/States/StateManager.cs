// <copyright file="StateManager.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp.States
{
    using System.Threading;

    internal class StateManager
    {
        private State currentState;

        public StateManager()
        {
            this.Initial = new InitialState(this);
            this.Starting = new ProcessStartingState(this);
            this.Started = new StartedState(this);
            this.Exiting = new ExitingState(this);
            this.Killing = new KillingState(this);
            this.Exited = new ExitedState(this);
            this.Disposed = new DisposedState(this);
            this.CurrentState = this.Initial;
        }

        public State CurrentState
        {
            get => this.currentState;
            set => this.currentState = value;
        }

        internal State Initial { get; set; }

        internal State Starting { get; set; }

        internal StartedState Started { get; set; }

        internal State Exiting { get; set; }

        internal State Killing { get; set; }

        internal ExitedState Exited { get; set; }

        internal State Disposed { get; set; }

        public bool TryEnter(LauncherBase p, State fromState, State toState)
        {
            if (Interlocked.CompareExchange(ref this.currentState, toState, fromState) == fromState)
            {
                fromState.Leave(p);
                return true;
            }

            return false;
        }
    }
}
