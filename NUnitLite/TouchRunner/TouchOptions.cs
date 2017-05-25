// TouchOptions.cs: MonoTouch.Dialog-based options
//
// Authors:
//	Sebastien Pouliot  <sebastien@xamarin.com>
//
// Copyright 2011-2012 Xamarin Inc.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//

using System;

#if XAMCORE_2_0
using Foundation;
using UIKit;
#else
using MonoTouch.Foundation;
using MonoTouch.UIKit;
#endif

#if !__WATCHOS__
using MonoTouch.Dialog;
#endif

using Mono.Options;

namespace MonoTouch.NUnit.UI {
	
	public class TouchOptions {

		static public TouchOptions Current = new TouchOptions ();
		
		public TouchOptions ()
		{
			var defaults = NSUserDefaults.StandardUserDefaults;
			TerminateAfterExecution = defaults.BoolForKey ("execution.autoexit");
			AutoStart = defaults.BoolForKey ("execution.autostart");
			EnableNetwork = defaults.BoolForKey ("network.enabled");
			EnableXml = defaults.BoolForKey ("xml.enabled");
			HostName = defaults.StringForKey ("network.host.name");
			HostPort = (int)defaults.IntForKey ("network.host.port");
			Transport = defaults.StringForKey ("network.transport");
			SortNames = defaults.BoolForKey ("display.sort");
			
			bool b;
			if (bool.TryParse (Environment.GetEnvironmentVariable ("NUNIT_AUTOEXIT"), out b))
				TerminateAfterExecution = b;
			if (bool.TryParse (Environment.GetEnvironmentVariable ("NUNIT_AUTOSTART"), out b))
				AutoStart = b;
			if (bool.TryParse (Environment.GetEnvironmentVariable ("NUNIT_ENABLE_NETWORK"), out b))
				EnableNetwork = b;
			if (!string.IsNullOrEmpty (Environment.GetEnvironmentVariable ("NUNIT_HOSTNAME")))
				HostName = Environment.GetEnvironmentVariable ("NUNIT_HOSTNAME");
			int i;
			if (int.TryParse (Environment.GetEnvironmentVariable ("NUNIT_HOSTPORT"), out i))
				HostPort = i;
			if (bool.TryParse (Environment.GetEnvironmentVariable ("NUNIT_SORTNAMES"), out b))
				SortNames = b;
			if (!string.IsNullOrEmpty (Environment.GetEnvironmentVariable ("NUNIT_TRANSPORT")))
				Transport = Environment.GetEnvironmentVariable ("NUNIT_TRANSPORT");
			if (bool.TryParse (Environment.GetEnvironmentVariable ("NUNIT_ENABLE_XML_OUTPUT"), out b))
				EnableXml = b;
			if (bool.TryParse (Environment.GetEnvironmentVariable ("NUNIT_SKIP_LOG_HEADER"), out b))
			    SkipLogHeader = b;

			var os = new OptionSet () {
				{ "autoexit", "If the app should exit once the test run has completed.", v => TerminateAfterExecution = true },
				{ "autostart", "If the app should automatically start running the tests.", v => AutoStart = true },
				{ "hostname=", "Comma-separated list of host names or IP address to (try to) connect to", v => HostName = v },
				{ "hostport=", "HTTP/TCP port to connect to.", v => HostPort = int.Parse (v) },
				{ "enablenetwork", "Enable the network reporter.", v => EnableNetwork = true },
				{ "transport=", "Select transport method. Either TCP (default) or HTTP.", v => Transport = v },
				{ "enablexml", "Enable the xml reported.", v => EnableXml = false },
				{ "skiplogheader", "Do not write extra device related information to the logs (default: false)", v => SkipLogHeader = true },
			};
			
			try {
				os.Parse (Environment.GetCommandLineArgs ());
			} catch (OptionException oe) {
				Console.WriteLine ("{0} for options '{1}'", oe.Message, oe.OptionName);
			}
		}
		
		private bool EnableNetwork { get; set; }

		public bool EnableXml { get; set; }

		public bool SkipLogHeader { get; set; }
		
		public string HostName { get; private set; }
		
		public int HostPort { get; private set; }
		
		public bool AutoStart { get; set; }
		
		public bool TerminateAfterExecution { get; set; }
		
		public string Transport { get; set; } = "TCP";

		public bool ShowUseNetworkLogger {
			get { return (EnableNetwork && !String.IsNullOrWhiteSpace (HostName) && (HostPort > 0)); }
		}

		public bool SortNames { get; set; }
		
#if !__WATCHOS__
		[CLSCompliant (false)]
		public UIViewController GetViewController ()
		{
#if TVOS
			var network = new StringElement (string.Format ("Enabled: {0}", EnableNetwork));
#else
			var network = new BooleanElement ("Enable", EnableNetwork);
#endif

			var host = new EntryElement ("Host Name", "name", HostName);
			host.KeyboardType = UIKeyboardType.ASCIICapable;
			
			var port = new EntryElement ("Port", "name", HostPort.ToString ());
			port.KeyboardType = UIKeyboardType.NumberPad;
			
#if TVOS
			var sort = new StringElement (string.Format ("Sort Names: ", SortNames));
#else
			var sort = new BooleanElement ("Sort Names", SortNames);
#endif

			var root = new RootElement ("Options") {
				new Section ("Remote Server") { network, host, port },
				new Section ("Display") { sort }
			};
				
			var dv = new DialogViewController (root, true) { Autorotate = true };
			dv.ViewDisappearing += delegate {
#if !TVOS
				EnableNetwork = network.Value;
#endif
				HostName = host.Value;
				ushort p;
				if (UInt16.TryParse (port.Value, out p))
					HostPort = p;
				else
					HostPort = -1;
#if !TVOS
				SortNames = sort.Value;
#endif
				
				var defaults = NSUserDefaults.StandardUserDefaults;
				defaults.SetBool (EnableNetwork, "network.enabled");
				defaults.SetString (HostName ?? String.Empty, "network.host.name");
				defaults.SetInt (HostPort, "network.host.port");
				defaults.SetBool (SortNames, "display.sort");
			};
			
			return dv;
		}
#endif // !__WATCHOS__
	}
}
