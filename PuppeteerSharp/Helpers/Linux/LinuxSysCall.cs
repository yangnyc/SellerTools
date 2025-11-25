// <copyright file="LinuxSysCall.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PuppeteerSharp.Helpers.Linux
{
    using System.Runtime.InteropServices;

    internal static class LinuxSysCall
    {
        internal const FileAccessPermissions ExecutableFilePermissions =
            FileAccessPermissions.UserRead | FileAccessPermissions.UserWrite | FileAccessPermissions.UserExecute |
            FileAccessPermissions.GroupRead |
            FileAccessPermissions.GroupExecute |
            FileAccessPermissions.OtherRead |
            FileAccessPermissions.OtherExecute;

        [DllImport("libc", SetLastError = true, EntryPoint = "chmod")]
        internal static extern int Chmod(string path, FileAccessPermissions mode);
    }
}
