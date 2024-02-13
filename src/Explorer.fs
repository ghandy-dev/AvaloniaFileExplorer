module Explorer

open Elmish

open Avalonia.Controls
open Avalonia.FuncUI.DSL
open Avalonia.FuncUI.Types
open Avalonia.FuncUI

open System
open System.IO
open System.Diagnostics
open Avalonia.Layout

type Msg =
    | OpenDirectory of DirectoryInfo
    | SearchFiles of string
    | Refresh
    | Error

[<RequireQualifiedAccessAttribute>]
type ExternalMsg = OpenDirectory of DirectoryInfo

type State = {
    CurrentFolder: DirectoryInfo
    Files: FileSystemInfo list
    FilteredFiles: FileSystemInfo list
}

let fileTemplate (item: FileSystemInfo) dispatch : IView =
    StackPanel.create [
        StackPanel.orientation Orientation.Horizontal
        StackPanel.spacing 4.0
        StackPanel.horizontalAlignment HorizontalAlignment.Stretch
        StackPanel.verticalAlignment VerticalAlignment.Stretch
        StackPanel.onDoubleTapped (
            (fun _ ->
                match item with
                | :? DirectoryInfo as dir -> dispatch (OpenDirectory dir)
                | :? FileInfo as file ->
                    use p = new Process()
                    p.StartInfo <- new ProcessStartInfo("explorer", "\"" + file.FullName + "\"")
                    p.Start() |> ignore
                | _ -> ()
            )
        )
        StackPanel.children [
            match item with
            | :? DirectoryInfo as dir -> Images.folder
            | :? FileInfo as file ->
                match file.Extension with
                | ".txt" -> Images.text
                | _ -> Images.unknown
            | _ -> Images.unknown
            TextBlock.create [ TextBlock.text item.Name ]
        ]
    ]

let private explorerView items dispatch : IView =
    ListBox.create [
        ListBox.padding 4.0
        ListBox.dataItems items
        ListBox.itemTemplate (DataTemplateView<FileSystemInfo>.create (fun item -> fileTemplate item dispatch))
    ]

let private enumerateFilesAndFolders (dir: DirectoryInfo) =
    let files =
        dir.EnumerateFiles("*", new EnumerationOptions(IgnoreInaccessible = true))
        |> Seq.cast<FileSystemInfo>
        |> List.ofSeq
        |> List.append (
            dir.EnumerateDirectories("*", new EnumerationOptions(IgnoreInaccessible = true)) |> Seq.cast<FileSystemInfo> |> List.ofSeq
        )

    files

let init () =
    let path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
    let dir = new DirectoryInfo(path)
    let files = enumerateFilesAndFolders dir

    {
        CurrentFolder = dir
        Files = files
        FilteredFiles = files
    }

let update msg state =
    match msg with
    | OpenDirectory dir ->
        if dir.FullName = state.CurrentFolder.FullName then
            state, Cmd.none, None
        else
            let files = enumerateFilesAndFolders dir

            {
                state with
                    CurrentFolder = dir
                    Files = files
                    FilteredFiles = files
            },
            Cmd.none,
            Some(ExternalMsg.OpenDirectory dir)
    | SearchFiles pattern ->
        let files =
            state.Files |> List.filter (fun x -> x.Name.Contains(pattern, StringComparison.OrdinalIgnoreCase))

        { state with FilteredFiles = files }, Cmd.none, None

    | Refresh ->
        let files = enumerateFilesAndFolders state.CurrentFolder

        {
            state with
                Files = files
                FilteredFiles = files
        },
        Cmd.none,
        None
    | Error -> failwith "Not Implemented"

let view state dispatch =
    explorerView state.FilteredFiles dispatch
