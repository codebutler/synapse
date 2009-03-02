
using System;

using Qyoto;

namespace Synapse.QtClient.Widgets
{	
	public class MainTabWidget : QWidget
	{
		QStackedWidget m_Pages;
		TabBar         m_TabBar;
		
		public MainTabWidget(QWidget parent) : base (parent)
		{
			var layout = new QVBoxLayout(this);
			//layout.sizeConstraint  = QLayout.SizeConstraint.SetNoConstraint;
			layout.Spacing = 0;
			layout.Margin = 0;
			
			m_Pages = new QStackedWidget(this);
			m_Pages.Layout().Margin = 0; 
			//m_Pages.Layout().sizeConstraint = QLayout.SizeConstraint.SetDefaultConstraint;
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
			public TabBar (MainTabWidget parent) : base (parent)
			{				
				var layout = new QHBoxLayout(this);
				layout.sizeConstraint = QLayout.SizeConstraint.SetNoConstraint;
				layout.Spacing = 0;
				layout.SetContentsMargins(10, 0, 10, 5);
				layout.AddStretch(1);
				
				base.MinimumWidth = 0;
			}
			
			public int CurrentIndex {
				set {
					for (int x = 0; x < base.Layout().Count(); x++) {
						var tab = (Tab)base.Layout().ItemAt(x).Widget();
						if (tab != null)
							tab.Selected = (x == value);
					}
				}
			}
			
			public void AddTab (string text)
			{
				var layout = (QHBoxLayout)base.Layout();
				var stretch = layout.TakeAt(layout.Count() -1);

				var tab = new Tab(text, this);
				tab.Clicked += HandleTabClicked;
				((QHBoxLayout)base.Layout()).AddWidget(tab, 0);

				layout.AddItem(stretch);
			}
			
			void HandleTabClicked (object o, EventArgs args)
			{
				var index = base.Layout().IndexOf((QWidget)o);
				((MainTabWidget)base.ParentWidget()).CurrentIndex = index;
			}
			
			class Tab : QLabel 
			{
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
						base.Update();
					}
				}
				
				protected override void PaintEvent (Qyoto.QPaintEvent arg1)
				{
					using (QStylePainter painter = new QStylePainter(this)) {
						
						//QStyleOptionFrame opt = new QStyleOptionFrame();
						QStyleOptionTab opt = new QStyleOptionTab();
						
						// FIXME: if foo opt.position = QStyleOptionTab.TabPosition.End;
						
						opt.InitFrom(this);
						
						opt.State = m_Selected ? (uint)QStyle.StateFlag.State_Selected : (uint)QStyle.StateFlag.State_None;
						
						//if () {
							opt.State |= (uint)QStyle.SubControl.SC_ScrollBarLast;
						//}
						
						painter.DrawPrimitive(QStyle.PrimitiveElement.PE_Widget, opt);
						painter.DrawPrimitive(QStyle.PrimitiveElement.PE_Frame, opt);						
						
						var textWidth = base.FontMetrics().Width(base.Text);
						if (base.ContentsRect().Width() >= textWidth) {
							painter.DrawItemText(base.ContentsRect(), 
						                     	(int)TextFlag.TextSingleLine | (int) AlignmentFlag.AlignHCenter, 
						                     	base.Palette, true, base.Text, base.ForegroundRole());
						} else {
							
						}
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
