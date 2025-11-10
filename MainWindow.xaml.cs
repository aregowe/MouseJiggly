using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Threading;

namespace MouseJiggly
{
    /// <summary>
    /// MainWindow class - The primary application window for Mouse Jiggly.
    /// 
    /// This application prevents computers from entering sleep mode or activating screensavers
    /// by simulating mouse activity at regular intervals. It provides two operational modes:
    /// 
    /// 1. Standard Mode: Physically moves the mouse cursor 1 pixel back and forth
    /// 2. Zen Mode: Simulates mouse movement without visible cursor displacement
    /// 
    /// The application uses Windows API calls (user32.dll) for low-level mouse control,
    /// ensuring efficient and reliable operation without requiring external dependencies.
    /// 
    /// Built with .NET 9.0 and C# 12 for modern performance and features.
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Private Fields

        /// <summary>
        /// Timer that controls the jiggle frequency. 
        /// Fires at intervals specified by the user (1-99 seconds).
        /// </summary>
        private DispatcherTimer? jiggleTimer;

        /// <summary>
        /// Direction flag for alternating mouse movement.
        /// True = move right/positive direction, False = move left/negative direction.
        /// This ensures the mouse returns to its original position after each jiggle cycle.
        /// </summary>
        private bool moveRight = true;

        /// <summary>
        /// Current jiggle interval in seconds (1-99).
        /// Default value is 4 seconds, which provides a good balance between
        /// keeping the system active and not being too intrusive.
        /// </summary>
        private int jiggleInterval = 4;

        /// <summary>
        /// Flag indicating whether Standard Jiggle mode is currently active.
        /// When true, the cursor will physically move 1 pixel back and forth.
        /// Cannot be true simultaneously with isZenModeEnabled.
        /// </summary>
        private bool isJiggleEnabled = false;

        /// <summary>
        /// Flag indicating whether Zen Mode is currently active.
        /// When true, mouse movement is simulated without visible cursor movement.
        /// Cannot be true simultaneously with isJiggleEnabled.
        /// </summary>
        private bool isZenModeEnabled = false;

        #endregion

        #region Windows API Declarations

        /// <summary>
        /// Sets the cursor position to the specified screen coordinates.
        /// This is a Windows API function from user32.dll.
        /// </summary>
        /// <param name="X">The new x-coordinate of the cursor, in screen coordinates.</param>
        /// <param name="Y">The new y-coordinate of the cursor, in screen coordinates.</param>
        /// <returns>True if successful, false otherwise.</returns>
        /// <remarks>
        /// Used in Standard Mode to physically move the cursor.
        /// Coordinates are in pixels relative to the primary monitor's top-left corner.
        /// ExactSpelling and SetLastError=false optimize P/Invoke performance.
        /// </remarks>
        [DllImport("user32.dll", ExactSpelling = true, SetLastError = false)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetCursorPos(int X, int Y);

        /// <summary>
        /// Retrieves the current cursor position in screen coordinates.
        /// This is a Windows API function from user32.dll.
        /// </summary>
        /// <param name="lpPoint">Output parameter that receives the cursor position.</param>
        /// <returns>True if successful, false otherwise.</returns>
        /// <remarks>
        /// Essential for Standard Mode to determine current position before moving.
        /// ExactSpelling and SetLastError=false optimize P/Invoke performance.
        /// </remarks>
        [DllImport("user32.dll", ExactSpelling = true, SetLastError = false)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetCursorPos(out POINT lpPoint);

        /// <summary>
        /// Synthesizes mouse motion and button clicks.
        /// This is a Windows API function from user32.dll.
        /// </summary>
        /// <param name="dwFlags">Controls various aspects of mouse motion and button clicking.</param>
        /// <param name="dx">The mouse's absolute position along the x-axis or its amount of motion.</param>
        /// <param name="dy">The mouse's absolute position along the y-axis or its amount of motion.</param>
        /// <param name="dwData">Additional data (used for wheel movement, etc.).</param>
        /// <param name="dwExtraInfo">An additional value associated with the mouse event.</param>
        /// <remarks>
        /// Used in Zen Mode to send relative mouse movements that cancel each other out.
        /// This keeps the system active without visible cursor movement.
        /// ExactSpelling and SetLastError=false optimize P/Invoke performance.
        /// </remarks>
        [DllImport("user32.dll", ExactSpelling = true, SetLastError = false)]
        private static extern void mouse_event(uint dwFlags, int dx, int dy, uint dwData, UIntPtr dwExtraInfo);

        /// <summary>
        /// Flag for mouse_event function indicating a mouse move event.
        /// When this flag is set, dx and dy parameters are interpreted as relative movements.
        /// </summary>
        private const uint MOUSEEVENTF_MOVE = 0x0001;

        /// <summary>
        /// Structure representing a point in 2D coordinate space.
        /// Used with GetCursorPos to retrieve the current cursor position.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            /// <summary>The x-coordinate of the point.</summary>
            public int X;
            /// <summary>The y-coordinate of the point.</summary>
            public int Y;
        }

        #endregion

        #region Constructor and Initialization

        /// <summary>
        /// Initializes a new instance of the MainWindow class.
        /// Sets up the UI components and initializes the jiggle timer.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            InitializeTimer();
            
            // Ensure proper cleanup when window closes
            Closing += MainWindow_Closing;
        }

