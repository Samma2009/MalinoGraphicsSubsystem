using System.Collections.Generic;
using System;
using libmalino;
using System.IO;
using System.Drawing;

namespace MGS
{
    /// <summary>
    /// Manages the mouse.
    /// </summary>
    public static class MouseManager
    {
        #region Fields

        /// <summary>
        /// The state the mouse was in the last frame.
        /// </summary>
        public static MouseState LastMouseState;

        /// <summary>
        /// The state the mouse is currently in.
        /// </summary>
        public static MouseState MouseState;
        /// <summary>
        /// The sensitivity of the mouse, 1.0f is the default.
        /// </summary>
        public static float MouseSensitivity;

        private static uint screenWidth;
        private static uint screenHeight;

        // These values are used as flags for the Delta X and Y properties, explained more there.
        private static bool hasReadDeltaX;
        private static bool hasReadDeltaY;

        // Temporary 'cache' delta values.
        private static int deltaX;
        private static int deltaY;

        private static int scrollDelta;

        /// <summary>
        /// The X location of the mouse.
        /// </summary>
        public static uint X;

        /// <summary>
        /// The Y location of the mouse.
        /// </summary>
        public static uint Y;
        static FileStream mfs;
        static byte[] buffer = new byte[3];

        #endregion

        static MouseManager()
        {
            MouseSensitivity = 1f;
            try
            {
                malino.LoadAllKernelModules();
			    InitStream();
            }
            catch{}

        }

        static void InitStream()
        {
            const string mouseDevice = "/dev/input/mice";
            mfs =  new FileStream(mouseDevice, FileMode.Open, FileAccess.Read);
        }

        /// <summary>
        /// The width of the mouse screen area (i.e. max value of X).
        /// </summary>
        public static uint ScreenWidth
        {
            get => screenWidth;
            set
            {
                screenWidth = value;

                if (X >= screenWidth)
                {
                    X = screenWidth - 1;
                }
            }
        }

        /// <summary>
        /// The screen height (i.e. max value of Y).
        /// </summary>
        public static uint ScreenHeight
        {
            get => screenHeight;
            set
            {
                screenHeight = value;

                if (Y >= screenHeight)
                {
                    Y = screenHeight - 1;
                }
            }
        }

        /// <summary>
        /// The 'delta' for the mouse scroll wheel. Needs to be manually reset.
        /// </summary>
        public static int ScrollDelta {
            get {
                return scrollDelta;
            }
            internal set => scrollDelta = value;
        }

        public static bool ScrollWheelPresent => false;

        #region Methods

        public static void GetMousePos()
        {  

            try
            {
                int bytesRead = mfs.Read(buffer, 0, buffer.Length);

                if (bytesRead == buffer.Length)
                {
                    int leftButton = (buffer[0] & 0x1) > 0 ? 1 : 0;
                    int rightButton = (buffer[0] & 0x2) > 0 ? 1 : 0;
                    int middleButton = (buffer[0] & 0x4) > 0 ? 1 : 0;

                    if (leftButton == 1) MouseState = MouseState.Left;
                    else if (rightButton == 1) MouseState = MouseState.Right;
                    else if (middleButton == 1) MouseState = MouseState.Middle;
                    else MouseState = MouseState.None;
                    
                    int xMovement = (buffer[1] > 127 ? buffer[1] - 256 : buffer[1]);
                    int yMovement = (buffer[2] > 127 ? buffer[2] - 256 : buffer[2]);

                    // Update the coordinates
                    X = (uint)Math.Max(0, X + xMovement);
                    Y = (uint)Math.Max(0, Y - yMovement);

                    if (X > screenWidth) X=screenWidth;
                    if (Y > ScreenHeight) Y=screenHeight;

                }
            }
            catch
            {
            }
        }

        /// <summary>
        /// Reset the scroll delta to 0.
        /// </summary>
        public static void ResetScrollDelta() {
            ScrollDelta = 0;
        }

        #endregion
    }
    public enum MouseState
    {
        /// <summary>
        /// No button is pressed.
        /// </summary>
        None = 0b0000_0000,

        /// <summary>
        /// The left mouse button is pressed.
        /// </summary>
        Left = 0b0000_0001,

        /// <summary>
        /// The right mouse button is pressed.
        /// </summary>
        Right = 0b0000_0010,

        /// <summary>
        /// The middle mouse button is pressed.
        /// </summary>
        Middle = 0b0000_0100,

        /// <summary>
        /// The fourth mouse button is pressed.
        /// </summary>
        FourthButton = 0b0000_1000,

        /// <summary>
        /// The fifth mouse button is pressed.
        /// </summary>
        FifthButton = 0b0001_0000
    }
}
