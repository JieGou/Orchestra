﻿namespace Orchestra.Changelog
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Catel;
    using Catel.IoC;
    using Catel.Logging;
    using Catel.Reflection;

    public class ChangelogService : IChangelogService
    {
        private static readonly ILog Log = LogManager.GetCurrentClassLogger();

        private readonly Dictionary<Type, IChangelogProvider> _providers = new Dictionary<Type, IChangelogProvider>();

        private readonly ITypeFactory _typeFactory;
        private readonly IChangelogSnapshotService _changelogSnapshotService;

        public ChangelogService(ITypeFactory typeFactory, IChangelogSnapshotService changelogSnapshotService)
        {
            ArgumentNullException.ThrowIfNull(typeFactory);
            ArgumentNullException.ThrowIfNull(changelogSnapshotService);

            _typeFactory = typeFactory;
            _changelogSnapshotService = changelogSnapshotService;
        }

        public virtual async Task<Changelog> GetChangelogSinceSnapshotAsync()
        {
            var snapshot = await _changelogSnapshotService.DeserializeSnapshotAsync();
            var changelog = await GetChangelogAsync();
            if (snapshot is null)
            {
                await _changelogSnapshotService.SerializeSnapshotAsync(changelog);
                return new Changelog();
            }

            var delta = snapshot.GetDelta(changelog);
            delta.Title = LanguageHelper.GetString("Orchestra_ChangelogWhatsNew");
            return delta;
        }

        public virtual async Task<Changelog> GetChangelogAsync()
        {
            var changelog = new Changelog();

            var providerTypes = TypeCache.GetTypesImplementingInterface(typeof(IChangelogProvider));

            foreach (var providerType in providerTypes)
            {
                if (!providerType.IsClassEx() ||
                    providerType.IsAbstractEx())
                {
                    continue;
                }

                if (!_providers.TryGetValue(providerType, out var provider))
                {
                    provider = _typeFactory.CreateInstance(providerType) as IChangelogProvider;

                    if (provider is null)
                    {
                        continue;
                    }

                    _providers[providerType] = provider;
                }

                try
                {
                    Log.Debug($"Retrieving changelog from '{provider.GetType().FullName}'");

                    var providerItems = await GetChangelogAsync(provider);
                    changelog.Items.AddRange(providerItems);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, $"Failed to get changelog from provider '{provider.GetType().FullName}'");
                }
            }

            return changelog;
        }

        protected virtual Task<IEnumerable<ChangelogItem>> GetChangelogAsync(IChangelogProvider provider)
        {
            return provider.GetChangelogAsync();
        }
    }
}
