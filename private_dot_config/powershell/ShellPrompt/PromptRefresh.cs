using System.Reflection;

namespace ShellPrompt;

internal static class PromptRefresh
{
    public static void Invoke()
    {
        var type = FindPsConsoleReadLine()
                   ?? throw new InvalidOperationException(
                       "Microsoft.PowerShell.PSConsoleReadLine was not found. Load PSReadLine before using ShellPrompt.");

        var method = type.GetMethod("InvokePrompt", BindingFlags.Public | BindingFlags.Static, binder: null, types: Type.EmptyTypes, modifiers: null)
                     ?? throw new InvalidOperationException("PSConsoleReadLine.InvokePrompt() was not found.");

        method.Invoke(null, null);
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
