# Mouse Jiggly - API Reference

## Table of Contents
1. [Class Overview](#class-overview)
2. [MainWindow Class](#mainwindow-class)
3. [App Class](#app-class)
4. [Windows API Functions](#windows-api-functions)
5. [Constants](#constants)
6. [Structures](#structures)
7. [Usage Examples](#usage-examples)

---

## Class Overview

### Namespace: `MouseJiggly`

| Class | Type | Description |
|-------|------|-------------|
| `App` | Application | WPF application entry point and lifecycle manager |
| `MainWindow` | Window | Main application window with UI and business logic |
| `POINT` | Struct | Represents a point in 2D coordinate space |

---

## MainWindow Class

### Full Signature
```csharp
public partial class MainWindow : System.Windows.Window
```

### Inheritance Hierarchy
```
System.Object
  └─ System.Windows.Threading.DispatcherObject
      └─ System.Windows.DependencyObject
          └─ System.Windows.Media.Visual
              └─ System.Windows.UIElement
                  └─ System.Windows.FrameworkElement
                      └─ System.Windows.Controls.Control
                          └─ System.Windows.Controls.ContentControl
                              └─ System.Windows.Window
                                  └─ MouseJiggly.MainWindow
```

---

### Fields

#### `jiggleTimer`
```csharp
private DispatcherTimer? jiggleTimer
```
**Description**: Timer that controls the jiggle frequency. Fires at intervals specified by the user (1-99 seconds).

**Type**: `System.Windows.Threading.DispatcherTimer` (nullable)

**Initialization**: Created in `InitializeTimer()` method

**Lifecycle**:
- Created once during constructor execution
- Started when any mode is enabled
- Stopped when all modes are disabled
- Interval updated when slider changes

---

#### `moveRight`
```csharp
private bool moveRight = true
```
**Description**: Direction flag for alternating mouse movement. True = move right/positive direction, False = move left/negative direction.

**Type**: `bool`

**Default Value**: `true`

**Behavior**:
- Toggles on each jiggle cycle
- Ensures cursor returns to original position
- Used in both Standard and Zen modes

**State Transitions**:
```
true → Jiggle Right (+1) → Set to false
false → Jiggle Left (-1) → Set to true
```

---

#### `jiggleInterval`
```csharp
private int jiggleInterval = 4
```
**Description**: Current jiggle interval in seconds (1-99). Default value is 4 seconds.

**Type**: `int`

**Valid Range**: 1 to 99 (inclusive)

**Default Value**: 4 seconds

**Constraints**:
- Enforced by XAML Slider (Minimum="1", Maximum="99")
- Slider has `IsSnapToTickEnabled="True"` ensuring integer values

**Updated By**: `IntervalSlider_ValueChanged` event handler

---

#### `isJiggleEnabled`
```csharp
private bool isJiggleEnabled = false
```
**Description**: Flag indicating whether Standard Jiggle mode is currently active.

**Type**: `bool`

**Default Value**: `false`

**Behavior**:
- When `true`: Cursor physically moves 1 pixel back and forth
- Cannot be `true` simultaneously with `isZenModeEnabled`
- Controlled by "Jiggle?" checkbox

**State Management**:
```csharp
// Mutual exclusivity enforced:
if (isJiggleEnabled && isZenModeEnabled)
{
    // One will be set to false
}
```

---

#### `isZenModeEnabled`
```csharp
private bool isZenModeEnabled = false
```
**Description**: Flag indicating whether Zen Mode is currently active.

**Type**: `bool`

**Default Value**: `false`

**Behavior**:
- When `true`: Mouse movement simulated without visible cursor movement
- Cannot be `true` simultaneously with `isJiggleEnabled`
- Controlled by "Zen Mode?" checkbox

---

### Constructor

#### `MainWindow()`
```csharp
public MainWindow()
```
**Description**: Initializes a new instance of the MainWindow class. Sets up UI components and initializes the jiggle timer.

**Parameters**: None

**Returns**: N/A (Constructor)

**Exceptions**: None thrown directly

**Call Sequence**:
1. Base `Window` constructor called
2. `InitializeComponent()` - Loads XAML, creates controls
3. `InitializeTimer()` - Creates and configures timer

**Example**:
```csharp
// Called automatically by WPF framework via App.xaml StartupUri
var window = new MainWindow();
window.Show();
```

---

### Methods

#### `InitializeTimer()`
```csharp
private void InitializeTimer()
```
**Description**: Initializes the jiggle timer with default settings. The timer is created but not started.

**Access Modifier**: `private`

**Parameters**: None

**Returns**: `void`

**Side Effects**:
- Creates `DispatcherTimer` instance
- Sets interval to `jiggleInterval` seconds (default 4)
- Subscribes `JiggleTimer_Tick` to timer's Tick event

**Preconditions**: None

**Postconditions**:
- `jiggleTimer` is not null
- Timer is configured but not running

**Implementation**:
```csharp
jiggleTimer = new DispatcherTimer();
jiggleTimer.Interval = TimeSpan.FromSeconds(jiggleInterval);
jiggleTimer.Tick += JiggleTimer_Tick;
```

**Thread Safety**: Called on UI thread during construction (safe)

---

#### `JiggleTimer_Tick(object?, EventArgs)`
```csharp
private void JiggleTimer_Tick(object? sender, EventArgs e)
```
**Description**: Event handler called on each timer tick. Determines which jiggle mode is active and performs the appropriate action.

**Access Modifier**: `private`

**Parameters**:
- `sender` (`object?`): The timer that raised the event (typically `jiggleTimer`)
- `e` (`EventArgs`): Event arguments (not used)

**Returns**: `void`

**Behavior**:
```
If isZenModeEnabled == true:
    Call PerformZenJiggle()
Else If isJiggleEnabled == true:
    Call PerformStandardJiggle()
Else:
    Do nothing (shouldn't occur as timer should be stopped)
```

**Execution Frequency**: Every `jiggleInterval` seconds while timer is running

**Execution Duration**: < 2ms typical

**Example Timing**:
```
T = 0s:    Timer starts
T = 4s:    JiggleTimer_Tick fires
T = 8s:    JiggleTimer_Tick fires
T = 12s:   JiggleTimer_Tick fires
...
```

---

#### `PerformStandardJiggle()`
```csharp
private void PerformStandardJiggle()
```
**Description**: Performs a standard jiggle operation - moves the cursor 1 pixel and back. This movement is visible to the user.

**Access Modifier**: `private`

**Parameters**: None

**Returns**: `void`

**Algorithm**:
```
1. GetCursorPos(out POINT currentPos)
2. IF moveRight == true:
       SetCursorPos(currentPos.X + 1, currentPos.Y)
       SET moveRight = false
   ELSE:
       SetCursorPos(currentPos.X - 1, currentPos.Y)
       SET moveRight = true
3. UpdateStatus("Standard jiggle active (every {interval}s)")
```

**Side Effects**:
- Moves physical cursor position by 1 pixel
- Toggles `moveRight` flag
- Updates status label text

**Windows API Calls**:
- `GetCursorPos()`: 1 call
- `SetCursorPos()`: 1 call

**Execution Time**: < 1ms typical

**Visual Impact**: Minimal (1-pixel movement barely visible)

**Example Execution Sequence**:
```
Cycle 1:
  - Current position: (1000, 500)
  - moveRight = true
  - Action: SetCursorPos(1001, 500)
  - After: moveRight = false

Cycle 2:
  - Current position: (1001, 500)
  - moveRight = false
  - Action: SetCursorPos(1000, 500)
  - After: moveRight = true
  
Result: Cursor back at original position
```

---

#### `PerformZenJiggle()`
```csharp
private void PerformZenJiggle()
```
**Description**: Performs a zen mode jiggle - simulates mouse activity without visible cursor movement.

**Access Modifier**: `private`

**Parameters**: None

**Returns**: `void`

**Algorithm**:
```
1. IF moveRight == true:
       mouse_event(MOUSEEVENTF_MOVE, 1, 0, 0, 0)   // Move +1
       mouse_event(MOUSEEVENTF_MOVE, -1, 0, 0, 0)  // Move -1
       SET moveRight = false
   ELSE:
       mouse_event(MOUSEEVENTF_MOVE, -1, 0, 0, 0)  // Move -1
       mouse_event(MOUSEEVENTF_MOVE, 1, 0, 0, 0)   // Move +1
       SET moveRight = true
2. UpdateStatus("Zen mode active (every {interval}s) - cursor invisible")
```

**Side Effects**:
- Sends mouse movement events to Windows
- Toggles `moveRight` flag
- Updates status label text
- **Does NOT** change visible cursor position (net movement = 0)

**Windows API Calls**:
- `mouse_event()`: 2 calls

**Execution Time**: < 1ms typical

**Visual Impact**: None (cursor appears stationary)

**Technical Explanation**:
The two `mouse_event` calls with opposite movements cancel spatially:
```
Call 1: mouse_event(MOVE, +1, 0) → System: "Mouse moved right"
Call 2: mouse_event(MOVE, -1, 0) → System: "Mouse moved left"
Net Position Change: +1 + (-1) = 0
Input Events Recorded: 2
```

Windows sees 2 input events, resets idle timer, but cursor stays in place.

---

#### `JiggleCheckBox_Changed(object, RoutedEventArgs)`
```csharp
private void JiggleCheckBox_Changed(object sender, RoutedEventArgs e)
```
**Description**: Event handler for Standard Jiggle checkbox state changes. Enables or disables standard mode and ensures mutual exclusivity with Zen mode.

**Access Modifier**: `private`

**Parameters**:
- `sender` (`object`): The checkbox control that raised the event
- `e` (`RoutedEventArgs`): Routed event arguments

**Returns**: `void`

**Triggered By**:
- User checking "Jiggle?" checkbox
- User unchecking "Jiggle?" checkbox
- Programmatic change to `JiggleCheckBox.IsChecked`

**Behavior**:
```csharp
isJiggleEnabled = (JiggleCheckBox.IsChecked == true);

// Enforce mutual exclusivity
if (isJiggleEnabled && isZenModeEnabled)
{
    ZenModeCheckBox.IsChecked = false;
    isZenModeEnabled = false;
}

UpdateTimerState();
```

**State Transitions**:
| Initial State | Action | Final State | Side Effects |
|--------------|--------|-------------|--------------|
| Both off | Check Jiggle | Jiggle on | Timer starts |
| Jiggle on | Uncheck Jiggle | Both off | Timer stops |
| Zen on | Check Jiggle | Jiggle on | Zen unchecked |

---

#### `ZenModeCheckBox_Changed(object, RoutedEventArgs)`
```csharp
private void ZenModeCheckBox_Changed(object sender, RoutedEventArgs e)
```
**Description**: Event handler for Zen Mode checkbox state changes. Enables or disables zen mode and ensures mutual exclusivity with Standard mode.

**Access Modifier**: `private`

**Parameters**:
- `sender` (`object`): The checkbox control that raised the event
- `e` (`RoutedEventArgs`): Routed event arguments

**Returns**: `void`

**Behavior**: Identical to `JiggleCheckBox_Changed` but for Zen mode

---

#### `IntervalSlider_ValueChanged(object, RoutedPropertyChangedEventArgs<double>)`
```csharp
private void IntervalSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
```
**Description**: Event handler for interval slider value changes. Updates the jiggle frequency and modifies the running timer if active.

**Access Modifier**: `private`

**Parameters**:
- `sender` (`object`): The slider control that raised the event
- `e` (`RoutedPropertyChangedEventArgs<double>`): Event args containing old and new values

**Returns**: `void`

**Properties of `e`**:
- `e.OldValue` (`double`): Previous slider value
- `e.NewValue` (`double`): New slider value

**Behavior**:
```csharp
jiggleInterval = (int)IntervalSlider.Value;
IntervalLabel.Text = $"{jiggleInterval}s";

if (jiggleTimer != null)
{
    jiggleTimer.Interval = TimeSpan.FromSeconds(jiggleInterval);
}

UpdateStatus();
```

**Update Timing**:
- Changes take effect immediately
- Current timer cycle completes with old interval
- Next cycle uses new interval

**Example**:
```
T = 0s:  Timer starts with 4s interval
T = 2s:  User changes slider to 10s
T = 4s:  Timer fires (completes 4s cycle)
T = 14s: Timer fires (first 10s cycle)
```

---

#### `UpdateTimerState()`
```csharp
private void UpdateTimerState()
```
**Description**: Updates the timer's running state based on current mode settings. Starts the timer if any mode is enabled, stops it if all modes are disabled.

**Access Modifier**: `private`

**Parameters**: None

**Returns**: `void`

**Algorithm**:
```csharp
if (jiggleTimer == null) return;  // Safety check

if (isJiggleEnabled || isZenModeEnabled)
{
    jiggleTimer.Start();
    UpdateStatus();
}
else
{
    jiggleTimer.Stop();
    UpdateStatus("Status: Inactive");
}
```

**Called By**:
- `JiggleCheckBox_Changed()`
- `ZenModeCheckBox_Changed()`

**Ensures**:
- Timer running ↔ At least one mode enabled
- Timer stopped ↔ All modes disabled

---

#### `UpdateStatus(string?)`
```csharp
private void UpdateStatus(string? customMessage = null)
```
**Description**: Updates the status label with current operational state. Can accept a custom message or automatically generate one based on active mode.

**Access Modifier**: `private`

**Parameters**:
- `customMessage` (`string?`, optional): Custom status message. If provided, this exact message is displayed. If null, status is automatically determined.

**Returns**: `void`

**Behavior**:
```csharp
if (customMessage != null)
{
    StatusLabel.Text = customMessage;
    return;
}

// Auto-determine status
if (isJiggleEnabled)
    StatusLabel.Text = $"Status: Standard jiggle active (every {jiggleInterval}s)";
else if (isZenModeEnabled)
    StatusLabel.Text = $"Status: Zen mode active (every {jiggleInterval}s)";
else
    StatusLabel.Text = "Status: Inactive";
```

**Message Examples**:
- `"Status: Inactive"`
- `"Status: Standard jiggle active (every 4s)"`
- `"Status: Zen mode active (every 10s)"`
- `"Zen mode active (every 5s) - cursor invisible"` (custom during jiggle)

**UI Update**: Directly modifies `StatusLabel.Text` (safe on UI thread)

---

## App Class

### Full Signature
```csharp
public partial class App : System.Windows.Application
```

### Description
Application entry point and lifecycle manager. Provides application-wide services and event handling.

### Constructor

#### `App()`
```csharp
public App()
```
**Description**: Initializes a new instance of the App class.

**Parameters**: None

**Returns**: N/A

**Behavior**: Default constructor, framework-generated via partial class.

---

## Windows API Functions

### `SetCursorPos`
```csharp
[DllImport("user32.dll")]
private static extern bool SetCursorPos(int X, int Y);
```

**Description**: Sets the cursor position to specified screen coordinates.

**DLL**: `user32.dll`

**Parameters**:
- `X` (`int`): New x-coordinate of cursor (screen pixels, 0 = leftmost)
- `Y` (`int`): New y-coordinate of cursor (screen pixels, 0 = topmost)

**Returns**: `bool`
- `true`: Success
- `false`: Failure (rare, check `Marshal.GetLastWin32Error()`)

**Coordinate System**:
- Origin (0, 0): Top-left of primary monitor
- Multi-monitor: Can extend to negative values (monitors left/above primary)
- Physical pixels (not affected by DPI scaling for API calls)

**Usage in Code**:
```csharp
GetCursorPos(out POINT pos);
SetCursorPos(pos.X + 1, pos.Y);  // Move right 1 pixel
```

**Thread Safety**: Safe from any thread

**Permissions**: No special permissions required (user-level API)

---

### `GetCursorPos`
```csharp
[DllImport("user32.dll")]
private static extern bool GetCursorPos(out POINT lpPoint);
```

**Description**: Retrieves the current cursor position in screen coordinates.

**DLL**: `user32.dll`

**Parameters**:
- `lpPoint` (`out POINT`): Output parameter that receives cursor position

**Returns**: `bool`
- `true`: Success
- `false`: Failure (extremely rare)

**Output Structure**: Populates `POINT` struct with `X` and `Y` fields

**Usage in Code**:
```csharp
GetCursorPos(out POINT currentPosition);
Console.WriteLine($"Cursor at ({currentPosition.X}, {currentPosition.Y})");
```

**Performance**: < 0.01ms typical

---

### `mouse_event`
```csharp
[DllImport("user32.dll")]
private static extern void mouse_event(uint dwFlags, int dx, int dy, uint dwData, UIntPtr dwExtraInfo);
```

**Description**: Synthesizes mouse motion and button clicks.

**DLL**: `user32.dll`

**Parameters**:
- `dwFlags` (`uint`): Controls various aspects of mouse motion and button clicking (use `MOUSEEVENTF_MOVE` constant)
- `dx` (`int`): Relative horizontal movement (negative = left, positive = right)
- `dy` (`int`): Relative vertical movement (negative = up, positive = down)
- `dwData` (`uint`): Additional data (used for wheel movement, 0 for move)
- `dwExtraInfo` (`UIntPtr`): Additional value associated with event (usually 0)

**Returns**: `void` (no return value)

**Flags** (we only use one):
- `MOUSEEVENTF_MOVE (0x0001)`: Movement occurred

**Usage in Code**:
```csharp
mouse_event(MOUSEEVENTF_MOVE, 1, 0, 0, UIntPtr.Zero);   // Move right
mouse_event(MOUSEEVENTF_MOVE, -1, 0, 0, UIntPtr.Zero);  // Move left
```

**Coordinate Mode**: Relative (dx/dy are deltas, not absolute positions)

**Important Notes**:
- Legacy API (newer code should use `SendInput`, but `mouse_event` simpler for our needs)
- Movements are relative to current cursor position
- Multiple calls stack (but can be made to cancel as in Zen Mode)

---

## Constants

### `MOUSEEVENTF_MOVE`
```csharp
private const uint MOUSEEVENTF_MOVE = 0x0001;
```

**Description**: Flag for `mouse_event` function indicating a mouse move event.

**Type**: `uint`

**Value**: `0x0001` (hexadecimal) = `1` (decimal)

**Usage**: First parameter to `mouse_event` when synthesizing cursor movement

**Other Available Flags** (not used in our code):
- `MOUSEEVENTF_LEFTDOWN = 0x0002`: Left button down
- `MOUSEEVENTF_LEFTUP = 0x0004`: Left button up
- `MOUSEEVENTF_RIGHTDOWN = 0x0008`: Right button down
- `MOUSEEVENTF_RIGHTUP = 0x0010`: Right button up
- `MOUSEEVENTF_WHEEL = 0x0800`: Wheel movement

---

## Structures

### `POINT`
```csharp
[StructLayout(LayoutKind.Sequential)]
public struct POINT
{
    public int X;
    public int Y;
}
```

**Description**: Represents a point in 2D coordinate space. Used with `GetCursorPos` to retrieve cursor position.

**Access Modifier**: `public`

**Attributes**: 
- `[StructLayout(LayoutKind.Sequential)]`: Ensures fields laid out in memory in declaration order (required for P/Invoke)

**Fields**:

#### `X`
**Type**: `int`  
**Description**: The x-coordinate of the point (horizontal position)  
**Range**: -32768 to 32767 (typically, depending on monitor configuration)

#### `Y`
**Type**: `int`  
**Description**: The y-coordinate of the point (vertical position)  
**Range**: -32768 to 32767 (typically, depending on monitor configuration)

**Usage Example**:
```csharp
POINT cursorPos;
GetCursorPos(out cursorPos);
Console.WriteLine($"Cursor X: {cursorPos.X}, Y: {cursorPos.Y}");
```

**Memory Layout**:
```
Offset 0-3: X (4 bytes, int)
Offset 4-7: Y (4 bytes, int)
Total: 8 bytes
```

---

## Usage Examples

### Example 1: Enabling Standard Mode Programmatically
```csharp
// In MainWindow.xaml.cs
public void EnableStandardMode()
{
    JiggleCheckBox.IsChecked = true;
    // Event handler automatically called, timer starts
}
```

### Example 2: Changing Interval Programmatically
```csharp
// Set jiggle interval to 10 seconds
IntervalSlider.Value = 10;
// ValueChanged event fires, updates timer and label
```

### Example 3: Checking Current State
```csharp
public bool IsAnyModeActive()
{
    return isJiggleEnabled || isZenModeEnabled;
}

public string GetCurrentMode()
{
    if (isJiggleEnabled) return "Standard";
    if (isZenModeEnabled) return "Zen";
    return "Inactive";
}
```

### Example 4: Manual Jiggle (One-Time)
```csharp
// Perform single jiggle without starting timer
public void JiggleOnce(bool useZenMode = false)
{
    if (useZenMode)
        PerformZenJiggle();
    else
        PerformStandardJiggle();
}
```

### Example 5: Disabling All Modes
```csharp
public void DisableAllModes()
{
    JiggleCheckBox.IsChecked = false;
    ZenModeCheckBox.IsChecked = false;
    // Timer automatically stops via UpdateTimerState()
}
```

### Example 6: Custom Status Message
```csharp
// Show temporary custom message
UpdateStatus("Jiggling paused - user active");

// Restore to automatic status
UpdateStatus();  // null parameter = auto-determine
```

### Example 7: Checking Cursor Position
```csharp
public void LogCursorPosition()
{
    GetCursorPos(out POINT pos);
    Console.WriteLine($"Current cursor position: ({pos.X}, {pos.Y})");
}
```

---

## API Versioning

**Current Version**: 1.0  
**API Stability**: Stable (all public members)  
**Breaking Changes**: None planned  

**Future Considerations**:
- Might add public methods for external control (if turned into library)
- Could expose events for jiggle occurrences
- Potential for configuration API

---

*Last Updated: November 9, 2025*
*API Version: 1.0*
