using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Consensus.Models.Exceptions;
using Godot;

namespace Consensus.Utils;

public static class BasicUtil
{
    public static T Must<T>([NotNull] T? v, string caller, [CallerArgumentExpression(nameof(v))] string? paramName = null) where T : class
    {
        if (v == null)
        {
            var finalErr = $"[{caller}] {paramName} is not exists.";
            GD.PrintErr(finalErr);
            throw new NotYetInitializationException();
        }
        return v;
    }
}