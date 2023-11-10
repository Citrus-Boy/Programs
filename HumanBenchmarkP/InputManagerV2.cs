using System.Collections.Generic;
using System.Collections;
using System.Runtime.InteropServices;
using System;
using System.Security.Policy;
using System.Linq.Expressions;

namespace Citrus.Input
{
    class MouseContoller
    {
        public uint GoTo(int x, int y, bool xyIsD = false)
        {
            return (new MouseContoller.MouseEvent(x, y, xyIsD)).Trigger();
        }
        public uint Click(byte button = 1)
        {
            return (new MouseEvent((MouseEvent.Button)button)).Trigger();
        }
        public struct MouseEvent
        {
            [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
            public static extern uint SendInput(uint cInputs, UnmanagedInput[] pInputs, int size);

            [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
            public static extern UIntPtr GetMessageExtraInfo();

            MouseEventType type;

            int data;
            uint flags;

            int x;
            int y;
            bool xyIsDelta;

            Button button;

            uint time;

            List<UnmanagedInput> inputs;
            UnmanagedInput[] inputsArray;
            int unmanagedInputSize { get { return Marshal.SizeOf(typeof(UnmanagedInput)); } }
            enum MouseEventType : byte
            {
                Move = 0,
                Click = 1,
            }
            public enum Button : byte
            {
                None = 0,
                LeftClick = 1,
                RightClick = 2,
                MiddleClick = 3,
                Other = 4
            }
            public enum PressType : byte
            {
                Click = 0,
                Press = 1,
                Release = 2,
                
            }
            enum Flags : uint
            {
                MoveMouse = 0x0001,
                LeftMouseDown = 0x0002,
                LeftMouseUp = 0x0004,
                RightMouseDown = 0x0008,
                RightMouseUp = 0x0010,
                MiddleMouseDown = 0x0020,
                MiddleMouseUp = 0x0040,
                XButtonDown = 0x0080,
                XButtonUp = 0x0100,
                DataIsVerticalMouseWheel = 0x0800,
                DataIsHorizontalMouseWheel = 0x1000,
                TreatAsSeperateEvents = 0x2000,
                MapCoordinateToDesktop = 0x4000,
                MapXYToCoordinate = 0x8000
            }
            public MouseEvent(int X, int Y, bool xyIsD = false)
            {
                type = MouseEventType.Move;
                x = X;
                y = Y;
                xyIsDelta = xyIsD;

                data = 0;
                button = Button.None;
                time = 0;
                flags = 0;

                flags += (uint)Flags.MoveMouse;
                if (!xyIsDelta) flags += (uint)Flags.MapXYToCoordinate;
                inputs = new List<UnmanagedInput>();
                UnmanagedInput unmanagedInput = new UnmanagedInput();
                unmanagedInput.type = 0;
                unmanagedInput.union = new UnmanagedInput.InputUnion();
                unmanagedInput.union.mouseInput = new UnmanagedInput.InputUnion.MouseInput();
                unmanagedInput.union.mouseInput.x = x;
                unmanagedInput.union.mouseInput.y = y;
                unmanagedInput.union.mouseInput.data = data;
                unmanagedInput.union.mouseInput.flags = flags;
                unmanagedInput.union.mouseInput.time = time;
                unmanagedInput.union.mouseInput.extraMessageInfo = GetMessageExtraInfo();
                inputs.Add( unmanagedInput );
                inputsArray = inputs.ToArray();
            }
            public MouseEvent(Button buttonIn, int dat = 0, PressType pt = PressType.Click)
            {
                type = MouseEventType.Click;
                x = 0;
                y = 0;
                xyIsDelta = true;

                data = dat;
                button = buttonIn;
                time = 0;

                flags = 0;

                uint oppFlags = 0;
                switch (button)
                {
                    case Button.LeftClick:
                        if(pt == PressType.Release)
                        {
                            flags += (uint)Flags.LeftMouseUp;
                        }
                        else
                        {
                            flags += (uint)Flags.LeftMouseDown;
                            oppFlags += (uint)Flags.LeftMouseUp;
                        }
                        break;
                    default:
                        break;
                }
                inputs = new List<UnmanagedInput>();

                UnmanagedInput unmanagedInput = new UnmanagedInput();
                unmanagedInput.type = 0;
                unmanagedInput.union = new UnmanagedInput.InputUnion();
                unmanagedInput.union.mouseInput = new UnmanagedInput.InputUnion.MouseInput();
                unmanagedInput.union.mouseInput.data = data;
                unmanagedInput.union.mouseInput.flags = flags;
                unmanagedInput.union.mouseInput.time = time;
                unmanagedInput.union.mouseInput.extraMessageInfo = GetMessageExtraInfo();
                inputs.Add( unmanagedInput);
                if (pt == PressType.Click)
                {
                    unmanagedInput = new UnmanagedInput();
                    unmanagedInput.type = 0;
                    unmanagedInput.union = new UnmanagedInput.InputUnion();
                    unmanagedInput.union.mouseInput = new UnmanagedInput.InputUnion.MouseInput();
                    unmanagedInput.union.mouseInput.data = data;
                    unmanagedInput.union.mouseInput.flags = oppFlags;
                    unmanagedInput.union.mouseInput.time = time;
                    unmanagedInput.union.mouseInput.extraMessageInfo = GetMessageExtraInfo();
                    inputs.Add(unmanagedInput);
                }
                inputsArray = inputs.ToArray();
            }
            public uint Trigger()
            {
                return SendInput((uint)inputsArray.Length, inputsArray, unmanagedInputSize);
            }
        }
    }
    class KeyboardController
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern UIntPtr GetMessageExtraInfo();

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern uint SendInput(uint cInputs, UnmanagedInput[] pInputs, int size);

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern short VkKeyScanA(char character);

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern uint MapVirtualKeyA(uint uCode, uint uMapType = 0);

