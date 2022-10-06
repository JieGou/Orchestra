﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StatusService.cs" company="WildGums">
//   Copyright (c) 2008 - 2014 WildGums. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Orchestra.Services
{
    using System;
    using System.Timers;
    using System.Windows.Threading;
    using Catel;
    using Catel.Logging;
    using Orc.Controls.Services;

    public class StatusService : IStatusService
    {
        #region Fields
        private readonly IStatusFilterService _statusFilterService;

        private IStatusRepresenter _statusRepresenter;
        private string _lastStatus;
        #endregion

        #region Constructors
        public StatusService(IStatusFilterService statusFilterService)
        {
            ArgumentNullException.ThrowIfNull(statusFilterService);

            _statusFilterService = statusFilterService;

            var statusLogListener = new Orchestra.Logging.StatusLogListener(this);

            LogManager.AddListener(statusLogListener);
        }
        #endregion

        #region IStatusService Members
        public void UpdateStatus(string status)
        {
            var finalStatus = _statusFilterService.GetStatus(status);
            if (string.IsNullOrWhiteSpace(finalStatus))
            {
                return;
            }

            SetStatus(status);

            _lastStatus = finalStatus;

            var resetTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(8)
            };
            resetTimer.Tick += OnResetTimerTick;
            resetTimer.Tag = finalStatus;
            resetTimer.Start();
        }
        #endregion

        #region Methods
        public void Initialize(IStatusRepresenter statusRepresenter)
        {
            ArgumentNullException.ThrowIfNull(statusRepresenter);

            _statusRepresenter = statusRepresenter;
        }

        private void OnResetTimerTick(object sender, EventArgs e)
        {
            var timer = (DispatcherTimer)sender;

            var finalStatus = (string)timer.Tag;

            timer.Stop();
            timer.Tick -= OnResetTimerTick;

            if (string.Equals(_lastStatus, finalStatus))
            {
                SetStatus("Ready");
            }
        }

        private void SetStatus(string status)
        {
            if (!string.IsNullOrWhiteSpace(status))
            {
                var statusLines = status.Split(new[] { "\n", "\r\n", Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                if (statusLines.Length > 0)
                {
                    status = statusLines[0];
                }
            }

            _statusRepresenter.UpdateStatus(status);
        }
        #endregion
    }
}
