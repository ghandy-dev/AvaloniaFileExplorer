module Toolbar

open Collections.Generic

open System
open System.IO

open Elmish

open Avalonia.Controls
open Avalonia.FuncUI.DSL
open Avalonia.FuncUI.Types
open Avalonia.Layout
open Avalonia.Controls.Primitives

type Msg =
    | OpenDirectory of DirectoryInfo
    | SearchFiles of string
    | Back
    | Forward
    | Refresh
    | UpTo
    | WindowResized of int

[<RequireQualifiedAccessAttribute>]
type ExternalMsg =
    | OpenDirectory of DirectoryInfo
    | SearchFiles of string
    | Refresh

type State = {
    CurrentFolder: DirectoryInfo
    Breadcrumbs: DirectoryInfo list
    BackwardsHistory: Stack<DirectoryInfo>
    ForwardsHistory: Stack<DirectoryInfo>
    WindowWidth: int
}

let rec private getParentTree (acc: DirectoryInfo list) (dir: DirectoryInfo) =
    match dir.Parent with
    | null -> acc
    | parent -> getParentTree (parent :: acc) parent

let private toolbarButtonStyle = [
    yield Button.width 32
    yield Button.height 32
    yield Button.background "transparent"
    yield Button.foreground "transparent"
]

let private buttons state dispatch : IView =
    StackPanel.create [
        StackPanel.spacing 16
        StackPanel.column 0
        StackPanel.margin 4.0
        StackPanel.orientation Orientation.Horizontal
        StackPanel.children [
            Button.create [
                Button.margin (12, 0, 0, 0)
                yield! toolbarButtonStyle
                Button.content Images.backwardsArrow
                Button.tip "Back"
                Button.onClick (fun _ -> dispatch Back)
                Button.isEnabled ((Stack.count state.BackwardsHistory) > 0)
            ]
            Button.create [
                Button.content Images.forwardsArrow
                yield! toolbarButtonStyle
                Button.tip "Forward"
                Button.onClick (fun _ -> dispatch Forward)
                Button.isEnabled ((Stack.count state.ForwardsHistory) > 0)
            ]
            Button.create [
                Button.content Images.upArrow
                yield! toolbarButtonStyle
                Button.tip "Up to"
                Button.onClick (fun _ -> dispatch UpTo)
                Button.isEnabled (state.CurrentFolder.Parent <> null)
            ]
            Button.create [
                Button.content Images.refresh
                yield! toolbarButtonStyle
                Button.tip "Refresh"
                Button.onClick (fun _ -> dispatch Refresh)
            ]
        ]
    ]

let private flyoutItemTemplate (item: DirectoryInfo) dispatch : IView =
    Button.create [
        Button.content item.Name
        Button.onClick (fun _ -> dispatch (OpenDirectory item))
        Button.background "transparent"
        Button.margin 0
        Button.horizontalAlignment HorizontalAlignment.Stretch
        Button.verticalAlignment VerticalAlignment.Stretch
    ]

let private addressBar state dispatch : IView =
    Border.create [
        Border.cornerRadius 4.0
        Border.column 1
        Border.margin 4.0
        Border.background "#363636"
        Border.child (
            ScrollViewer.create [
                ScrollViewer.verticalScrollBarVisibility ScrollBarVisibility.Hidden
                ScrollViewer.horizontalScrollBarVisibility ScrollBarVisibility.Visible
                ScrollViewer.content (
                    WrapPanel.create [
                        WrapPanel.orientation Orientation.Horizontal
                        WrapPanel.children [
                            let maxItems = max (state.WindowWidth / 200) 1

                            if state.Breadcrumbs.Length > maxItems then
                                Button.create [
                                    Button.margin (1.0, 4.0)
                                    Button.background "transparent"
                                    Button.content ("...")
                                    Button.flyout (
                                        Flyout.create [
                                            Flyout.placement PlacementMode.LeftEdgeAlignedBottom
                                            Flyout.showMode FlyoutShowMode.Transient
                                            Flyout.content (
                                                ScrollViewer.create [
                                                    ScrollViewer.content (
                                                        StackPanel.create [
                                                            StackPanel.children [
                                                                yield!
                                                                    state.Breadcrumbs
                                                                    |> List.take (state.Breadcrumbs.Length - 4)
                                                                    |> List.map (fun subfolder -> flyoutItemTemplate subfolder dispatch)
                                                            ]
                                                        ]
                                                    )
                                                ]
                                            )
                                        ]
                                    )
                                ]

                            let skipCount =
                                if state.Breadcrumbs.Length > maxItems then
                                    state.Breadcrumbs.Length - maxItems
                                else
                                    0

                            for breadcrumb in state.Breadcrumbs |> List.skip skipCount do
                                View.createWithKey $"{breadcrumb.Name}" Button.create [
                                    Button.margin (1.0, 4.0)
                                    Button.background "transparent"
                                    Button.content (breadcrumb.Name)
                                    Button.onClick (fun _ ->
                                        let dir = new DirectoryInfo(breadcrumb.FullName)
                                        dispatch (OpenDirectory dir)
                                    )
                                ]

                                let subFolders =
                                    breadcrumb.EnumerateDirectories("*", new EnumerationOptions(IgnoreInaccessible = true)) |> List.ofSeq

                                if subFolders.Length > 0 then
                                    Button.create [
                                        Button.margin (1.0, 4.0)
                                        Button.background "transparent"
                                        Button.content Images.chevron
                                        Button.flyout (
                                            Flyout.create [
                                                Flyout.placement PlacementMode.RightEdgeAlignedBottom
                                                Flyout.showMode FlyoutShowMode.Transient
                                                Flyout.content (
                                                    ScrollViewer.create [
                                                        ScrollViewer.content (
                                                            StackPanel.create [
                                                                StackPanel.children [
                                                                    yield!
                                                                        subFolders
                                                                        |> List.map (fun subfolder -> flyoutItemTemplate subfolder dispatch)
                                                                ]
                                                            ]
                                                        )
                                                    ]
                                                )
                                            ]
                                        )
                                    ]
                        ]
                    ]
                )
            ]
        )
    ]

