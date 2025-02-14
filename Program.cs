using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using Timer = System.Windows.Forms.Timer;

namespace AwayBuster;

internal static class Program
{
    /// <summary>
    ///     The interval in seconds between idle checks
    /// </summary>
    private const int TIMER_INTERVAL_SECONDS = 60;

    /// <summary>
    ///     The maximum allowed idle time in seconds before triggering mouse movement
    /// </summary>
    private const int MAX_IDLE_SECONDS = 120;

    /// <summary>
    ///     Retrieves the time elapsed since the last user input
    /// </summary>
    /// <returns>A TimeSpan representing the duration since the last user input</returns>
    private static TimeSpan RetrieveIdleTime()
    {
        var lastInputInfo = new LastInputInfo { cbSize = (uint)LastInputInfo.SizeOf };
        if (!Interop.GetLastInputInfo(ref lastInputInfo))
        {
            Console.WriteLine("GetLastInputInfo failed");
            return TimeSpan.Zero;
        }

        var elapsedTicks = Environment.TickCount - (int)lastInputInfo.dwTime;

        return elapsedTicks > 0 ? new TimeSpan(0, 0, 0, 0, elapsedTicks) : TimeSpan.Zero;
    }

    /// <summary>
    ///     Simulates mouse movement to prevent system idle state
    ///     by moving the cursor right and then left by 1 pixel
    /// </summary>
    private static void BustIdle()
    {
        var inputs = new Input[2];

        inputs[0].type = Interop.INPUT_MOUSE;
        inputs[0].U.mi.dx = 1;
        inputs[0].U.mi.dy = 0;
        inputs[0].U.mi.mouseData = 0;
        inputs[0].U.mi.dwFlags = Interop.MOUSEEVENTF_MOVE;
        inputs[0].U.mi.time = 0;
        inputs[0].U.mi.dwExtraInfo = UIntPtr.Zero;

        inputs[1].type = Interop.INPUT_MOUSE;
        inputs[1].U.mi.dx = -1;
        inputs[1].U.mi.dy = 0;
        inputs[1].U.mi.mouseData = 0;
        inputs[1].U.mi.dwFlags = Interop.MOUSEEVENTF_MOVE;
        inputs[1].U.mi.time = 0;
        inputs[1].U.mi.dwExtraInfo = UIntPtr.Zero;

        var numberOfInputsSent = Interop.SendInput((uint)inputs.Length, inputs, Marshal.SizeOf<Input>());
        if (numberOfInputsSent != inputs.Length) Console.WriteLine("SendInput failed: {0}", Marshal.GetLastWin32Error());
    }

    /// <summary>
    ///     Creates a red-tinted version of the icon
    /// </summary>
    /// <param name="originalIcon">The original icon to tint</param>
    /// <returns>A red-tinted copy of the original icon</returns>
    private static Icon CreateRedTintedIcon(Icon originalIcon)
    {
        using var bitmap = new Bitmap(originalIcon.ToBitmap());
        using var graphics = Graphics.FromImage(bitmap);

        // Create a red color matrix
        var colorMatrix = new ColorMatrix([
            [1,    0,    0,    0,    0],
            [0, 0.5f,    0,    0,    0],
            [0,    0, 0.5f,    0,    0],
            [0,    0,    0,    1,    0],
            [0.5f, 0,    0,    0,    1]
        ]);

        using var imageAttributes = new ImageAttributes();
        imageAttributes.SetColorMatrix(colorMatrix);

        graphics.DrawImage(bitmap, new Rectangle(0, 0, bitmap.Width, bitmap.Height), 0, 0, bitmap.Width, bitmap.Height, GraphicsUnit.Pixel, imageAttributes);

        return Icon.FromHandle(bitmap.GetHicon());
    }

    /// <summary>
    ///     Main entry point of the application. Sets up a system tray icon with a context menu
    ///     and initializes a timer to check for system idle state.
    /// </summary>
    [STAThread]
    private static void Main()
    {
        // Initialize timer for periodic idle checks
        var timer = new Timer();
        timer.Interval = TIMER_INTERVAL_SECONDS * 1000;
        timer.Tick += (_, _) =>
        {
            var idleTime = RetrieveIdleTime();
            if (idleTime.TotalSeconds > MAX_IDLE_SECONDS)
                BustIdle();
        };

        var icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath) ?? SystemIcons.Application;
        var redTintedIcon = CreateRedTintedIcon(icon);

        // Set up system tray icon
        var trayIcon = new NotifyIcon
        {
            Icon = icon,
            Text = "AwayBuster",
            Visible = true
        };

        // Create context menu with Enable/Disable and Exit options
        var menu = new ContextMenuStrip();
        var enableOption = new ToolStripMenuItem("Enable") { CheckOnClick = true, Checked = true };
        enableOption.CheckedChanged += (_, _) =>
        {
            trayIcon.Icon = enableOption.Checked ? icon : redTintedIcon;
            timer.Enabled = enableOption.Checked;
        };
        menu.Items.Add(enableOption);
        menu.Items.Add("Exit", null, (_, _) =>
        {
            trayIcon.Visible = false;
            Application.Exit();
        });
        trayIcon.ContextMenuStrip = menu;

        // Start the timer and run the winforms application
        timer.Start();
        Application.Run();
    }
}
