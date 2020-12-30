using Ninu.Emulator;
using SharpDX.DirectInput;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ninu
{
    public class InputManager : IDisposable
    {
        private readonly DirectInput _directInput = new();

        private readonly Keyboard _keyboard;
        public readonly List<Joystick> _joysticks = new();

        private List<InputMapping> _mappings = new();
        private List<Device> _uniqueDevicesFromMappings = new();

        private KeyboardState _keyboardState = new();
        private JoystickState _joystickState = new();

        public InputManager()
        {
            _keyboard = new Keyboard(_directInput);
            _keyboard.SetCooperativeLevel(IntPtr.Zero, CooperativeLevel.Background | CooperativeLevel.NonExclusive);

            var devices = _directInput.GetDevices(DeviceType.Gamepad, DeviceEnumerationFlags.AttachedOnly)
                .Concat(_directInput.GetDevices(DeviceType.Joystick, DeviceEnumerationFlags.AttachedOnly));

            foreach (var device in devices)
            {
                var joystick = new Joystick(_directInput, device.InstanceGuid);
                _joysticks.Add(joystick);

                joystick.SetCooperativeLevel(IntPtr.Zero, CooperativeLevel.Background | CooperativeLevel.NonExclusive);
            }
        }

        public void Dispose()
        {
            _keyboard.Unacquire();
            _keyboard.Dispose();

            foreach (var joystick in _joysticks)
            {
                joystick.Unacquire();
                joystick.Dispose();
            }

            _directInput.Dispose();
        }

        /// <summary>
        /// Performs an acquire on all devices including the keyboard.
        /// </summary>
        public void AcquireAll()
        {
            _keyboard.Acquire();

            foreach (var joystick in _joysticks)
            {
                joystick.Acquire();
            }
        }

        /// <summary>
        /// Clears all existing input mappings and sets up the new mappings.
        /// </summary>
        /// <param name="mappings">The new mappings to use.</param>
        public void SetMappings(IEnumerable<InputMapping> mappings)
        {
            if (mappings is null)
            {
                throw new ArgumentNullException(nameof(mappings));
            }

            _mappings = mappings.ToList();

            _uniqueDevicesFromMappings = _mappings
                .Select(x => x.Device)
                .Distinct()
                .ToList();
        }

        public GamepadButtons GetPressedButtons()
        {
            foreach (var device in _uniqueDevicesFromMappings)
            {
                device.Poll();
            }

            // TODO: Optimize this so we don't call get state on the same device more than once.
            var buttons = GamepadButtons.None;

            foreach (var mapping in _mappings)
            {
                switch (mapping.Device)
                {
                    case Keyboard keyboard:
                        keyboard.GetCurrentState(ref _keyboardState);

                        if (_keyboardState.IsPressed((Key)mapping.DirectInputButton))
                        {
                            buttons |= mapping.GamepadButton;
                        }

                        break;

                    case Joystick joystick:
                        joystick.GetCurrentState(ref _joystickState);

                        if (mapping.DirectInputButton >= DirectInputButton.JoystickButton1 && mapping.DirectInputButton <= DirectInputButton.JoystickButton128)
                        {
                            if (_joystickState.Buttons[mapping.DirectInputButton - DirectInputButton.JoystickButton1])
                            {
                                buttons |= mapping.GamepadButton;
                            }
                        }
                        else if (mapping.DirectInputButton >= DirectInputButton.JoystickPov0North && mapping.DirectInputButton <= DirectInputButton.JoystickPov3West)
                        {
                            switch (mapping.DirectInputButton)
                            {
                                // TODO: Make POVs expressions such that they are between 45 degree angles.

                                // POV 0
                                case DirectInputButton.JoystickPov0North when _joystickState.PointOfViewControllers[0] == 0 * 100:
                                    buttons |= mapping.GamepadButton;
                                    break;

                                case DirectInputButton.JoystickPov0East when _joystickState.PointOfViewControllers[0] == 90 * 100:
                                    buttons |= mapping.GamepadButton;
                                    break;

                                case DirectInputButton.JoystickPov0South when _joystickState.PointOfViewControllers[0] == 180 * 100:
                                    buttons |= mapping.GamepadButton;
                                    break;

                                case DirectInputButton.JoystickPov0West when _joystickState.PointOfViewControllers[0] == 270 * 100:
                                    buttons |= mapping.GamepadButton;
                                    break;

                                // POV 1
                                case DirectInputButton.JoystickPov1North when _joystickState.PointOfViewControllers[0] == 0 * 100:
                                    buttons |= mapping.GamepadButton;
                                    break;

                                case DirectInputButton.JoystickPov1East when _joystickState.PointOfViewControllers[0] == 90 * 100:
                                    buttons |= mapping.GamepadButton;
                                    break;

                                case DirectInputButton.JoystickPov1South when _joystickState.PointOfViewControllers[0] == 180 * 100:
                                    buttons |= mapping.GamepadButton;
                                    break;

                                case DirectInputButton.JoystickPov1West when _joystickState.PointOfViewControllers[0] == 270 * 100:
                                    buttons |= mapping.GamepadButton;
                                    break;

                                // POV 2
                                case DirectInputButton.JoystickPov2North when _joystickState.PointOfViewControllers[0] == 0 * 100:
                                    buttons |= mapping.GamepadButton;
                                    break;

                                case DirectInputButton.JoystickPov2East when _joystickState.PointOfViewControllers[0] == 90 * 100:
                                    buttons |= mapping.GamepadButton;
                                    break;

                                case DirectInputButton.JoystickPov2South when _joystickState.PointOfViewControllers[0] == 180 * 100:
                                    buttons |= mapping.GamepadButton;
                                    break;

                                case DirectInputButton.JoystickPov2West when _joystickState.PointOfViewControllers[0] == 270 * 100:
                                    buttons |= mapping.GamepadButton;
                                    break;

                                // POV 3
                                case DirectInputButton.JoystickPov3North when _joystickState.PointOfViewControllers[0] == 0 * 100:
                                    buttons |= mapping.GamepadButton;
                                    break;

                                case DirectInputButton.JoystickPov3East when _joystickState.PointOfViewControllers[0] == 90 * 100:
                                    buttons |= mapping.GamepadButton;
                                    break;

                                case DirectInputButton.JoystickPov3South when _joystickState.PointOfViewControllers[0] == 180 * 100:
                                    buttons |= mapping.GamepadButton;
                                    break;

                                case DirectInputButton.JoystickPov3West when _joystickState.PointOfViewControllers[0] == 270 * 100:
                                    buttons |= mapping.GamepadButton;
                                    break;
                            }
                        }

                        break;
                }
            }

            return buttons;
        }
    }
}