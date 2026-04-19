using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Consensus.Models.Exceptions;
using Godot;

namespace Consensus.Utils;

public static class BasicUtil
{
    public static T Must<T>([NotNull] T? v, string caller, [CallerArgumentExpression(nameof(v))] string? paramName = null) where T : class
    {
        if (v == null) throw CreateEx(caller, paramName);
        return v;
    }

    public static T Must<T>(T? v, string caller, [CallerArgumentExpression(nameof(v))] string? paramName = null) where T : struct
    {
        if (!v.HasValue) throw CreateEx(caller, paramName);
        return v.Value;
    }

    private static NotYetInitializationException CreateEx(string caller, string? paramName)
    {
        var finalErr = $"[{caller}] {paramName} is not exists.";
        GD.PrintErr(finalErr);
        return new NotYetInitializationException(finalErr);
    }
}