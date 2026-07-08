# Unity Version Fix

## What Changed

`ProjectSettings/ProjectVersion.txt` was updated from Unity `2022.3.62f1` to the
installed Unity 6.5 editor version:

```text
m_EditorVersion: 6000.5.2f1
m_EditorVersionWithRevision: 6000.5.2f1
```

`README.md` and `NEXT_STEPS.md` were also updated to name Unity `6000.5.2f1` as
the installed editor target.

## Why Unity Hub Was Reverting

Unity Hub reads `ProjectSettings/ProjectVersion.txt` to decide which editor
version a project targets. Because that file still listed `2022.3.62f1`, Hub kept
selecting or showing the project as a Unity 2022.3 project.

## What To Do Next

Close Unity Hub completely, reopen it, then open:

```text
D:\DREAM CORE
```

Use Unity `6000.5.2f1`.

Unity was not run during this fix.
