﻿// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Microsoft.UI;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Snap.Hutao.Core.LifeCycle;
using Snap.Hutao.Core.Setting;
using Snap.Hutao.Service;
using Snap.Hutao.Win32;
using Snap.Hutao.Win32.Foundation;
using Snap.Hutao.Win32.Graphics.Dwm;
using Snap.Hutao.Win32.UI.WindowsAndMessaging;
using System.IO;
using Windows.Graphics;
using Windows.UI;
using static Snap.Hutao.Win32.DwmApi;
using static Snap.Hutao.Win32.User32;

namespace Snap.Hutao.Core.Windowing;

[SuppressMessage("", "CA1001")]
internal sealed class XamlWindowController
{
    private readonly Window window;
    private readonly XamlWindowOptions options;
    private readonly IServiceProvider serviceProvider;
    private readonly XamlWindowSubclass subclass;
    private readonly XamlWindowNonRudeHWND windowNonRudeHWND;

    public XamlWindowController(Window window, in XamlWindowOptions options, IServiceProvider serviceProvider)
    {
        this.window = window;
        this.options = options;
        this.serviceProvider = serviceProvider;

        serviceProvider.GetRequiredService<ICurrentXamlWindowReference>().Window = window;
        subclass = new(window, options);
        windowNonRudeHWND = new(options.Hwnd);

        InitializeCore();
    }

    private static void TransformToCenterScreen(ref RectInt32 rect)
    {
        DisplayArea displayArea = DisplayArea.GetFromRect(rect, DisplayAreaFallback.Nearest);
        RectInt32 workAreaRect = displayArea.WorkArea;

        rect.Width = Math.Min(workAreaRect.Width, rect.Width);
        rect.Height = Math.Min(workAreaRect.Height, rect.Height);

        rect.X = workAreaRect.X + ((workAreaRect.Width - rect.Width) / 2);
        rect.Y = workAreaRect.Y + ((workAreaRect.Height - rect.Height) / 2);
    }

    private void InitializeCore()
    {
        RuntimeOptions runtimeOptions = serviceProvider.GetRequiredService<RuntimeOptions>();
        AppOptions appOptions = serviceProvider.GetRequiredService<AppOptions>();

        window.AppWindow.Title = SH.FormatAppNameAndVersion(runtimeOptions.Version);
        window.AppWindow.SetIcon(Path.Combine(runtimeOptions.InstalledLocation, "Assets/Logo.ico"));
        ExtendsContentIntoTitleBar();

        RecoverOrInitWindowSize();
        UpdateElementTheme(appOptions.ElementTheme);
        UpdateImmersiveDarkMode(options.TitleBar, default!);

        // appWindow.Show(true);
        // appWindow.Show can't bring window to top.
        window.Activate();
        options.BringToForeground();
        UpdateSystemBackdrop(appOptions.BackdropType);
        appOptions.PropertyChanged += OnOptionsPropertyChanged;

        subclass.Initialize();

        window.Closed += OnWindowClosed;
        options.TitleBar.ActualThemeChanged += UpdateImmersiveDarkMode;
    }

    private void RecoverOrInitWindowSize()
    {
        // Set first launch size
        double scale = options.GetRasterizationScale();
        SizeInt32 scaledSize = options.InitSize.Scale(scale);
        RectInt32 rect = StructMarshal.RectInt32(scaledSize);

        if (!string.IsNullOrEmpty(options.PersistRectKey))
        {
            RectInt32 persistedRect = (CompactRect)LocalSetting.Get(options.PersistRectKey, (CompactRect)rect);
            if (persistedRect.Size() >= options.InitSize.Size())
            {
                rect = persistedRect.Scale(scale);
            }
        }

        TransformToCenterScreen(ref rect);
        window.AppWindow.MoveAndResize(rect);
    }

    private void SaveOrSkipWindowSize()
    {
        if (string.IsNullOrEmpty(options.PersistRectKey))
        {
            return;
        }

        WINDOWPLACEMENT windowPlacement = WINDOWPLACEMENT.Create();
        GetWindowPlacement(options.Hwnd, ref windowPlacement);

        // prevent save value when we are maximized.
        if (!windowPlacement.ShowCmd.HasFlag(SHOW_WINDOW_CMD.SW_SHOWMAXIMIZED))
        {
            double scale = 1.0 / options.GetRasterizationScale();
            LocalSetting.Set(options.PersistRectKey, (CompactRect)window.AppWindow.GetRect().Scale(scale));
        }
    }

