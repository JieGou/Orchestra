﻿namespace Orchestra.Tests
{
    using System;
    using System.ComponentModel;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using Catel;
    using NUnit.Framework;

    [TestFixture]
    public class CloseApplicationWatcherBaseFacts
    {
        private const int OnWindowClosingWaitingTimeout = 5000;

        [TestCase]
        public async Task Verify_Closing_Allows_Cancel_When_Returning_False_Async()
        {
            var watcher = new TestCloseApplicationWatcher(true);
            await RunOnWindowClosingAndWaitForFinishAsync(watcher, OnWindowClosingWaitingTimeout);

            Assert.IsTrue(watcher.IsClosingRun, "Closing did not run");
            Assert.IsFalse(watcher.IsClosedRun, "Closed did run");
        }

        [TestCase]
        public async Task Verify_Closing_Closed_Operations_Are_Executing_Async()
        {
            var watcher = new TestCloseApplicationWatcher(false);
            await RunOnWindowClosingAndWaitForFinishAsync(watcher, OnWindowClosingWaitingTimeout);

            Assert.IsTrue(watcher.IsClosingRun, "Closing did not run");
            Assert.IsTrue(watcher.IsClosedRun, "Closed did not run");
        }

        private async Task RunOnWindowClosingAndWaitForFinishAsync(TestCloseApplicationWatcher watcher, int timeout)
        {
            Argument.IsNotNull(() => watcher);

            bool isWatcherCompleted = false;
            using (var cts = new CancellationTokenSource(timeout))
            {
                // Use a semaphore to prevent the [TestMethod] from returning prematurely.
                using (var semaphore = await RunStaThreadAsync(() =>
                {
                    var window = new System.Windows.Window();

                    // access handler method
                    var onWindowClosing = typeof(CloseApplicationWatcherBase).GetMethod("OnWindowClosing", BindingFlags.Static | BindingFlags.NonPublic);

                    Assert.IsNotNull(onWindowClosing);

                    var cancelEventArgs = new CancelEventArgs();
                    var cancelEventArgsRetry = new CancelEventArgs();

                    window.Closing += (sender, e) =>
                    {
                        onWindowClosing.Invoke(watcher, new object[] { window, cancelEventArgsRetry });
                    };

                    onWindowClosing.Invoke(watcher, new object[] { window, cancelEventArgs });

                    while (!cts.IsCancellationRequested)
                    {
                        // Note: we do await IsClosedRun, even when we expect it to be false. This will
                        // allow the engine to await whether it is ran or not
                        isWatcherCompleted = watcher.IsClosedRun && watcher.IsClosingRun;
                        if (isWatcherCompleted)
                        {
                            break;
                        }
                    }
                }))
                {
                    await semaphore.WaitAsync();
                }
            }
        }

        private async Task<SemaphoreSlim> RunStaThreadAsync(Action action)
        {
            var semaphore = new SemaphoreSlim(1);
            await semaphore.WaitAsync();

            var thread = new Thread(() =>
            {
                try
                {
                    // Verify new thread able to host UI component
                    Assert.IsTrue(Thread.CurrentThread.GetApartmentState() == ApartmentState.STA);

                    action();

                    semaphore.Release();
                }
                catch (InvalidOperationException)
                {
                    // Handle dispatcher access exception happens in test
                }
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();

            return semaphore;
        }
    }
}