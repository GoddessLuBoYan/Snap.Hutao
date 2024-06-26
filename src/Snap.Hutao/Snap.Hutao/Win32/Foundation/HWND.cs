﻿// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

namespace Snap.Hutao.Win32.Foundation;

internal readonly struct HWND
{
    public readonly nint Value;

    public HWND(nint value) => Value = value;

    public bool IsNull => Value is 0;

    public static unsafe implicit operator HWND(nint value) => *(HWND*)&value;
}