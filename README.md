# TogglTime
A command line utility for submitting your time to Toggl.

# Setup

1. Edit the [`src/TogglTime/App.Config`](https://github.com/kspearrin/TogglTime/blob/master/src/TogglTime/App.config) file with your Toggl default information. This includes your [Toggl API Token](https://toggl.com/app/profile) (found on the Toggl website under "My Profile") and your default workspace, project, task, and billable settings.
2. Run `build.cmd` to build TogglTime.
3. Run `src/TogglTime/bin/Release/TogglTime.exe` to start TogglTime. Feel free to copy the `Release` folder out somewhere more usable for each configuration build and/or create a shortcut to `TogglTime.exe` on your desktop.

This configuration and build can also all be done from within Visual Studio.

# Command line arguments

Various command line arguments are available if you do not want to be prompted for them each time you run TogglTime.

`TogglTime.exe [[workspaceId:int] [projectId:int] [taskId:int] [billable:bool]]`

*If you do not have a Task ID, `-` can be passed for the third argument.*

**TIP:** Using the above command line arguments, create a simple `cmd` script for each project configuration on your desktop that can launch TogglTime by just double clicking it.

# Linux/Mac

Those on Linux or Mac can install [Mono](http://www.mono-project.com/download/) in order to run TogglTime. With [Mono](http://www.mono-project.com/download/) installed you can now build using `build.sh` and then execute TogglTime with `mono`:

`mono TogglTime.exe [[workspaceId:int] [projectId:int] [taskId:int] [billable:bool]]`

**TIP:** Using the above command line arguments, create a simple shell script for each project configuration on your desktop that can launch TogglTime by just double clicking it. See [here](http://stackoverflow.com/questions/5125907/how-to-run-a-shell-script-in-os-x-by-double-clicking) for instructions on setting something like that up.
