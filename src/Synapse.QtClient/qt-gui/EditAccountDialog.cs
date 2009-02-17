// ------------------------------------------------------------------------------
//  <autogenerated>
//      This code was generated by a tool.
//      Mono Runtime Version: 2.0.50727.42
// 
//      Changes to this file may cause incorrect behavior and will be lost if 
//      the code is regenerated.
//  </autogenerated>
// ------------------------------------------------------------------------------

namespace Synapse.QtClient.Windows {
    using System;
    using Qyoto;
    
    
    public partial class EditAccountDialog : QDialog {
        
        protected QTabWidget tabWidget;
        
        protected QWidget tab;
        
        protected QLabel label;
        
        protected QLineEdit jidLineEdit;
        
        protected QLabel label_2;
        
        protected QLineEdit passwordLineEdit;
        
        protected QLabel label_3;
        
        protected QComboBox resourceCombo;
        
        protected QWidget widget;
        
        protected QPushButton pushButton;
        
        protected QWidget tab_4;
        
        protected QLabel label_5;
        
        protected QLabel label_4;
        
        protected QLineEdit serverLineEdit;
        
        protected QLabel label_6;
        
        protected QSpinBox portSpinBox;
        
        protected QLabel label_7;
        
        protected QWidget tab_2;
        
        protected QCheckBox autoConnectCheckBox;
        
        protected QDialogButtonBox buttonBox;
        
