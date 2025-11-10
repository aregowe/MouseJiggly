# Mouse Jiggly - Developer Guide

## Table of Contents
1. [Getting Started](#getting-started)
2. [Development Environment Setup](#development-environment-setup)
3. [Project Structure](#project-structure)
4. [Building and Testing](#building-and-testing)
5. [Debugging](#debugging)
6. [Code Style Guide](#code-style-guide)
7. [Common Development Tasks](#common-development-tasks)
8. [Troubleshooting Development Issues](#troubleshooting-development-issues)
9. [Contributing](#contributing)
10. [Release Process](#release-process)

---

## Getting Started

### Prerequisites

**Required**:
- Windows 10 (version 1809+) or Windows 11
- .NET 9.0 SDK or later
- Text editor or IDE

**Recommended**:
- Visual Studio 2022 (Community Edition or higher)
- Git for version control
- Windows Terminal for better command-line experience

### Quick Start

```powershell
# Clone the repository
git clone https://github.com/yourusername/MouseJiggly.git
cd MouseJiggly

# Restore dependencies (automatic with build, but can be explicit)
dotnet restore

# Build the project
dotnet build

# Run the application
dotnet run

# Or run directly
.\bin\Debug\net9.0-windows\MouseJiggly.exe
```

---

## Development Environment Setup

### Option 1: Visual Studio 2022

**Installation**:
1. Download Visual Studio 2022 Community from https://visualstudio.microsoft.com/
2. During installation, select:
   - **Workload**: ".NET desktop development"
   - **Individual components**: 
     - .NET 9.0 Runtime
     - WPF Designer
     - NuGet package manager

**Opening the Project**:
1. Launch Visual Studio
2. File → Open → Project/Solution
3. Navigate to `MouseJiggly.csproj`
4. Click Open

**Running**:
- Press `F5` to start with debugging
- Press `Ctrl+F5` to start without debugging

**Benefits**:
- IntelliSense code completion
- Visual WPF designer
- Integrated debugger
- NuGet package management UI
- Error highlighting

### Option 2: Visual Studio Code

**Installation**:
1. Download VS Code from https://code.visualstudio.com/
2. Install extensions:
   - C# (Microsoft)
   - C# Dev Kit (Microsoft)
   - .NET Install Tool (Microsoft)

**Opening the Project**:
```powershell
cd MouseJiggly
code .
```

**Running**:
```powershell
# In integrated terminal
dotnet run
```

**Benefits**:
- Lightweight
- Cross-platform
- Good extension ecosystem
- Integrated terminal

### Option 3: Command Line Only

**Requirements**:
- .NET 9.0 SDK
- Any text editor (Notepad++, Sublime Text, etc.)

**Workflow**:
```powershell
# Edit files in your favorite editor
notepad MainWindow.xaml.cs

# Build from command line
dotnet build

# Run
.\bin\Debug\net9.0-windows\MouseJiggly.exe
```

---

## Project Structure

```
MouseJiggly/
│
├── MouseJiggly.csproj          # Project file (MSBuild configuration)
│   ├── Defines target framework (net9.0-windows)
│   ├── Sets output type (WinExe)
│   ├── Enables WPF (UseWPF = true)
│   └── Specifies application icon
│
├── App.xaml                    # Application definition (XAML)
│   ├── Declares App class
│   ├── Sets startup window (StartupUri)
│   └── Application-wide resources
│
├── App.xaml.cs                 # Application code-behind
│   └── App class implementation (partial)
│
├── MainWindow.xaml             # Main window UI definition (XAML)
│   ├── Window properties (size, title, etc.)
│   ├── Grid layout structure
│   ├── Control declarations (CheckBoxes, Slider, etc.)
│   └── Event bindings
│
├── MainWindow.xaml.cs          # Main window code-behind
│   ├── MainWindow class (partial)
│   ├── Fields (timer, state flags, etc.)
│   ├── Windows API declarations (P/Invoke)
│   ├── Constructor and initialization
│   ├── Event handlers
│   ├── Jiggle operation methods
│   └── Helper methods
│
├── README.md                   # User documentation
├── ARCHITECTURE.md             # Technical architecture docs
├── API_REFERENCE.md            # API documentation
├── DEVELOPER_GUIDE.md          # This file
│
├── bin/                        # Build output (gitignored)
│   ├── Debug/
│   │   └── net9.0-windows/
│   │       └── MouseJiggly.exe
│   └── Release/
│       └── net9.0-windows/
│           └── MouseJiggly.exe
│
└── obj/                        # Intermediate build files (gitignored)
    └── ...
```

### File Relationships

```
App.xaml ←→ App.xaml.cs
    (partial class, linked by x:Class attribute)
    
MainWindow.xaml ←→ MainWindow.xaml.cs
    (partial class, linked by x:Class attribute)
    
App.xaml → MainWindow.xaml
    (StartupUri="MainWindow.xaml" creates instance)
    
MouseJiggly.csproj → All .cs and .xaml files
    (includes all source files in compilation)
```

---

## Building and Testing

### Build Configurations

#### Debug Build
```powershell
dotnet build -c Debug
```
**Characteristics**:
- No code optimization
- Debug symbols included (.pdb files)
- Assertions enabled
- Larger file size
- Faster compilation

**Output**: `bin\Debug\net9.0-windows\MouseJiggly.exe`

**Use For**: Development, debugging, testing

#### Release Build
```powershell
dotnet build -c Release
```
**Characteristics**:
- Code optimization enabled
- No debug symbols (unless explicitly included)
- Smaller file size
- Slower compilation
- Better performance

**Output**: `bin\Release\net9.0-windows\MouseJiggly.exe`

**Use For**: Distribution, performance testing

### Build Options

**Clean Build** (removes previous build artifacts):
```powershell
dotnet clean
dotnet build
```

**Rebuild** (clean + build in one command):
```powershell
dotnet build --no-incremental
```

**Verbose Build Output**:
```powershell
dotnet build -v detailed
```
Verbosity levels: `q[uiet]`, `m[inimal]`, `n[ormal]`, `d[etailed]`, `diag[nostic]`

### Testing

**Manual Testing Checklist**:
- [ ] Standard Mode enables/disables correctly
- [ ] Zen Mode enables/disables correctly
- [ ] Only one mode can be active at a time
- [ ] Slider changes interval (check label updates)
- [ ] Status displays correct messages
- [ ] Timer starts when mode enabled
- [ ] Timer stops when all modes disabled
- [ ] Standard Mode: Cursor visibly moves 1 pixel
- [ ] Zen Mode: Cursor appears stationary
- [ ] Computer doesn't sleep during active jiggle
- [ ] No crashes or exceptions
- [ ] Window can be minimized/restored
- [ ] Application closes cleanly

**Automated Testing** (not currently implemented):
```csharp
// Future: Unit tests with xUnit or NUnit
[Fact]
public void JiggleMode_MutuallyExclusive()
{
    var window = new MainWindow();
    window.JiggleCheckBox.IsChecked = true;
    window.ZenModeCheckBox.IsChecked = true;
    
    // One should be automatically unchecked
    Assert.False(window.JiggleCheckBox.IsChecked == true && 
                 window.ZenModeCheckBox.IsChecked == true);
}
```

### Performance Testing

**Memory Usage**:
```powershell
# Run app, then check Task Manager or:
Get-Process MouseJiggly | Select-Object Name, WS
```
Expected: < 10 MB working set

**CPU Usage**:
```powershell
# Monitor for 60 seconds with jiggle active
Get-Counter '\Process(MouseJiggly)\% Processor Time' -MaxSamples 60
```
Expected: < 1% average

---

## Debugging

### Visual Studio Debugging

**Starting Debug Session**:
- Press `F5` or Debug → Start Debugging
- Application launches with debugger attached

**Breakpoints**:
1. Click in left margin of code editor (or press `F9`)
2. Red dot appears
3. Execution pauses when line is hit

**Useful Breakpoints**:
- `JiggleTimer_Tick` - See each jiggle execution
- `PerformStandardJiggle` / `PerformZenJiggle` - Inspect cursor operations
- Event handlers - Watch user interactions

**Debugging Windows**:
- **Locals** (Debug → Windows → Locals): View variables in current scope
- **Watch** (Debug → Windows → Watch): Monitor specific expressions
- **Call Stack** (Debug → Windows → Call Stack): See execution path
- **Immediate Window** (Debug → Windows → Immediate): Execute code at breakpoint

**Step Commands**:
- `F10`: Step Over (execute current line, don't enter methods)
- `F11`: Step Into (enter method calls)
- `Shift+F11`: Step Out (exit current method)
- `F5`: Continue (run to next breakpoint)

### Command-Line Debugging

**Attach to Running Process**:
```powershell
# Start app normally
.\MouseJiggly.exe

# In another terminal, find process ID
Get-Process MouseJiggly

# Attach debugger (Visual Studio)
# Debug → Attach to Process → Select MouseJiggly.exe
```

**Debug Output**:
Add to code:
```csharp
System.Diagnostics.Debug.WriteLine($"Jiggle executed at {DateTime.Now}");
```

View in Visual Studio Output window during debugging.

### Common Debugging Scenarios

#### Issue: Timer Not Firing
**Check**:
1. Set breakpoint in `UpdateTimerState()`
2. Verify `jiggleTimer.Start()` is called
3. Set breakpoint in `JiggleTimer_Tick`
4. Confirm it fires after expected interval

#### Issue: Cursor Not Moving
**Check**:
1. Breakpoint in `PerformStandardJiggle()`
2. Inspect `currentPos` after `GetCursorPos`
3. Step through `SetCursorPos` call
4. Check return value (should be true)

#### Issue: Modes Not Mutually Exclusive
**Check**:
1. Breakpoint in checkbox event handlers
2. Verify `isJiggleEnabled` and `isZenModeEnabled` flags
3. Confirm mutual exclusion logic executes

---

## Code Style Guide

### General Principles
- **Clarity over cleverness**: Code should be easy to understand
- **Consistency**: Follow existing patterns in the codebase
- **Documentation**: Complex logic should have explanatory comments
- **Simplicity**: Avoid over-engineering for this small app

### Naming Conventions

**Classes**: PascalCase
```csharp
public class MainWindow { }
```

**Methods**: PascalCase
```csharp
private void PerformStandardJiggle() { }
```

**Private Fields**: camelCase
```csharp
private DispatcherTimer jiggleTimer;
private bool isJiggleEnabled;
```

**Constants**: SCREAMING_SNAKE_CASE or PascalCase
```csharp
private const uint MOUSEEVENTF_MOVE = 0x0001;
```

**Properties**: PascalCase
```csharp
public int JiggleInterval { get; set; }
```

**Event Handlers**: ControlName_EventName
```csharp
private void JiggleCheckBox_Changed(object sender, RoutedEventArgs e)
```

**XAML Controls**: PascalCase with descriptive names
```xml
<CheckBox x:Name="JiggleCheckBox" />
<Slider x:Name="IntervalSlider" />
```

### Code Formatting

**Indentation**: 4 spaces (not tabs)

**Braces**: Opening brace on same line for methods/classes, own line for control structures
```csharp
// Classes and methods
public class MyClass
{
    public void MyMethod()
    {
        // code
    }
}

// Control structures
if (condition)
{
    // code
}
else
{
    // code
}
```

**Spacing**:
```csharp
// Space after keywords
if (condition)
while (condition)
for (int i = 0; i < 10; i++)

// Space around operators
int result = a + b * c;
bool isValid = x > 0 && y < 10;

// No space before semicolon or comma
DoSomething(param1, param2);
```

### Documentation

**XML Documentation for Public Methods**:
```csharp
/// <summary>
/// Performs a standard jiggle operation.
/// </summary>
/// <remarks>
/// This method moves the cursor 1 pixel and back.
/// </remarks>
private void PerformStandardJiggle()
{
    // Implementation
}
```

**Inline Comments**:
```csharp
// Single-line comments for brief explanations
int offset = 1; // Pixel offset for jiggle

/* Multi-line comments for longer explanations
   that span multiple lines */
```

**Comment Style**:
- Explain "why", not "what" (code shows what)
- Update comments when code changes
- Remove commented-out code (use version control instead)

### Example: Well-Formatted Method

```csharp
/// <summary>
/// Updates the timer's running state based on current mode settings.
/// </summary>
/// <remarks>
/// Starts the timer if any mode is enabled, stops it if all modes are disabled.
/// This ensures consistent timer management across all checkbox state changes.
/// </remarks>
private void UpdateTimerState()
{
    // Safety check: ensure timer exists
    if (jiggleTimer == null)
    {
        return;
    }

    // Start timer if any mode is active
    if (isJiggleEnabled || isZenModeEnabled)
    {
        jiggleTimer.Start();
        UpdateStatus();
    }
    else
    {
        // Stop timer when no mode is active
        jiggleTimer.Stop();
        UpdateStatus("Status: Inactive");
    }
}
```

---

## Common Development Tasks

### Adding a New Jiggle Mode

**1. Add UI Control (MainWindow.xaml)**:
```xml
<CheckBox x:Name="CustomModeCheckBox" 
          Grid.Row="3" 
          Content="Custom Mode?" 
          FontSize="16" 
          Margin="0,0,0,15"
          Checked="CustomModeCheckBox_Changed" 
          Unchecked="CustomModeCheckBox_Changed"/>
```

**2. Add State Field (MainWindow.xaml.cs)**:
```csharp
private bool isCustomModeEnabled = false;
```

**3. Add Event Handler**:
```csharp
private void CustomModeCheckBox_Changed(object sender, RoutedEventArgs e)
{
    isCustomModeEnabled = CustomModeCheckBox.IsChecked == true;
    
    // Enforce mutual exclusivity with other modes
    if (isCustomModeEnabled)
    {
        if (isJiggleEnabled)
        {
            JiggleCheckBox.IsChecked = false;
            isJiggleEnabled = false;
        }
        if (isZenModeEnabled)
        {
            ZenModeCheckBox.IsChecked = false;
            isZenModeEnabled = false;
        }
    }
    
    UpdateTimerState();
}
```

**4. Add Jiggle Method**:
```csharp
private void PerformCustomJiggle()
{
    // Your custom jiggle logic here
    // Example: Move cursor in a circle
    GetCursorPos(out POINT pos);
    
    // Calculate circle position
    double angle = DateTime.Now.Millisecond / 1000.0 * 2 * Math.PI;
    int offsetX = (int)(Math.Cos(angle) * 5);
    int offsetY = (int)(Math.Sin(angle) * 5);
    
    SetCursorPos(pos.X + offsetX, pos.Y + offsetY);
    
    UpdateStatus($"Custom mode active (every {jiggleInterval}s)");
}
```

**5. Update Timer Tick Handler**:
```csharp
private void JiggleTimer_Tick(object? sender, EventArgs e)
{
    if (isCustomModeEnabled)
    {
        PerformCustomJiggle();
    }
    else if (isZenModeEnabled)
    {
        PerformZenJiggle();
    }
    else if (isJiggleEnabled)
    {
        PerformStandardJiggle();
    }
}
```

**6. Update Timer State Check**:
```csharp
private void UpdateTimerState()
{
    if (jiggleTimer == null) return;

    if (isJiggleEnabled || isZenModeEnabled || isCustomModeEnabled)
    {
        jiggleTimer.Start();
        UpdateStatus();
    }
    else
    {
        jiggleTimer.Stop();
        UpdateStatus("Status: Inactive");
    }
}
```

### Adding a Settings System

**1. Create Settings Class**:
```csharp
using System.Text.Json;

public class AppSettings
{
    public string LastMode { get; set; } = "None";
    public int LastInterval { get; set; } = 4;
    public bool RememberState { get; set; } = true;
    
    private static string SettingsPath => 
        Path.Combine(Environment.GetFolderPath(
            Environment.SpecialFolder.ApplicationData), 
            "MouseJiggly", "settings.json");
    
    public static AppSettings Load()
    {
        try
        {
            if (File.Exists(SettingsPath))
            {
                string json = File.ReadAllText(SettingsPath);
                return JsonSerializer.Deserialize<AppSettings>(json) 
                       ?? new AppSettings();
            }
        }
        catch { }
        
        return new AppSettings();
    }
    
    public void Save()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(SettingsPath)!);
            string json = JsonSerializer.Serialize(this, 
                new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(SettingsPath, json);
        }
        catch { }
    }
}
```

**2. Load Settings on Startup (MainWindow constructor)**:
```csharp
public MainWindow()
{
    InitializeComponent();
    InitializeTimer();
    
    // Load saved settings
    var settings = AppSettings.Load();
    IntervalSlider.Value = settings.LastInterval;
    
    if (settings.RememberState)
    {
        if (settings.LastMode == "Standard")
            JiggleCheckBox.IsChecked = true;
        else if (settings.LastMode == "Zen")
            ZenModeCheckBox.IsChecked = true;
    }
}
```

**3. Save Settings on Close**:
```csharp
protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
{
    base.OnClosing(e);
    
    var settings = new AppSettings
    {
        LastMode = isJiggleEnabled ? "Standard" : 
                   isZenModeEnabled ? "Zen" : "None",
        LastInterval = jiggleInterval,
        RememberState = true
    };
    settings.Save();
}
```

### Adding Logging

**1. Add NuGet Package** (optional, or use built-in Debug/Trace):
```powershell
dotnet add package Serilog.Sinks.File
```

**2. Initialize Logger**:
```csharp
using Serilog;

// In App.xaml.cs
protected override void OnStartup(StartupEventArgs e)
{
    base.OnStartup(e);
    
    Log.Logger = new LoggerConfiguration()
        .WriteTo.File("logs/mousejiggly-.txt", rollingInterval: RollingInterval.Day)
        .CreateLogger();
    
    Log.Information("Mouse Jiggly started");
}

protected override void OnExit(ExitEventArgs e)
{
    Log.Information("Mouse Jiggly exiting");
    Log.CloseAndFlush();
    base.OnExit(e);
}
```

**3. Add Logging to Methods**:
```csharp
private void PerformStandardJiggle()
{
    Log.Debug("Performing standard jiggle");
    GetCursorPos(out POINT currentPos);
    Log.Debug($"Current position: ({currentPos.X}, {currentPos.Y})");
    
    // ... rest of method
}
```

---

## Troubleshooting Development Issues

### Issue: Build Errors

**Error**: `The type or namespace name 'Windows' does not exist`
**Solution**: Ensure `<TargetFramework>net9.0-windows</TargetFramework>` in .csproj

**Error**: `UseWPF must be set to true`
**Solution**: Add `<UseWPF>true</UseWPF>` to .csproj PropertyGroup

**Error**: `Cannot resolve symbol 'GetCursorPos'`
**Solution**: Check `[DllImport("user32.dll")]` attribute is present

### Issue: Runtime Errors

**Error**: `NullReferenceException` on `jiggleTimer`
**Solution**: Ensure `InitializeTimer()` is called in constructor

**Error**: XAML controls not found
**Solution**: Rebuild project (`dotnet clean` then `dotnet build`)

### Issue: Designer Issues

**Error**: WPF Designer not loading in Visual Studio
**Solution**: 
1. Close designer
2. Clean solution
3. Rebuild solution
4. Reopen XAML file

**Error**: "Could not load file or assembly" in designer
**Solution**: Check target framework is installed (net9.0-windows)

---

## Contributing

### Contribution Workflow

1. **Fork the repository** on GitHub
2. **Clone your fork**:
   ```powershell
   git clone https://github.com/YOUR-USERNAME/MouseJiggly.git
   ```
3. **Create feature branch**:
   ```powershell
   git checkout -b feature/my-new-feature
   ```
4. **Make changes** following code style guide
5. **Test thoroughly** (manual testing checklist)
6. **Commit with clear messages**:
   ```powershell
   git commit -m "Add circular jiggle pattern mode"
   ```
7. **Push to your fork**:
   ```powershell
   git push origin feature/my-new-feature
   ```
8. **Create Pull Request** on GitHub

### Commit Message Guidelines

**Format**:
```
<type>: <subject>

<body>

<footer>
```

**Types**:
- `feat`: New feature
- `fix`: Bug fix
- `docs`: Documentation changes
- `style`: Code style changes (formatting, no logic change)
- `refactor`: Code refactoring
- `test`: Adding tests
- `chore`: Build process, tooling changes

**Examples**:
```
feat: Add system tray minimize functionality

- Window minimizes to system tray instead of taskbar
- Right-click tray icon shows context menu
- Double-click tray icon restores window

Closes #42
```

```
fix: Prevent timer drift over long durations

Previously, timer could drift by several seconds over 24+ hours.
Now uses DateTime comparison to maintain accuracy.

Fixes #56
```

---

## Release Process

### Version Numbering

**Format**: MAJOR.MINOR.PATCH (Semantic Versioning)
- **MAJOR**: Breaking changes
- **MINOR**: New features (backward compatible)
- **PATCH**: Bug fixes

**Example**: 1.2.3
- Major version: 1
- Minor version: 2
- Patch version: 3

### Release Checklist

- [ ] All features implemented and tested
- [ ] No known critical bugs
- [ ] Documentation updated (README, ARCHITECTURE, API_REFERENCE)
- [ ] Version number updated in .csproj
- [ ] CHANGELOG.md updated
- [ ] Build release version
- [ ] Test release executable on clean Windows install
- [ ] Create Git tag: `git tag v1.0.0`
- [ ] Push tag: `git push origin v1.0.0`
- [ ] Create GitHub release with changelog
- [ ] Attach compiled executables to release
- [ ] Announce release (if applicable)

### Building Release Packages

**Framework-Dependent**:
```powershell
dotnet publish -c Release -r win-x64 --self-contained false -p:PublishSingleFile=true
```
Package: `MouseJiggly-v1.0.0-win-x64.zip` (include README)

**Self-Contained**:
```powershell
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:PublishTrimmed=true
```
Package: `MouseJiggly-v1.0.0-win-x64-standalone.zip`

---

## Additional Resources

### Documentation
- [WPF Documentation](https://docs.microsoft.com/en-us/dotnet/desktop/wpf/)
- [.NET 9.0 Documentation](https://docs.microsoft.com/en-us/dotnet/core/whats-new/dotnet-9)
- [C# Programming Guide](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/)

### Tools
- [Visual Studio 2022](https://visualstudio.microsoft.com/)
- [Visual Studio Code](https://code.visualstudio.com/)
- [.NET SDK](https://dotnet.microsoft.com/download)

### Windows API Reference
- [SetCursorPos](https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-setcursorpos)
- [GetCursorPos](https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-getcursorpos)
- [mouse_event](https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-mouse_event)

---

**Happy Coding! 🖱️**

*Last Updated: November 9, 2025*
