﻿// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using System.Runtime.CompilerServices;

namespace Snap.Hutao.Core.Validation;

/// <summary>
/// 封装验证方法,简化微软验证
/// </summary>
public static class Must
{
    /// <summary>
    /// Throws an <see cref="ArgumentNullException"/> if the specified parameter's value is null.
    /// </summary>
    /// <typeparam name="T">The type of the parameter.</typeparam>
    /// <param name="value">The value of the argument.</param>
    /// <param name="parameterName">The name of the parameter to include in any thrown exception.</param>
    /// <returns>The value of the parameter.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <c>null</c>.</exception>
    public static T NotNull<T>([NotNull] T value, [CallerArgumentExpression("value")] string? parameterName = null)
        where T : class // ensures value-types aren't passed to a null checking method
    {
        return value ?? throw new ArgumentNullException(parameterName);
    }

    /// <summary>
    /// Unconditionally throws an <see cref="NotSupportedException"/>.
    /// </summary>
    /// <returns>Nothing. This method always throws.</returns>
    [DoesNotReturn]
    public static System.Exception NeverHappen()
    {
        throw new NotSupportedException("该行为不应发生，请联系开发者进一步确认");
    }

    /// <summary>
    /// Unconditionally throws an <see cref="NotSupportedException"/>.
    /// </summary>
    /// <typeparam name="T">The type that the method should be typed to return (although it never returns anything).</typeparam>
    /// <returns>Nothing. This method always throws.</returns>
    [DoesNotReturn]
    [return: MaybeNull]
    public static T NeverHappen<T>()
    {
        throw new NotSupportedException("该行为不应发生，请联系开发者进一步确认");
    }

    /// <summary>
    /// 尝试抛出任务取消异常
    /// </summary>
    /// <param name="token">取消令牌</param>
    /// <param name="message">取消消息</param>
    /// <exception cref="TaskCanceledException">任务被取消</exception>
    [SuppressMessage("", "CA1068")]
    public static void ThrowOnCanceled(CancellationToken token, string message)
    {
        if (token.IsCancellationRequested)
        {
            throw new TaskCanceledException("Image source has changed.");
        }
    }
}