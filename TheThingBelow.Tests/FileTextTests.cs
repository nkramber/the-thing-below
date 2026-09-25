using System;
using System.IO;
using TheThingBelow.Storage;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The one read of the text of a file of the game: the cap on its size and the strict decode
/// (F-129). The save store tests read the same rules through the files of a save.
/// </summary>
public sealed class FileTextTests
{
    private const long Cap = 1000;

    [Fact]
    public void AStreamThatGrowsPastTheCapDuringTheReadFailsAtTheCap()
    {
        // The P2-1 finding of the review of PR #80: the check of the length ran before the read,
        // so a file that another program grew in between passed the cap. The read counts each
        // byte, and it stops at the first byte past the cap.
        var endless = new EndlessStream();

        StorageException error = Assert.Throws<StorageException>(
            () => FileText.ReadStream(endless, "a-test-file.json", Cap, "the file of a test"));

        Assert.Equal("a-test-file.json", error.Path);
        Assert.Contains($"holds more than {Cap} bytes", error.Message, StringComparison.Ordinal);
        Assert.True(endless.Served <= Cap + 1, $"the read took {endless.Served} bytes from the stream, past the cap of {Cap}");
    }

    [Fact]
    public void AStreamOfExactlyTheCapReads()
    {
        using MemoryStream full = new(new byte[Cap]);

        Assert.Equal((int)Cap, FileText.ReadStream(full, "a-test-file.json", Cap, "the file of a test").Length);
    }

    /// <summary>A stream that gives a byte of the letter a on each read, with no end.</summary>
    private sealed class EndlessStream : Stream
    {
        public long Served { get; private set; }

        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => false;

        public override long Length => throw new NotSupportedException("the stream grows with no end");

        public override long Position
        {
            get => this.Served;
            set => throw new NotSupportedException("the stream takes no seek");
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            buffer.AsSpan(offset, count).Fill((byte)'a');
            this.Served += count;
            return count;
        }

        public override void Flush()
        {
        }

        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException("the stream takes no seek");

        public override void SetLength(long value) => throw new NotSupportedException("the stream takes no length");

        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException("the stream takes no write");
    }
}
