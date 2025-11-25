// <copyright file="EmulationManager.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using PuppeteerSharp.Cdp.Messaging;
    using PuppeteerSharp.Helpers;
    using PuppeteerSharp.Media;

    internal class EmulationManager
    {
        private readonly ConcurrentSet<CDPSession> secondaryClients = [];
        private readonly ILogger logger;
        private CDPSession client;
        private bool emulatingMobile;
        private bool hasTouch;
        private ViewPortOptions viewport;

        public EmulationManager(CDPSession client)
        {
            this.client = client;
            this.logger = client.Connection.LoggerFactory.CreateLogger<EmulationManager>();
        }

        public bool JavascriptEnabled { get; private set; } = true;

        internal void UpdateClient(CDPSession client)
        {
            this.client = client;
            this.secondaryClients.Remove(client);
        }

        internal async Task RegisterSpeculativeSessionAsync(CDPSession client)
        {
            this.secondaryClients.Add(client);
            await this.ApplyViewportAsync(client).ConfigureAwait(false);
            client.Disconnected += (sender, e) => this.secondaryClients.Remove(client);
        }

        internal async Task EmulateTimezoneAsync(string timezoneId)
        {
            try
            {
                await this.client.SendAsync(
                    "Emulation.setTimezoneOverride",
                    new EmulateTimezoneRequest { TimezoneId = timezoneId ?? string.Empty, }).ConfigureAwait(false);
            }
            catch (Exception ex) when (ex.Message.Contains("Invalid timezone"))
            {
                throw new PuppeteerException($"Invalid timezone ID: {timezoneId}");
            }
        }

        internal Task EmulateVisionDeficiencyAsync(VisionDeficiency type)
            => this.client.SendAsync(
                "Emulation.setEmulatedVisionDeficiency",
                new EmulationSetEmulatedVisionDeficiencyRequest { Type = type, });

        internal Task EmulateCPUThrottlingAsync(decimal? factor = null)
        {
            if (factor is < 1)
            {
                throw new ArgumentException("Throttling rate should be greater or equal to 1", nameof(factor));
            }

            return this.client.SendAsync(
                "Emulation.setCPUThrottlingRate",
                new EmulationSetCPUThrottlingRateRequest { Rate = factor ?? 1, });
        }

        internal async Task EmulateIdleStateAsync(EmulateIdleOverrides overrides = null)
        {
            if (overrides != null)
            {
                await this.client.SendAsync(
                    "Emulation.setIdleOverride",
                    new EmulationSetIdleOverrideRequest
                    {
                        IsUserActive = overrides.IsUserActive,
                        IsScreenUnlocked = overrides.IsScreenUnlocked,
                    }).ConfigureAwait(false);
            }
            else
            {
                await this.client.SendAsync("Emulation.clearIdleOverride").ConfigureAwait(false);
            }
        }

        internal async Task<bool> EmulateViewportAsync(ViewPortOptions viewport)
        {
            this.viewport = viewport;

            await this.ApplyViewportAsync(this.client).ConfigureAwait(false);

            var mobile = viewport.IsMobile;
            var hasTouch = viewport.HasTouch;
            var reloadNeeded = this.emulatingMobile != mobile || this.hasTouch != hasTouch;
            this.emulatingMobile = mobile;
            this.hasTouch = hasTouch;

            if (!reloadNeeded)
            {
                // If the page will be reloaded, no need to adjust secondary clients.
                await Task.WhenAll(this.secondaryClients.Select(this.ApplyViewportAsync)).ConfigureAwait(false);
            }

            return reloadNeeded;
        }

        internal Task EmulateMediaTypeAsync(MediaType type)
            => this.client.SendAsync(
                "Emulation.setEmulatedMedia",
                new EmulationSetEmulatedMediaTypeRequest { Media = type });

        internal Task EmulateMediaFeaturesAsync(IEnumerable<MediaFeatureValue> features)
            => this.client.SendAsync(
                "Emulation.setEmulatedMedia",
                new EmulationSetEmulatedMediaFeatureRequest { Features = features });

        internal Task SetGeolocationAsync(GeolocationOption options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (options.Longitude < -180 || options.Longitude > 180)
            {
                throw new ArgumentException($"Invalid longitude '{options.Longitude}': precondition - 180 <= LONGITUDE <= 180 failed.");
            }

            if (options.Latitude < -90 || options.Latitude > 90)
            {
                throw new ArgumentException($"Invalid latitude '{options.Latitude}': precondition - 90 <= LATITUDE <= 90 failed.");
            }

            if (options.Accuracy < 0)
            {
                throw new ArgumentException($"Invalid accuracy '{options.Accuracy}': precondition 0 <= ACCURACY failed.");
            }

            return this.client.SendAsync("Emulation.setGeolocationOverride", options);
        }

        internal Task ResetDefaultBackgroundColorAsync()
            => this.client.SendAsync("Emulation.setDefaultBackgroundColorOverride");

        internal Task SetTransparentBackgroundColorAsync()
            => this.client.SendAsync("Emulation.setDefaultBackgroundColorOverride", new EmulationSetDefaultBackgroundColorOverrideRequest
            {
                Color = new EmulationSetDefaultBackgroundColorOverrideColor
                {
                    R = 0,
                    G = 0,
                    B = 0,
                    A = 0,
                },
            });

        internal Task SetJavaScriptEnabledAsync(bool enabled)
        {
            if (enabled == this.JavascriptEnabled)
            {
                return Task.CompletedTask;
            }

            this.JavascriptEnabled = enabled;
            return this.client.SendAsync("Emulation.setScriptExecutionDisabled", new EmulationSetScriptExecutionDisabledRequest
            {
                Value = !enabled,
            });
        }

        private async Task ApplyViewportAsync(CDPSession client)
        {
            var viewport = this.viewport;
            if (viewport == null)
            {
                return;
            }

            var mobile = viewport.IsMobile;
            var width = viewport.Width;
            var height = viewport.Height;
            var deviceScaleFactor = viewport.DeviceScaleFactor;
            var screenOrientation = viewport.IsLandscape
                ? new ScreenOrientation { Angle = 90, Type = ScreenOrientationType.LandscapePrimary, }
                : new ScreenOrientation { Angle = 0, Type = ScreenOrientationType.PortraitPrimary, };
            var hasTouch = viewport.HasTouch;

            await Task.WhenAll(
            [
                client.SendAsync("Emulation.setDeviceMetricsOverride", new EmulationSetDeviceMetricsOverrideRequest
                {
                    Mobile = mobile,
                    Width = width,
                    Height = height,
                    DeviceScaleFactor = deviceScaleFactor,
                    ScreenOrientation = screenOrientation,
                }).ContinueWith(
                    task =>
                    {
                        if (task.IsFaulted)
                        {
                            this.logger.LogError(task.Exception!.Message);
                        }
                    },
                    TaskScheduler.Default),
                client.SendAsync(
                    "Emulation.setTouchEmulationEnabled",
                    new EmulationSetTouchEmulationEnabledRequest { Enabled = hasTouch, }),
            ]).ConfigureAwait(false);
        }
    }
}
