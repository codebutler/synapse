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


public partial class EditAccountDialog : QDialog {
    
    protected QTabWidget tabWidget;
    
    protected QWidget tab;
    
    protected QLabel label;
    
    protected QLineEdit lineEdit;
    
    protected QLabel label_2;
    
    protected QLineEdit lineEdit_2;
    
    protected QLabel label_3;
    
    protected QComboBox comboBox;
    
    protected QCheckBox checkBox;
    
    protected QWidget widget;
    
    protected QPushButton pushButton;
    
    protected QWidget tab_2;
    
    protected QWidget tab_4;
    
    protected QLabel label_4;
    
    protected QLineEdit lineEdit_3;
    
    protected QLabel label_6;
    
    protected QSpinBox spinBox;
    
    protected QLabel label_7;
    
    protected QWidget tab_3;
    
    protected QDialogButtonBox buttonBox;
    
    protected void SetupUi() {
        base.ObjectName = "EditAccountDialog";
        this.Geometry = new QRect(0, 0, 400, 300);
        this.WindowTitle = "EditAccountDialog";
        QVBoxLayout verticalLayout;
        verticalLayout = new QVBoxLayout(this);
        verticalLayout.Margin = 6;
        this.tabWidget = new QTabWidget(this);
        this.tabWidget.ObjectName = "tabWidget";
        this.tabWidget.CurrentIndex = 1;
        verticalLayout.AddWidget(this.tabWidget);
        this.tab = new QWidget(this.tabWidget);
        this.tab.ObjectName = "tab";
        QVBoxLayout verticalLayout_2;
        verticalLayout_2 = new QVBoxLayout(this.tab);
        QGridLayout gridLayout;
        gridLayout = new QGridLayout();
        verticalLayout_2.AddLayout(gridLayout);
        this.label = new QLabel(this.tab);
        this.label.ObjectName = "label";
        this.label.Text = "Jabber ID:";
        gridLayout.AddWidget(this.label, 0, 0, 1, 1);
        this.lineEdit = new QLineEdit(this.tab);
        this.lineEdit.ObjectName = "lineEdit";
        gridLayout.AddWidget(this.lineEdit, 0, 1, 1, 1);
        this.label_2 = new QLabel(this.tab);
        this.label_2.ObjectName = "label_2";
        this.label_2.Text = "Password:";
        gridLayout.AddWidget(this.label_2, 1, 0, 1, 1);
        this.lineEdit_2 = new QLineEdit(this.tab);
        this.lineEdit_2.ObjectName = "lineEdit_2";
        this.lineEdit_2.echoMode = QLineEdit.EchoMode.Password;
        gridLayout.AddWidget(this.lineEdit_2, 1, 1, 1, 1);
        this.label_3 = new QLabel(this.tab);
        this.label_3.ObjectName = "label_3";
        this.label_3.Text = "Resource:";
        gridLayout.AddWidget(this.label_3, 2, 0, 1, 1);
        this.comboBox = new QComboBox(this.tab);
        this.comboBox.ObjectName = "comboBox";
        this.comboBox.Editable = true;
        gridLayout.AddWidget(this.comboBox, 2, 1, 1, 1);
        this.checkBox = new QCheckBox(this.tab);
        this.checkBox.ObjectName = "checkBox";
        this.checkBox.Text = "Connect Automatically";
        verticalLayout_2.AddWidget(this.checkBox);
        QSpacerItem verticalSpacer;
        verticalSpacer = new QSpacerItem(20, 40, QSizePolicy.Policy.Expanding, QSizePolicy.Policy.Minimum);
        verticalLayout_2.AddItem(verticalSpacer);
        this.widget = new QWidget(this.tab);
        this.widget.ObjectName = "widget";
        QHBoxLayout horizontalLayout;
        horizontalLayout = new QHBoxLayout(this.widget);
        horizontalLayout.Margin = 0;
        this.pushButton = new QPushButton(this.widget);
        this.pushButton.ObjectName = "pushButton";
        this.pushButton.Text = "Change Password...";
        horizontalLayout.AddWidget(this.pushButton);
        QSpacerItem horizontalSpacer;
        horizontalSpacer = new QSpacerItem(40, 20, QSizePolicy.Policy.Expanding, QSizePolicy.Policy.Minimum);
        horizontalLayout.AddItem(horizontalSpacer);
        verticalLayout_2.AddWidget(this.widget);
        this.tabWidget.AddTab(this.tab, "Account");
        this.tab_2 = new QWidget(this.tabWidget);
        this.tab_2.ObjectName = "tab_2";
        this.tabWidget.AddTab(this.tab_2, "Profile");
        this.tab_4 = new QWidget(this.tabWidget);
        this.tab_4.ObjectName = "tab_4";
        QVBoxLayout verticalLayout_3;
        verticalLayout_3 = new QVBoxLayout(this.tab_4);
        QGridLayout gridLayout_2;
        gridLayout_2 = new QGridLayout();
        verticalLayout_3.AddLayout(gridLayout_2);
        this.label_4 = new QLabel(this.tab_4);
        this.label_4.ObjectName = "label_4";
        this.label_4.Text = "Connect Server:";
        gridLayout_2.AddWidget(this.label_4, 0, 0, 1, 1);
        this.lineEdit_3 = new QLineEdit(this.tab_4);
        this.lineEdit_3.ObjectName = "lineEdit_3";
        gridLayout_2.AddWidget(this.lineEdit_3, 0, 1, 1, 1);
        this.label_6 = new QLabel(this.tab_4);
        this.label_6.ObjectName = "label_6";
        this.label_6.Text = "Port:";
        gridLayout_2.AddWidget(this.label_6, 0, 2, 1, 1);
        this.spinBox = new QSpinBox(this.tab_4);
        this.spinBox.ObjectName = "spinBox";
        this.spinBox.Maximum = 9999999;
        this.spinBox.SingleStep = 1;
        this.spinBox.Value = 5222;
        gridLayout_2.AddWidget(this.spinBox, 0, 3, 1, 1);
        this.label_7 = new QLabel(this.tab_4);
        this.label_7.ObjectName = "label_7";
        this.label_7.Enabled = false;
        this.label_7.Text = "(Encryption is required)";
        this.label_7.Alignment = global::Qyoto.Qyoto.GetCPPEnumValue("Qt", "AlignCenter");
        verticalLayout_3.AddWidget(this.label_7);
        QSpacerItem verticalSpacer_2;
        verticalSpacer_2 = new QSpacerItem(20, 170, QSizePolicy.Policy.Expanding, QSizePolicy.Policy.Minimum);
        verticalLayout_3.AddItem(verticalSpacer_2);
        this.tabWidget.AddTab(this.tab_4, "Connection");
        this.tab_3 = new QWidget(this.tabWidget);
        this.tab_3.ObjectName = "tab_3";
        this.tabWidget.AddTab(this.tab_3, "Privacy");
        this.buttonBox = new QDialogButtonBox(this);
        this.buttonBox.ObjectName = "buttonBox";
        this.buttonBox.Orientation = Qt.Orientation.Horizontal;
        this.buttonBox.StandardButtons = (global::Qyoto.Qyoto.GetCPPEnumValue("QDialogButtonBox", "Cancel") | global::Qyoto.Qyoto.GetCPPEnumValue("QDialogButtonBox", "Ok"));
        verticalLayout.AddWidget(this.buttonBox);
        QMetaObject.ConnectSlotsByName(this);
    }
}
