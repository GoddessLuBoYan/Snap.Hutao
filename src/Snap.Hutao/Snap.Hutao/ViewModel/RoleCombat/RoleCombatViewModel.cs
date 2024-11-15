﻿// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml.Controls;
using Snap.Hutao.Service.Notification;
using Snap.Hutao.Service.RoleCombat;
using Snap.Hutao.Service.User;
using Snap.Hutao.UI.Xaml.Data;
using Snap.Hutao.ViewModel.Complex;
using Snap.Hutao.ViewModel.User;
using Snap.Hutao.Web.Hutao.Response;
using Snap.Hutao.Web.Hutao.RoleCombat;
using System.Collections.ObjectModel;

namespace Snap.Hutao.ViewModel.RoleCombat;

[ConstructorGenerated]
[Injection(InjectAs.Scoped)]
internal sealed partial class RoleCombatViewModel : Abstraction.ViewModel, IRecipient<UserAndUidChangedMessage>
{
    private readonly IRoleCombatService roleCombatService;
    private readonly IServiceProvider serviceProvider;
    private readonly IInfoBarService infoBarService;
    private readonly ITaskContext taskContext;
    private readonly IUserService userService;

    public AdvancedCollectionView<RoleCombatView>? RoleCombatEntries { get; set => SetProperty(ref field, value); }

    public partial HutaoRoleCombatDatabaseViewModel HutaoRoleCombatDatabaseViewModel { get; }

    public void Receive(UserAndUidChangedMessage message)
    {
        if (message.UserAndUid is { } userAndUid)
        {
            _ = UpdateRoleCombatCollectionAsync(userAndUid);
        }
        else
        {
            RoleCombatEntries?.MoveCurrentTo(default);
        }
    }

    protected override async ValueTask<bool> LoadOverrideAsync()
    {
        if (await roleCombatService.InitializeAsync().ConfigureAwait(false))
        {
            if (await userService.GetCurrentUserAndUidAsync().ConfigureAwait(false) is { } userAndUid)
            {
                await UpdateRoleCombatCollectionAsync(userAndUid).ConfigureAwait(false);
            }
            else
            {
                infoBarService.Warning(SH.MustSelectUserAndUid);
            }
        }

        return true;
    }

    [SuppressMessage("", "SH003")]
    private async Task UpdateRoleCombatCollectionAsync(UserAndUid userAndUid)
    {
        try
        {
            ObservableCollection<RoleCombatView> collection;
            using (await EnterCriticalSectionAsync().ConfigureAwait(false))
            {
                collection = await roleCombatService
                    .GetRoleCombatViewCollectionAsync(userAndUid)
                    .ConfigureAwait(false);
            }

            AdvancedCollectionView<RoleCombatView> roleCombatEntries = collection.ToAdvancedCollectionView();

            await taskContext.SwitchToMainThreadAsync();
            RoleCombatEntries = roleCombatEntries;
            RoleCombatEntries.MoveCurrentTo(RoleCombatEntries.SourceCollection.FirstOrDefault(s => s.Engaged));
        }
        catch (OperationCanceledException)
        {
        }
    }

    [Command("RefreshCommand")]
    private async Task RefreshAsync()
    {
        if (RoleCombatEntries is not null)
        {
            if (await userService.GetCurrentUserAndUidAsync().ConfigureAwait(false) is { } userAndUid)
            {
                try
                {
                    using (await EnterCriticalSectionAsync().ConfigureAwait(false))
                    {
                        await roleCombatService
                            .RefreshRoleCombatAsync(userAndUid)
                            .ConfigureAwait(false);
                    }
                }
                catch (OperationCanceledException)
                {
                }

                await taskContext.SwitchToMainThreadAsync();
                RoleCombatEntries.MoveCurrentTo(RoleCombatEntries.SourceCollection.FirstOrDefault(s => s.Engaged));
            }
        }
    }

    [Command("UploadRoleCombatRecordCommand")]
    private async Task UploadRoleCombatRecordAsync()
    {
        if (await userService.GetCurrentUserAndUidAsync().ConfigureAwait(false) is { } userAndUid)
        {
            using (IServiceScope scope = serviceProvider.CreateScope())
            {
                HutaoRoleCombatClient roleCombatClient = scope.ServiceProvider.GetRequiredService<HutaoRoleCombatClient>();
                if (await roleCombatClient.GetPlayerRecordAsync(userAndUid).ConfigureAwait(false) is { } record)
                {
                    HutaoResponse response = await roleCombatClient.UploadRecordAsync(record).ConfigureAwait(false);

                    infoBarService.PrepareInfoBarAndShow(builder =>
                    {
                        builder
                            .SetSeverity(response is { ReturnCode: 0 } ? InfoBarSeverity.Success : InfoBarSeverity.Warning)
                            .SetMessage(response.GetLocalizationMessageOrMessage());
                    });
                }

                // TODO: Handle no records
            }
        }
        else
        {
            infoBarService.Warning(SH.MustSelectUserAndUid);
        }
    }
}