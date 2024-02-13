namespace CounterApp

open Avalonia
open Avalonia.FuncUI
open Avalonia.Controls.ApplicationLifetimes
open Avalonia.Themes.Fluent

type App() =
    inherit Application()

    override this.Initialize () =
        this.Styles.Add(FluentTheme())
        this.Styles.Load("avares://AvaloniaFileExplorer/Assets/Styles/Main.axaml")
        this.RequestedThemeVariant <- Styling.ThemeVariant.Dark

    override this.OnFrameworkInitializationCompleted () =
        match this.ApplicationLifetime with
        | :? IClassicDesktopStyleApplicationLifetime as desktopLifetime -> desktopLifetime.MainWindow <- Main.MainWindow()
        | _ -> ()

module Program =

    [<EntryPoint>]
    let main (args: string[]) =
        AppBuilder
            .Configure<App>()
            .UsePlatformDetect()
            .UseSkia()
            .StartWithClassicDesktopLifetime(args)
