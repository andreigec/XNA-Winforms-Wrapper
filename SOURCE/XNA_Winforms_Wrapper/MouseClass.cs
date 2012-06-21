using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Project.View.Client
{
    public class MouseClass
    {
        #region mouseButtons enum

        public enum mouseButtons
        {
            left,
            middle,
            right,
            mouseWheelUp,
            mouseWheelDown
        }

        #endregion

        public Vector2 CurrentPos;

        private MouseState CurrentState;
        private MouseState OldState;
        private GameTime gt;

        public MouseClass()
        {
            ButtonsHold = new Dictionary<mouseButtons, Vector2>();
            ButtonsDown = new Dictionary<mouseButtons, Vector2>();
        }

        public Dictionary<mouseButtons, Vector2> ButtonsHold { get; private set; }
        public Dictionary<mouseButtons, Vector2> ButtonsDown { get; private set; }

        public void UpdateButtons(MouseState ms, GameTime gtIN)
        {
            gt = gtIN;
            CurrentState = ms;
            var x = ms.X;
            var y = ms.Y;

            if (x < 0)
                x = 0;
            if (y < 0)
                y = 0;
            
            CurrentPos = new Vector2(x, y);

            ButtonsHold = new Dictionary<mouseButtons, Vector2>();
            ButtonsDown = new Dictionary<mouseButtons, Vector2>();
            UpdateButtons(ms);
        }

        public bool LeftButtonAny()
        {
            return ButtonsDown.Any(s => s.Key == mouseButtons.left) || LeftButtonHold();
        }

        public bool LeftButtonHold()
        {
            return ButtonsHold.Any(s => s.Key == mouseButtons.left);
        }

        public bool RightButtonAny()
        {
            return ButtonsDown.Any(s => s.Key == mouseButtons.right) || RightButtonHold();
        }

        public bool RightButtonHold()
        {
            return ButtonsHold.Any(s => s.Key == mouseButtons.right);
        }

        public void SwitchStates()
        {
            OldState = CurrentState;
        }

        public bool ButtonsPressed()
        {
            return (ButtonsHold.Count > 0 || ButtonsDown.Count > 0);
        }

        private void UpdateButtons(MouseState ms)
        {
            var v = new Vector2(ms.X, ms.Y);

            if (CurrentState.LeftButton == ButtonState.Pressed)
            {
                if (OldState.LeftButton == ButtonState.Pressed)
                    ButtonsHold.Add(mouseButtons.left, v);
                else
                    ButtonsDown.Add(mouseButtons.left, v);
            }

            if (CurrentState.MiddleButton == ButtonState.Pressed)
            {
                if (OldState.MiddleButton == ButtonState.Pressed)
                    ButtonsHold.Add(mouseButtons.middle, v);
                else
                    ButtonsDown.Add(mouseButtons.middle, v);
            }

            if (CurrentState.RightButton == ButtonState.Pressed)
            {
                if (OldState.RightButton == ButtonState.Pressed)
                    ButtonsHold.Add(mouseButtons.right, v);
                else
                    ButtonsDown.Add(mouseButtons.right, v);
            }

            var dif = CurrentState.ScrollWheelValue - OldState.ScrollWheelValue;
            if (dif > 0)
                ButtonsDown.Add(mouseButtons.mouseWheelUp, v);
            else if (dif < 0)
                ButtonsDown.Add(mouseButtons.mouseWheelDown, v);
        }

        public int GetScrollDiff()
        {
            return CurrentState.ScrollWheelValue - OldState.ScrollWheelValue;
        }
    }
}