//
// NoAccountsWidget.cs
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
using Clutter;

namespace Synapse.GtkClient.Widgets
{
	public class NoAccountsWidget : Gtk.VBox
	{
		Clutter.Embed     embed;
		Clutter.Actor     bg;
		Clutter.Actor     octy;
		Clutter.Animation anim;
		Clutter.Timeline  timeline;

		public NoAccountsWidget ()
		{
			//this.BorderWidth = 6;
			this.Visible = false;

			embed = new Clutter.Embed();
			embed.Stage.Color = new Clutter.Color(0x03, 0x06, 0x0b);
			this.PackStart(embed, true, true, 0);
			embed.Show();

			var vbox = new Gtk.VBox() {
				Spacing = 6,
				BorderWidth = 6,
			};
			this.PackStart(vbox, false, true, 0);
			vbox.Show();

			var welcome = new Gtk.Label() {
				Markup = "<span color=\"white\" weight=\"bold\">Welcome.</span>",
				Ypad = 2,
				Xalign = 0.0f,
			};
			vbox.PackStart(welcome, false, true, 0);
			welcome.Show();

			var info = new Gtk.Label() {
				Markup = "<span color=\"white\">Click <i>Add Account</i> below to begin.</span>",
				Xalign = 0.0f,
			};
			vbox.PackStart(info, false, true, 0);
			info.Show();
			
			bg = Clutter.GtkUtil.TextureNewFromPixbuf(new Gdk.Pixbuf(null, "oceanbg.png"));
			embed.Stage.Add(bg);
			bg.SetPosition(0, 0);
			bg.Show();

			octy = Clutter.GtkUtil.TextureNewFromPixbuf(new Gdk.Pixbuf(null, "octy.png"));
			embed.Stage.Add(octy);
			octy.SetPosition(15, 60);
			octy.Show();

			timeline = new Clutter.Timeline() {
				Duration = 4000,
				Loop = true,
			};
			anim = new Clutter.Animation() {
				Object = octy,
				Mode = (ulong)AnimationMode.EaseInOutQuad,
				Timeline = timeline,
				Loop = true,
			};
			anim.SetDuration(4000);
			GLib.Value v = new GLib.Value(100.0f);
			anim.Bind("y", v);
			timeline.Completed += delegate {
				if (timeline.Direction == TimelineDirection.Forward)
					timeline.Direction = TimelineDirection.Backward;
				else
					timeline.Direction = TimelineDirection.Forward;
				timeline.Rewind();
			};
			timeline.Start();

			var hbox = new HBox(false, 6) {
				BorderWidth = 6,
			};
			this.PackStart(hbox, false, true, 0);
			hbox.Show();

			var quit = new Gtk.Button() {
				Relief = ReliefStyle.None,
			};
			quit.Clicked += delegate { Gtk.Application.Quit(); };
			var quit_label = new Label() {
				Markup = "<span color=\"white\">_Quit</span>",
				UseUnderline = true,
			};
			quit.Add(quit_label);
			hbox.PackEnd(quit, false, true, 0);
			quit_label.Show();
			quit.Show();

			var add_account = new Gtk.Button() {
				Relief = ReliefStyle.None,
			};
			var add_account_label = new Gtk.Label() {
				Markup = "<span color=\"white\">_Add Account</span>",
				UseUnderline = true,
			};
			add_account.Add(add_account_label);
			hbox.PackEnd(add_account, false, true, 0);
			add_account_label.Show();
			add_account.Show();
		}
	}
}

