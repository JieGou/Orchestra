﻿namespace Orchestra.Examples.Ribbon.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Catel.Logging;
    using Catel.MVVM;
    using Catel.Reflection;
    using Catel.Services;
    using Orc.FileSystem;
    using Orchestra.Examples.ViewModels;
    using Orchestra.Services;
    using Orchestra.ViewModels;
    using Orchestra.Windows;

    public class RibbonViewModel : ViewModelBase
    {
        private static readonly ILog Log = LogManager.GetCurrentClassLogger();

        private readonly INavigationService _navigationService;
        private readonly IUIVisualizerService _uiVisualizerService;
        private readonly IRecentlyUsedItemsService _recentlyUsedItemsService;
        private readonly IProcessService _processService;
        private readonly IMessageService _messageService;
        private readonly ISelectDirectoryService _selectDirectoryService;
        private readonly IDirectoryService _directoryService;
        private readonly IManageAppDataService _manageAppDataService;

        public RibbonViewModel(INavigationService navigationService, IUIVisualizerService uiVisualizerService,
            ICommandManager commandManager, IRecentlyUsedItemsService recentlyUsedItemsService, IProcessService processService,
            IMessageService messageService, ISelectDirectoryService selectDirectoryService, IDirectoryService directoryService,
            IManageAppDataService manageAppDataService)
        {
            ArgumentNullException.ThrowIfNull(navigationService);
            ArgumentNullException.ThrowIfNull(uiVisualizerService);
            ArgumentNullException.ThrowIfNull(commandManager);
            ArgumentNullException.ThrowIfNull(recentlyUsedItemsService);
            ArgumentNullException.ThrowIfNull(processService);
            ArgumentNullException.ThrowIfNull(messageService);
            ArgumentNullException.ThrowIfNull(selectDirectoryService);
            ArgumentNullException.ThrowIfNull(directoryService);
            ArgumentNullException.ThrowIfNull(manageAppDataService);

            _navigationService = navigationService;
            _uiVisualizerService = uiVisualizerService;
            _recentlyUsedItemsService = recentlyUsedItemsService;
            _processService = processService;
            _messageService = messageService;
            _selectDirectoryService = selectDirectoryService;
            _directoryService = directoryService;
            _manageAppDataService = manageAppDataService;

            OpenDataDirectory = new TaskCommand(OnOpenDataDirectoryExecuteAsync);
            OpenWindow = new TaskCommand(OnOpenWindowExecuteAsync);
            OpenProject = new TaskCommand(OnOpenProjectExecuteAsync);
            OpenRecentlyUsedItem = new TaskCommand<string>(OnOpenRecentlyUsedItemExecuteAsync);
            OpenInExplorer = new TaskCommand<string>(OnOpenInExplorerExecuteAsync);
            UnpinItem = new Command<string>(OnUnpinItemExecute);
            PinItem = new Command<string>(OnPinItemExecute);
            ShowAllMonitorInfo = new TaskCommand(OnShowAllMonitorInfoExecuteAsync);

            ShowKeyboardMappings = new TaskCommand(OnShowKeyboardMappingsExecuteAsync);

            commandManager.RegisterCommand("File.Open", OpenProject, this);

            var assembly = AssemblyHelper.GetRequiredEntryAssembly();
            Title = assembly.Title() ?? string.Empty;
        }

        public List<RecentlyUsedItem> RecentlyUsedItems { get; private set; }

        public List<RecentlyUsedItem> PinnedItems { get; private set; }

        public TaskCommand OpenDataDirectory { get; private set; }

        private async Task OnOpenDataDirectoryExecuteAsync()
        {
            _manageAppDataService.OpenApplicationDataDirectory(Catel.IO.ApplicationDataTarget.UserRoaming);
        }

        public TaskCommand OpenWindow { get; private set; }

        private async Task OnOpenWindowExecuteAsync()
        {
            await _uiVisualizerService.ShowDialogAsync<ExampleViewModel>();
        }

        /// <summary>
        /// Gets the OpenProject command.
        /// </summary>
        public TaskCommand OpenProject { get; private set; }

        /// <summary>
        /// Method to invoke when the OpenProject command is executed.
        /// </summary>
        private async Task OnOpenProjectExecuteAsync()
        {
            var result = await _selectDirectoryService.DetermineDirectoryAsync(new DetermineDirectoryContext
            {
                Title = "Select a project directory"
            });

            if (result.Result)
            {
                await _messageService.ShowAsync("You have chosen " + result.DirectoryName);
            }
        }

        /// <summary>
        /// Gets the OpenRecentlyUsedItem command.
        /// </summary>
        public TaskCommand<string> OpenRecentlyUsedItem { get; private set; }

        /// <summary>
        /// Method to invoke when the OpenRecentlyUsedItem command is executed.
        /// </summary>
        private Task OnOpenRecentlyUsedItemExecuteAsync(string parameter)
        {
            return _messageService.ShowAsync($"Just opened a recently used item: {parameter}");
        }

        /// <summary>
        /// Gets the OpenInExplorer command.
        /// </summary>
        public TaskCommand<string> OpenInExplorer { get; private set; }

        /// <summary>
        /// Method to invoke when the OpenInExplorer command is executed.
        /// </summary>
        private async Task OnOpenInExplorerExecuteAsync(string parameter)
        {
            if (!_directoryService.Exists(parameter))
            {
                await _messageService.ShowWarningAsync("The directory doesn't seem to exist. Cannot open the project in explorer.");
                return;
            }

            _processService.StartProcess(new ProcessContext
            {
                UseShellExecute = true,
                FileName = parameter
            });
        }

        /// <summary>
        /// Gets the Unpin command.
        /// </summary>
        public Command<string> UnpinItem { get; private set; }

        /// <summary>
        /// Method to invoke when the Unpin command is executed.
        /// </summary>
        private void OnUnpinItemExecute(string parameter)
        {
            _recentlyUsedItemsService.UnpinItem(parameter);
        }

        /// <summary>
        /// Gets the Pin command.
        /// </summary>
        public Command<string> PinItem { get; private set; }

        /// <summary>
        /// Method to invoke when the Pin command is executed.
        /// </summary>
        private void OnPinItemExecute(string parameter)
        {
            _recentlyUsedItemsService.PinItem(parameter);
        }

        /// <summary>
        /// Gets the ShowKeyboardMappings command.
        /// </summary>
        public TaskCommand ShowKeyboardMappings { get; private set; }

        /// <summary>
        /// Method to invoke when the ShowKeyboardMappings command is executed.
        /// </summary>
        private async Task OnShowKeyboardMappingsExecuteAsync()
        {
            await _uiVisualizerService.ShowDialogAsync<KeyboardMappingsCustomizationViewModel>();
        }

        public TaskCommand ShowAllMonitorInfo { get; private set; }

        private async Task OnShowAllMonitorInfoExecuteAsync()
        {
            try
            {
                var monitorInfos = MonitorInfo.GetAllMonitors();

                var monitorInfoMessage = string.Join<string>("\n\n", monitorInfos.Select(x =>
                        $"{x.DeviceNameFull}\n{x.FriendlyName}\nResolution: {x.ScreenWidth}x{x.ScreenHeight}\n" +
                        $"Working Area: {x.WorkingArea}\nDpi Scale: {x.DpiScale?.ToString() ?? "Undefined"}\n" +
                        $"\nEDID: {x.ManufactureCode} {x.ProductCodeId}"
                ));

                await _messageService.ShowAsync(monitorInfoMessage);
            }
            catch(Exception ex)
            {
                Log.Error(ex);
            }
        }

        protected override async Task InitializeAsync()
        {
            await base.InitializeAsync();

            InitializeDemoData();

            _recentlyUsedItemsService.Updated += OnRecentlyUsedItemsServiceUpdated;

            UpdateRecentlyUsedItems();
        }

        protected override Task CloseAsync()
        {
            _recentlyUsedItemsService.Updated -= OnRecentlyUsedItemsServiceUpdated;

            return base.CloseAsync();
        }

        private void OnRecentlyUsedItemsServiceUpdated(object sender, EventArgs e)
        {
            UpdateRecentlyUsedItems();
        }

        private void InitializeDemoData()
        {
            if (_recentlyUsedItemsService.Items.Count() == 0)
            {
                for (var i = 1; i < 4; i++)
                {
                    var item = new RecentlyUsedItem(string.Format("Demo recently used item {0}", i), DateTime.Today.AddDays(i * -1));

                    _recentlyUsedItemsService.AddItem(item);
                }
            }

            if (_recentlyUsedItemsService.PinnedItems.Count() == 0)
            {
                for (var i = 1; i < 4; i++)
                {
                    var item = new RecentlyUsedItem(string.Format("Demo pinned item {0}", i), DateTime.Today.AddDays(i * -1));

                    _recentlyUsedItemsService.AddItem(item);
                    _recentlyUsedItemsService.PinItem(item.Name);
                }
            }
        }

        private void UpdateRecentlyUsedItems()
        {
            RecentlyUsedItems = new List<RecentlyUsedItem>(_recentlyUsedItemsService.Items);
            PinnedItems = new List<RecentlyUsedItem>(_recentlyUsedItemsService.PinnedItems);
        }
    }
}
