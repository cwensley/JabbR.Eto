JabbR.Eto
=========
### A cross platform JabbR client for OS X, Windows, and Linux

Description
-----------

This project aims to create a great cross platform desktop client for [JabbR](http://jabbr.net).
This makes liberal use of the following libraries:

* [Eto.Forms](https://github.com/picoe/Eto) - Cross Platform UI toolkit
* [JabbR.Client](https://github.com/davidfowl/JabbR.Client) - .NET API for creating JabbR clients 
* [SignalR](https://github.com/SignalR/SignalR) - Async library for .NET to help build real-time, multi-user interactive web applications
* [Json.NET](http://json.codeplex.com/) - Json.NET is a popular high-performance JSON framework for .NET

Screenshots
-----------

OS X Screenshot
![OS X](http://cwensley.github.com/JabbR.Eto/images/screenshots/Main-OSX.png)

WPF Screenshot
![WPF](http://cwensley.github.com/JabbR.Eto/images/screenshots/Main-WPF.png)

How To Build
------------

Cloning:

This uses submodules, so ensure you do a recursive clone of this project!

OS X:

1. Install [Xcode 4.3 from Mac App Store](http://itunes.apple.com/us/app/xcode/id497799835?ls=1&mt=12)
2. Install [Mono SDK 2.10.9 for OS X](http://mono-project.com/Downloads)
3. Install [MonoDevelop 3.0.2 for OS X](http://monodevelop.com/Download)
4. In terminal.app: git clone --recursive git://github.com/cwensley/JabbR.Eto.git
5. Open Source/JabbR.Eto.Mac.sln in MonoDevelop
6. Build & Run! 

Windows:

1. Install [Visual C# 2010 Express](http://www.microsoft.com/visualstudio/en-us/products/2010-editions/visual-csharp-express)
2. Install git client of choice
3. In console (or via gui): git clone --recursive  git://github.com/cwensley/JabbR.Eto.git
4. Open Source/JabbR.Eto.Wpf.sln in Visual Studio
5. Build & Run!

Status
------

JabbR.Eto is in a very early state and is missing many features.

Currently you can log in (via username/password), view and /join channels, and chat in
channels.

Contributing
------------

You can help out by submitting missing features into the github, or fork the project and start coding!