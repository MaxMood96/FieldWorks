// Copyright (c) 2012-2013 SIL International
// This software is licensed under the LGPL, version 2.1 or later
// (http://www.gnu.org/licenses/lgpl-2.1.html)

using System;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using SIL.FieldWorks.Common.FwUtils;

namespace SIL.FieldWorks.Common.FwUtils.Attributes
{
	/// ----------------------------------------------------------------------------------------
	/// <summary>
	/// NUnit helper attribute that sets the message box adapter before running tests and
	/// resets it afterwards
	/// </summary>
	/// ----------------------------------------------------------------------------------------
	[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class |
		AttributeTargets.Interface | AttributeTargets.Method, AllowMultiple = true)]
	public class SetMessageBoxAdapterAttribute : TestActionAttribute
	{
		private static IMessageBox s_CurrentAdapter;
		private IMessageBox m_PreviousAdapter;
		private Type m_AdapterType;

		/// <summary/>
		public SetMessageBoxAdapterAttribute(): this(typeof(MessageBoxStub))
		{
		}

		/// <summary/>
		public SetMessageBoxAdapterAttribute(Type adapterType)
		{
			m_AdapterType = adapterType;
		}

		/// <summary>
		/// Set the message box adapter
		/// </summary>
		public override void BeforeTest(ITest test)
		{
			base.BeforeTest(test);
			m_PreviousAdapter = s_CurrentAdapter;
			s_CurrentAdapter = (IMessageBox)Activator.CreateInstance(m_AdapterType);
			MessageBoxUtils.Manager.SetMessageBoxAdapter(s_CurrentAdapter);
		}

		/// <summary>
		/// Restore previous message box adapter
		/// </summary>
		public override void AfterTest(ITest test)
		{
			base.AfterTest(test);

			s_CurrentAdapter = m_PreviousAdapter;
			if (s_CurrentAdapter != null)
				MessageBoxUtils.Manager.SetMessageBoxAdapter(s_CurrentAdapter);
			else
				MessageBoxUtils.Manager.Reset();
		}
	}
}
