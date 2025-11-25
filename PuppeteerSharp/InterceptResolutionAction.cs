// <copyright file="InterceptResolutionAction.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp;

using System.Text.Json.Serialization;
using PuppeteerSharp.Helpers.Json;

[JsonConverter(typeof(JsonStringEnumMemberConverter<InterceptResolutionAction>))]
internal enum InterceptResolutionAction
{
    Abort,
    Respond,
    Continue,
    Disabled,
    None,
    AlreadyHandled,
}
