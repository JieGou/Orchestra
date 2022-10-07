﻿namespace Orchestra.Views
{
    using System.Windows;
    using System.Windows.Media;
    using Catel.Windows;

    /// <summary>
    /// Interaction logic for SplashScreen.xaml
    /// </summary>
    public partial class SplashScreen : DataWindow
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SplashScreen" /> class.
        /// </summary>
        public SplashScreen()
            : base(DataWindowMode.Custom)
        {
            InitializeComponent();

            var application = Application.Current;

            Background = (application is not null) ? Orc.Theming.ThemeManager.Current.GetAccentColorBrush() : Brushes.DodgerBlue;
        }
    }
}
