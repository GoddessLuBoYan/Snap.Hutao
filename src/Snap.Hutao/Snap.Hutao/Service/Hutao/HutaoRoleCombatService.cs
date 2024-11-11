﻿// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Microsoft.Extensions.Caching.Memory;
using Snap.Hutao.Web.Hutao.Response;
using Snap.Hutao.Web.Hutao.RoleCombat;
using Snap.Hutao.Web.Response;

namespace Snap.Hutao.Service.Hutao;

[ConstructorGenerated]
[Injection(InjectAs.Scoped, typeof(IHutaoRoleCombatService))]
internal sealed partial class HutaoRoleCombatService : IHutaoRoleCombatService
{
    private readonly TimeSpan cacheExpireTime = TimeSpan.FromHours(1);

    private readonly IObjectCacheRepository objectCacheRepository;
    private readonly IServiceProvider serviceProvider;
    private readonly IMemoryCache memoryCache;

    public async ValueTask<RoleCombatStatisticsItem> GetRoleCombatStatisticsItemAsync(bool last = false)
    {
        using (IServiceScope scope = serviceProvider.CreateScope())
        {
            HutaoRoleCombatClient homaClient = scope.ServiceProvider.GetRequiredService<HutaoRoleCombatClient>();
            return await FromCacheOrWebAsync(nameof(RoleCombatStatisticsItem), last, homaClient.GetStatisticsAsync).ConfigureAwait(false);
        }
    }

    private async ValueTask<T> FromCacheOrWebAsync<T>(string typeName, bool last, Func<bool, CancellationToken, ValueTask<HutaoResponse<T>>> taskFunc)
        where T : class, new()
    {
        string kind = last ? "Last" : "Current";
        string key = $"{nameof(HutaoRoleCombatService)}.Cache.{typeName}.{kind}";
        if (memoryCache.TryGetValue(key, out object? cache))
        {
            T? t = cache as T;
            ArgumentNullException.ThrowIfNull(t);
            return t;
        }

        if (await objectCacheRepository.GetObjectOrDefaultAsync<T>(key).ConfigureAwait(false) is { } value)
        {
            return memoryCache.Set(key, value, cacheExpireTime);
        }

        Response<T> webResponse = await taskFunc(last, default).ConfigureAwait(false);
        T? data = webResponse.Data;

        if (data is not null)
        {
            await objectCacheRepository.AddObjectCacheAsync(key, cacheExpireTime, data).ConfigureAwait(false);
        }

        return memoryCache.Set(key, data ?? new(), cacheExpireTime);
    }
}