using System;
using System.IO;
using TheThingBelow.Tools;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// Every command reads its options through one parser, so an unknown option, an absent value,
/// an empty value, and a repeated option read the same in each command (T-1, T-2).
/// </summary>
public sealed class OptionParserTests
{
    [Fact]
    public void AValueAndAFlagRead()
    {
        using StringWriter errors = new StringWriter();

        OptionParser? options = OptionParser.Read("atlas", ["--root", ".", "--check"], ["--root", "--sheets"], ["--check"], errors);

        Assert.NotNull(options);
        Assert.Equal(".", options.Value("--root"));
        Assert.Null(options.Value("--sheets"));
        Assert.Equal("out", options.ValueOr("--sheets", "out"));
        Assert.True(options.Holds("--check"));
        Assert.Equal(string.Empty, errors.ToString());
    }

    [Fact]
    public void AnUnknownOptionNamesTheCommandAndItsOptions()
    {
        using StringWriter errors = new StringWriter();

        OptionParser? options = OptionParser.Read("atlas", ["--all"], ["--root"], ["--check"], errors);

        Assert.Null(options);
        Assert.Contains("'--all' is unknown", errors.ToString(), StringComparison.Ordinal);
        Assert.Contains("atlas takes --root <value>, and --check", errors.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public void AnOptionWithNoValueIsAFault()
    {
        using StringWriter errors = new StringWriter();

        OptionParser? options = OptionParser.Read("atlas", ["--root"], ["--root"], [], errors);

        Assert.Null(options);
        Assert.Contains("--root needs a value after it", errors.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public void AnEmptyValueIsAFault()
    {
        using StringWriter errors = new StringWriter();

        OptionParser? options = OptionParser.Read("atlas", ["--root", string.Empty], ["--root"], [], errors);

        Assert.Null(options);
        Assert.Contains("the value of the option --root is empty", errors.ToString(), StringComparison.Ordinal);
    }

    /// <summary>A repeated option would take the last value in silence (T-2).</summary>
    [Theory]
    [InlineData("--root", ".", "--root", ".")]
    [InlineData("--check", "--root", ".", "--check")]
    public void ARepeatedOptionIsAFault(params string[] args)
    {
        using StringWriter errors = new StringWriter();

        OptionParser? options = OptionParser.Read("atlas", args, ["--root"], ["--check"], errors);

        Assert.Null(options);
        Assert.Contains("is on the command line two times", errors.ToString(), StringComparison.Ordinal);
    }
}
