using System.Collections.Generic;
using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Smoke.Mapping.Data;

// Records the SQL that reaches the provider, so a check can tell provider execution from in-memory mapping.
public sealed class SqlCapture : DbCommandInterceptor
{
    private readonly List<string> _commands = new();

    public IReadOnlyList<string> Commands => _commands;

    public void Clear() => _commands.Clear();

    public override InterceptionResult<DbDataReader> ReaderExecuting(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<DbDataReader> result)
    {
        _commands.Add(command.CommandText);
        return base.ReaderExecuting(command, eventData, result);
    }
}
