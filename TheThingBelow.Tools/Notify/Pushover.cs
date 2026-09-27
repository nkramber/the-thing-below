using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Text.Json;

namespace TheThingBelow.Tools.Notify;

/// <summary>One Pushover message: the title, the text, and the link of the page that it names.</summary>
/// <param name="Title">The title, at most <see cref="Pushover.TitleLimit"/> characters.</param>
/// <param name="Text">The text, at most <see cref="Pushover.TextLimit"/> characters.</param>
/// <param name="Link">The link, at most <see cref="Pushover.LinkLimit"/> characters, or an empty text for no link.</param>
public sealed record PushoverMessage(string Title, string Text, string Link);

/// <summary>The two keys of the Pushover account. No file, log, or message holds their values (D-1201).</summary>
/// <param name="UserKey">The user key of the owner.</param>
/// <param name="ApiToken">The token of the application.</param>
public sealed record PushoverKeys(string UserKey, string ApiToken)
{
    /// <summary>Gives the name of each key and never its value, so a log of the record shows no secret.</summary>
    /// <returns>The text of the record with no key value.</returns>
    public override string ToString() => $"{nameof(PushoverKeys)} {{ {Pushover.UserKeyVariable}, {Pushover.ApiTokenVariable} }}";
}

/// <summary>
/// Sends one message through the message API of Pushover (D-1201, D-1207). Pushover is the one
/// external service of the alerts, and Tools calls it with the HTTP client of .NET (G-13).
/// </summary>
public static class Pushover
{
    /// <summary>The variable that holds the user key, as the repository secret names it.</summary>
    public const string UserKeyVariable = "PUSHOVER_USER_KEY";

    /// <summary>The variable that holds the token of the application, as the repository secret names it.</summary>
    public const string ApiTokenVariable = "PUSHOVER_API_TOKEN";

    /// <summary>The longest title that Pushover takes.</summary>
    public const int TitleLimit = 250;

    /// <summary>The longest text that Pushover takes.</summary>
    public const int TextLimit = 1024;

    /// <summary>The longest link that Pushover takes.</summary>
    public const int LinkLimit = 512;

    /// <summary>The text that takes the place of a key value in each message of a fault.</summary>
    public const string Hidden = "[hidden]";

    /// <summary>The message API of Pushover.</summary>
    public static readonly Uri MessagesUri = new("https://api.pushover.net/1/messages.json");

    /// <summary>The time that one send can take.</summary>
    public static readonly TimeSpan SendLimit = TimeSpan.FromSeconds(30);

    /// <summary>Reads the two keys from the environment.</summary>
    /// <param name="variable">Gives the value of one variable, or null when the variable is absent.</param>
    /// <returns>The keys.</returns>
    /// <exception cref="InvalidOperationException">A variable is absent or empty. The message names the variable and never a value (T-2).</exception>
    public static PushoverKeys ReadKeys(Func<string, string?> variable)
    {
        ArgumentNullException.ThrowIfNull(variable);
        return new PushoverKeys(KeyOf(variable, UserKeyVariable), KeyOf(variable, ApiTokenVariable));
    }

    /// <summary>Checks the length of each part of a message.</summary>
    /// <param name="message">The message.</param>
    /// <exception cref="InvalidOperationException">A part is empty or past its limit, and the message names the part (T-2).</exception>
    public static void Check(PushoverMessage message)
    {
        ArgumentNullException.ThrowIfNull(message);
        CheckPart("title", message.Title, TitleLimit, false);
        CheckPart("text", message.Text, TextLimit, false);
        CheckPart("link", message.Link, LinkLimit, true);
    }