let private searchBar state dispatch : IView =
    Border.create [
        Border.cornerRadius 4.0
        Border.column 2
        Border.margin 4.0
        Border.background "#363636"
        Border.child (
            StackPanel.create [
                StackPanel.children [
                    TextBox.create [
                        TextBox.background "#363636"
                        TextBox.borderThickness 0
                        TextBox.margin 4.0
                        TextBox.watermark $"Search {state.CurrentFolder.Name}"
                        TextBox.text ""
                        TextBox.onTextChanged (fun pattern -> dispatch (SearchFiles pattern))
                    ]
                ]
            ]
        )
    ]

let private toolbar state dispatch : IView =
    Grid.create [
        Grid.background "#2B2B2B"
        Grid.columnDefinitions "200, *, 280"
        Grid.children [
            buttons state dispatch
            addressBar state dispatch
            searchBar state dispatch
        ]
    ]

let init () =
    let docPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
    let dir = new DirectoryInfo(docPath)
    let breadcrumbs = getParentTree [ dir ] dir

    {
        CurrentFolder = dir
        Breadcrumbs = breadcrumbs
        BackwardsHistory = Stack.create ()
        ForwardsHistory = Stack.create ()
        WindowWidth = 1000
    }

let update (msg: Msg) state =
    match msg with
    | OpenDirectory dir ->
        let breadcrumbs = getParentTree [ dir ] dir

        let state' =
            if state.CurrentFolder = dir then
                {
                    state with
                        CurrentFolder = dir
                        Breadcrumbs = breadcrumbs
                }
            else
                {
                    state with
                        CurrentFolder = dir
                        Breadcrumbs = breadcrumbs
                        BackwardsHistory = Stack.push state.CurrentFolder state.BackwardsHistory
                }

        state', Cmd.none, Some(ExternalMsg.OpenDirectory dir)
    | SearchFiles pattern -> state, Cmd.none, Some(ExternalMsg.SearchFiles pattern)
    | Back ->
        match Stack.pop state.BackwardsHistory with
        | Some dir ->
            {
                state with
                    CurrentFolder = dir
                    ForwardsHistory = Stack.push state.CurrentFolder state.ForwardsHistory
            },
            Cmd.ofMsg (OpenDirectory dir),
            Some(ExternalMsg.OpenDirectory dir)
        | None -> state, Cmd.none, None
    | Forward ->
        match Stack.pop state.ForwardsHistory with
        | Some dir ->
            {
                state with
                    CurrentFolder = dir
                    BackwardsHistory = Stack.push state.CurrentFolder state.BackwardsHistory
            },
            Cmd.ofMsg (OpenDirectory dir),
            Some(ExternalMsg.OpenDirectory dir)
        | None -> state, Cmd.none, None
    | Refresh -> state, Cmd.none, Some(ExternalMsg.Refresh)
    | UpTo ->
        match state.CurrentFolder.Parent |> Option.ofObj with
        | None -> state, Cmd.none, None
        | Some dir ->
            {
                state with
                    CurrentFolder = dir
                    BackwardsHistory = Stack.push state.CurrentFolder state.BackwardsHistory
            },
            Cmd.ofMsg (OpenDirectory dir),
            Some(ExternalMsg.OpenDirectory dir)
    | WindowResized width -> { state with WindowWidth = width }, Cmd.none, None

let view state dispatch = toolbar state dispatch
