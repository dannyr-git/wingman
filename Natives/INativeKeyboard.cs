using System;

namespace wingman.Natives
{
    public interface INativeKeyboard
    {
        event Func<string, bool> OnKeyDown;
        event Func<string, bool> OnKeyUp;
        //void SendInput(string input);
        bool IsKeyPressed(string key);


        public void ShiftUp();
        public void ShiftDown();

        public void CtrlDown();
        public void CtrlUp();

        public void AltDown();
        public void AltUp();

        public void AltReset();
        public void CtrlReset();
        public void ShiftReset();

        //public void SendString(string input);
    }
}
