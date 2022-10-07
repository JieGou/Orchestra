﻿namespace Orchestra.Logging
{
    using System;
    using System.Collections.Generic;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Documents;
    using System.Windows.Media;
    using Catel.Logging;

    public class RichTextBoxLogListener : LogListenerBase
    {
        private static readonly Dictionary<LogEvent, Brush> ColorSets = new Dictionary<LogEvent, Brush>();

        private readonly RichTextBox _textBox;

        static RichTextBoxLogListener()
        {
            ColorSets[LogEvent.Debug] = Brushes.Gray;
            ColorSets[LogEvent.Info] = Brushes.Black;
            ColorSets[LogEvent.Warning] = Brushes.DarkOrange;
            ColorSets[LogEvent.Error] = Brushes.Red;
        }

        public RichTextBoxLogListener(RichTextBox richTextBox)
        {
            ArgumentNullException.ThrowIfNull(richTextBox);

            _textBox = richTextBox;

            Clear();
        }

        public void Clear()
        {
            _textBox.Dispatcher.Invoke(() => _textBox.Document.Blocks.Clear());
        }

        protected override void Write(ILog log, string message, LogEvent logEvent, object? extraData, LogData? logData, DateTime time)
        {
            _textBox.Dispatcher.Invoke(() =>
            {
                var finalMessage = string.Format("{0} {1}", time.ToString("HH:mm:ss.fff"), message);

                var paragraph = new Paragraph(new Run(finalMessage));

                FixParagraphSpacing(paragraph);

                paragraph.Foreground = ColorSets[logEvent];
                _textBox.Document.Blocks.Add(paragraph);
                _textBox.ScrollToEnd();
            });
        }

        private void FixParagraphSpacing(Paragraph paragraph)
        {
            paragraph.SetCurrentValue(Block.MarginProperty, new Thickness(0));
        }
    }
}
