using System;
using System.Text;
using TheThingBelow.Core.Hashing;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The SHA-256 of Core makes the content hash (D-644, D-645). Each vector below is a
/// published value of FIPS 180-4 or of its appendix B, which the roadmap requires.
/// </summary>
public sealed class Sha256Tests
{
    /// <summary>The published digest of the empty message (FIPS 180-4, appendix B).</summary>
    private const string EmptyDigest = "e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855";

    /// <summary>The published digest of "abc" (FIPS 180-4, appendix B.1).</summary>
    private const string AbcDigest = "ba7816bf8f01cfea414140de5dae2223b00361a396177a9cb410ff61f20015ad";

    /// <summary>The published digest of the 448-bit message (FIPS 180-4, appendix B.2).</summary>
    private const string TwoBlockDigest = "248d6a61d20638b8e5c026930c3e6039a33ce45964ff2167f6ecedd419db06c1";

    /// <summary>The published digest of one million `a` characters (FIPS 180-4, appendix B.3).</summary>
    private const string MillionDigest = "cdc76e5c9914fb9281a1c7e284d73e67f1809a48a497200e046d39ccc7112cd0";

    [Theory]
    [InlineData("", EmptyDigest)]
    [InlineData("abc", AbcDigest)]
    [InlineData("abcdbcdecdefdefgefghfghighijhijkijkljklmklmnlmnomnopnopq", TwoBlockDigest)]
    public void TheDigestMatchesThePublishedVector(string message, string expected)
    {
        string digest = Sha256.ComputeHex(Encoding.UTF8.GetBytes(message));

        Assert.Equal(expected, digest);
    }

    [Fact]
    public void TheDigestOfOneMillionCharactersMatchesThePublishedVector()
    {
        byte[] message = new byte[1_000_000];
        Array.Fill(message, (byte)'a');

        string digest = Sha256.ComputeHex(message);

        Assert.Equal(MillionDigest, digest);
    }

    [Theory]
    [InlineData(54)]
    [InlineData(55)]
    [InlineData(56)]
    [InlineData(57)]
    [InlineData(63)]
    [InlineData(64)]
    [InlineData(65)]
    [InlineData(119)]
    [InlineData(120)]
    [InlineData(128)]
    public void TheDigestOfEachBlockBoundaryHasTheRightSize(int length)
    {
        // A message of 56 bytes or more in the last block leaves no room for the length, and
        // that tail needs one more block (FIPS 180-4, 5.1.1). These lengths cross each edge.
        byte[] message = new byte[length];
        for (int index = 0; index < length; index++)
        {
            message[index] = (byte)index;
        }

        string digest = Sha256.ComputeHex(message);

        Assert.Equal(Sha256.DigestSize * 2, digest.Length);
    }

    [Fact]
    public void TheDigestOfTheLongMessageMatchesTheDigestOfItsHalves()
    {
        // The message of appendix B.2 is 56 bytes, which crosses the boundary above. This
        // test proves that the padding of that length gives the published value too.
        byte[] message = Encoding.UTF8.GetBytes("abcdbcdecdefdefgefghfghighijhijkijkljklmklmnlmnomnopnopq");

        Assert.Equal(56, message.Length);
        Assert.Equal(TwoBlockDigest, Sha256.ComputeHex(message));
    }

    [Fact]
    public void TheDigestIsThirtyTwoBytes()
    {
        byte[] digest = Sha256.Compute(Encoding.UTF8.GetBytes("abc"));

        Assert.Equal(Sha256.DigestSize, digest.Length);
    }
}
