// <copyright file="MouseTransaction.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp.Input
{
    using System;

    internal class MouseTransaction
    {
        public Action<TransactionData> Update { get; set; }

        public Action Commit { get; set; }

        public Action Rollback { get; set; }

        public class TransactionData
        {
            public Point? Position { get; set; }

            public MouseButton? Buttons { get; set; }
        }
    }
}