    /// <summary>Sends one message, and fails when Pushover does not take it.</summary>
    /// <param name="message">The message.</param>
    /// <param name="keys">The keys of the account.</param>
    /// <param name="handler">The HTTP handler: the network in a command, and a fake in a test.</param>
    /// <returns>The request id that Pushover gives the message.</returns>
    /// <exception cref="InvalidOperationException">
    /// The message is not valid, the send failed, the HTTP status is not a success, or the reply
    /// holds a status other than 1. The message of the fault holds the HTTP status and the errors
    /// of Pushover, and each key value in it becomes <see cref="Hidden"/> (T-2).
    /// </exception>
    public static string Send(PushoverMessage message, PushoverKeys keys, HttpMessageHandler handler)
    {
        ArgumentNullException.ThrowIfNull(keys);
        ArgumentNullException.ThrowIfNull(handler);
        Check(message);

        List<KeyValuePair<string, string>> fields =
        [
            new("token", keys.ApiToken),
            new("user", keys.UserKey),
            new("title", message.Title),
            new("message", message.Text),
        ];
        if (message.Link.Length > 0)
        {
            fields.Add(new("url", message.Link));
        }

        using HttpClient client = new(handler, disposeHandler: false) { Timeout = SendLimit };
        using HttpRequestMessage request = new(HttpMethod.Post, MessagesUri) { Content = new FormUrlEncodedContent(fields) };
        int status;
        string body;
        try
        {
            using HttpResponseMessage response = client.Send(request);
            status = (int)response.StatusCode;
            body = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
        }
        catch (Exception fault) when (fault is HttpRequestException or OperationCanceledException)
        {
            throw new InvalidOperationException(Hide($"The send of the Pushover message '{message.Title}' to {MessagesUri} failed (D-1201, T-2). {fault.Message}", keys), fault);
        }

        return RequestOf(body, status, message.Title, keys);
    }

    /// <summary>Replaces each key value in a text with <see cref="Hidden"/>.</summary>
    /// <param name="text">The text.</param>
    /// <param name="keys">The keys of the account.</param>
    /// <returns>The text with no key value.</returns>
    public static string Hide(string text, PushoverKeys keys)
    {
        ArgumentNullException.ThrowIfNull(text);
        ArgumentNullException.ThrowIfNull(keys);
        return text.Replace(keys.ApiToken, Hidden, StringComparison.Ordinal).Replace(keys.UserKey, Hidden, StringComparison.Ordinal);
    }

    private static string RequestOf(string body, int status, string title, PushoverKeys keys)
    {
        string where = $"the Pushover reply to the message '{title}', with the HTTP status {status.ToString(CultureInfo.InvariantCulture)},";
        JsonDocument document;
        try
        {
            document = JsonDocument.Parse(body);
        }
        catch (JsonException fault)
        {
            throw new InvalidOperationException(Hide($"{where} holds no JSON (D-1201, T-2). {fault.Message}", keys), fault);
        }

        using (document)
        {
            JsonElement root = document.RootElement;
            bool taken = root.ValueKind == JsonValueKind.Object
                && root.TryGetProperty("status", out JsonElement value)
                && value.ValueKind == JsonValueKind.Number
                && value.TryGetInt32(out int code)
                && code == 1;
            if (!taken || status < 200 || status > 299)
            {
                throw new InvalidOperationException(Hide($"{where} did not take the message (D-1201, T-2). Errors: {ErrorsOf(root)}.", keys));
            }

            if (!root.TryGetProperty("request", out JsonElement request) || request.ValueKind != JsonValueKind.String || string.IsNullOrEmpty(request.GetString()))
            {
                throw new InvalidOperationException(Hide($"{where} holds no request id (D-1201, T-2).", keys));
            }

            return request.GetString()!;
        }
    }

    private static string ErrorsOf(JsonElement root)
    {
        if (root.ValueKind != JsonValueKind.Object || !root.TryGetProperty("errors", out JsonElement errors) || errors.ValueKind != JsonValueKind.Array)
        {
            return "the reply names no error";
        }

        List<string> items = [];
        foreach (JsonElement item in errors.EnumerateArray())
        {
            items.Add(item.ValueKind == JsonValueKind.String ? item.GetString()! : item.GetRawText());
        }

        return items.Count == 0 ? "the reply names no error" : string.Join("; ", items);
    }

    private static string KeyOf(Func<string, string?> variable, string name)
    {
        string? value = variable(name);
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException($"The variable {name} is absent or empty. The workflow reads it from the repository secret of the same name (D-1201, T-2).");
        }

        return value.Trim();
    }

    private static void CheckPart(string part, string value, int limit, bool canBeEmpty)
    {
        ArgumentNullException.ThrowIfNull(value);
        if (!canBeEmpty && value.Length == 0)
        {
            throw new InvalidOperationException($"The {part} of a Pushover message is empty, and Pushover needs one (T-2).");
        }

        if (value.Length > limit)
        {
            throw new InvalidOperationException($"The {part} of the Pushover message holds {value.Length.ToString(CultureInfo.InvariantCulture)} characters, and the limit of Pushover is {limit.ToString(CultureInfo.InvariantCulture)} (T-2).");
        }
    }
}
