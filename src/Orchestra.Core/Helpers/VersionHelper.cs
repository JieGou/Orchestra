﻿namespace Orchestra
{
    using System.Reflection;
    using Catel.Reflection;

    public static class VersionHelper
    {
        /// <summary>
        /// Gets the current version of the specified assembly.
        /// </summary>
        /// <param name="assembly">The assembly. If <c>null</c>, <see cref="AssemblyHelper.GetEntryAssembly"/> will be used.</param>
        /// <returns>System.String.</returns>
        public static string GetCurrentVersion(Assembly? assembly = null)
        {
            if (assembly is null)
            {
                assembly = Catel.Reflection.AssemblyHelper.GetEntryAssembly();
            }

            if (assembly is null)
            {
                throw new OrchestraException($"Entry assembly could not be determined automatically");
            }

            var version = assembly.InformationalVersion();
            if (string.IsNullOrWhiteSpace(version))
            {
                version = assembly.Version();
            }

            return version;
        }
    }
}
