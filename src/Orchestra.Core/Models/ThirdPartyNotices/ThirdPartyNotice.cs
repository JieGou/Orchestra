﻿namespace Orchestra
{
    public class ThirdPartyNotice
    {
        public ThirdPartyNotice()
        {
            Title = string.Empty;
            Content = string.Empty;
            Url = string.Empty;
        }

        public string Title { get; protected set; }

        public string Content { get; protected set; }

        public string Url { get; protected set; }
    }
}
