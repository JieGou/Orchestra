﻿namespace Orchestra.Examples.Ribbon.Views
{
    public partial class RibbonView 
    {
        #region Constructors
        public RibbonView()
        {
            InitializeComponent();

            ribbon.AddAboutButton();
        }
        #endregion

        #region Methods
        protected override void OnViewModelChanged()
        {
            base.OnViewModelChanged();

#pragma warning disable WPF0041
            backstageTabControl.DataContext = ViewModel;
#pragma warning restore WPF0041
        }
        #endregion
    }
}