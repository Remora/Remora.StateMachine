//
//  SPDX-FileName: State.cs
//  SPDX-FileCopyrightText: Copyright (c) Jarl Gullberg
//  SPDX-License-Identifier: LGPL-3.0-or-later
//

using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Remora.Results;

namespace Remora.StateMachine;

/// <summary>
/// Serves as a virtual no-op base for states.
/// </summary>
[PublicAPI]
public abstract record State : IState
{
    /// <inheritdoc />
    #pragma warning disable SA1206
    public required IStateMachineController Controller { get; init; }
    #pragma warning restore SA1206

    /// <inheritdoc />
    public virtual ValueTask<Result> EnterAsync(CancellationToken ct = default) => new(Result.FromSuccess());

    /// <inheritdoc />
    public virtual ValueTask<Result> ExitAsync(CancellationToken ct = default) => new(Result.FromSuccess());
}
