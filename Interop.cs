using System.Runtime.InteropServices;

namespace AwayBuster;

public static partial class Interop
{
    public const uint INPUT_MOUSE = 0;
    public const uint MOUSEEVENTF_MOVE = 0x0001;

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool GetLastInputInfo(ref LastInputInfo plii);

    // Import SendInput from user32.dll
    [LibraryImport("user32.dll", SetLastError = true)]
    public static partial uint SendInput(uint nInputs, [MarshalAs(UnmanagedType.LPArray)] [In] Input[] pInputs, int cbSize);
}

[StructLayout(LayoutKind.Sequential)]
public struct LastInputInfo
{
    public static readonly int SizeOf = Marshal.SizeOf<LastInputInfo>();

    [MarshalAs(UnmanagedType.U4)] public UInt32 cbSize;
    [MarshalAs(UnmanagedType.U4)] public UInt32 dwTime;
}

// Define the INPUT structure
[StructLayout(LayoutKind.Sequential)]
public struct Input
{
    public uint type;
    public InputUnion U;
}

[StructLayout(LayoutKind.Explicit)]
public struct InputUnion
{
    [FieldOffset(0)] public MouseInput mi;
    [FieldOffset(0)] public KeyboardInput ki;
    [FieldOffset(0)] public HardwareInput hi;
}

[StructLayout(LayoutKind.Sequential)]
public struct MouseInput
{
    public int dx;
    public int dy;
    public uint mouseData;
    public uint dwFlags;
    public uint time;
    public UIntPtr dwExtraInfo;
}

[StructLayout(LayoutKind.Sequential)]
public struct KeyboardInput
{
    public ushort wVk;
    public ushort wScan;
    public uint dwFlags;
    public uint time;
    public UIntPtr dwExtraInfo;
}

[StructLayout(LayoutKind.Sequential)]
public struct HardwareInput
{
    public uint uMsg;
    public ushort wParamL;
    public ushort wParamH;
}