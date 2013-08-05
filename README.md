JabbR.Desktop
=========
### A cross platform JabbR client for OS X, Windows, and Linux

Download
--------

* [Windows Installer](http://download.picoe.ca/JabbReto/windows/setup.exe)
* [OS X Mac App Store](https://itunes.apple.com/us/app/jabbreto/id564990712?mt=12)

Description
-----------

This project aims to create a great cross platform desktop client for [JabbR](http://jabbr.net).
This makes liberal use of the following libraries:

* [Eto.Forms](https://github.com/picoe/Eto) - Cross Platform UI toolkit
* [JabbR.Client](https://github.com/JabbR/JabbR) - .NET API for creating JabbR clients 
* [SignalR](https://github.com/SignalR/SignalR) - Async library for .NET to help build real-time, multi-user interactive web applications
* [Json.NET](http://json.codeplex.com/) - Json.NET is a popular high-performance JSON framework for .NET
* [MahApps.Metro](https://github.com/MahApps/MahApps.Metro) - For Metro WPF styling
* [CefGlue](http://xilium.bitbucket.org/cefglue/) - Embedded chromium for WPF version

Screenshots
-----------

OS X Screenshot
![OS X](http://cwensley.github.com/JabbR.Eto/images/screenshots/Main-OSX.png)

Windows Screenshot
![WPF](http://cwensley.github.com/JabbR.Eto/images/screenshots/Main-MetroWPF.png)

Ubuntu Screenshot
![WPF](http://cwensley.github.com/JabbR.Eto/images/screenshots/Main-Ubuntu.png)

How To Build
------------

Cloning:

This uses submodules, so ensure you do a recursive clone of this project!

OS X:

1. Install [Xcode from Mac App Store](http://itunes.apple.com/us/app/xcode/id497799835?ls=1&mt=12)
2. Install [Mono SDK 3.0.6 for OS X](http://mono-project.com/Downloads)
3. Install [Xamarin Studio v4 for OS X](http://monodevelop.com/Download)
4. In terminal.app: git clone --recursive git://github.com/JabbR/JabbR.Desktop.git
5. Open Source/JabbR.Mac.sln in MonoDevelop
6. Build & Run! 

Windows:

1. Install [Visual 2012 Express for Windows Desktop](http://www.microsoft.com/visualstudio/eng/products/visual-studio-express-for-windows-desktop)
2. Install git client of choice
3. In console (or via gui): git clone --recursive git://github.com/JabbR/JabbR.Desktop.git
4. Open Source/JabbR.Windows.sln in Visual Studio
5. Build & Run!

Status
------

JabbR.Desktop is in a very early state and is missing many features or can be unstable.

Current features:

* Sign in using username/password
* Multiple servers
* Channel listing
* Private chats with other users
* Shows all media and links
* Collapsing notifications and media
* Autocomplete user names and channel rooms
* Gravatar icons
* More!

Contributing
------------

You can help out by submitting missing features, bugs or requests into github issues, or fork the project and start coding!