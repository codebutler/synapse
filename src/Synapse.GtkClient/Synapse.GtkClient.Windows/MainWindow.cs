//
// MainWindow.cs
// 
// Copyright (C) 2010 Eric Butler
//
// Authors:
//   Eric Butler <eric@codebutler.com>
//   Christian Hergert <chris@dronelabs.com>
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
using Gtk;
using Synapse.ServiceStack;
using Synapse.Xmpp;
using Synapse.Xmpp.Services;
using Synapse.GtkClient.Widgets;

namespace Synapse.GtkClient.Windows
{
	public class MainWindow : Window
	{
		NoAccountsWidget m_NoAccountsWidget;
		RosterWidget     m_RosterWidget;
		
		public MainWindow () : base (WindowType.Toplevel)
		{
			base.Title = "Synapse";
			base.Decorated = false;
			base.DefaultHeight = 480;
			base.DefaultWidth = 235;
			base.ModifyBg(StateType.Normal, new Gdk.Color(0x03, 0x06, 0x0b));

			var vbox = new Gtk.VBox();
			base.Add(vbox);
			vbox.Show();

			var ebox = new Gtk.EventBox();
            ebox.ModifyBg(StateType.Normal, new Gdk.Color(0x03, 0x06, 0x0b));
            vbox.PackStart(ebox, false, true, 0);
            ebox.ButtonPressEvent += delegate(object o, Gtk.ButtonPressEventArgs e) {
                base.BeginMoveDrag((int)e.Event.Button,
                                  (int)e.Event.XRoot,
                                  (int)e.Event.YRoot,
                                  e.Event.Time);
            };
            ebox.Show();

            var hbox = new Gtk.HBox();
            ebox.Add(hbox);
            hbox.Show();
			

			var spacer = new Gtk.Label() {
				WidthRequest = 32,
			};
			hbox.PackStart(spacer, false, true, 0);
			spacer.Show();

			var title = new Gtk.Label() {
				Ypad = 3,
				Markup = "<span color=\"white\" weight=\"bold\">Synapse</span>",
			};
			hbox.PackStart(title, true, true, 0);
			title.Show();

			var minimize = new EventBox() {
				WidthRequest = 16,
			};
			minimize.ModifyBg(StateType.Normal, new Gdk.Color(0x03, 0x06, 0x0b));
			hbox.PackStart(minimize, false, true, 0);
			minimize.Show();

			var min_img = new Gtk.Image(null, "menu-icon.png");
			minimize.Add(min_img);
			min_img.Show();

			var close = new EventBox() {
				WidthRequest = 16,
			};
			close.ButtonPressEvent += delegate {
				Gtk.Application.Quit();
			};
			close.ModifyBg(StateType.Normal, new Gdk.Color(0x03, 0x06, 0x0b));
			hbox.PackStart(close, false, true, 0);
			close.Show();

			var close_img = new Gtk.Image(null, "stock-close_12.png");
			close.Add(close_img);
			close_img.Show();

			m_NoAccountsWidget = new NoAccountsWidget();
			vbox.PackStart(m_NoAccountsWidget);
			
			m_RosterWidget = new RosterWidget();
			vbox.PackStart(m_RosterWidget);
			
			var accountService = ServiceManager.Get<AccountService>();
			accountService.AccountAdded += AccountAdded;
			accountService.AccountRemoved += AccountRemoved;
			foreach (Account account in accountService.Accounts)
				AccountAdded(account);
			
			HideShowNoAccountsWidget();
		}
		
		void AccountAdded (Account account)
		{
			Gtk.Application.Invoke(delegate {
				m_RosterWidget.AddAccount(account);
				HideShowNoAccountsWidget();
			});
		}
		
		void AccountRemoved (Account account)
		{
			Gtk.Application.Invoke(delegate {
				m_RosterWidget.RemoveAccount(account);
				HideShowNoAccountsWidget();
			});
		}
		
		void HideShowNoAccountsWidget ()
		{
			if (m_RosterWidget.AccountsCount > 0) {
				// FIXME: Clear max width
				m_NoAccountsWidget.Hide();
				m_RosterWidget.Show();
			} else {
				// FIXME: Set max width to 384px
				m_RosterWidget.Hide();
				m_NoAccountsWidget.Show();
			}
		}
	}
}

