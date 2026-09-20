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
    [InlineData(54, "675f28acc0b90a72d1c3a570fe83ac565555db358cf01826dc8eefb2bf7ca0f3")]
    [InlineData(55, "463eb28e72f82e0a96c0a4cc53690c571281131f672aa229e0d45ae59b598b59")]
    [InlineData(56, "da2ae4d6b36748f2a318f23e7ab1dfdf45acdc9d049bd80e59de82a60895f562")]
    [InlineData(57, "2fe741af801cc238602ac0ec6a7b0c3a8a87c7fc7d7f02a3fe03d1c12eac4d8f")]
    [InlineData(63, "29af2686fd53374a36b0846694cc342177e428d1647515f078784d69cdb9e488")]
    [InlineData(64, "fdeab9acf3710362bd2658cdc9a29e8f9c757fcf9811603a8c447cd1d9151108")]
    [InlineData(65, "4bfd2c8b6f1eec7a2afeb48b934ee4b2694182027e6d0fc075074f2fabb31781")]
    [InlineData(119, "da18797ed7c3a777f0847f429724a2d8cd5138e6ed2895c3fa1a6d39d18f7ec6")]
    [InlineData(120, "f52b23db1fbb6ded89ef42a23ce0c8922c45f25c50b568a93bf1c075420bbb7c")]
    [InlineData(128, "471fb943aa23c511f6f72f8d1652d9c880cfa392ad80503120547703e56a2be5")]
    public void TheDigestOfEachBlockBoundaryMatchesTheReferenceValue(int length, string expected)
    {
        // A message of 56 bytes or more in the last block leaves no room for the length, and
        // that tail needs one more block (FIPS 180-4, 5.1.1). These lengths cross each edge,
        // and each value comes from the SHA-256 of the Python standard library, read on
        // 2026-09-20. The message is the bytes 0 to length minus 1.
        byte[] message = new byte[length];
        for (int index = 0; index < length; index++)
        {
            message[index] = (byte)index;
        }

        string digest = Sha256.ComputeHex(message);

        Assert.Equal(expected, digest);
    }

    [Fact]
    public void TheDigestOfTheFiftySixByteMessageMatchesThePublishedVector()
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
