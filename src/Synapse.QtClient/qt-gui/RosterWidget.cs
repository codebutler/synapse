// ------------------------------------------------------------------------------
//  <autogenerated>
//      This code was generated by a tool.
//      Mono Runtime Version: 2.0.50727.42
// 
//      Changes to this file may cause incorrect behavior and will be lost if 
//      the code is regenerated.
//  </autogenerated>
// ------------------------------------------------------------------------------

using System;
using Qyoto;


public partial class RosterWidget : QWidget {
    
    protected QWidget m_AccountsContainer;
    
    protected QSplitter splitter;
    
    protected QWidget widget;
    
    protected QTabWidget tabWidget;
    
    protected QWidget friendsTab;
    
    protected Synapse.QtClient.Widgets.AvatarGrid<Synapse.UI.AccountItemPair> rosterGrid;
    
    protected QLabel statsLabel;
    
    protected QSlider rosterIconSizeSlider;
    
    protected QWidget chatroomsTab;
    
    protected QLabel label;
    
    protected QLineEdit m_ChatNameEdit;
    
    protected QPushButton m_JoinChatButton;
    
    protected QTreeView mucTree;
    
    protected QWidget activityTab;
    
    protected QPushButton m_ShoutButton;
    
    protected QPushButton m_PostLinkButton;
    
    protected QPushButton m_PostFileButton;
    
    protected QWidget shoutContainer;
    
    protected QLineEdit shoutLineEdit;
    
    protected QLabel shoutCharsLabel_2;
    
    protected QWebView m_ActivityWebView;
    
