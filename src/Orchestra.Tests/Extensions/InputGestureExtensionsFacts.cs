﻿namespace Orchestra.Tests
{
    using System.Windows.Input;
    using Catel.Windows.Input;
    using NUnit.Framework;
    using InputGesture = Catel.Windows.Input.InputGesture;

    [TestFixture]
    public class InputGestureExtensionsFacts
    {
        [TestCase(Key.A, ModifierKeys.Control, false)]
        [TestCase(Key.A, ModifierKeys.Shift, false)]
        [TestCase(Key.A, ModifierKeys.None, false)]
        [TestCase(Key.None, ModifierKeys.Control, false)]
        [TestCase(Key.None, ModifierKeys.None, true)]
        public void TheIsEmptyMethod(Key key, ModifierKeys modifierKeys, bool expectedValue)
        {
            var inputGesture = new InputGesture(key, modifierKeys);

            Assert.That(inputGesture.IsEmpty(), Is.EqualTo(expectedValue));
        }
    }
}