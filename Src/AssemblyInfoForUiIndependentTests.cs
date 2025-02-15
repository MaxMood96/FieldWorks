// Copyright (c) 2012-2019 SIL International
// This software is licensed under the LGPL, version 2.1 or later
// (http://www.gnu.org/licenses/lgpl-2.1.html)

using SIL.LCModel.Core.Attributes;
using SIL.FieldWorks.Common.FwUtils.Attributes;
using SIL.LCModel.Utils.Attributes;
using SIL.TestUtilities;

// This file is for test fixtures for UI independent projects, i.e. projects that don't
// reference System.Windows.Forms et al.

// Cleanup all singletons after running tests
[assembly: CleanupSingletons]

// Redirect HKCU if environment variable BUILDAGENT_SUBKEY is set
[assembly: RedirectHKCU]

// Initialize ICU
[assembly: InitializeIcu(IcuVersion = 70)]

// NOTE: it is important that OfflineSldr comes before InitializeIcu!
// Turns the SLDR API into offline mode
[assembly: OfflineSldr]
