//
//  SPDX-FileName: CoercedException.cs
//  SPDX-FileCopyrightText: Copyright (c) Jarl Gullberg
//  SPDX-License-Identifier: LGPL-3.0-or-later
//

using System;

namespace Remora.StateMachine.Tests;

/// <summary>
/// Represents a coerced test exception; that is, not a "real" error, but one intentionally injected.
/// </summary>
public class CoercedException : Exception
{
}
