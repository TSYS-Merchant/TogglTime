# TogglTime
A command line utility for submitting your time to Toggl.

# Setup

1. Edit the [`App.Config`](https://github.com/kspearrin/TogglTime/blob/master/src/TogglTime/App.config) file with your Toggl default information. This includes your [Toggl API Token](https://github.com/toggl/toggl_api_docs#api-token) and your default settings for workspace, project, task, and billable.
2. Run `build.cmd`.
3. Run `TogglTime.exe` from the `Release` `bin` folder. Feel free to copy the `Release` folder out somewhere more usable and/or create a shortcut to `TogglTime.exe` on your desktop.

This configuration and build can also all be done from within Visual Studio.

# Command line arguments

Various command line arguments are available if you do not want to be prompted for them each time you run TogglTime.

`TogglTime [workspaceId:int] [projectId:int] [taskId:int] [billable:bool]`


