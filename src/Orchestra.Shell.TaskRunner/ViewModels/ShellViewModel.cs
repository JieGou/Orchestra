﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ShellViewModel.cs" company="WildGums">
//   Copyright (c) 2008 - 2014 WildGums. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Orchestra.ViewModels
{
    using System;
    using System.Threading.Tasks;
    using Catel;
    using Catel.Logging;
    using Catel.MVVM;
    using Catel.Threading;
    using Services;

    public class ShellViewModel : ViewModelBase
    {
        private static readonly ILog Log = LogManager.GetCurrentClassLogger();

        private readonly ITaskRunnerService _taskRunnerService;

        public ShellViewModel(ITaskRunnerService taskRunnerService, ICommandManager commandManager, IShellConfigurationService shellConfigurationService)
        {
            ArgumentNullException.ThrowIfNull(taskRunnerService);
            ArgumentNullException.ThrowIfNull(commandManager);
            ArgumentNullException.ThrowIfNull(shellConfigurationService);

            _taskRunnerService = taskRunnerService;

            Run = new TaskCommand(OnRunExecuteAsync, OnRunCanExecute);

            commandManager.RegisterCommand("Runner.Run", Run, this);

            DeferValidationUntilFirstSaveCall = shellConfigurationService.DeferValidationUntilFirstSaveCall;

            Title = taskRunnerService.Title;
            taskRunnerService.TitleChanged += (sender, args) => Title = taskRunnerService.Title;
        }

        #region Properties
        public bool IsRunning { get; private set; }

        public object ConfigurationContext { get; set; }
        #endregion

        #region Commands
        /// <summary>
        /// Gets the Run command.
        /// </summary>
        public TaskCommand Run { get; private set; }

        private bool OnRunCanExecute()
        {
            if (IsRunning)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Method to invoke when the Run command is executed.
        /// </summary>
        private async Task OnRunExecuteAsync()
        {
            Validate(true);

            if (HasErrors)
            {
                Log.Warning("There are errors that need to be fixed, please do that before running.");

                var validationSummary = this.GetValidationSummary(true);
                foreach (var error in validationSummary.FieldErrors)
                {
                    Log.Warning("  * {0}", error.Message);
                }

                foreach (var error in validationSummary.BusinessRuleErrors)
                {
                    Log.Warning("  * {0}", error.Message);
                }

                return;
            }

            IsRunning = true;

            try
            {
                await Task.Run(() => _taskRunnerService.RunAsync(ConfigurationContext), true);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to execute the command");
            }
            finally
            {
                IsRunning = false;
            }
        }
        #endregion
    }
}
