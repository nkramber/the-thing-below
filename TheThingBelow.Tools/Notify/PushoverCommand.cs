using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;

namespace TheThingBelow.Tools.Notify;

/// <summary>
/// The `pushover` command of PR-108 (D-1207). The `notify` workflow runs it with the repository
/// secrets in its environment, so the night watcher on the Mac holds no copy of a secret.
/// </summary>
public static class PushoverCommand
{
    /// <summary>The name of the command.</summary>
    public const string Name = "pushover";

    /// <summary>The option of the title.</summary>
    public const string TitleOption = "--title";

    /// <summary>The option of the text.</summary>
    public const string MessageOption = "--message";

    /// <summary>The option of the link, which can be absent.</summary>
    public const string LinkOption = "--link";

    /// <summary>Runs the command with the network and the environment of this process.</summary>
    /// <param name="args">The arguments after the command name.</param>
    /// <param name="output">The writer of the line of a sent message.</param>
    /// <param name="errors">The writer of each error.</param>
    /// <returns>The exit code: 0 when Pushover took the message, else <see cref="Program.FaultExitCode"/>.</returns>
    public static int Run(IReadOnlyList<string> args, TextWriter output, TextWriter errors)
    {
        using SocketsHttpHandler handler = new();
        return Run(args, output, errors, Environment.GetEnvironmentVariable, handler);
    }

    /// <summary>Runs the command with the given environment and HTTP handler.</summary>
    /// <param name="args">The arguments after the command name.</param>
    /// <param name="output">The writer of the line of a sent message.</param>
    /// <param name="errors">The writer of each error.</param>
    /// <param name="variable">Gives the value of one variable of the environment, or null.</param>
    /// <param name="handler">The HTTP handler: the network in a command, and a fake in a test.</param>
    /// <returns>The exit code: 0 when Pushover took the message, else <see cref="Program.FaultExitCode"/>.</returns>
    public static int Run(IReadOnlyList<string> args, TextWriter output, TextWriter errors, Func<string, string?> variable, HttpMessageHandler handler)
    {
        ArgumentNullException.ThrowIfNull(args);
        ArgumentNullException.ThrowIfNull(output);
        ArgumentNullException.ThrowIfNull(errors);
        ArgumentNullException.ThrowIfNull(variable);
        ArgumentNullException.ThrowIfNull(handler);

        OptionParser? options = OptionParser.Read(Name, args, [TitleOption, MessageOption, LinkOption], [], errors);
        if (options is null)
        {
            return Program.FaultExitCode;
        }

        string? title = options.Value(TitleOption);
        string? text = options.Value(MessageOption);
        if (title is null || text is null)
        {
            errors.WriteLine($"Error: {Name} needs {TitleOption} and {MessageOption}. {LinkOption} can be absent.");
            return Program.FaultExitCode;
        }

        PushoverMessage message = new(title, text, options.Value(LinkOption) ?? string.Empty);
        return Send(message, output, errors, variable, handler, Name);
    }

    /// <summary>Sends one message, and writes the result or the fault.</summary>
    /// <param name="message">The message.</param>
    /// <param name="output">The writer of the line of a sent message.</param>
    /// <param name="errors">The writer of each error.</param>
    /// <param name="variable">Gives the value of one variable of the environment, or null.</param>
    /// <param name="handler">The HTTP handler.</param>
    /// <param name="command">The name of the command, for the message of a fault.</param>
    /// <returns>The exit code: 0 when Pushover took the message, else <see cref="Program.FaultExitCode"/>.</returns>
    public static int Send(PushoverMessage message, TextWriter output, TextWriter errors, Func<string, string?> variable, HttpMessageHandler handler, string command)
    {
        ArgumentNullException.ThrowIfNull(message);
        ArgumentNullException.ThrowIfNull(output);
        ArgumentNullException.ThrowIfNull(errors);
        ArgumentException.ThrowIfNullOrEmpty(command);

        try
        {
            string request = Pushover.Send(message, Pushover.ReadKeys(variable), handler);
            output.WriteLine($"Pushover took the message '{message.Title}' as the request {request}.");
            return 0;
        }
        catch (InvalidOperationException fault)
        {
            errors.WriteLine($"Error: {command} stopped: {fault.Message}");
            return Program.FaultExitCode;
        }
    }
}
