# RubyGraphCalculator — UI specification

The simulator teaches how a signal turns into a spectrum. The user builds a signal as the sum
of up to four sine waves and sees two charts at once: the sum on top, its spectrum below.

Mockup: https://claude.ai/artifact/Rdu6ssuEniE5ZzAXbFsKjo (two artboards: main window, first launch).

The UI language is English.

## 1. Window layout

- Left: a parameter panel of constant width (about 340 px). Right: two charts, one above the other.
  Bottom: a status bar.
- Minimum window size 1024 × 768; the window can be resized. When it grows, the charts grow;
  the panel keeps its width. If the signal cards do not fit, the panel scrolls; the charts stay in place.
- Top chart: "Sum of signals". Bottom chart: "Spectrum".

## 2. Signals

- From 0 to 4 sine waves. Each has a frequency, an amplitude and a phase.
- Sample rate and duration are common to all signals (sums need the same time grid).
- Ranges are the Core ranges (see `RubyGraphCalculator.Core/CLAUDE.md`).
  Default sample rate: 10 000 Hz.
- On first launch (no saved session) there are no signals and both charts are empty.
- "+ Add signal" adds a card; it is disabled when there are 4 signals.
  Each card has a remove button (✕) in its header.
- Cards are numbered "Signal 1 … 4". After a removal the numbers shift, so there are no gaps.

### Signal card

- Fields: Frequency (Hz), Amplitude, Phase, each with its unit next to it.
- Phase unit: two radio buttons "°" and "rad" next to the phase field; degrees by default.
- A validation message is shown in red under the field it belongs to.

## 3. Phase display

- The phase field shows the value exactly as the user entered it (for example `450`).
- A hint under the field shows the normalized angle (`= 90.000°`), only when it differs from the entered value.
- The hint uses 3 decimal places (degrees or radians, matching the selected unit).
- Switching "° ↔ rad" converts the entered value to the other unit without normalizing it:
  `450° → 7.854 rad`, hint `= 1.571 rad`.
- Switching the unit does not start a recalculation: the angle itself does not change.

## 4. Amplitude of the sum

- The sum is not limited or normalized: four signals of amplitude 5 can reach 20.
  Normalizing would break the rule "a sine with amplitude A gives a spectrum peak of height A".
- The top chart's Y axis is fixed at ±(sum of the amplitudes), so it does not jump when a phase changes.

## 5. Charts

### Sum of signals (top)

- Shows only the sum, not the individual components.
- Initial view: the first 5 periods of the lowest frequency. If the signal is shorter, the whole signal.
- Axis labels: "Time, s" and the value axis.

### Spectrum (bottom)

- X axis: 0 to sample rate / 2 (Nyquist frequency), in Hz, linear.
- Y axis: 0 to the largest amplitude with a small margin.
- Drawn as vertical lines from the axis to each value (stem plot), so individual spectrum points
  and spectral leakage are visible.
- Peak labels "<frequency> Hz · <amplitude>": at most 4 peaks, only peaks above 10 % of the largest one.

### Both charts

- Zoom and pan with the mouse; a "Reset zoom" button for each chart.
- Signals can have up to 3.84 million samples (20 s at 192 kHz): use a ScottPlot plot type made
  for large evenly spaced data (Signal), not Scatter.

## 6. Nyquist warning

- If a signal's frequency exceeds sample rate / 2, a warning is shown in the common parameters block.
  It names the signal and the frequency, for example:
  "Signal 3: 6000 Hz is above the Nyquist frequency (5000 Hz). In the spectrum it appears at 4000 Hz (aliasing)."
- The signal is still generated; the spectrum shows the alias.

## 7. When the charts update

- Automatically, when the cursor leaves a field whose value has changed.
- Also when a signal is added or removed.
- Leaving a field without changing it does not recalculate.
- After a session is loaded, the charts are built immediately.

## 8. Validation

- A value is validated when the cursor leaves the field.
- An invalid value shows a red message under the field, and the focus returns to the field.
- While a field is invalid, these actions still work: removing a signal, "Clear screen",
  closing the window.
- Closing the window with an invalid field: no save prompt; a message "The window will close"
  with OK / Cancel. Cancel keeps the window open.

## 9. Calculation

- Runs in the background; the window stays responsive.
- While it runs: all parameter fields are disabled, the status bar shows a progress indicator
  and a "Cancel calculation" button.
- After a cancel: both charts are cleared, the parameters stay as entered.
  A new calculation starts with the next change.
- The window cannot be closed during a calculation: closing shows "Cancel the calculation first".

## 10. Session

- Only the parameters are saved (sample rate, duration, and for each signal: frequency, amplitude,
  phase as entered, phase unit). Chart data is never saved.
- Format: JSON, with a format version number.
- Location: `%AppData%\RubyGraphCalculator\data` (never the program folder: it can be read-only,
  for example under `C:\Program Files`).
- Only one session is kept: the last saved one.
- On start: if a saved session exists, it is loaded and the charts are built; otherwise a clean screen.
- A damaged or unreadable file: show a message to the user, write a warning to the log, start with a clean screen.

### On closing

- Ask "Save the session?" with Yes / No / Cancel only if the current parameters differ from the saved ones.
- No saved session and an empty screen: no question.
- Yes: save. No: keep the previously saved session. Cancel: keep the window open.
- Saving an empty session deletes the saved file (the single "Yes" is enough).

### Buttons

- "Clear screen": removes all signals from the screen; the saved file is not touched.
- "Delete saved session": deletes the saved file after a confirmation
  ("Delete the saved session? This cannot be undone."); the screen is not touched.
- On first launch with no saved session and no signals, both buttons are disabled.

## 11. Status bar

- Shows the state (Ready / calculating with the Cancel button), the number of samples,
  the spectrum step, and whether the session differs from the saved one.

## Open questions

- ViewModel tests and `SignalGenerator`: `SignalGenerator` is a static class, so it cannot be mocked.
  Either add an `ISignalGenerator` interface in Core, or test the ViewModel with the real generator
  and change the rule in `RubyGraphCalculator.UI.Tests/CLAUDE.md`.
- Mockup states not drawn yet: validation error, calculation in progress, dialogs.