    protected void SetupUi() {
        base.ObjectName = "RosterWidget";
        this.Geometry = new QRect(0, 0, 319, 612);
        this.WindowTitle = "RosterWidget";
        QVBoxLayout verticalLayout_6;
        verticalLayout_6 = new QVBoxLayout(this);
        verticalLayout_6.Spacing = 0;
        verticalLayout_6.Margin = 0;
        this.m_AccountsContainer = new QWidget(this);
        this.m_AccountsContainer.ObjectName = "m_AccountsContainer";
        QSizePolicy m_AccountsContainer_sizePolicy;
        m_AccountsContainer_sizePolicy = new QSizePolicy(QSizePolicy.Policy.Preferred, QSizePolicy.Policy.Preferred);
        m_AccountsContainer_sizePolicy.SetVerticalStretch(0);
        m_AccountsContainer_sizePolicy.SetHorizontalStretch(0);
        m_AccountsContainer_sizePolicy.SetHeightForWidth(this.m_AccountsContainer.SizePolicy.HasHeightForWidth());
        this.m_AccountsContainer.SizePolicy = m_AccountsContainer_sizePolicy;
        verticalLayout_6.AddWidget(this.m_AccountsContainer);
        this.splitter = new QSplitter(this);
        this.splitter.ObjectName = "splitter";
        this.splitter.Orientation = Qt.Orientation.Vertical;
        verticalLayout_6.AddWidget(this.splitter);
        this.widget = new QWidget(this.splitter);
        this.widget.ObjectName = "widget";
        QVBoxLayout verticalLayout_2;
        verticalLayout_2 = new QVBoxLayout(this.widget);
        verticalLayout_2.Margin = 0;
        this.tabWidget = new QTabWidget(this.widget);
        this.tabWidget.ObjectName = "tabWidget";
        this.tabWidget.tabPosition = QTabWidget.TabPosition.South;
        this.tabWidget.tabShape = QTabWidget.TabShape.Rounded;
        this.tabWidget.CurrentIndex = 0;
        this.tabWidget.UsesScrollButtons = false;
        verticalLayout_2.AddWidget(this.tabWidget);
        this.friendsTab = new QWidget(this.tabWidget);
        this.friendsTab.ObjectName = "friendsTab";
        QVBoxLayout verticalLayout_4;
        verticalLayout_4 = new QVBoxLayout(this.friendsTab);
        verticalLayout_4.Spacing = 0;
        verticalLayout_4.Margin = 0;
        this.rosterGrid = new Synapse.QtClient.Widgets.AvatarGrid<Synapse.UI.AccountItemPair>(this.friendsTab);
        this.rosterGrid.ObjectName = "rosterGrid";
        this.rosterGrid.FrameShape = QFrame.Shape.NoFrame;
        this.rosterGrid.Alignment = ((global::Qyoto.Qyoto.GetCPPEnumValue("Qt", "AlignLeading") | global::Qyoto.Qyoto.GetCPPEnumValue("Qt", "AlignLeft")) | global::Qyoto.Qyoto.GetCPPEnumValue("Qt", "AlignTop"));
        verticalLayout_4.AddWidget(this.rosterGrid);
        QHBoxLayout horizontalLayout;
        horizontalLayout = new QHBoxLayout();
        verticalLayout_4.AddLayout(horizontalLayout);
        horizontalLayout.Spacing = 6;
        this.statsLabel = new QLabel(this.friendsTab);
        this.statsLabel.ObjectName = "statsLabel";
        QSizePolicy statsLabel_sizePolicy;
        statsLabel_sizePolicy = new QSizePolicy(QSizePolicy.Policy.Expanding, QSizePolicy.Policy.Preferred);
        statsLabel_sizePolicy.SetVerticalStretch(0);
        statsLabel_sizePolicy.SetHorizontalStretch(0);
        statsLabel_sizePolicy.SetHeightForWidth(this.statsLabel.SizePolicy.HasHeightForWidth());
        this.statsLabel.SizePolicy = statsLabel_sizePolicy;
        this.statsLabel.Text = "";
        horizontalLayout.AddWidget(this.statsLabel);
        this.rosterIconSizeSlider = new QSlider(this.friendsTab);
        this.rosterIconSizeSlider.ObjectName = "rosterIconSizeSlider";
        QSizePolicy rosterIconSizeSlider_sizePolicy;
        rosterIconSizeSlider_sizePolicy = new QSizePolicy(QSizePolicy.Policy.Fixed, QSizePolicy.Policy.Fixed);
        rosterIconSizeSlider_sizePolicy.SetVerticalStretch(0);
        rosterIconSizeSlider_sizePolicy.SetHorizontalStretch(0);
        rosterIconSizeSlider_sizePolicy.SetHeightForWidth(this.rosterIconSizeSlider.SizePolicy.HasHeightForWidth());
        this.rosterIconSizeSlider.SizePolicy = rosterIconSizeSlider_sizePolicy;
        this.rosterIconSizeSlider.Minimum = 16;
        this.rosterIconSizeSlider.Maximum = 60;
        this.rosterIconSizeSlider.SingleStep = 1;
        this.rosterIconSizeSlider.Orientation = Qt.Orientation.Horizontal;
        this.rosterIconSizeSlider.InvertedAppearance = false;
        this.rosterIconSizeSlider.InvertedControls = false;
        horizontalLayout.AddWidget(this.rosterIconSizeSlider);
        this.tabWidget.AddTab(this.friendsTab, "Friends");
        this.chatroomsTab = new QWidget(this.tabWidget);
        this.chatroomsTab.ObjectName = "chatroomsTab";
        QVBoxLayout verticalLayout_3;
        verticalLayout_3 = new QVBoxLayout(this.chatroomsTab);
        verticalLayout_3.Spacing = 0;
        verticalLayout_3.Margin = 0;
        QGridLayout gridLayout;
        gridLayout = new QGridLayout();
        verticalLayout_3.AddLayout(gridLayout);
        gridLayout.Margin = 6;
        gridLayout.Spacing = 6;
        this.label = new QLabel(this.chatroomsTab);
        this.label.ObjectName = "label";
        this.label.Text = "Join Conference Room:";
        gridLayout.AddWidget(this.label, 0, 0, 1, 1);
        this.m_ChatNameEdit = new QLineEdit(this.chatroomsTab);
        this.m_ChatNameEdit.ObjectName = "m_ChatNameEdit";
        this.m_ChatNameEdit.MaxLength = 150;
        gridLayout.AddWidget(this.m_ChatNameEdit, 1, 0, 1, 1);
        this.m_JoinChatButton = new QPushButton(this.chatroomsTab);
        this.m_JoinChatButton.ObjectName = "m_JoinChatButton";
        this.m_JoinChatButton.Text = "Join";
        gridLayout.AddWidget(this.m_JoinChatButton, 1, 1, 1, 1);
        this.mucTree = new QTreeView(this.chatroomsTab);
        this.mucTree.ObjectName = "mucTree";
        this.mucTree.FrameShape = QFrame.Shape.NoFrame;
        this.mucTree.Animated = true;
        this.mucTree.HeaderHidden = true;
        verticalLayout_3.AddWidget(this.mucTree);
        this.tabWidget.AddTab(this.chatroomsTab, "Conferences");
        this.activityTab = new QWidget(this.tabWidget);
        this.activityTab.ObjectName = "activityTab";
        QVBoxLayout verticalLayout;
        verticalLayout = new QVBoxLayout(this.activityTab);
        verticalLayout.Spacing = 0;
        verticalLayout.sizeConstraint = QLayout.SizeConstraint.SetDefaultConstraint;
        verticalLayout.Margin = 0;
        QHBoxLayout horizontalLayout_2;
        horizontalLayout_2 = new QHBoxLayout();
        verticalLayout.AddLayout(horizontalLayout_2);
        horizontalLayout_2.Spacing = 12;
        horizontalLayout_2.Margin = 6;
        QSpacerItem horizontalSpacer;
        horizontalSpacer = new QSpacerItem(40, 20, QSizePolicy.Policy.Expanding, QSizePolicy.Policy.Minimum);
        horizontalLayout_2.AddItem(horizontalSpacer);
        this.m_ShoutButton = new QPushButton(this.activityTab);
        this.m_ShoutButton.ObjectName = "m_ShoutButton";
        this.m_ShoutButton.StyleSheet = "";
        this.m_ShoutButton.Text = "Shout!";
        this.m_ShoutButton.icon = new QIcon("../../../../../usr/share/icons/gnome/16x16/actions/insert-text.png../../../../../usr/share/icons/gnome/16x16/actions/insert-text.png");
        this.m_ShoutButton.Checkable = true;
        this.m_ShoutButton.AutoExclusive = false;
        this.m_ShoutButton.Flat = true;
        horizontalLayout_2.AddWidget(this.m_ShoutButton);
        this.m_PostLinkButton = new QPushButton(this.activityTab);
        this.m_PostLinkButton.ObjectName = "m_PostLinkButton";
        this.m_PostLinkButton.Text = "Post Link";
        this.m_PostLinkButton.icon = new QIcon("../../../../../usr/share/icons/gnome/16x16/actions/insert-link.png../../../../../usr/share/icons/gnome/16x16/actions/insert-link.png");
        this.m_PostLinkButton.Checkable = true;
        this.m_PostLinkButton.AutoExclusive = false;
        this.m_PostLinkButton.Flat = true;
        horizontalLayout_2.AddWidget(this.m_PostLinkButton);
        this.m_PostFileButton = new QPushButton(this.activityTab);
        this.m_PostFileButton.ObjectName = "m_PostFileButton";
        this.m_PostFileButton.Text = "Post File";
        this.m_PostFileButton.icon = new QIcon("../../../../../usr/share/icons/gnome/16x16/actions/insert-image.png../../../../../usr/share/icons/gnome/16x16/actions/insert-image.png");
        this.m_PostFileButton.Checkable = true;
        this.m_PostFileButton.AutoExclusive = false;
        this.m_PostFileButton.Flat = true;
        horizontalLayout_2.AddWidget(this.m_PostFileButton);
        QSpacerItem horizontalSpacer_2;
        horizontalSpacer_2 = new QSpacerItem(40, 20, QSizePolicy.Policy.Expanding, QSizePolicy.Policy.Minimum);
        horizontalLayout_2.AddItem(horizontalSpacer_2);
        this.shoutContainer = new QWidget(this.activityTab);
        this.shoutContainer.ObjectName = "shoutContainer";
        QHBoxLayout horizontalLayout_3;
        horizontalLayout_3 = new QHBoxLayout(this.shoutContainer);
        horizontalLayout_3.Spacing = 6;
        horizontalLayout_3.SetContentsMargins(6, 0, 6, 6);
        this.shoutLineEdit = new QLineEdit(this.shoutContainer);
        this.shoutLineEdit.ObjectName = "shoutLineEdit";
        this.shoutLineEdit.MaxLength = 150;
        horizontalLayout_3.AddWidget(this.shoutLineEdit);
        this.shoutCharsLabel_2 = new QLabel(this.shoutContainer);
        this.shoutCharsLabel_2.ObjectName = "shoutCharsLabel_2";
        this.shoutCharsLabel_2.Text = "0";
        horizontalLayout_3.AddWidget(this.shoutCharsLabel_2);
        verticalLayout.AddWidget(this.shoutContainer);
        this.m_ActivityWebView = new QWebView(this.activityTab);
        this.m_ActivityWebView.ObjectName = "m_ActivityWebView";
        this.m_ActivityWebView.Url = new QUrl("about:blank");
        verticalLayout.AddWidget(this.m_ActivityWebView);
        this.tabWidget.AddTab(this.activityTab, "Activity");
        this.splitter.AddWidget(this.widget);
        QObject.Connect(m_ShoutButton, Qt.SIGNAL("toggled(bool)"), shoutContainer, Qt.SLOT("setShown(bool)"));
        QObject.Connect(m_ChatNameEdit, Qt.SIGNAL("returnPressed()"), m_JoinChatButton, Qt.SLOT("click()"));
        QMetaObject.ConnectSlotsByName(this);
    }
}
