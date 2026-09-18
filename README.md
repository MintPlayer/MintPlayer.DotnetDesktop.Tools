# MintPlayer.DotnetDesktop.Tools

[![Coverage](https://coverage.mintplayer.com/badge/MintPlayer/MintPlayer.DotnetDesktop.Tools.svg)](https://coverage.mintplayer.com/r/MintPlayer/MintPlayer.DotnetDesktop.Tools)

Windows desktop libraries for .NET: a browser picker dialog, installed-browser detection,
icon extraction, an interactive Karnaugh map control, a Quine-McCluskey solver and a 3D
modeling engine.

## MintPlayer.BrowserDialog
Dialog that lets the user pick from the installed webbrowsers

### Preview
![Dialog that lets the user pick from the installed webbrowsers](https://github.com/MintPlayer/MintPlayer.PlatformBrowser/blob/master/BrowserDialog.png)

### Version info

| License                                                                                                               | Build status                                                                                           |
|-----------------------------------------------------------------------------------------------------------------------|--------------------------------------------------------------------------------------------------------|
| [![License](https://img.shields.io/badge/License-Apache%202.0-green.svg)](https://opensource.org/licenses/Apache-2.0) | ![.NET Core](https://github.com/MintPlayer/MintPlayer.BrowserDialog/workflows/.NET%20Core/badge.svg)   |

| Package                    | Release                                                                                                                                                 | Preview                                                                                                                                                    | Downloads |
|----------------------------|---------------------------------------------------------------------------------------------------------------------------------------------------------|------------------------------------------------------------------------------------------------------------------------------------------------------------|-----------|
| MintPlayer.PlatformBrowser | [![NuGet Version](https://img.shields.io/nuget/v/MintPlayer.PlatformBrowser.svg?style=flat)](https://www.nuget.org/packages/MintPlayer.PlatformBrowser) | [![NuGet Version](https://img.shields.io/nuget/vpre/MintPlayer.PlatformBrowser.svg?style=flat)](https://www.nuget.org/packages/MintPlayer.PlatformBrowser) | [![NuGet](https://img.shields.io/nuget/dt/MintPlayer.PlatformBrowser.svg?style=flat)](https://www.nuget.org/packages/MintPlayer.PlatformBrowser) |
| MintPlayer.IconUtils       | [![NuGet Version](https://img.shields.io/nuget/v/MintPlayer.IconUtils.svg?style=flat)](https://www.nuget.org/packages/MintPlayer.IconUtils)             | [![NuGet Version](https://img.shields.io/nuget/vpre/MintPlayer.IconUtils.svg?style=flat)](https://www.nuget.org/packages/MintPlayer.IconUtils)             | [![NuGet](https://img.shields.io/nuget/dt/MintPlayer.IconUtils.svg?style=flat)](https://www.nuget.org/packages/MintPlayer.IconUtils)             |
| MintPlayer.BrowserDialog   | [![NuGet Version](https://img.shields.io/nuget/v/MintPlayer.BrowserDialog.svg?style=flat)](https://www.nuget.org/packages/MintPlayer.BrowserDialog)     | [![NuGet Version](https://img.shields.io/nuget/vpre/MintPlayer.BrowserDialog.svg?style=flat)](https://www.nuget.org/packages/MintPlayer.BrowserDialog)     | [![NuGet](https://img.shields.io/nuget/dt/MintPlayer.BrowserDialog.svg?style=flat)](https://www.nuget.org/packages/MintPlayer.BrowserDialog)     |

## MintPlayer.PlatformBrowser
This package allows you to retrieve the web browsers (including Microsoft Edge) installed on the system.

### Installation

#### NuGet package manager
Open the NuGet package manager and install the `MintPlayer.PlatformBrowser` package in the project

#### Package manager console

    Install-Package MintPlayer.PlatformBrowser

### Usage
Create a ```PlatformBrowser``` and call it:

    var platformBrowser = new PlatformBrowser();
    var browsers = await platformBrowser.GetInstalledBrowsers();

It implements ```IPlatformBrowser``` from **MintPlayer.PlatformBrowser.Abstractions**, so you
can depend on the contract without taking the registry and UWP implementation with it, and
substitute it in tests.

### Copy-n-paste code sample

    var platformBrowser = new PlatformBrowser();
    var browsers = await platformBrowser.GetInstalledBrowsers();
    foreach (var browser in browsers)
    {
        Console.WriteLine($"Browser: {browser.Name}");
        Console.WriteLine($"Executable: {browser.ExecutablePath}");
        Console.WriteLine($"Icon path: {browser.IconPath}");
        Console.WriteLine($"Icon index: {browser.IconIndex}");
        Console.WriteLine();
    }
	
## MintPlayer.IconUtils

### Installation
#### NuGet package manager
Open the NuGet package manager and install the **MintPlayer.IconUtils** package in the project
#### Package manager console
    Install-Package MintPlayer.IconUtils

### Usage

    var extractor = new IconExtractor();
    var icons = await extractor.Split(icoPath);
    var images = await extractor.ExtractImagesFromIcon(icons[0]);

## MintPlayer.BrowserDialog

### Installation
#### NuGet package manager
Open the NuGet package manager and install the **MintPlayer.BrowserDialog** package in the project
#### Package manager console
    Install-Package MintPlayer.BrowserDialog

### Usage

    var dialog = new BrowserDialog();
    if (dialog.ShowDialog() == DialogResult.OK)
    {
        MessageBox.Show($"You picked {dialog.SelectedBrowser.Name}.\r\nThe executable path is {dialog.SelectedBrowser.ExecutablePath}");
    }

## Contributors

| Pull-request                                                                                            | Contributor                                       |
|---------------------------------------------------------------------------------------------------------|---------------------------------------------------|
| [Target .NET Standard](https://github.com/MintPlayer/MintPlayer.PlatformBrowser/pull/1)                 | [merijndejonge](https://github.com/merijndejonge) |
| [Also lists browsers from CurrentUser](https://github.com/MintPlayer/MintPlayer.PlatformBrowser/pull/2) | [mderu](https://github.com/mderu)                 |
