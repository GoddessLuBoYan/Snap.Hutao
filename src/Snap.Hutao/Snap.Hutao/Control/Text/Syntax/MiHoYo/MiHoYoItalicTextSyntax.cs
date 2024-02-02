﻿// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

namespace Snap.Hutao.Control.Text.Syntax.MiHoYo;

internal sealed class MiHoYoItalicTextSyntax : MiHoYoXmlElementSyntax
{
    public MiHoYoItalicTextSyntax(string text, int start, int end)
        : base(MiHoYoSyntaxKind.ItalicText, text, start, end)
    {
    }

    public MiHoYoItalicTextSyntax(string text, TextPosition position)
        : base(MiHoYoSyntaxKind.ItalicText, text, position)
    {
    }

    public override TextPosition ContentPosition { get => new(Position.Start + 3, Position.End - 4); }
}