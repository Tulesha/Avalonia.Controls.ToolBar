# Avalonia.Controls.ToolBar

[![NuGet](https://img.shields.io/nuget/v/AvaloniaControlsToolBar)](https://www.nuget.org/packages/AvaloniaControlsToolBar) [![downloads](https://img.shields.io/nuget/dt/AvaloniaControlsToolBar)](https://www.nuget.org/packages/AvaloniaControlsToolBar)

## Description

ToolBar and ToolBarTray controls for Avalonia (port of WPF)

## ToolBar API

| Property Name    | Value          | Description                                            |
|------------------|----------------|--------------------------------------------------------|
| Orientation      | `Orientation`  | Defines the orientation of the `ToolBar`               |
| Band             | `int`          | Defines which group the `Toolbar` has                  |
| BandIndex        | `int`          | Defines which index in group the `ToolBar` has         |
| IsOverflowOpen   | `bool`         | Defines whether the overflow menu `ToolBar` is open    |
| HasOverflowItems | `bool`         | Defines whether the `Toolbar` has overflowing elements |
| IsOverflowItem   | `bool`         | Defines is the `ToolBar` item overflowing              |
| OverflowMode     | `OverflowMode` | Defines the overflow mode of the `ToolBar`             |

## ToolBarTray API

| Property Name | Value                 | Description                                                                             |
|---------------|-----------------------|-----------------------------------------------------------------------------------------|
| Background    | `IBrush?`             | Defines the background of `ToolBarTray`                                                 |
| Orientation   | `Orientation`         | Defines the orientation of the `ToolBarTray` (affect to `ToolBar.Orientation` Property) |
| IsLocked      | `bool`                | Defines whether the `ToolBarTray` is locked                                             |
| ToolBars      | `Collection<ToolBar>` | Defines `Content` of `ToolBarTray`                                                      |

## ToolBar.Demo

![](https://github.com/Tulesha/Avalonia.Controls.ToolBar/blob/main/.github/workflows/ToolBarSample.gif)

## ToolBarTray.Demo

![](https://github.com/Tulesha/Avalonia.Controls.ToolBar/blob/main/.github/workflows/ToolBarTraySample.gif)
