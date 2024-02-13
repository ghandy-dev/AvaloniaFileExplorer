module Main

open Elmish

open Avalonia
open Avalonia.Controls
open Avalonia.FuncUI.DSL
open Avalonia.FuncUI.Elmish
open Avalonia.FuncUI.Elmish.ElmishHook
open Avalonia.FuncUI.Elmish.Program
open Avalonia.FuncUI.Hosts
open Avalonia.FuncUI.Types

type Msg =
    | TreeMsg of Tree.Msg
    | ExplorerMsg of Explorer.Msg
    | ToolbarMsg of Toolbar.Msg
    | WindowResized of Size

type State = {
    TreeState: Tree.State
    ExplorerState: Explorer.State
    ToolbarState: Toolbar.State
}

let init () =
    {
        TreeState = Tree.init ()
        ExplorerState = Explorer.init ()
        ToolbarState = Toolbar.init ()
    },
    Cmd.none

let handleTreeExternalMsg =
    function
    | None -> Cmd.none
    | Some msg' ->
        match msg' with
        | Tree.ExternalMsg.OpenDirectory dir ->
            Cmd.batch [
                Cmd.ofMsg (ExplorerMsg(Explorer.Msg.OpenDirectory dir))
                Cmd.ofMsg (ToolbarMsg(Toolbar.Msg.OpenDirectory dir))
            ]

let handleToolbarExternalMsg =
    function
    | None -> Cmd.none
    | Some msg' ->
        match msg' with
        | Toolbar.ExternalMsg.SearchFiles input -> Cmd.ofMsg (ExplorerMsg(Explorer.Msg.SearchFiles input))
        | Toolbar.ExternalMsg.OpenDirectory dir -> Cmd.ofMsg (ExplorerMsg(Explorer.Msg.OpenDirectory dir))
        | Toolbar.ExternalMsg.Refresh -> Cmd.ofMsg (ExplorerMsg(Explorer.Msg.Refresh))

let handleExplorerExternalMsg =
    function
    | None -> Cmd.none
    | Some msg' ->
        match msg' with
        | Explorer.ExternalMsg.OpenDirectory dir -> Cmd.ofMsg (ToolbarMsg(Toolbar.Msg.OpenDirectory dir))

let update msg (state: State) =
    match msg with
    | TreeMsg msg' ->
        let res, _, externalCmd = Tree.update msg' state.TreeState
        let handled = handleTreeExternalMsg externalCmd
        { state with TreeState = res }, handled
    | ExplorerMsg msg' ->
        let res, _, externalCmd = Explorer.update msg' state.ExplorerState
        let handled = handleExplorerExternalMsg externalCmd
        { state with ExplorerState = res }, Cmd.batch [ handled ]
    | ToolbarMsg msg' ->
        let res, cmd, externalCmd = Toolbar.update msg' state.ToolbarState
        let handled = handleToolbarExternalMsg externalCmd
        let mapped = Cmd.map ToolbarMsg cmd
        { state with ToolbarState = res }, Cmd.batch [ handled ; mapped ]
    | WindowResized msg' -> state, Cmd.ofMsg (ToolbarMsg(Toolbar.Msg.WindowResized(msg'.Width |> int)))

let top state dispatch =
    StackPanel.create [
        StackPanel.dock Dock.Top
        StackPanel.children [ Toolbar.view state.ToolbarState (ToolbarMsg >> dispatch) ]
    ]

let splitView state dispatch : IView =
    SplitView.create [
        SplitView.displayMode SplitViewDisplayMode.CompactInline
        SplitView.isPaneOpen true
        SplitView.openPaneLength 240
        SplitView.compactPaneLengthProperty 240
        SplitView.background "#171717"
        Tree.view state.TreeState (TreeMsg >> dispatch) |> SplitView.pane
        Explorer.view state.ExplorerState (ExplorerMsg >> dispatch) |> SplitView.content
    ]

let view state (dispatch: Msg -> unit) : IView =
    DockPanel.create [
        DockPanel.lastChildFill true
        DockPanel.children [ top state dispatch ; splitView state dispatch ]
    ]


type MainWindow() as this =
    inherit HostWindow()

    do
        base.Title <- "Avalonia F# File Explorer"
        base.Width <- 1000
        base.Height <- 600

        let sizeObs = this.GetObservable(MainWindow.ClientSizeProperty)

        let windowObs (state: State) =
            let sub (dispatch: Msg -> unit) =
                sizeObs.Subscribe(fun size -> dispatch (WindowResized size))

            [ [ "WindowResized" ], sub ]

        Program.mkProgram init update view
        |> Program.withSubscription windowObs
        |> Program.withHost this
#if DEBUG
        // |> Program.withConsoleTrace
#endif
        |> Program.run
