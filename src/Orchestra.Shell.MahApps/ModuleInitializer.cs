﻿using System.Windows.Input;
using Catel.IoC;
using Catel.MVVM;
using Catel.Services;
using Orchestra;
using Orchestra.Services;
using InputGesture = Catel.Windows.Input.InputGesture;

/// <summary>
/// Used by the ModuleInit. All code inside the Initialize method is ran as soon as the assembly is loaded.
/// </summary>
public static partial class ModuleInitializer
{
    static partial void InitializeSpecific()
    {
        var serviceLocator = ServiceLocator.Default;

        serviceLocator.RegisterType<IFlyoutService, FlyoutService>();
        serviceLocator.RegisterType<IAboutService, MahAppsAboutService>();
        serviceLocator.RegisterType<IMessageService, MahAppsMessageService>();
        serviceLocator.RegisterType<IUIVisualizerService, MahAppsUIVisualizerService>();

        var commandManager = serviceLocator.ResolveRequiredType<ICommandManager>();

        commandManager.CreateCommand("Help.About", throwExceptionWhenCommandIsAlreadyCreated: false);
        commandManager.CreateCommand("Close", new InputGesture(Key.Escape), throwExceptionWhenCommandIsAlreadyCreated: false);

        var thirdPartyNoticesService = serviceLocator.ResolveRequiredType<IThirdPartyNoticesService>();
        thirdPartyNoticesService.AddWithTryCatch(() => new ResourceBasedThirdPartyNotice("MahApps", "https://mahapps.com/", "Orchestra.Shell.MahApps", "Orchestra", "Resources.ThirdPartyNotices.mahapps.txt"));
    }
}
