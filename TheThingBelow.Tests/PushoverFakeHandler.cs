using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace TheThingBelow.Tests;

/// <summary>
/// A fake HTTP handler for the Pushover tests. It keeps each request body, and it gives one fixed
/// reply or throws one fixed fault. No test reaches the network (D-1201).
/// </summary>
public sealed class PushoverFakeHandler : HttpMessageHandler
{
    private readonly HttpStatusCode status;
    private readonly string reply;
    private readonly Exception? fault;

    private PushoverFakeHandler(HttpStatusCode status, string reply, Exception? fault)
    {
        this.status = status;
        this.reply = reply;
        this.fault = fault;
    }

    /// <summary>Gets the body of each request, in the order of the sends.</summary>
    public List<string> Bodies { get; } = [];

    /// <summary>Gets the address of each request, in the order of the sends.</summary>
    public List<Uri> Addresses { get; } = [];

    /// <summary>Makes a handler that gives one reply.</summary>
    /// <param name="status">The HTTP status of the reply.</param>
    /// <param name="reply">The body of the reply.</param>
    /// <returns>The handler.</returns>
    public static PushoverFakeHandler Replying(HttpStatusCode status, string reply) => new(status, reply, null);

    /// <summary>Makes a handler that takes each message, as Pushover does.</summary>
    /// <returns>The handler.</returns>
    public static PushoverFakeHandler Taking() => Replying(HttpStatusCode.OK, """{"status":1,"request":"5042853c-402d-4a18-abcb-168734a801de"}""");

    /// <summary>Makes a handler that throws one fault on each send.</summary>
    /// <param name="fault">The fault.</param>
    /// <returns>The handler.</returns>
    public static PushoverFakeHandler Throwing(Exception fault) => new(HttpStatusCode.OK, string.Empty, fault);

    /// <inheritdoc/>
    protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        this.Addresses.Add(request.RequestUri!);
        this.Bodies.Add(request.Content!.ReadAsStringAsync(cancellationToken).GetAwaiter().GetResult());
        if (this.fault is not null)
        {
            throw this.fault;
        }

        return new HttpResponseMessage(this.status) { Content = new StringContent(this.reply) };
    }

    /// <inheritdoc/>
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        return Task.FromResult(this.Send(request, cancellationToken));
    }
}
