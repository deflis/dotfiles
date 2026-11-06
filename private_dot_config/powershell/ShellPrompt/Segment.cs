namespace ShellPrompt;

internal readonly record struct Segment(char Separator, Rgb Background, Rgb Foreground, string Text);
