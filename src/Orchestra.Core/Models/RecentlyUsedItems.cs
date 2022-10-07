﻿namespace Orchestra
{
    using System.Collections.Generic;
    using Catel.Data;

    public class RecentlyUsedItems : ModelBase
    {
        public RecentlyUsedItems()
        {
            Items = new List<RecentlyUsedItem>();
            PinnedItems = new List<RecentlyUsedItem>();
        }

        public List<RecentlyUsedItem> Items { get; private set; }

        public List<RecentlyUsedItem> PinnedItems { get; private set; }
    }
}