        public void Press(VirtualKey vk)
        {
            UnmanagedInput ui = new UnmanagedInput()
            {
                type = 1,
                union = new UnmanagedInput.InputUnion()
                {
                    keyboardInput = new UnmanagedInput.InputUnion.KeyboardInput()
                    {
                        virtualKey = (byte)vk,
                        keyScanCode = (ushort)MapVirtualKeyA((byte)vk),
                        extraMessageInfo = GetMessageExtraInfo()
                    }
                }
            };
            SendInput(1, new UnmanagedInput[] { ui}, Marshal.SizeOf(typeof(UnmanagedInput)));
        }
        public void Release(VirtualKey vk)
        {
            UnmanagedInput ui = new UnmanagedInput()
            {
                type = 1,
                union = new UnmanagedInput.InputUnion()
                {
                    keyboardInput = new UnmanagedInput.InputUnion.KeyboardInput()
                    {
                        virtualKey = (byte)vk,
                        keyScanCode = (ushort)MapVirtualKeyA((byte)vk),
                        flags = (uint)KeyboardEventFlag.ReleaseThisKey,
                        extraMessageInfo = GetMessageExtraInfo()
                    }
                }
            };
            SendInput(1, new UnmanagedInput[] { ui }, Marshal.SizeOf(typeof(UnmanagedInput)));
        }
    }
    enum VirtualKey : byte
    {
        None = 0xff,
        Shift = 0xa0,
        A = 0x41,
        B = 0x42,
        C = 0x43,
        D = 0x44,
        E = 0x45,
        F = 0x46,
        G = 0x47,
        H = 0x48,
        I = 0x49,
        J = 0x4a,
        K = 0x4b,
        L = 0x4c,
        M = 0x4d,
        N = 0x4e,
        O = 0x4f,
        P = 0x50,
        Q = 0x51,
        R = 0x52,
        S = 0x53,
        T = 0x54,
        U = 0x55,
        V = 0x56,
        W = 0x57,
        X = 0x58,
        Y = 0x59,
        Z = 0x5a
    }
    enum KeyboardEventFlag : uint
    {
        ExtendedKey = 0x0001,
        ReleaseThisKey = 0x0002,
        UseScanCodeOnly = 0x0008,
        Unicode = 0x0004
    }

    struct UnmanagedInput
    {
        public uint type;
        public InputUnion union;

        [StructLayout(LayoutKind.Explicit)]
        public struct InputUnion
        {

            [FieldOffset(0)]
            public MouseInput mouseInput;

            [StructLayout(LayoutKind.Sequential)]
            public struct MouseInput
            {

                public int x;
                public int y;
                public int data;
                public uint flags;
                public uint time;
                public UIntPtr extraMessageInfo;
            }

            [FieldOffset(0)]
            public KeyboardInput keyboardInput;

            [StructLayout(LayoutKind.Sequential)]
            public struct KeyboardInput
            {
                public ushort virtualKey;
                public ushort keyScanCode;
                public uint flags;
                public uint time;
                public UIntPtr extraMessageInfo;
            }

            [FieldOffset(0)]
            public HARDWAREINPUT hi;

            [StructLayout(LayoutKind.Sequential)]
            public struct HARDWAREINPUT
            {
                public uint uMsg;
                public ushort wParamL;
                public ushort wParamH;
            }
        }
    }


}