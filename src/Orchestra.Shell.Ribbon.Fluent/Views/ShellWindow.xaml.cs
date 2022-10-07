﻿namespace Orchestra.Views
{
    using Catel.IoC;
    using Catel.Windows;
    using Services;

    /// <summary>
    /// Interaction logic for ShellWindow.xaml.
    /// </summary>
    public partial class ShellWindow : IShell
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ShellWindow"/> class.
        /// </summary>
        public ShellWindow()
        {
            var serviceLocator = ServiceLocator.Default;

            InitializeComponent();

            serviceLocator.RegisterInstance(pleaseWaitProgressBar, "busyIndicatorService");

            var statusService = serviceLocator.ResolveRequiredType<IStatusService>();
            statusService.Initialize(statusTextBlock);

            var dependencyResolver = this.GetDependencyResolver();
            var ribbonService = dependencyResolver.ResolveRequired<IRibbonService>();

            var ribbonContent = ribbonService.GetRibbon();
            if (ribbonContent is not null)
            {
                ribbonContentControl.SetCurrentValue(ContentProperty, ribbonContent);

                var ribbon = ribbonContent.FindVisualDescendantByType<Fluent.Ribbon>();
                if (ribbon is not null)
                {
                    serviceLocator.RegisterInstance<Fluent.Ribbon>(ribbon);
                }
            }

            var statusBarContent = ribbonService.GetStatusBar();
            if (statusBarContent is not null)
            {
                customStatusBarItem.SetCurrentValue(ContentProperty, statusBarContent);
            }

            var mainView = ribbonService.GetMainView();
            if (mainView is not null)
            {
                contentControl.Content = mainView;

                ShellDimensionsHelper.ApplyDimensions(this, mainView);
            }
        }
    }
}
