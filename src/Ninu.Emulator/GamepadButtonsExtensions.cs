namespace Ninu.Emulator
{
    public static class GamepadButtonsExtensions
    {
        public static byte ToControlByte(this GamepadButtons buttons)
        {
            byte data = 0;

            data |= (byte)((((buttons & GamepadButtons.A) == GamepadButtons.A) ? 1 : 0) << 7);
            data |= (byte)((((buttons & GamepadButtons.B) == GamepadButtons.B) ? 1 : 0) << 6);
            data |= (byte)((((buttons & GamepadButtons.Select) == GamepadButtons.Select) ? 1 : 0) << 5);
            data |= (byte)((((buttons & GamepadButtons.Start) == GamepadButtons.Start) ? 1 : 0) << 4);
            data |= (byte)((((buttons & GamepadButtons.Up) == GamepadButtons.Up) ? 1 : 0) << 3);
            data |= (byte)((((buttons & GamepadButtons.Down) == GamepadButtons.Down) ? 1 : 0) << 2);
            data |= (byte)((((buttons & GamepadButtons.Left) == GamepadButtons.Left) ? 1 : 0) << 1);
            data |= (byte)((((buttons & GamepadButtons.Right) == GamepadButtons.Right) ? 1 : 0) << 0);

            return data;
        }
    }
}