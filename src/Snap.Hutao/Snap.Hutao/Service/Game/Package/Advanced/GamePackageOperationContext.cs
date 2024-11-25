﻿// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Snap.Hutao.Core.DependencyInjection.Abstraction;
using Snap.Hutao.Web.Hoyolab.HoyoPlay.Connect.Branch;
using Snap.Hutao.Web.Hoyolab.HoyoPlay.Connect.ChannelSDK;
using System.IO;

namespace Snap.Hutao.Service.Game.Package.Advanced;

internal readonly struct GamePackageOperationContext
{
    public readonly GamePackageOperationKind Kind;
    public readonly IGameAssetOperation Asset;
    public readonly IGameFileSystem GameFileSystem;
    public readonly BranchWrapper LocalBranch;
    public readonly BranchWrapper RemoteBranch;
    public readonly GameChannelSDK? GameChannelSDK;
    public readonly string ExtractOrGameDirectory;
    public readonly string ProxiedChunksDirectory;

    public GamePackageOperationContext(
        IServiceProvider serviceProvider,
        GamePackageOperationKind kind,
        IGameFileSystem gameFileSystem,
        BranchWrapper localBranch,
        BranchWrapper remoteBranch,
        GameChannelSDK? gameChannelSDK,
        string? extractDirectory)
    {
        Kind = kind;
        Asset = serviceProvider.GetRequiredService<IDriverMediaTypeAwareFactory<IGameAssetOperation>>().Create(gameFileSystem.GameDirectory);
        GameFileSystem = gameFileSystem;
        LocalBranch = localBranch;
        RemoteBranch = remoteBranch;
        GameChannelSDK = gameChannelSDK;
        ExtractOrGameDirectory = extractDirectory ?? gameFileSystem.GameDirectory;

        ProxiedChunksDirectory = kind is GamePackageOperationKind.Verify
            ? Path.Combine(gameFileSystem.ChunksDirectory, "repair")
            : gameFileSystem.ChunksDirectory;
    }
}