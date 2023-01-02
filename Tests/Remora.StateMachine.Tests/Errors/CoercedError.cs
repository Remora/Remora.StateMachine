//
//  SPDX-FileName: CoercedError.cs
//  SPDX-FileCopyrightText: Copyright (c) Jarl Gullberg
//  SPDX-License-Identifier: LGPL-3.0-or-later
//

using Remora.Results;

namespace Remora.StateMachine.Tests;

/// <summary>
/// Represents a coerced test error; that is, not a "real" error, but one intentionally injected.
/// </summary>
public record CoercedError() : ResultError("coerced failure");
