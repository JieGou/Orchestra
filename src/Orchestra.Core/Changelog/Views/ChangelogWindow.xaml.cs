﻿namespace Orchestra.Changelog.Views
{
    using Catel;
    using Catel.Windows;

    public partial class ChangelogWindow
    {
        public ChangelogWindow()
            : base(DataWindowMode.Custom)
        {
            AddCustomButton(DataWindowButton.FromAsync(LanguageHelper.GetRequiredString("OK"), OnOkExecuteAsync, OnOkCanExecute));
            AddCustomButton(DataWindowButton.FromAsync(LanguageHelper.GetRequiredString("Orchestra_ChangelogRemindMeLater"), OnCancelExecuteAsync, OnCancelCanExecute));

            InitializeComponent();
        }
    }
}
