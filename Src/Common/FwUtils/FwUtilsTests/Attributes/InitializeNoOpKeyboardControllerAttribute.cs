// Copyright (c) 2013 SIL International
// This software is licensed under the LGPL, version 2.1 or later
// (http://www.gnu.org/licenses/lgpl-2.1.html)

using System;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using SIL.Keyboarding;
using SIL.Windows.Forms.Keyboarding;

namespace SIL.FieldWorks.Common.FwUtils.Attributes
{
	/// ----------------------------------------------------------------------------------------
	/// <summary>
	/// NUnit helper attribute that creates a dummy keyboard controller. This is suitable for
	/// unit tests that don't test any keyboarding functions. A test or suite that requires
	/// feedback from the real keyboard controller should use the
	/// InitializeRealKeyboardControllerAttribute instead.
	/// </summary>
	/// ----------------------------------------------------------------------------------------
	[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class |
		AttributeTargets.Interface | AttributeTargets.Method, AllowMultiple = true)]
	public class InitializeNoOpKeyboardControllerAttribute: TestActionAttribute
	{
		/// <summary>
		/// Create a dummy keyboard controller
		/// </summary>
		public override void BeforeTest(ITest test)
		{
			base.BeforeTest(test);
			// If we already have a keyboard controller we'd better dispose it or we'll end up with missing dispose calls.
			if (Keyboard.Controller != null)
				Keyboard.Controller.Dispose();

			KeyboardController.Initialize(new DummyKeyboardAdaptor());

		}

		/// <summary>
		/// Unset keyboard controller
		/// </summary>
		public override void AfterTest(ITest test)
		{
			// Shut down (and implicitly dispose) the keyboard controller we created.

			base.AfterTest(test);
			KeyboardController.Shutdown();
			Keyboard.Controller = new DefaultKeyboardController();
		}
	}
}
