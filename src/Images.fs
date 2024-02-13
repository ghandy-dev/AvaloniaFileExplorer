module Images

open System

open Avalonia.Controls
open Avalonia.FuncUI.DSL
open Avalonia.Media.Imaging
open Avalonia.Platform

let folder =
    Image.create [
        Image.source (new Bitmap(AssetLoader.Open(new Uri("avares://AvaloniaFileExplorer/Assets/Icons/folder_16x16.png"))))
        Image.height 16
        Image.width 16
    ]

let text =
    Image.create [
        Image.source (new Bitmap(AssetLoader.Open(new Uri("avares://AvaloniaFileExplorer/Assets/Icons/text_16x16.png"))))
        Image.height 16
        Image.width 16
    ]

let unknown =
    Image.create [
        Image.source (new Bitmap(AssetLoader.Open(new Uri("avares://AvaloniaFileExplorer/Assets/Icons/unknown_16x16.png"))))
        Image.height 16
        Image.width 16
    ]

let forwardsArrow =
    Image.create [
        Image.source (new Bitmap(AssetLoader.Open(new Uri("avares://AvaloniaFileExplorer/Assets/Icons/forwards_32x32.png"))))
        Image.height 24
        Image.width 24
    ]

let backwardsArrow =
    Image.create [
        Image.source (new Bitmap(AssetLoader.Open(new Uri("avares://AvaloniaFileExplorer/Assets/Icons/backwards_32x32.png"))))
        Image.height 24
        Image.width 24
    ]

let upArrow =
    Image.create [
        Image.source (new Bitmap(AssetLoader.Open(new Uri("avares://AvaloniaFileExplorer/Assets/Icons/up_32x32.png"))))
        Image.height 24
        Image.width 24
    ]

let refresh =
    Image.create [
        Image.source (new Bitmap(AssetLoader.Open(new Uri("avares://AvaloniaFileExplorer/Assets/Icons/refresh_32x32.png"))))
        Image.height 24
        Image.width 24
    ]

let chevron =
    Image.create [
        Image.source (new Bitmap(AssetLoader.Open(new Uri("avares://AvaloniaFileExplorer/Assets/Icons/chevron_32x32.png"))))
        Image.height 16
        Image.width 16
    ]
