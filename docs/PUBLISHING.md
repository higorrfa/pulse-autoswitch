# Publishing

The public source tree contains generic code, documentation and assets. Runtime data and driver tools are outside the Git index.

## Checklist

1. Run `scripts/Build.ps1`, `scripts/Test.ps1` and `scripts/Package.ps1`.
2. Test requirement checks and launcher generation from an extracted package.
3. Verify `git status` and the staged diff. Exclude `bin/`, `dist/`, `work/`, local settings, logs, personalized launchers, credentials and driver packages.
4. Keep `config.example.xml` free of local device IDs and paths.
5. Document media shortcuts as experimental and keep unvalidated features explicit.
6. Verify license notices and the commit author.

The GitHub workflow builds and tests on Windows, downloads the pinned official Inno Setup compiler after verifying its checksum and signature, and builds both the ZIP and setup executable as workflow artifacts. It does not install drivers or perform physical tests. Release assets can be attached separately from the source history. Packages are unsigned. Use `scripts/BuildInstaller.ps1 -Compiler <ISCC.exe path>` for a local setup build.

The README is bilingual; source code, user interface, scripts and detailed documentation use English. Code comments are omitted; protocol and interoperability references are maintained in documentation.
