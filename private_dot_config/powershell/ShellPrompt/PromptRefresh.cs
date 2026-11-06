using System.Reflection;

namespace ShellPrompt;

/// <summary>
/// ワーカースレッドから直接 InvokePrompt せず、フラグ経由でパイプライン側が再描画する。
/// </summary>
internal static class PromptRefresh
{
    private static int _pending;
    private static MethodInfo? _invokePrompt;

    public static void Request() => Interlocked.Exchange(ref _pending, 1);

    public static void Pump()
    {
        if (Interlocked.Exchange(ref _pending, 0) == 0)
        {
            return;
        }

        InvokePrompt();
    }

    private static void InvokePrompt()
    {
        _invokePrompt ??= ResolveInvokePrompt()
                          ?? throw new InvalidOperationException(
                              "PSConsoleReadLine.InvokePrompt(ConsoleKeyInfo?, object) was not found. Load PSReadLine before using ShellPrompt.");
        AsyncSegmentCache.EnterRedraw();
        try
        {
            _invokePrompt.Invoke(null, new object?[] { null, null });
        }
        finally
        {
            AsyncSegmentCache.ExitRedraw();
        }
    }

    /// <summary>
    /// PSReadLine 2.4 系は引数なし InvokePrompt を持たず、
    /// InvokePrompt(ConsoleKeyInfo?, object) の省略可能引数だけがある。
    /// </summary>
    private static MethodInfo? ResolveInvokePrompt()
    {
        var type = FindPsConsoleReadLine();
        return type?.GetMethod(
            "InvokePrompt",
            BindingFlags.Public | BindingFlags.Static,
            binder: null,
            types: new[] { typeof(ConsoleKeyInfo?), typeof(object) },
            modifiers: null);
    }

    private static Type? FindPsConsoleReadLine()
    {
        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            var type = asm.GetType("Microsoft.PowerShell.PSConsoleReadLine");
            if (type is not null)
            {
                return type;
            }
        }

        return Type.GetType("Microsoft.PowerShell.PSConsoleReadLine, Microsoft.PowerShell.PSReadLine", throwOnError: false);
    }
}
