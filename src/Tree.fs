module Tree

open Elmish

open System
open System.IO

open Avalonia
open Avalonia.Controls
open Avalonia.FuncUI
open Avalonia.FuncUI.DSL
open Avalonia.FuncUI.Types
open Avalonia.Media.Imaging
open Avalonia.Platform
open Avalonia.Layout

type Node<'Node> = Node of 'Node * Node<'Node> seq

type FileSystemTree = Node<DirectoryInfo>

type State = { RootNodes: FileSystemTree array }

[<RequireQualifiedAccessAttribute>]
type ExternalMsg = OpenDirectory of DirectoryInfo

type Msg =
    | UpdateTree
    | OpenDirectory of DirectoryInfo

let rec private fromDir (dirInfo: DirectoryInfo) =
    let subItems =
        seq { yield! dirInfo.EnumerateDirectories("*", new EnumerationOptions(IgnoreInaccessible = true)) |> Seq.map fromDir }

    Node(dirInfo, subItems)

let nodeSelector item =
    match item with
    | Node(_, children) -> children

let nodeTemplate (item: FileSystemTree) dispatch : IView =
    match item with
    | Node(dir, _) ->
        StackPanel.create [
            StackPanel.orientation Orientation.Horizontal
            StackPanel.spacing 4.0
            // StackPanel.horizontalAlignment HorizontalAlignment.Stretch
            StackPanel.onPointerReleased (fun _ -> dispatch (OpenDirectory dir)) // why does this trigger 2 "release" events
            StackPanel.children [
                Images.folder
                TextBlock.create [
                    // TextBlock.horizontalAlignment HorizontalAlignment.Stretch
                    TextBlock.text dir.Name
                ]
            ]
        ]

let private treeView (roots: FileSystemTree array) dispatch : IView =
    Border.create [
        Border.borderThickness (0, 0, 1, 0)
        Border.borderBrush "#363636"
        Border.child (
            TreeView.create [
                TreeView.dock Dock.Left
                TreeView.padding 5
                TreeView.dataItems roots
                TreeView.itemTemplate (
                    DataTemplateView<FileSystemTree>
                        .create ((fun item -> nodeSelector item), (fun item -> nodeTemplate item dispatch))
                )
            ]
        )
    ]

let private getDriveTrees =
    let driveTrees =
        Directory.GetLogicalDrives()
        |> Array.map (fun drive ->
            let dir = new DirectoryInfo(drive)
            fromDir dir
        )

    driveTrees

let init () =
    let drives = getDriveTrees
    { RootNodes = drives }

let update msg state =
    match msg with
    | UpdateTree ->
        let drives = getDriveTrees

        { RootNodes = drives }, Cmd.none, None
    | OpenDirectory dir -> state, Cmd.none, Some(ExternalMsg.OpenDirectory dir)

let view state dispatch = treeView state.RootNodes dispatch
