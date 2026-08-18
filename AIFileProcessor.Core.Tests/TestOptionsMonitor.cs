//-----------------------------------------------------------------------
// <copyright file="TestOptionsMonitor.cs" company="Midnight Mission Systems">
//     Copyright (c) Midnight Mission Systems. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace AIFileProcessor.Core.Tests;

using System;
using Microsoft.Extensions.Options;

/// <summary>
/// Minimal test-only implementation of <see cref="IOptionsMonitor{T}"/> that wraps
/// a single value and optionally supports updates via <see cref="Update"/>.
/// </summary>
/// <typeparam name="T">The options type.</typeparam>
internal sealed class TestOptionsMonitor<T> : IOptionsMonitor<T>
    where T : class, new()
{
    private T _current;

    internal TestOptionsMonitor(T initial)
    {
        _current = initial ?? new T();
    }

    public T CurrentValue => _current;

    public T Get(string? name) => _current;

    public IDisposable? OnChange(Action<T, string?> listener) => null;

    /// <summary>
    /// Updates the current value, simulating a configuration change.
    /// </summary>
    internal void Update(T newValue)
    {
        _current = newValue ?? new T();
    }
}