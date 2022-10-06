﻿namespace Orchestra
{
    using System.IO;
    using Catel;

    public class FileBasedThirdPartyNotice : ThirdPartyNotice
    {
        public FileBasedThirdPartyNotice(string title, string url, string fileName)
        {
            ArgumentNullException.ThrowIfNull(title);

            Title = title;
            Url = url;
            Content = File.ReadAllText(fileName);
        }
    }
}
