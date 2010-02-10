//
// NetworkService.cs
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
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System;
using Synapse.ServiceStack;
using Hyena;
using Mono.Addins;

namespace Synapse.Services
{
	public delegate void NetworkStateChangeHandler (NetworkState state);
	
    public enum NetworkState : uint 
	{
        Unknown = 0,
        Asleep,
        Connecting,
        Connected,
        Disconnected
    }	
	
	public interface INetworkProvider
	{
		event NetworkStateChangeHandler StateChanged;
		NetworkState State { get; }
	}
	
	public class NetworkService : IService, IDelayedInitializeService
	{
		INetworkProvider m_Provider;
		
		public event NetworkStateChangeHandler StateChanged;
		
		public void DelayedInitialize ()
		{
			foreach (TypeExtensionNode node in AddinManager.GetExtensionNodes("/Synapse/PlatformServices/NetworkProvider")) {
				try {
					m_Provider = (INetworkProvider)node.CreateInstance(typeof(INetworkProvider));
					m_Provider.StateChanged += HandleNetworkStateChanged;
					Log.DebugFormat("Loaded INetworkProvider: {0}", m_Provider.GetType().FullName);
					break;
				} catch (Exception ex) {
					Log.Exception("INetworkProvider extension failed to load", ex);
				}
			}
		}
		
		public NetworkState State {
			get { return m_Provider.State; }
		}
		
		void HandleNetworkStateChanged (NetworkState state)
		{
			if (StateChanged != null)
				StateChanged(state);
		}
		
		string IService.ServiceName {
			get { return "NetworkService"; }
		}
	}
}
