﻿namespace Orchestra.Examples.MahApps.ViewModels
{
    using System;
    using Catel.Fody;
    using Catel.MVVM;
    using Models;

    /// <summary>
    /// UserControl view model.
    /// </summary>
    public class PersonViewModel : ViewModelBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PersonViewModel"/> class.
        /// </summary>
        public PersonViewModel(Person person)
        {
            ArgumentNullException.ThrowIfNull(person);

            Person = person;
        }

        /// <summary>
        /// Gets the title of the view model.
        /// </summary>
        /// <value>The title.</value>
        public override string Title
        {
            get
            {
                var title = Person.ToString();
                if (string.IsNullOrWhiteSpace(title))
                {
                    title = "New person";
                }

                return title;
            }
        }

        [Model]
        [Expose("FirstName")]
        [Expose("MiddleName")]
        [Expose("LastName")]
        public Person Person { get; private set; }
    }
}
