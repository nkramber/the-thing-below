using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using TheThingBelow.Tools;
using TheThingBelow.Tools.Notify;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>The Pushover sender and the `pushover` command (D-1201, D-1207). No test reaches the network.</summary>
public sealed class PushoverTests
{
    private const string Token = "fake-token-of-the-tests-000001";

    private const string User = "fake-user-of-the-tests-0000001";

    private static readonly PushoverKeys Keys = new(User, Token);

    private static readonly PushoverMessage Message = new("The Thing Below: the night failed", "Failed legs: windows-2025 (failure).", "https://github.com/o/r/actions/runs/1");

    [Fact]
    public void ATakenMessageGivesTheRequestIdAndPostsEachField()
    {
        PushoverFakeHandler handler = PushoverFakeHandler.Taking();

        string request = Pushover.Send(Message, Keys, handler);

        Assert.Equal("5042853c-402d-4a18-abcb-168734a801de", request);
        Assert.Equal(Pushover.MessagesUri, Assert.Single(handler.Addresses));
        string body = Assert.Single(handler.Bodies);
        Assert.Contains($"token={Token}", body, StringComparison.Ordinal);
        Assert.Contains($"user={User}", body, StringComparison.Ordinal);
        Assert.Contains("title=The+Thing+Below", body, StringComparison.Ordinal);
        Assert.Contains("url=https", body, StringComparison.Ordinal);
    }

    [Fact]
    public void AMessageWithNoLinkPostsNoUrlField()
    {
        PushoverFakeHandler handler = PushoverFakeHandler.Taking();

        Pushover.Send(Message with { Link = string.Empty }, Keys, handler);

        Assert.DoesNotContain("url=", Assert.Single(handler.Bodies), StringComparison.Ordinal);
    }

