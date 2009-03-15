
using System;

using Qyoto;

using Synapse.Core;

namespace Synapse.QtClient.Widgets
{	
	public class MainTabWidget : QWidget
	{
		QStackedWidget m_Pages;
		QStyle m_Style;
		TabBar         m_TabBar;
		
		public MainTabWidget(QWidget parent) : base (parent)
		{
			var layout = new QVBoxLayout(this);
			layout.Spacing = 0;
			layout.Margin = 0;
			
			m_Pages = new QStackedWidget(this);
			m_Pages.Layout().Margin = 0;
			layout.AddWidget(m_Pages, 1);
			
			m_TabBar = new TabBar(this);
			layout.AddWidget(m_TabBar, 0);
		}
		
		public int CurrentIndex {
			get {
				return m_Pages.CurrentIndex;
			}
			set {
				m_Pages.CurrentIndex = value;
				m_TabBar.CurrentIndex = value;
			}
		}
		
		public QTabWidget.TabPosition tabPosition {
			get {
				return QTabWidget.TabPosition.South;
			}
			set {
			}	
		}
		
		public void AddTab (QWidget page, string label)
		{
			m_Pages.AddWidget(page);
			m_TabBar.AddTab(label);
			CurrentIndex = CurrentIndex;
		}
		
		class TabBar : QFrame
		{
			Tab m_FirstTab, m_LastTab;
			
			public TabBar (MainTabWidget parent) : base (parent)
			{				
				base.SetStyleSheet(Util.ReadResource("mainwindow-tabs.qss"));
				var layout = new QHBoxLayout(this);
				layout.sizeConstraint = QLayout.SizeConstraint.SetNoConstraint;
				layout.Spacing = 0;
				layout.Margin = 0;
				layout.AddStretch(1);
				layout.AddWidget(new QSizeGrip(this), 0, (uint)AlignmentFlag.AlignBottom);
				
				base.MinimumWidth = 0;
			}
			
			public int CurrentIndex {
				set {
					if (value < 0 || value > base.Layout().Count() - 2)
						return;
					
					for (int x = 0; x < base.Layout().Count(); x++) {
						var tab = (base.Layout().ItemAt(x).Widget() as Tab);;
						if (tab != null)
							tab.Selected = (x == value);
					}
				}
			}
			
			public void AddTab (string text)
			{
				var layout = (QHBoxLayout)base.Layout();

				var tab = new Tab(text, this);
				tab.Clicked += HandleTabClicked;
				((QHBoxLayout)base.Layout()).InsertWidget(layout.Count() - 2, tab, 0);

				if (m_LastTab != null)
					m_LastTab.Last = false;
				
				if (m_FirstTab == null) {
					tab.First = true;
					m_FirstTab = tab;
				}
				
				tab.Last = true;
				m_LastTab = tab;
			}
			
			public void ReloadStylesheet ()
			{
				base.StyleSheet = base.StyleSheet;
			}
			
			void HandleTabClicked (object o, EventArgs args)
			{
				var index = base.Layout().IndexOf((QWidget)o);
				((MainTabWidget)base.ParentWidget()).CurrentIndex = index;
			}
			
			class Tab : QLabel 
			{
				bool m_First = false;
				bool m_Last = false;
				bool m_Selected = false;
				
				public event EventHandler Clicked;
				
				public Tab (string text, QWidget parent) : base (text, parent)
				{
					base.Alignment = (uint)QLabel.AlignmentFlag.AlignLeft | (uint)QLabel.AlignmentFlag.AlignVCenter;
					base.MinimumWidth = 32;
				}
				
				[Q_PROPERTY("bool", "Selected")]
				public bool Selected {
					get {
						return m_Selected;
					}
					set {
						m_Selected = value;
						((TabBar)base.Parent()).ReloadStylesheet();
					}
				}
				
				[Q_PROPERTY("bool", "First")]
				public bool First {
					get {
						return m_First;
					}
					set {
						m_First = value;
						((TabBar)base.Parent()).ReloadStylesheet();
					}
				}				
				
				[Q_PROPERTY("bool", "Last")]
				public bool Last {
					get {
						return m_Last;
					}
					set {
						m_Last = value;
						((TabBar)base.Parent()).ReloadStylesheet();
					}
				}
				
				protected override void MousePressEvent (Qyoto.QMouseEvent ev)
				{
					base.MousePressEvent (ev);
					
					if (ev.Button() == MouseButton.LeftButton) {
						var evnt = Clicked;
						if (evnt != null)
							Clicked(this, EventArgs.Empty);
					}
				}
			}
		}
	}
}
