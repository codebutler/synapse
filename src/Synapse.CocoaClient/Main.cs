//
// Main.cs
//
// Copyright (C) 2010 Eric Butler
//
// Authors:
//   Eric Butler <eric@codebutler.com>
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License

using System;
using Monobjc;
using Monobjc.Cocoa;
using Hyena;
using Synapse.Core;
using Synapse.ServiceStack;
using Synapse.Services;
using Synapse.Xmpp;
using Synapse.Xmpp.Services;

namespace Synapse.CocoaClient
{
	public static class Application
	{
		static void Main (string[] args)
		{
			ObjectiveCRuntime.LoadFramework("Cocoa");
			ObjectiveCRuntime.Initialize();
			
			NSApplication.Bootstrap();
			NSApplication.LoadNib("MainMenu.nib");
			NSApplication.RunApplication();
		}
	}
}
