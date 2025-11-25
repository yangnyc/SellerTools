// <copyright file="EvaluationFailedException.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp
{
    using System;

    /// <summary>
    /// Exception thrown by <see cref="ExecutionContext.ExecuteEvaluationAsync(string, object)"/>.
    /// </summary>
    [Serializable]
    public class EvaluationFailedException : PuppeteerException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EvaluationFailedException"/> class.
        /// </summary>
        public EvaluationFailedException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EvaluationFailedException"/> class.
        /// </summary>
        /// <param name="message">Message.</param>
        public EvaluationFailedException(string message)
            : base(RewriteErrorMeesage(message))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EvaluationFailedException"/> class.
        /// </summary>
        /// <param name="message">Message.</param>
        /// <param name="innerException">Inner exception.</param>
        public EvaluationFailedException(string message, Exception innerException)
            : base(RewriteErrorMeesage(message), innerException)
        {
        }
    }
}
