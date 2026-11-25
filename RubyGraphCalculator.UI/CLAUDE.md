# RubyGraphCalculator.UI
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

## Logging (Serilog)
- Configured once at application startup (`App.xaml.cs`).
- Output: to a file (`logs/` folder, in `.gitignore`) and to the debug window.
- The dependency is injected as `Serilog.ILogger` so it can be mocked in tests.
- Exceptions from `RubyGraphCalculator.Core` are caught in `RubyGraphCalculator.UI` and logged.

## Folder structure
- Folders by MVVM role: `Views/`, `ViewModels/`, `Commands/`, `Behaviors/`.
  New code goes into the matching folder; ask before creating a new top-level folder
- Namespace matches the folder: `RubyGraphCalculator.UI.Views`
- `App.xaml` stays in the project root; its `StartupUri` points to `Views/MainWindow.xaml`
