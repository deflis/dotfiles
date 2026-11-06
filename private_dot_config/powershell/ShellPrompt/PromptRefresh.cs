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
                              "PSConsoleReadLine.InvokePrompt() was not found. Load PSReadLine before using ShellPrompt.");
        _invokePrompt.Invoke(null, null);
    }

    private static MethodInfo? ResolveInvokePrompt()
    {
        var type = FindPsConsoleReadLine();
        return type?.GetMethod(
            "InvokePrompt",
            BindingFlags.Public | BindingFlags.Static,
            binder: null,
            types: Type.EmptyTypes,
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