        /// <summary>
        /// Event handler called when the window is closing.
        /// Ensures the timer is stopped and resources are cleaned up properly.
        /// </summary>
        private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            // Stop the timer if it's running
            if (jiggleTimer != null)
            {
                jiggleTimer.Stop();
                jiggleTimer = null;
            }
        }

        /// <summary>
        /// Initializes the jiggle timer with default settings.
        /// The timer is created but not started - it will begin when user enables a jiggle mode.
        /// </summary>
        /// <remarks>
        /// The timer uses DispatcherTimer which runs on the UI thread, ensuring thread-safe
        /// updates to the UI elements (status label, etc.) without requiring Invoke calls.
        /// </remarks>
        private void InitializeTimer()
        {
            jiggleTimer = new DispatcherTimer();
            jiggleTimer.Interval = TimeSpan.FromSeconds(jiggleInterval);
            jiggleTimer.Tick += JiggleTimer_Tick;
        }

        #endregion

        #region Timer Event Handlers

        /// <summary>
        /// Event handler called on each timer tick.
        /// Determines which jiggle mode is active and performs the appropriate action.
        /// </summary>
        /// <param name="sender">The timer that raised the event.</param>
        /// <param name="e">Event arguments (not used).</param>
        /// <remarks>
        /// This method acts as a dispatcher, routing to the appropriate jiggle method
        /// based on the current mode. Only one mode can be active at a time.
        /// Optimized with MethodImpl for aggressive inlining to reduce overhead.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void JiggleTimer_Tick(object? sender, EventArgs e)
        {
            if (isZenModeEnabled)
            {
                // Zen Mode: Invisible jiggle
                PerformZenJiggle();
            }
            else if (isJiggleEnabled)
            {
                // Standard Mode: Visible cursor movement
                PerformStandardJiggle();
            }
        }

        #endregion

        #region Jiggle Operations

        /// <summary>
        /// Performs a standard jiggle operation - moves the cursor 1 pixel and back.
        /// This movement is visible to the user and physically changes cursor position.
        /// </summary>
        /// <remarks>
        /// Algorithm:
        /// 1. Get current cursor position using Windows API
        /// 2. Move cursor 1 pixel in the current direction (right or left)
        /// 3. Toggle direction for next jiggle to return to original position
        /// 
        /// The alternating pattern ensures that after two jiggles, the cursor
        /// returns to its original position, minimizing disruption to the user.
        /// 
        /// This method prevents screen savers and sleep mode by generating actual
        /// cursor movement events that the operating system recognizes as user activity.
        /// Optimized with AggressiveInlining for minimal overhead.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void PerformStandardJiggle()
        {
            // Get the current cursor position from Windows (single API call)
            GetCursorPos(out POINT currentPos);

            // Conditional move optimization: single branch, toggle in-place
            int offset = moveRight ? 1 : -1;
            SetCursorPos(currentPos.X + offset, currentPos.Y);
            moveRight = !moveRight;
        }

        /// <summary>
        /// Performs a zen mode jiggle - simulates mouse activity without visible cursor movement.
        /// This is achieved by sending offsetting mouse movement events.
        /// </summary>
        /// <remarks>
        /// Algorithm:
        /// 1. Send a relative mouse movement in one direction (1 pixel)
        /// 2. Immediately send an equal movement in the opposite direction (-1 pixel)
        /// 3. Toggle direction pattern for variation in event generation
        /// 
        /// How Zen Mode Works:
        /// The mouse_event API with MOUSEEVENTF_MOVE flag sends relative mouse movements.
        /// By sending two movements that cancel each other out (+1 then -1, or vice versa),
        /// we generate mouse activity events that the OS recognizes, preventing sleep/screensaver,
        /// but the net cursor displacement is zero - the cursor appears stationary to the user.
        /// 
        /// Benefits of Zen Mode:
        /// - Cursor doesn't visibly move on screen
        /// - No interference with user's work or gaming
        /// - Still generates OS-level mouse events to prevent idle state
        /// - Useful for presentations, monitoring displays, or situations where
        ///   visible cursor movement would be distracting
        /// Optimized with AggressiveInlining for minimal overhead.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void PerformZenJiggle()
        {
            // Zen mode: optimized with minimal branching
            // Send offsetting movements that cancel spatially but register as activity
            int offset = moveRight ? 1 : -1;
            mouse_event(MOUSEEVENTF_MOVE, offset, 0, 0, UIntPtr.Zero);
            mouse_event(MOUSEEVENTF_MOVE, -offset, 0, 0, UIntPtr.Zero);
            moveRight = !moveRight;
        }

        #endregion

        #region UI Event Handlers

        /// <summary>
        /// Event handler for Standard Jiggle checkbox state changes.
        /// Enables or disables standard mode and ensures mutual exclusivity with Zen mode.
        /// </summary>
        /// <param name="sender">The checkbox that raised the event.</param>
        /// <param name="e">Event arguments.</param>
        /// <remarks>
        /// Business Logic:
        /// - Standard Mode and Zen Mode are mutually exclusive
        /// - If user enables Standard Mode while Zen Mode is active, Zen Mode is automatically disabled
        /// - This prevents conflicting jiggle operations from running simultaneously
        /// - Timer state is updated to reflect the new configuration
        /// </remarks>
        private void JiggleCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            // Update state based on checkbox
            isJiggleEnabled = JiggleCheckBox.IsChecked == true;

            // Enforce mutual exclusivity: Standard and Zen modes cannot both be active
            if (isJiggleEnabled && isZenModeEnabled)
            {
                // User enabled Standard mode, so disable Zen mode
                ZenModeCheckBox.IsChecked = false;
                isZenModeEnabled = false;
            }

            // Start or stop the timer based on new state
            UpdateTimerState();
        }

        /// <summary>
        /// Event handler for Zen Mode checkbox state changes.
        /// Enables or disables zen mode and ensures mutual exclusivity with Standard mode.
        /// </summary>
        /// <param name="sender">The checkbox that raised the event.</param>
        /// <param name="e">Event arguments.</param>
        /// <remarks>
        /// Business Logic:
        /// - Standard Mode and Zen Mode are mutually exclusive
        /// - If user enables Zen Mode while Standard Mode is active, Standard Mode is automatically disabled
        /// - This prevents conflicting jiggle operations from running simultaneously
        /// - Timer state is updated to reflect the new configuration
        /// </remarks>
        private void ZenModeCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            // Update state based on checkbox
            isZenModeEnabled = ZenModeCheckBox.IsChecked == true;

            // Enforce mutual exclusivity: Standard and Zen modes cannot both be active
            if (isZenModeEnabled && isJiggleEnabled)
            {
                // User enabled Zen mode, so disable Standard mode
                JiggleCheckBox.IsChecked = false;
                isJiggleEnabled = false;
            }

            // Start or stop the timer based on new state
            UpdateTimerState();
        }

        /// <summary>
        /// Event handler for interval slider value changes.
        /// Updates the jiggle frequency and modifies the running timer if active.
        /// </summary>
        /// <param name="sender">The slider that raised the event.</param>
        /// <param name="e">Event arguments containing the new value.</param>
        /// <remarks>
        /// The slider allows values from 1 to 99 seconds.
        /// Changes take effect immediately:
        /// - UI label is updated to show new interval
        /// - If timer is running, its interval is changed (next tick uses new timing)
        /// - Status message is updated to reflect new frequency
        /// 
        /// Note: The current timer cycle completes before the new interval takes effect.
        /// </remarks>
        private void IntervalSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            // Get new interval value from slider (automatically rounded due to IsSnapToTickEnabled)
            jiggleInterval = (int)e.NewValue;
            
            // Update the label showing current interval (check if initialized first)
            if (IntervalLabel != null)
            {
                IntervalLabel.Text = $"{jiggleInterval}s";
            }

            // Update the timer interval if it exists (may be null during initialization)
            if (jiggleTimer != null)
            {
                jiggleTimer.Interval = TimeSpan.FromSeconds(jiggleInterval);
            }

            // Refresh status display to show new interval (check if StatusLabel initialized)
            if (StatusLabel != null)
            {
                UpdateStatus();
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Updates the timer's running state based on current mode settings.
        /// Starts the timer if any mode is enabled, stops it if all modes are disabled.
        /// </summary>
        /// <remarks>
        /// Timer Management Logic:
        /// - Timer starts when either Standard or Zen mode is enabled
        /// - Timer stops when both modes are disabled
        /// - Status display is updated to reflect current state
        /// 
        /// This centralized method ensures consistent timer management across
        /// all checkbox state changes.
        /// </remarks>
        private void UpdateTimerState()
        {
            // Safety check: ensure timer exists
            if (jiggleTimer == null) return;

            // Start timer if any mode is active
            if (isJiggleEnabled || isZenModeEnabled)
            {
                jiggleTimer.Start();
                UpdateStatus(); // Show active status
            }
            else
            {
                // Stop timer when no mode is active
                jiggleTimer.Stop();
                UpdateStatus("Status: Inactive"); // Show inactive status
            }
        }

        /// <summary>
        /// Updates the status label with current operational state.
        /// Can accept a custom message or automatically generate one based on active mode.
        /// </summary>
        /// <param name="customMessage">
        /// Optional custom status message. If provided, this exact message is displayed.
        /// If null, status is automatically determined from current mode settings.
        /// </param>
        /// <remarks>
        /// Status Display Priority:
        /// 1. If customMessage provided: use it (for temporary status updates during jiggle)
        /// 2. If Standard Mode active: show "Standard jiggle active (every Xs)"
        /// 3. If Zen Mode active: show "Zen mode active (every Xs)"
        /// 4. Otherwise: show "Inactive"
        /// 
        /// This provides clear feedback to users about:
        /// - Whether the application is preventing sleep
        /// - Which mode is currently active
        /// - How frequently jiggles are occurring
        /// Optimized to reduce string allocations.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void UpdateStatus(string? customMessage = null)
        {
            // If custom message provided, use it directly
            if (customMessage != null)
            {
                StatusLabel.Text = customMessage;
                return;
            }

            // Otherwise, determine status from current mode (optimized path)
            StatusLabel.Text = isJiggleEnabled 
                ? $"Status: Standard jiggle active (every {jiggleInterval}s)"
                : isZenModeEnabled 
                    ? $"Status: Zen mode active (every {jiggleInterval}s)"
                    : "Status: Inactive";
        }

        #endregion
    }
}