    private void OnOptionsPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is not AppOptions options)
        {
            return;
        }

        _ = e.PropertyName switch
        {
            nameof(AppOptions.BackdropType) => UpdateSystemBackdrop(options.BackdropType),
            nameof(AppOptions.ElementTheme) => UpdateElementTheme(options.ElementTheme),
            _ => false,
        };
    }

    private void OnWindowClosed(object sender, WindowEventArgs args)
    {
        if (LocalSetting.Get(SettingKeys.IsNotifyIconEnabled, true) && !serviceProvider.GetRequiredService<App>().IsExiting)
        {
            args.Handled = true;
            window.Hide();
        }
        else
        {
            SaveOrSkipWindowSize();
            subclass?.Dispose();
            windowNonRudeHWND?.Dispose();

            ICurrentXamlWindowReference currentXamlWindowReference = serviceProvider.GetRequiredService<ICurrentXamlWindowReference>();
            if (currentXamlWindowReference.Window == window)
            {
                currentXamlWindowReference.Window = default!;
            }
        }
    }

    private void ExtendsContentIntoTitleBar()
    {
        AppWindowTitleBar appTitleBar = window.AppWindow.TitleBar;
        appTitleBar.IconShowOptions = IconShowOptions.HideIconAndSystemMenu;
        appTitleBar.ExtendsContentIntoTitleBar = true;

        UpdateTitleButtonColor();
        UpdateDragRectangles();
        options.TitleBar.ActualThemeChanged += (_, _) => UpdateTitleButtonColor();
        options.TitleBar.SizeChanged += (_, _) => UpdateDragRectangles();
    }

    private bool UpdateSystemBackdrop(BackdropType backdropType)
    {
        SystemBackdrop? actualBackdop = backdropType switch
        {
            BackdropType.Transparent => new Backdrop.TransparentBackdrop(),
            BackdropType.MicaAlt => new MicaBackdrop() { Kind = MicaKind.BaseAlt },
            BackdropType.Mica => new MicaBackdrop() { Kind = MicaKind.Base },
            BackdropType.Acrylic => new DesktopAcrylicBackdrop(),
            _ => null,
        };

        window.SystemBackdrop = new Backdrop.SystemBackdropDesktopWindowXamlSourceAccess(actualBackdop);

        return true;
    }

    private bool UpdateElementTheme(ElementTheme theme)
    {
        ((FrameworkElement)window.Content).RequestedTheme = theme;

        return true;
    }

    private void UpdateTitleButtonColor()
    {
        AppWindowTitleBar appTitleBar = window.AppWindow.TitleBar;

        appTitleBar.ButtonBackgroundColor = Colors.Transparent;
        appTitleBar.ButtonInactiveBackgroundColor = Colors.Transparent;

        bool isDarkMode = Control.Theme.ThemeHelper.IsDarkMode(options.TitleBar.ActualTheme);

        Color systemBaseLowColor = Control.Theme.SystemColors.BaseLowColor(isDarkMode);
        appTitleBar.ButtonHoverBackgroundColor = systemBaseLowColor;

        Color systemBaseMediumLowColor = Control.Theme.SystemColors.BaseMediumLowColor(isDarkMode);
        appTitleBar.ButtonPressedBackgroundColor = systemBaseMediumLowColor;

        // The Foreground doesn't accept Alpha channel. So we translate it to gray.
        byte light = (byte)((systemBaseMediumLowColor.R + systemBaseMediumLowColor.G + systemBaseMediumLowColor.B) / 3);
        byte result = (byte)((systemBaseMediumLowColor.A / 255.0) * light);
        appTitleBar.ButtonInactiveForegroundColor = Color.FromArgb(0xFF, result, result, result);

        Color systemBaseHighColor = Control.Theme.SystemColors.BaseHighColor(isDarkMode);
        appTitleBar.ButtonForegroundColor = systemBaseHighColor;
        appTitleBar.ButtonHoverForegroundColor = systemBaseHighColor;
        appTitleBar.ButtonPressedForegroundColor = systemBaseHighColor;
    }

    private unsafe void UpdateImmersiveDarkMode(FrameworkElement titleBar, object discard)
    {
        BOOL isDarkMode = Control.Theme.ThemeHelper.IsDarkMode(titleBar.ActualTheme);
        DwmSetWindowAttribute(options.Hwnd, DWMWINDOWATTRIBUTE.DWMWA_USE_IMMERSIVE_DARK_MODE, ref isDarkMode);
    }

    private void UpdateDragRectangles()
    {
        AppWindowTitleBar appTitleBar = window.AppWindow.TitleBar;

        double scale = options.GetRasterizationScale();

        // 48 is the navigation button leftInset
        RectInt32 dragRect = StructMarshal.RectInt32(48, 0, options.TitleBar.ActualSize).Scale(scale);
        appTitleBar.SetDragRectangles([dragRect]);
    }
}