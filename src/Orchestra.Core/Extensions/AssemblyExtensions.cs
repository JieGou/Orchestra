﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AssemblyExtensions.cs" company="WildGums">
//   Copyright (c) 2008 - 2014 WildGums. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Orchestra
{
    using System.Drawing;
    using System.Reflection;
    using System.Windows.Media.Imaging;
    using Catel;

    public static class AssemblyExtensions
    {
        public static Icon ExtractAssemblyIcon(this Assembly assembly)
        {
            ArgumentNullException.ThrowIfNull(assembly);

            return IconHelper.ExtractIconFromFile(assembly.Location);
        }

        public static BitmapImage ExtractLargestIcon(this Assembly assembly)
        {
            ArgumentNullException.ThrowIfNull(assembly);

            return IconHelper.ExtractLargestIconFromFile(assembly.Location);
        }
    }
}