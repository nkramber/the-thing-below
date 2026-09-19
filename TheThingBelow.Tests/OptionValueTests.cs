using System;
using System.IO;
using TheThingBelow.Tools;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// Every command reads its option values through <see cref="OptionValue"/>, so the message
/// of an empty value reads the same in each command (T-2, F-83).
/// </summary>
public sealed class OptionValueTests
{
    [Fact]
    public void AnEmptyValueWritesTheMessageAndAsksTheCommandToStop()
    {
        using StringWriter errors = new StringWriter();

        bool empty = OptionValue.ReportEmpty("--root", string.Empty, errors);

        Assert.True(empty);
        Assert.Contains(
            "the value of the option --root is empty",
            errors.ToString(),
            StringComparison.Ordinal);
    }

    [Fact]
    public void AValueWithTextWritesNothing()
    {
        using StringWriter errors = new StringWriter();

        bool empty = OptionValue.ReportEmpty("--root", ".", errors);

        Assert.False(empty);
        Assert.Equal(string.Empty, errors.ToString());
    }

    /// <summary>A value of spaces alone is a path that no check reads as empty (T-1).</summary>
    [Fact]
    public void AValueOfSpacesIsNotEmpty()
    {
        using StringWriter errors = new StringWriter();

        bool empty = OptionValue.ReportEmpty("--root", " ", errors);

        Assert.False(empty);
        Assert.Equal(string.Empty, errors.ToString());
    }
}
