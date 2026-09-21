# PR review: commit and session end

Part of the `pr-review` skill (D-588). Load this file before you commit a review record or a response file, and before the session ends.

## Commit the record

Always commit the review record and the session handoff, then push them to the PR branch. Do it in the session that writes them.

| After | Commit these files | Who commits |
|---|---|---|
| A review or a repeat review | `docs/reviews/pr-<number>.md`, `docs/session-handoff.md`, and `docs/session-handoff-archive.md` when an entry moves | The reviewer |
| Work that answers a review | `docs/reviews/pr-<number>-response.md`, each corrected file, `docs/session-handoff.md`, and `docs/session-handoff-archive.md` when an entry moves | The author |

Make one commit that holds the record and its handoff entry. Never leave either file uncommitted or unpushed.
A push is the only way `review-gate` sees the record, because the gate reads the PR head.
A review is complete only when the remote holds the record. The session end gate below proves it.

An uncommitted review record has three effects:

- The next commit from the other provider absorbs it, and the history no longer shows who wrote what.
- An author can commit an approval that the author never read, and then report the wrong verdict.
- `review-gate` cannot read the record, because the record is not on the PR head.

Write the commit message in an impersonal voice. Name no provider, agent, harness, or model (T-6, D-22).

Fetch the remote and read the handoff again before you write the entry. Take the highest session number and add one.
Name the remote head in the state of the build.
Add the handoff entry at the top of the file, as a new entry (D-18).

Another provider can add an entry above yours while you work. Add your own entry. Never append to an older one, and never change the words of theirs.

## The handoff files

The reviewer and the author have the same rights over the two handoff files (D-824). Both files are in the metadata set, so a change to them never moves the effective head (D-610). Before the commit, make both files pass the handoff rules of the `ste-check` command, HANDOFF 1 to HANDOFF 4 (D-18, D-607):

- Add your own entry at the top of `docs/session-handoff.md`, with the next session number.
- Move each entry past the 10 newest to the top of `docs/session-handoff-archive.md`, with no change to its text.
- Correct the order of the entries, so the two files hold one list, newest first.
- Give your own entry a new number when another session took that number first. Fetch, read the highest number again, and add one.
- Keep the top entry under the size limit of 5 KB (D-611).

A move and a change of order are no edit of an entry. Never change the words of an entry of another session. A fault that needs a change of the words of another entry goes into your record. The author of that entry corrects it.

## Session end gate

Run these four commands after the commit, in this order. The evidence comes from the remote, not from the local checkout.

```
git push origin <branch>
git fetch origin
git status --short --branch
gh pr view <number> --json headRefOid --jq .headRefOid
```

The status line must show no `[ahead N]`. The hash from `gh pr view` must equal `git rev-parse HEAD`.
Write the push line in the Verification section of the review record, and name the remote head in the handoff entry.
A record with no push line is incomplete, and the next session treats it as unpushed.

After the gate passes, apply the completion gate of the `one-pr-one-session` skill. End the session only at the hand-over point of its role (D-582).

If the remote refuses the push, the review is not complete. Do not end the session.
Ask the owner to approve the push, and say in the handoff that the record has a commit and no push.
A sandbox that blocks the network denies the push without a message from git, so read the status line and not the push output.

At the start of a review or a repeat review, run `git fetch` and `git status --short --branch` too.
If the checkout is ahead of the remote with a commit from the other provider, push it first. Record that in the review file.
