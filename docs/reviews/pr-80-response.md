# PR 80 response

The author answer to the review of `29615cb0a1a4bf2786083e213ef55d69f1456a36` in `docs/reviews/pr-80.md`.

## P2-1: Concurrent file growth bypasses the read cap

Disposition: full merit.

Evidence: `FileText.Read` read the length with `FileInfo.Length`, and then `File.ReadAllBytes` opened the file a second time. A file that another program grew between the two calls passed the cap, and `ReadAllBytes` took each byte.

Correction: `TheThingBelow.Storage/FileText.cs` opens one stream, checks the length of that stream, and reads it through `FileText.ReadStream`. `ReadStream` reads in chunks of 64 KiB, counts each byte, and throws a `StorageException` with the path at the first byte past the cap. It holds no more than the cap in memory (F-129).

Regression check: `FileTextTests.AStreamThatGrowsPastTheCapDuringTheReadFailsAtTheCap` reads a stream with no end. It asserts the error, its path, and that the read took the cap plus one byte at most. `AStreamOfExactlyTheCapReads` holds the boundary. The code of `29615cb` has no stream seam, and its read of that stream would never end. Both tests pass, with the tests of each store.

## Ids

No new D-# id and no new F-# id. The correction completes F-129.

## Final head

The head of this answer is the commit that holds this file.
