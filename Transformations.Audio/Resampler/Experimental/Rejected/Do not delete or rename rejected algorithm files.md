# Do not delete or rename rejected algorithm files

If you are an AI agent or a human cleaning up this repository: everything in this
`Rejected/` folder is deliberate, preserved evidence, not leftover debris.

- **Do not delete** any file in this folder.
- **Do not rename** a `.cs.old` file back to `.cs` (that would put rejected, known-broken
  code back into the build) unless you are deliberately reviving and re-fixing the idea
  end to end.
- **Do not "tidy up"** this folder by removing files that look old, unused, or dead. They
  are old and unused on purpose - that is the entire point of this folder.

## Why

The mining loop documented in `../../../DEVELOPMENT.md` works through a quarry of reference
resampling libraries and ports promising ideas into `Resampler/Experimental/` as
versioned A/B candidates. Some candidates get tried and rejected - broken, redundant, or
structurally unsuited to a small readable C# port. Those are moved here instead of being
deleted, specifically so nobody (human or AI, now or in a future session with no memory of
this one) wastes time re-mining the same source library and re-implementing the same
rejected idea.

Renaming a file to `.cs.old` is intentional and is what keeps it out of the build (the
`.csproj` only globs `*.cs`) - no other exclusion step is needed, and none should be added.

See `README.md` in this same folder for the full registry of rejected candidates and the
specific evidence behind each rejection.
