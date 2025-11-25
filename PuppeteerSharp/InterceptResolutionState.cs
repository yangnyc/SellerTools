// <copyright file="InterceptResolutionState.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp;

internal class InterceptResolutionState(InterceptResolutionAction action, int? priority = null)
{
    public InterceptResolutionAction Action { get; set; } = action;

    public int? Priority { get; set; } = priority;
}
