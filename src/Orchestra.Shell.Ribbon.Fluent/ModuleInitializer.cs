﻿using Catel;
using Catel.IoC;
using Orchestra;
using Orchestra.Services;

/// <summary>
/// Used by the ModuleInit. All code inside the Initialize method is ran as soon as the assembly is loaded.
/// </summary>
public static partial class ModuleInitializer
{
    static partial void InitializeSpecific()
    {
        if (EnvironmentHelper.IsProcessHostedByTool)
        {
            return;
        }

        var serviceLocator = ServiceLocator.Default;

        var thirdPartyNoticesService = serviceLocator.ResolveRequiredType<IThirdPartyNoticesService>();
        thirdPartyNoticesService.AddWithTryCatch(() => new ResourceBasedThirdPartyNotice("Fluent.Ribbon", "https://github.com/fluentribbon/Fluent.Ribbon", "Orchestra.Shell.Ribbon.Fluent", "Orchestra", "Resources.ThirdPartyNotices.fluent.ribbon.txt"));
    }
}
