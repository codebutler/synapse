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
    
    
    public partial class ErrorDialog : QDialog {
        
        protected QLabel iconLabel;
        
        protected QLabel titleLabel;
        
        protected QLabel messageLabel;
        
        protected QWidget showDetailsButtonContainer;
        
        protected QPushButton showDetailsButton;
        
        protected QPlainTextEdit detailsTextEdit;
        
        protected QDialogButtonBox buttonBox;
        
        protected void SetupUi() {
            base.ObjectName = "ErrorDialog";
            this.Geometry = new QRect(0, 0, 400, 322);
            this.WindowTitle = "ErrorDialog";
            QVBoxLayout verticalLayout_2;
            verticalLayout_2 = new QVBoxLayout(this);
            verticalLayout_2.sizeConstraint = QLayout.SizeConstraint.SetFixedSize;
            verticalLayout_2.Margin = 6;
            QHBoxLayout horizontalLayout_2;
            horizontalLayout_2 = new QHBoxLayout();
            verticalLayout_2.AddLayout(horizontalLayout_2);
            horizontalLayout_2.sizeConstraint = QLayout.SizeConstraint.SetFixedSize;
            this.iconLabel = new QLabel(this);
            this.iconLabel.ObjectName = "iconLabel";
            QSizePolicy iconLabel_sizePolicy;
            iconLabel_sizePolicy = new QSizePolicy(QSizePolicy.Policy.Preferred, QSizePolicy.Policy.Minimum);
            iconLabel_sizePolicy.SetVerticalStretch(0);
            iconLabel_sizePolicy.SetHorizontalStretch(0);
            iconLabel_sizePolicy.SetHeightForWidth(this.iconLabel.SizePolicy.HasHeightForWidth());
            this.iconLabel.SizePolicy = iconLabel_sizePolicy;
            this.iconLabel.MinimumSize = new QSize(48, 48);
            this.iconLabel.Text = "";
            this.iconLabel.Alignment = ((global::Qyoto.Qyoto.GetCPPEnumValue("Qt", "AlignLeading") | global::Qyoto.Qyoto.GetCPPEnumValue("Qt", "AlignLeft")) | global::Qyoto.Qyoto.GetCPPEnumValue("Qt", "AlignTop"));
            horizontalLayout_2.AddWidget(this.iconLabel);
            QVBoxLayout verticalLayout;
            verticalLayout = new QVBoxLayout();
            horizontalLayout_2.AddLayout(verticalLayout);
            verticalLayout.Spacing = 6;
            verticalLayout.sizeConstraint = QLayout.SizeConstraint.SetFixedSize;
            this.titleLabel = new QLabel(this);
            this.titleLabel.ObjectName = "titleLabel";
            QSizePolicy titleLabel_sizePolicy;
            titleLabel_sizePolicy = new QSizePolicy(QSizePolicy.Policy.Preferred, QSizePolicy.Policy.Minimum);
            titleLabel_sizePolicy.SetVerticalStretch(0);
            titleLabel_sizePolicy.SetHorizontalStretch(0);
            titleLabel_sizePolicy.SetHeightForWidth(this.titleLabel.SizePolicy.HasHeightForWidth());
            this.titleLabel.SizePolicy = titleLabel_sizePolicy;
            this.titleLabel.Text = "<b>Title</b>";
            this.titleLabel.WordWrap = true;
            verticalLayout.AddWidget(this.titleLabel);
            this.messageLabel = new QLabel(this);
            this.messageLabel.ObjectName = "messageLabel";
            QSizePolicy messageLabel_sizePolicy;
            messageLabel_sizePolicy = new QSizePolicy(QSizePolicy.Policy.Preferred, QSizePolicy.Policy.Minimum);
            messageLabel_sizePolicy.SetVerticalStretch(0);
            messageLabel_sizePolicy.SetHorizontalStretch(0);
            messageLabel_sizePolicy.SetHeightForWidth(this.messageLabel.SizePolicy.HasHeightForWidth());
            this.messageLabel.SizePolicy = messageLabel_sizePolicy;
            this.messageLabel.Text = "Message";
            this.messageLabel.WordWrap = true;
            verticalLayout.AddWidget(this.messageLabel);
            this.showDetailsButtonContainer = new QWidget(this);
            this.showDetailsButtonContainer.ObjectName = "showDetailsButtonContainer";
            QSizePolicy showDetailsButtonContainer_sizePolicy;
            showDetailsButtonContainer_sizePolicy = new QSizePolicy(QSizePolicy.Policy.Preferred, QSizePolicy.Policy.Minimum);
            showDetailsButtonContainer_sizePolicy.SetVerticalStretch(0);
            showDetailsButtonContainer_sizePolicy.SetHorizontalStretch(0);
            showDetailsButtonContainer_sizePolicy.SetHeightForWidth(this.showDetailsButtonContainer.SizePolicy.HasHeightForWidth());
            this.showDetailsButtonContainer.SizePolicy = showDetailsButtonContainer_sizePolicy;
            QHBoxLayout horizontalLayout;
            horizontalLayout = new QHBoxLayout(this.showDetailsButtonContainer);
            horizontalLayout.Margin = 0;
            this.showDetailsButton = new QPushButton(this.showDetailsButtonContainer);
            this.showDetailsButton.ObjectName = "showDetailsButton";
            this.showDetailsButton.Text = "Show Details";
            this.showDetailsButton.Checkable = true;
            horizontalLayout.AddWidget(this.showDetailsButton);
            QSpacerItem horizontalSpacer;
            horizontalSpacer = new QSpacerItem(223, 17, QSizePolicy.Policy.Expanding, QSizePolicy.Policy.Minimum);
            horizontalLayout.AddItem(horizontalSpacer);
            verticalLayout.AddWidget(this.showDetailsButtonContainer);
            this.detailsTextEdit = new QPlainTextEdit(this);
            this.detailsTextEdit.ObjectName = "detailsTextEdit";
            this.detailsTextEdit.ReadOnly = true;
            verticalLayout.AddWidget(this.detailsTextEdit);
            this.buttonBox = new QDialogButtonBox(this);
            this.buttonBox.ObjectName = "buttonBox";
            this.buttonBox.Orientation = Qt.Orientation.Horizontal;
            this.buttonBox.StandardButtons = global::Qyoto.Qyoto.GetCPPEnumValue("QDialogButtonBox", "Close");
            verticalLayout_2.AddWidget(this.buttonBox);
            QObject.Connect(buttonBox, Qt.SIGNAL("accepted()"), this, Qt.SLOT("accept()"));
            QObject.Connect(buttonBox, Qt.SIGNAL("rejected()"), this, Qt.SLOT("reject()"));
            QObject.Connect(showDetailsButton, Qt.SIGNAL("toggled(bool)"), detailsTextEdit, Qt.SLOT("setVisible(bool)"));
            QMetaObject.ConnectSlotsByName(this);
        }
    }
}