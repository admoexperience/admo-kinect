// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved. 

// get past MouseData not being initialized warning...it needs to be there for p/invoke
#pragma warning disable 0649

using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Threading;

namespace Admo
{
	internal struct MouseInput
	{
		public int X;
		public int Y;
		public uint MouseData;
		public uint Flags;
		public uint Time;
		public IntPtr ExtraInfo;
	}

	internal struct Input
	{
		public int Type;
		public MouseInput MouseInput;
	}

	public static class MouseDriver
	{
		public const int InputMouse = 0;

		public const int MouseEventMove      = 0x01;
		public const int MouseEventLeftDown  = 0x02;
		public const int MouseEventLeftUp    = 0x04;
		public const int MouseEventRightDown = 0x08;
		public const int MouseEventRightUp   = 0x10;
		public const int MouseEventAbsolute  = 0x8000;
        public const int MouseEventWheel = 0x800;

		private static bool lastLeftDown;
        private static bool reset_internal;
        public static bool reset;

		[DllImport("user32.dll", SetLastError = true)]
		private static extern uint SendInput(uint numInputs, Input[] inputs, int size);

        [DllImport("user32.dll")]
        static extern void mouse_event(int flags, int dX, int dY, int buttons, int extraInfo);

        [DllImport("user32.dll")]
        static extern int ShowCursor(bool show);

        public static void MouseWheel(int delta)
        {
            mouse_event(MouseEventWheel, 0, 0, delta, 0);
        }

        public static void Show()
        {
            ShowCursor(true);
        }

        public static void Hide()
        {
            ShowCursor(false);
        }


        public static void SendMouseInput(int positionX, int positionY, int maxX, int maxY, bool leftDown, bool click)
		{
            

            if(positionX > int.MaxValue)
				throw new ArgumentOutOfRangeException("positionX");
			if(positionY > int.MaxValue)
				throw new ArgumentOutOfRangeException("positionY");

			Input[] i = new Input[2];

			// move the mouse to the position specified
			i[0] = new Input();
			i[0].Type = InputMouse;
			i[0].MouseInput.X = (positionX * 65535) / maxX;
			i[0].MouseInput.Y = (positionY * 65535) / maxY;
			i[0].MouseInput.Flags = MouseEventAbsolute | MouseEventMove;
            


            // send it off
            uint result2 = SendInput(2, i, Marshal.SizeOf(i[0]));
            if (result2 == 0)
                throw new Win32Exception(Marshal.GetLastWin32Error());
            
			// determine if we need to send a mouse down or mouse up event
            if (click)
            {
                
                i[1] = new Input();
                i[1].Type = InputMouse;


                i[1].MouseInput.Flags = MouseEventLeftDown;
                // send it off
                uint result = SendInput(2, i, Marshal.SizeOf(i[0]));
                if (result == 0)
                    throw new Win32Exception(Marshal.GetLastWin32Error());

                Thread.Sleep(50);
                i[1].MouseInput.Flags = MouseEventLeftUp;
                // send it off
                uint result1 = SendInput(2, i, Marshal.SizeOf(i[0]));
                if (result1 == 0)
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                
                reset = true;
                click = false;
                
            }
            else
            {
                if (!lastLeftDown && leftDown)
                {
                    i[1] = new Input();
                    i[1].Type = InputMouse;
                    i[1].MouseInput.Flags = MouseEventLeftDown;

                    // send it off
                    uint result = SendInput(2, i, Marshal.SizeOf(i[0]));
                    if (result == 0)
                        throw new Win32Exception(Marshal.GetLastWin32Error());
                }
                else if (lastLeftDown && !leftDown)
                {
                    i[1] = new Input();
                    i[1].Type = InputMouse;
                    i[1].MouseInput.Flags = MouseEventLeftUp;

                    // send it off
                    uint result = SendInput(2, i, Marshal.SizeOf(i[0]));
                    if (result == 0)
                        throw new Win32Exception(Marshal.GetLastWin32Error());
                }
                                
            }
            
            
			lastLeftDown = leftDown;

			
		}
	}
}
