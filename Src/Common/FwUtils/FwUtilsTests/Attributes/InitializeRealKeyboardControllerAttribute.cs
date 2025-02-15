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
	/// NUnit helper attribute that initializes the real keyboard controller before the tests and
	/// calls shutdown afterwards. If InitDummyAfterTests is <c>true</c> a dummy keyboard
	/// controller will be created after the test or suite has finished, i.e. in the AfterTest
	/// method.
	/// </summary>
	/// ----------------------------------------------------------------------------------------
	[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class |
		AttributeTargets.Interface | AttributeTargets.Method, AllowMultiple = true)]
	public class InitializeRealKeyboardControllerAttribute: TestActionAttribute
	{
		/// <summary>
		/// Initialize keyboard controller
		/// </summary>
		public override void BeforeTest(ITest test)
		{
			base.BeforeTest(test);
			if (Keyboard.Controller != null)
				Keyboard.Controller.Dispose();

			KeyboardController.Initialize();
			base.BeforeTest(test);
		}

		/// <summary>
		/// Shutdown keyboard controller
		/// </summary>
		public override void AfterTest(ITest test)
		{
			base.AfterTest(test);
			KeyboardController.Shutdown();
			Keyboard.Controller = new DefaultKeyboardController();
		}
	}
}
