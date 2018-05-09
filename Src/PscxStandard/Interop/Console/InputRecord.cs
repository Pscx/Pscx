//---------------------------------------------------------------------
// Original Author: jachymko
//
// Description: Structures for Win32 Console API.
//
// Creation Date: Jan 14, 2007
//---------------------------------------------------------------------
using System;
using System.Runtime.InteropServices;

namespace Pscx.Interop
{
    public enum InputRecordType : short
    {
        KeyboardEvent = 1,
        MouseEvent = 2,
        WindowBufferSizeEvent = 4,
        MenuEvent = 8,
        FocusEvent = 16,
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct InputRecord
    {
        [FieldOffset(0)]
        public InputRecordType EventType;

        [FieldOffset(2)]
        public MouseEventRecord MouseEvent;

    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MouseEventRecord
    {
        public Coordinates Location;
        public MouseButtonState ButtonState;
        public ControlKeyState ControlKeyState;
        public MouseEventFlags EventFlags;
    }

    [Flags]
    public enum MouseEventFlags
    {
        MouseMoved = 1,
        DoubleClick = 2,
        MouseWheeled = 4,
    }

    [Flags]
    public enum MouseButtonState
    {
        LeftmostButton = 1,
        RightmostButton = 2,
    }

    [Flags]
    public enum ControlKeyState
    {
        RightAltPressed = 1,
        LeftAltPressed = 2,
        RightCtrlPressed = 4,
        LeftCtrlPressed = 8,
        ShiftPressed = 0x10,
        NumLockOn = 0x20,
        ScrollLockOn = 0x40,
        CapsLockOn = 0x80,
        EnhancedKey = 0x100,
    }
}

