﻿// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

namespace Snap.Hutao.Service.GachaLog.QueryProvider;

[ConstructorGenerated]
[Injection(InjectAs.Transient, typeof(IGachaLogQueryProviderFactory))]
internal sealed partial class GachaLogQueryProviderFactory : IGachaLogQueryProviderFactory
{
    [SuppressMessage("", "SH301")]
    private readonly IServiceProvider serviceProvider;

    public IGachaLogQueryProvider Create(RefreshOption option)
    {
        return option switch
        {
            RefreshOption.SToken => serviceProvider.GetRequiredService<GachaLogQuerySTokenProvider>(),
            RefreshOption.WebCache => serviceProvider.GetRequiredService<GachaLogQueryWebCacheProvider>(),
            RefreshOption.ManualInput => serviceProvider.GetRequiredService<GachaLogQueryManualInputProvider>(),
            _ => throw Must.NeverHappen("不支持的刷新选项"),
        };
    }
}