using Ninu.Emulator;
using SharpDX.DirectInput;

namespace Ninu
{
    public sealed record InputMapping(
        Device Device,
        DirectInputButton DirectInputButton,
        GamepadButtons GamepadButton);
}