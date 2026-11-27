# RubyGraphCalculator.UI
- Detailed UI behaviour (layout, phase display, validation, calculation, session): see `docs/ui-spec.md`.
  Read it before changing any UI behaviour. The UI language is English
- Allowed packages: `ScottPlot.WPF`, `Serilog`, `Serilog.Sinks.File`, `Serilog.Sinks.Debug`.
  Other packages — only upon explicit request.
- No calculations are written here: `RubyGraphCalculator.UI` only calls classes and interfaces from `RubyGraphCalculator.Core`.
- The user selects degrees or radians in the UI (degrees by default). The ViewModel creates `Angle`
  with the chosen unit and passes it to `RubyGraphCalculator.Core`.

## MVVM
- No third-party MVVM libraries. `INotifyPropertyChanged` is implemented manually.
- All ViewModels inherit from `ViewModelBase` (`SetProperty` + `[CallerMemberName]`).
- Commands — only via custom `RelayCommand`. Do not create new `ICommand` implementations.
- ViewModel does not access the View and does not reference `ScottPlot` types.
  Data for charts is provided as `double[]` / `double[,]`.

## Charts (ScottPlot)
- Charts are updated only via attached behaviors (`Behaviors/` folder).
  Code-behind is not used for charts.

## Key UI rules (details in `docs/ui-spec.md`)
- Two charts in one window: the sum of up to 4 sine waves on top, its spectrum below
- Charts recalculate when the cursor leaves a changed field, and when a signal is added or removed;
  switching the phase unit does not recalculate
- The calculation runs in the background; fields are disabled and it can be cancelled.
  After a cancel both charts are cleared
- The phase field shows the value as entered; a hint shows the normalized angle (3 decimals) when it differs
- The session (parameters only, JSON) is stored in `%AppData%\RubyGraphCalculator\data`,
  never in the program folder

## Logging (Serilog)
- Configured once at application startup (`App.xaml.cs`).
- Output: to a file (`logs/` folder, in `.gitignore`) and to the debug window.
- The dependency is injected as `Serilog.ILogger` so it can be mocked in tests.
- Exceptions from `RubyGraphCalculator.Core` are caught in `RubyGraphCalculator.UI` and logged.

## Folder structure
- Folders by MVVM role: `Views/`, `ViewModels/`, `Commands/`, `Behaviors/`, `Services/`
  (`Services/`: session storage and other work with files)
  New code goes into the matching folder; ask before creating a new top-level folder
- Namespace matches the folder: `RubyGraphCalculator.UI.Views`
- `App.xaml` stays in the project root; its `StartupUri` points to `Views/MainWindow.xaml`
