module Counter

open Avalonia.Controls
open Avalonia.FuncUI.DSL
open Avalonia.Layout
open Avalonia.FuncUI.Types

type Model = { Value: int }

type Msg =
    | Increment
    | Decrement

let init () = { Value = 0 }

let update msg model : Model =
    match msg with
    | Increment -> { model with Value = model.Value + 1 }
    | Decrement -> { model with Value = model.Value - 1 }

let view model dispatch : IView =
    DockPanel.create [
        DockPanel.children [
            Button.create [
                Button.dock Dock.Bottom
                Button.onClick (fun _ -> Decrement |> dispatch)
                Button.content "-"
                Button.horizontalAlignment HorizontalAlignment.Stretch
                Button.horizontalContentAlignment HorizontalAlignment.Center
            ]
            Button.create [
                Button.dock Dock.Bottom
                Button.onClick (fun _ -> Increment |> dispatch)
                Button.content "+"
                Button.horizontalAlignment HorizontalAlignment.Stretch
                Button.horizontalContentAlignment HorizontalAlignment.Center
            ]
            TextBlock.create [
                TextBlock.dock Dock.Top
                TextBlock.fontSize 48.0
                TextBlock.verticalAlignment VerticalAlignment.Center
                TextBlock.horizontalAlignment HorizontalAlignment.Center
                TextBlock.text (string model.Value)
            ]
        ]
    ]