    [Fact]
    public void AStatusOtherThanOneFailsWithTheErrorsAndNoSecret()
    {
        // Pushover gives HTTP 400 and status 0 for a bad token. A reply that holds a key value
        // still gives a message with no secret (D-1201).
        string reply = $$"""{"token":"invalid","errors":["application token is invalid, see {{Token}}"],"status":0,"request":"r"}""";
        PushoverFakeHandler handler = PushoverFakeHandler.Replying(HttpStatusCode.BadRequest, reply);

        InvalidOperationException fault = Assert.Throws<InvalidOperationException>(() => Pushover.Send(Message, Keys, handler));

        Assert.Contains("HTTP status 400", fault.Message, StringComparison.Ordinal);
        Assert.Contains("application token is invalid", fault.Message, StringComparison.Ordinal);
        Assert.Contains(Pushover.Hidden, fault.Message, StringComparison.Ordinal);
        Assert.DoesNotContain(Token, fault.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AStatusOfOneWithAnHttpFaultStillFails()
    {
        PushoverFakeHandler handler = PushoverFakeHandler.Replying(HttpStatusCode.InternalServerError, """{"status":1,"request":"r"}""");

        InvalidOperationException fault = Assert.Throws<InvalidOperationException>(() => Pushover.Send(Message, Keys, handler));

        Assert.Contains("HTTP status 500", fault.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AReplyWithNoJsonFailsWithTheStatus()
    {
        PushoverFakeHandler handler = PushoverFakeHandler.Replying(HttpStatusCode.BadGateway, "<html>bad gateway</html>");

        InvalidOperationException fault = Assert.Throws<InvalidOperationException>(() => Pushover.Send(Message, Keys, handler));

        Assert.Contains("holds no JSON", fault.Message, StringComparison.Ordinal);
        Assert.Contains("HTTP status 502", fault.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ATakenReplyWithNoRequestIdFails()
    {
        PushoverFakeHandler handler = PushoverFakeHandler.Replying(HttpStatusCode.OK, """{"status":1}""");

        InvalidOperationException fault = Assert.Throws<InvalidOperationException>(() => Pushover.Send(Message, Keys, handler));

        Assert.Contains("no request id", fault.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AFailedSendNamesTheTitleAndHidesTheKeys()
    {
        PushoverFakeHandler handler = PushoverFakeHandler.Throwing(new HttpRequestException($"no route for user {User}"));

        InvalidOperationException fault = Assert.Throws<InvalidOperationException>(() => Pushover.Send(Message, Keys, handler));

        Assert.Contains("failed", fault.Message, StringComparison.Ordinal);
        Assert.Contains(Message.Title, fault.Message, StringComparison.Ordinal);
        Assert.DoesNotContain(User, fault.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ATextPastTheLimitFailsBeforeTheSend()
    {
        PushoverFakeHandler handler = PushoverFakeHandler.Taking();

        InvalidOperationException fault = Assert.Throws<InvalidOperationException>(
            () => Pushover.Send(Message with { Text = new string('x', Pushover.TextLimit + 1) }, Keys, handler));

        Assert.Contains("1025 characters", fault.Message, StringComparison.Ordinal);
        Assert.Empty(handler.Bodies);
    }

    [Fact]
    public void AnEmptyTitleFails()
    {
        InvalidOperationException fault = Assert.Throws<InvalidOperationException>(() => Pushover.Check(Message with { Title = string.Empty }));

        Assert.Contains("title", fault.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AnAbsentKeyNamesTheVariableAlone()
    {
        Dictionary<string, string?> environment = new() { [Pushover.ApiTokenVariable] = Token };

        InvalidOperationException fault = Assert.Throws<InvalidOperationException>(() => Pushover.ReadKeys(name => environment.GetValueOrDefault(name)));

        Assert.Contains(Pushover.UserKeyVariable, fault.Message, StringComparison.Ordinal);
        Assert.DoesNotContain(Token, fault.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void TheTextOfTheKeysHoldsNoValue()
    {
        Assert.DoesNotContain(Token, Keys.ToString(), StringComparison.Ordinal);
        Assert.DoesNotContain(User, Keys.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public void TheCommandSendsTheMessageOfItsOptions()
    {
        PushoverFakeHandler handler = PushoverFakeHandler.Taking();
        using StringWriter output = new();
        using StringWriter errors = new();

        int exitCode = PushoverCommand.Run(
            ["--title", "PR ready", "--message", "PR #90 is ready to merge.", "--link", "https://github.com/o/r/pull/90"],
            output,
            errors,
            Environment,
            handler);

        Assert.Equal(0, exitCode);
        Assert.Contains("Pushover took the message 'PR ready'", output.ToString(), StringComparison.Ordinal);
        Assert.Contains("url=https", Assert.Single(handler.Bodies), StringComparison.Ordinal);
    }

    [Fact]
    public void TheCommandWithNoMessageFails()
    {
        using StringWriter output = new();
        using StringWriter errors = new();

        int exitCode = PushoverCommand.Run(["--title", "PR ready"], output, errors, Environment, PushoverFakeHandler.Taking());

        Assert.Equal(Program.FaultExitCode, exitCode);
        Assert.Contains("needs --title and --message", errors.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public void TheCommandWritesAFaultOfPushoverAndHidesTheKeys()
    {
        PushoverFakeHandler handler = PushoverFakeHandler.Replying(HttpStatusCode.BadRequest, $$"""{"errors":["user {{User}} is invalid"],"status":0}""");
        using StringWriter output = new();
        using StringWriter errors = new();

        int exitCode = PushoverCommand.Run(["--title", "t", "--message", "m"], output, errors, Environment, handler);

        Assert.Equal(Program.FaultExitCode, exitCode);
        Assert.Contains("Error: pushover stopped:", errors.ToString(), StringComparison.Ordinal);
        Assert.DoesNotContain(User, errors.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public void TheProgramKnowsTheCommand()
    {
        using StringWriter output = new();
        using StringWriter errors = new();

        int exitCode = Program.Run([PushoverCommand.Name], output, errors);

        Assert.Equal(Program.FaultExitCode, exitCode);
        Assert.Contains("needs --title and --message", errors.ToString(), StringComparison.Ordinal);
    }

    private static string? Environment(string name) => name switch
    {
        Pushover.UserKeyVariable => User,
        Pushover.ApiTokenVariable => Token,
        _ => null,
    };
}