        protected void SetupUi() {
            base.ObjectName = "EditAccountDialog";
            this.Geometry = new QRect(0, 0, 368, 263);
            this.WindowTitle = "Edit Account";
            this.Modal = true;
            QVBoxLayout verticalLayout;
            verticalLayout = new QVBoxLayout(this);
            verticalLayout.Margin = 6;
            this.tabWidget = new QTabWidget(this);
            this.tabWidget.ObjectName = "tabWidget";
            this.tabWidget.Enabled = true;
            this.tabWidget.CurrentIndex = 0;
            verticalLayout.AddWidget(this.tabWidget);
            this.tab = new QWidget(this.tabWidget);
            this.tab.ObjectName = "tab";
            QVBoxLayout verticalLayout_2;
            verticalLayout_2 = new QVBoxLayout(this.tab);
            verticalLayout_2.Margin = 6;
            QFormLayout formLayout;
            formLayout = new QFormLayout();
            verticalLayout_2.AddLayout(formLayout);
            this.label = new QLabel(this.tab);
            this.label.ObjectName = "label";
            this.label.Text = "Jabber ID:";
            formLayout.SetWidget(0, QFormLayout.ItemRole.LabelRole, this.label);
            this.jidLineEdit = new QLineEdit(this.tab);
            this.jidLineEdit.ObjectName = "jidLineEdit";
            formLayout.SetWidget(0, QFormLayout.ItemRole.FieldRole, this.jidLineEdit);
            this.label_2 = new QLabel(this.tab);
            this.label_2.ObjectName = "label_2";
            this.label_2.Text = "Password:";
            formLayout.SetWidget(1, QFormLayout.ItemRole.LabelRole, this.label_2);
            this.passwordLineEdit = new QLineEdit(this.tab);
            this.passwordLineEdit.ObjectName = "passwordLineEdit";
            this.passwordLineEdit.echoMode = QLineEdit.EchoMode.Password;
            formLayout.SetWidget(1, QFormLayout.ItemRole.FieldRole, this.passwordLineEdit);
            this.label_3 = new QLabel(this.tab);
            this.label_3.ObjectName = "label_3";
            this.label_3.Text = "Resource:";
            formLayout.SetWidget(2, QFormLayout.ItemRole.LabelRole, this.label_3);
            this.resourceCombo = new QComboBox(this.tab);
            this.resourceCombo.ObjectName = "resourceCombo";
            this.resourceCombo.Editable = true;
            formLayout.SetWidget(2, QFormLayout.ItemRole.FieldRole, this.resourceCombo);
            QSpacerItem verticalSpacer;
            verticalSpacer = new QSpacerItem(20, 40, QSizePolicy.Policy.Minimum, QSizePolicy.Policy.Expanding);
            verticalLayout_2.AddItem(verticalSpacer);
            this.widget = new QWidget(this.tab);
            this.widget.ObjectName = "widget";
            QHBoxLayout horizontalLayout;
            horizontalLayout = new QHBoxLayout(this.widget);
            horizontalLayout.Margin = 0;
            this.pushButton = new QPushButton(this.widget);
            this.pushButton.ObjectName = "pushButton";
            this.pushButton.Enabled = false;
            this.pushButton.Text = "Change Password...";
            horizontalLayout.AddWidget(this.pushButton);
            QSpacerItem horizontalSpacer;
            horizontalSpacer = new QSpacerItem(40, 20, QSizePolicy.Policy.Expanding, QSizePolicy.Policy.Minimum);
            horizontalLayout.AddItem(horizontalSpacer);
            verticalLayout_2.AddWidget(this.widget);
            this.tabWidget.AddTab(this.tab, "Account");
            this.tab_4 = new QWidget(this.tabWidget);
            this.tab_4.ObjectName = "tab_4";
            QVBoxLayout verticalLayout_3;
            verticalLayout_3 = new QVBoxLayout(this.tab_4);
            verticalLayout_3.Margin = 6;
            this.label_5 = new QLabel(this.tab_4);
            this.label_5.ObjectName = "label_5";
            this.label_5.Text = "Synapse will attempt to automatically discover your connect server if you leave this field blank.";
            this.label_5.WordWrap = true;
            verticalLayout_3.AddWidget(this.label_5);
            QFormLayout formLayout_2;
            formLayout_2 = new QFormLayout();
            verticalLayout_3.AddLayout(formLayout_2);
            this.label_4 = new QLabel(this.tab_4);
            this.label_4.ObjectName = "label_4";
            this.label_4.Text = "Connect Server:";
            formLayout_2.SetWidget(0, QFormLayout.ItemRole.LabelRole, this.label_4);
            this.serverLineEdit = new QLineEdit(this.tab_4);
            this.serverLineEdit.ObjectName = "serverLineEdit";
            formLayout_2.SetWidget(0, QFormLayout.ItemRole.FieldRole, this.serverLineEdit);
            this.label_6 = new QLabel(this.tab_4);
            this.label_6.ObjectName = "label_6";
            this.label_6.Text = "Port:";
            formLayout_2.SetWidget(1, QFormLayout.ItemRole.LabelRole, this.label_6);
            this.portSpinBox = new QSpinBox(this.tab_4);
            this.portSpinBox.ObjectName = "portSpinBox";
            this.portSpinBox.Maximum = 9999999;
            this.portSpinBox.SingleStep = 1;
            this.portSpinBox.Value = 5222;
            formLayout_2.SetWidget(1, QFormLayout.ItemRole.FieldRole, this.portSpinBox);
            this.label_7 = new QLabel(this.tab_4);
            this.label_7.ObjectName = "label_7";
            this.label_7.Enabled = false;
            this.label_7.Text = "Note that the server must support TLS encryption.";
            this.label_7.Alignment = ((global::Qyoto.Qyoto.GetCPPEnumValue("Qt", "AlignLeading") | global::Qyoto.Qyoto.GetCPPEnumValue("Qt", "AlignLeft")) | global::Qyoto.Qyoto.GetCPPEnumValue("Qt", "AlignVCenter"));
            verticalLayout_3.AddWidget(this.label_7);
            QSpacerItem verticalSpacer_2;
            verticalSpacer_2 = new QSpacerItem(20, 170, QSizePolicy.Policy.Minimum, QSizePolicy.Policy.Expanding);
            verticalLayout_3.AddItem(verticalSpacer_2);
            this.tabWidget.AddTab(this.tab_4, "Connection");
            this.tab_2 = new QWidget(this.tabWidget);
            this.tab_2.ObjectName = "tab_2";
            QVBoxLayout verticalLayout_4;
            verticalLayout_4 = new QVBoxLayout(this.tab_2);
            verticalLayout_4.Margin = 6;
            this.autoConnectCheckBox = new QCheckBox(this.tab_2);
            this.autoConnectCheckBox.ObjectName = "autoConnectCheckBox";
            this.autoConnectCheckBox.Text = "Connect Automatically";
            verticalLayout_4.AddWidget(this.autoConnectCheckBox);
            QSpacerItem verticalSpacer_3;
            verticalSpacer_3 = new QSpacerItem(20, 40, QSizePolicy.Policy.Minimum, QSizePolicy.Policy.Expanding);
            verticalLayout_4.AddItem(verticalSpacer_3);
            this.tabWidget.AddTab(this.tab_2, "Options");
            this.buttonBox = new QDialogButtonBox(this);
            this.buttonBox.ObjectName = "buttonBox";
            this.buttonBox.Orientation = Qt.Orientation.Horizontal;
            this.buttonBox.StandardButtons = (global::Qyoto.Qyoto.GetCPPEnumValue("QDialogButtonBox", "Cancel") | global::Qyoto.Qyoto.GetCPPEnumValue("QDialogButtonBox", "Ok"));
            verticalLayout.AddWidget(this.buttonBox);
            QObject.Connect(buttonBox, Qt.SIGNAL("accepted()"), this, Qt.SLOT("accept()"));
            QObject.Connect(buttonBox, Qt.SIGNAL("rejected()"), this, Qt.SLOT("reject()"));
            QMetaObject.ConnectSlotsByName(this);
        }
    }
}
