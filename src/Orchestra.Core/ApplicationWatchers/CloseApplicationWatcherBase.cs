﻿namespace Orchestra
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Threading.Tasks;
    using System.Windows;
    using Catel;
    using Catel.IoC;
    using Catel.Logging;
    using Catel.Services;
    using Catel.Threading;

    public abstract class CloseApplicationWatcherBase : ApplicationWatcherBase
    {
        private static bool CanClose = false;
        private static bool IsHandlingClosing = false;

        private static readonly ILog Log = LogManager.GetCurrentClassLogger();

        private static bool IsClosingConfirmed;
        private static Window SubscribedWindow;
        private static readonly IList<CloseApplicationWatcherBase> Watchers = new List<CloseApplicationWatcherBase>();
        private static readonly IMessageService MessageService = ServiceLocator.Default.ResolveType<IMessageService>();

        protected CloseApplicationWatcherBase()
        {
            Watchers.Add(this);

            EnqueueShellActivatedAction(Subscribe);
        }

#pragma warning disable AvoidAsyncVoid
        private static async void OnWindowClosing(object sender, CancelEventArgs e)
#pragma warning restore AvoidAsyncVoid
        {
            if (CanClose)
            {
                e.Cancel = false;
                return;
            }

            // Step 1: always cancel the closing, we will take over from here
            e.Cancel = true;

            if (IsClosingConfirmed)
            {
                // We need to run the closing methods, so still need to cancel
                return;
            }

            if (IsHandlingClosing)
            {
                // Already handling
                return;
            }

            if (sender is not Window window)
            {
                Log.Debug("Main window is null");
                return;
            }

            // Clone event args
            var clonedEventArgs = new CancelEventArgs();

            try
            {
                IsHandlingClosing = true;

                Log.Debug("Closing main window");

                if (clonedEventArgs.Cancel)
                {
                    Log.Debug("Closing is cancelled");
                    return;
                }

                if (!IsClosingConfirmed)
                {
                    Log.Debug("Closing is not confirmed yet, perform closing operations first");

                    await TaskHelper.Run(() => PerformClosingOperationsAsync(window), true);
                }
            }
            finally
            {
                IsHandlingClosing = false;
            }

            if (clonedEventArgs.Cancel)
            {
                Log.Debug("At least 1 watcher requested canceling the closing of the window");
                return;
            }

            Log.Debug("Perform closed operations after closing confirmed");

            await PerformClosedOperationAsync();

            // Fully done, now really close
            CanClose = true;

            // Close window (again, we will not interfere this time)
            await DispatcherService.InvokeAsync(window.Close).ConfigureAwait(false);
        }

        private static async Task PerformClosingOperationsAsync(Window window)
        {
            try
            {
                Log.Debug("Prepare closing operations");

                IsClosingConfirmed = await ExecuteClosingAsync(PrepareClosingAsync).ConfigureAwait(false);
                if (!IsClosingConfirmed)
                {
                    Log.Debug("Closing is not confirmed, canceling closing operations");
                    return;
                }

                Log.Debug("Performing closing operations");

                IsClosingConfirmed = await ExecuteClosingAsync(ClosingAsync).ConfigureAwait(false);
                if (IsClosingConfirmed)
                {
                    Log.Debug("Closing confirmed, request closing again");

                    await CloseWindowAsync(window).ConfigureAwait(false);
                }
                else
                {
                    Log.Debug("Closing cancelled, request closing again");

                    NotifyClosingCanceled();
                }
            }
            catch (TaskCanceledException)
            {
                // Ignore, don't log
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to perform closing operations");

                await HandleClosingErrorAsync(window, ex);
            }
        }

        private static async Task PerformClosedOperationAsync()
        {
            await ExecuteClosedAsync(ClosedAsync);
        }

        private static async Task<bool> PrepareClosingAsync(CloseApplicationWatcherBase watcher)
        {
            try
            {
                Log.Debug($"Executing PrepareClosingAsync() for '{ObjectToStringHelper.ToFullTypeString(watcher)}'");

                var result = await watcher.PrepareClosingAsync();

                return result;
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Failed to execute PrepareClosingAsync() for '{ObjectToStringHelper.ToFullTypeString(watcher)}'");
                throw;
            }
        }

        private static async Task<bool> ClosingAsync(CloseApplicationWatcherBase watcher)
        {
            try
            {
                Log.Debug($"Executing ClosingAsync() for '{ObjectToStringHelper.ToFullTypeString(watcher)}'");

                var result = await watcher.ClosingAsync();

                return result;
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Failed to execute ClosingAsync() for '{ObjectToStringHelper.ToFullTypeString(watcher)}'");
                throw;
            }
        }

        private static async Task ClosedAsync(CloseApplicationWatcherBase watcher)
        {
            try
            {
                Log.Debug($"Executing ClosedAsync() for '{ObjectToStringHelper.ToFullTypeString(watcher)}'");
                await watcher.ClosedAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Failed to execute ClosedAsync() for '{ObjectToStringHelper.ToFullTypeString(watcher)}'");
                throw;
            }
        }

        private static async Task HandleClosingErrorAsync(Window window, Exception ex)
        {
            var message = string.IsNullOrEmpty(ex.Message) ? ex.ToString() : ex.Message;

            IsClosingConfirmed = false;

            var closingDetails = new ClosingDetails
            {
                Window = window,
                Exception = ex,
                CanBeClosed = true,
                CanKeepOpened = true,
                Message = $"Error. The application will be forced to close:\n{message}"
            };

            foreach (var watcher in Watchers)
            {
                watcher.ClosingFailed(closingDetails);
            }

            if (string.IsNullOrEmpty(closingDetails.Message) &&
                !closingDetails.CanBeClosed &&
                closingDetails.CanKeepOpened)
            {
                return;
            }

            var messageButton = MessageButton.OKCancel;

            if (!closingDetails.CanKeepOpened)
            {
                messageButton = MessageButton.OK;
            }

            if (await MessageService.ShowAsync(closingDetails.Message, "Error", messageButton, MessageImage.Error) == MessageResult.OK)
            {
                await CloseWindowAsync(window).ConfigureAwait(false);
            }
        }

        private static async Task CloseWindowAsync(Window window)
        {
            IsClosingConfirmed = true;
            // await DispatcherService.InvokeAsync(window.Close).ConfigureAwait(false);
        }

        private static void NotifyClosingCanceled()
        {
            foreach (var watcher in Watchers)
            {
                watcher.ClosingCanceled();
            }
        }

        private static async Task<bool> ExecuteClosingAsync(Func<CloseApplicationWatcherBase, Task<bool>> operation)
        {
            Log.Debug($"Execute operation for each of {Watchers.Count} watcher");

            foreach (var watcher in Watchers)
            {
                if (!await operation(watcher).ConfigureAwait(false))
                {
                    NotifyClosingCanceled();

                    return false;
                }
            }

            return true;
        }

        private static async Task ExecuteClosedAsync(Func<CloseApplicationWatcherBase, Task> operation)
        {
            Log.Debug($"Execute operation for each of {Watchers.Count} watcher");

            foreach (var watcher in Watchers)
            {
                try
                {
                    await operation(watcher);
                }
                catch (Exception)
                {
                    // Never break on a single watcher
                }
            }
        }

        protected virtual void ClosingFailed(ClosingDetails appClosingFaultDetails)
        {

        }

        protected virtual void ClosingCanceled()
        {

        }

        protected virtual Task<bool> PrepareClosingAsync()
        {
            return TaskHelper<bool>.FromResult(true);
        }

        protected virtual Task<bool> ClosingAsync()
        {
            return TaskHelper<bool>.FromResult(true);
        }

        protected virtual Task ClosedAsync()
        {
            return TaskHelper.Completed;
        }

        private static void Subscribe(Window window)
        {
            if (SubscribedWindow is not null && !SubscribedWindow.Equals(window))
            {
                SubscribedWindow.Closing -= OnWindowClosing;
                SubscribedWindow = null;
            }

            if (SubscribedWindow is null)
            {
                window.Closing += OnWindowClosing;
                SubscribedWindow = window;
            }
        }
    }
}
