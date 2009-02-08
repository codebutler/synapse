
EXTRA_DIST =  

THEMES_DIR = resources/themes
INSTALLED_THEMES_DIR = $(expanded_datadir)/$(PACKAGE)/themes/

install-data-hook:
	mkdir -p $(DESTDIR)$(INSTALLED_THEMES_DIR)
	cd $(THEMES_DIR) && find . -type f -exec $(INSTALL_DATA) -D -v '{}' '$(DESTDIR)$(INSTALLED_THEMES_DIR){}' \; ;

uninstall-hook:
	rm -rf $(DESTDIR)$(INSTALLED_THEMES_DIR)

ASSEMBLY_COMPILER_COMMAND = gmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -unsafe -warn:4 -optimize- -debug "-define:DEBUG"
ASSEMBLY = ../../build/Debug/Synapse.exe
ASSEMBLY_MDB = $(ASSEMBLY).mdb
COMPILE_TARGET = exe
PROJECT_REFERENCES =  \
	../../build/Debug/Synapse.Core.dll \
	../../build/Debug/Synapse.Services.dll \
	../../build/Debug/Synapse.UI.dll \
	../../build/Debug/Synapse.Xmpp.dll
BUILD_DIR = ../../build/Debug

SYNAPSE_SERVICES_DLL_SOURCE=../../build/Debug/Synapse.Services.dll
SYNAPSE_SERVICES_DLL=$(BUILD_DIR)/Synapse.Services.dll
SYNAPSE_DLL=
QTWEBKIT_DLL_CONFIG_SOURCE=../../../../../../usr/lib/cli/kdebindings-2.2/qtwebkit.dll.config
SYNAPSE_CORE_DLL_SOURCE=../../build/Debug/Synapse.Core.dll
SYNAPSE_CORE_DLL=$(BUILD_DIR)/Synapse.Core.dll
NDESK_DBUS_DLL_MDB_SOURCE=../../contrib/NDesk.DBus.dll.mdb
SYNAPSE_UI_DLL_SOURCE=../../build/Debug/Synapse.UI.dll
SYNAPSE_UI_DLL=$(BUILD_DIR)/Synapse.UI.dll
MONO_ROCKS_DLL_MDB_SOURCE=../../contrib/Mono.Rocks.dll.mdb
SYNAPSE_CORE_DLL_MDB_SOURCE=../../build/Debug/Synapse.Core.dll.mdb
SYNAPSE_CORE_DLL_MDB=$(BUILD_DIR)/Synapse.Core.dll.mdb
JABBER_NET_DLL_MDB_SOURCE=../../contrib/jabber-net.dll.mdb
SYNAPSE_UI_DLL_MDB_SOURCE=../../build/Debug/Synapse.UI.dll.mdb
SYNAPSE_UI_DLL_MDB=$(BUILD_DIR)/Synapse.UI.dll.mdb
HYENA_GUI_DLL_SOURCE=../../contrib/Hyena.Gui.dll
SYNAPSE_XMPP_DLL_MDB_SOURCE=../../build/Debug/Synapse.Xmpp.dll.mdb
SYNAPSE_XMPP_DLL_MDB=$(BUILD_DIR)/Synapse.Xmpp.dll.mdb
HYENA_GUI_DLL_CONFIG_SOURCE=../../contrib/Hyena.Gui.dll.config
SYNAPSE_SERVICES_DLL_MDB_SOURCE=../../build/Debug/Synapse.Services.dll.mdb
SYNAPSE_SERVICES_DLL_MDB=$(BUILD_DIR)/Synapse.Services.dll.mdb
HYENA_DLL_SOURCE=../../contrib/Hyena.dll
MONO_ROCKS_DLL_SOURCE=../../contrib/Mono.Rocks.dll
QTWEBKIT_DLL_SOURCE=../../../../../../usr/lib/cli/kdebindings-2.2/qtwebkit.dll
SYNAPSE_XMPP_DLL_SOURCE=../../build/Debug/Synapse.Xmpp.dll
SYNAPSE_XMPP_DLL=$(BUILD_DIR)/Synapse.Xmpp.dll
NDESK_DBUS_DLL_SOURCE=../../contrib/NDesk.DBus.dll
JABBER_NET_DLL_SOURCE=../../contrib/jabber-net.dll
SYNAPSE_EXE_MDB_SOURCE=../../build/Debug/Synapse.exe.mdb
SYNAPSE_EXE_MDB=$(BUILD_DIR)/Synapse.exe.mdb

AL=al2
SATELLITE_ASSEMBLY_NAME=$(notdir $(basename $(ASSEMBLY))).resources.dll

PROGRAMFILES = \
	$(SYNAPSE_SERVICES_DLL) \
	$(SYNAPSE_DLL) \
	$(QTWEBKIT_DLL_CONFIG) \
	$(SYNAPSE_CORE_DLL) \
	$(NDESK_DBUS_DLL_MDB) \
	$(SYNAPSE_UI_DLL) \
	$(MONO_ROCKS_DLL_MDB) \
	$(SYNAPSE_CORE_DLL_MDB) \
	$(JABBER_NET_DLL_MDB) \
	$(SYNAPSE_UI_DLL_MDB) \
	$(HYENA_GUI_DLL) \
	$(SYNAPSE_XMPP_DLL_MDB) \
	$(HYENA_GUI_DLL_CONFIG) \
	$(SYNAPSE_SERVICES_DLL_MDB) \
	$(HYENA_DLL) \
	$(MONO_ROCKS_DLL) \
	$(QTWEBKIT_DLL) \
	$(SYNAPSE_XMPP_DLL) \
	$(NDESK_DBUS_DLL) \
	$(JABBER_NET_DLL) \
	$(SYNAPSE_EXE_MDB)  

BINARIES = \
	$(SYNAPSE)  


RESGEN=resgen2
	
all: $(ASSEMBLY) $(PROGRAMFILES) $(BINARIES) 

FILES =  \
	AbstractSimpleFileEngine.cs \
	AssemblyInfo.cs \
	AvatarFileEngine.cs \
	BookmarkedMUCsModel.cs \
	ErrorDialog.cs \
	gtk-gui/generated.cs \
	gtk-gui/Synapse.QtClient.ErrorDialog.cs \
	Gui.cs \
	IUpdateableAction.cs \
	JSDebugObject.cs \
	KeyPressEater.cs \
	Main.cs \
	qt-gui/AccountStatusWidget.cs \
	qt-gui/AddFriendWindow.cs \
	qt-gui/AvatarSelectDialog.cs \
	qt-gui/ChatWindow.cs \
	qt-gui/DebugWindow.cs \
	qt-gui/EditAccountDialog.cs \
	qt-gui/EditGroupsWidget.cs \
	qt-gui/EditGroupsWindow.cs \
	qt-gui/MainWindow.cs \
	qt-gui/NoAccountsWidget.cs \
	qt-gui/PreferencesWindow.cs \
	qt-gui/ProfileWindow.cs \
	qt-gui/RosterWidget.cs \
	qt-gui/ServiceBrowserWindow.cs \
	QtTraceListener.cs \
	ResizableGraphicsPixmapItem.cs \
	ResourceFileEngine.cs \
	Synapse.QtClient.ExtensionNodes/ActionCodon.cs \
	Synapse.QtClient.ExtensionNodes/IActionCodon.cs \
	Synapse.QtClient.ExtensionNodes/SeparatorCodon.cs \
	Synapse.QtClient.Widgets/AccountStatusWidget.cs \
	Synapse.QtClient.Widgets/AvatarGrid.cs \
	Synapse.QtClient.Widgets/AvatarGrid.FadeInOutAnimation.cs \
	Synapse.QtClient.Widgets/AvatarGrid.InfoPopup.cs \
	Synapse.QtClient.Widgets/AvatarGrid.RosterItem.cs \
	Synapse.QtClient.Widgets/AvatarGrid.RosterItemGroup.cs \
	Synapse.QtClient.Widgets/AvatarGrid.Scene.cs \
	Synapse.QtClient.Widgets/AvatarLabel.cs \
	Synapse.QtClient.Widgets/ConversationTextEdit.cs \
	Synapse.QtClient.Widgets/ConversationWidget.cs \
	Synapse.QtClient.Widgets/EditGroupsWidget.cs \
	Synapse.QtClient.Widgets/FadingQLabel.cs \
	Synapse.QtClient.Widgets/LightboxContainerWidget.cs \
	Synapse.QtClient.Widgets/NoAccountsWidget.cs \
	Synapse.QtClient.Widgets/NotificationsWidget.cs \
	Synapse.QtClient.Widgets/RosterWidget.cs \
	Synapse.QtClient.Windows/AddFriendWindow.cs \
	Synapse.QtClient.Windows/AvatarSelectDialog.cs \
	Synapse.QtClient.Windows/ChatWindow.cs \
	Synapse.QtClient.Windows/DebugWindow.cs \
	Synapse.QtClient.Windows/EditAccountDialog.cs \
	Synapse.QtClient.Windows/EditGroupsWindow.cs \
	Synapse.QtClient.Windows/MainWindow.cs \
	Synapse.QtClient.Windows/PreferencesWindow.cs \
	Synapse.QtClient.Windows/ProfileWindow.cs \
	Synapse.QtClient.Windows/ServiceBrowserWindow.cs \
	Synapse.QtClient.Windows/TabbedChatsWindow.cs \
	TrayIcon.cs \
	WindowMover.cs 

DATA_FILES = 

RESOURCES =  \
	gtk-gui/gui.stetic \
	resources/collapse.png \
	resources/default-avatar.png \
	resources/effects.js \
	resources/expand.png \
	resources/feed.html \
	resources/friend-muclist.html \
	resources/handle.png \
	resources/loading.gif \
	resources/mainwindow.qss \
	resources/mainwindow-noaccounts.qss \
	resources/music-16.png \
	resources/newtab.html \
	resources/oceanbg.png \
	resources/octy.svg \
	resources/octy-22.png \
	resources/octy-32.png \
	resources/profile.html \
	resources/prototype.js \
	resources/ServiceDiscovery.html \
	resources/simple-add.png \
	resources/simple-search.png \
	resources/Template.html \
	resources/view-grid.png \
	resources/view-list.png \
	Synapse.QtClient.addin.xml 

EXTRAS = \
	synapse.in 

REFERENCES =  \
	$(GLIB_SHARP_20_LIBS) \
	$(GTK_SHARP_20_LIBS) \
	$(NDESK_DBUS_GLIB_10_LIBS) \
	$(QYOTO_LIBS) \
	Mono.Posix \
	System \
	System.Core \
	System.Drawing \
	System.Xml

DLL_REFERENCES =  \
	../../../../../../usr/lib/cli/kdebindings-2.2/qtwebkit.dll \
	../../contrib/Hyena.dll \
	../../contrib/jabber-net.dll \
	../../contrib/Mono.Addins.dll \
	../../contrib/Mono.Rocks.dll

CLEANFILES = $(PROGRAMFILES) $(BINARIES) 

include $(top_srcdir)/Makefile.include

QTWEBKIT_DLL_CONFIG = $(BUILD_DIR)/qtwebkit.dll.config
NDESK_DBUS_DLL_MDB = $(BUILD_DIR)/NDesk.DBus.dll.mdb
MONO_ROCKS_DLL_MDB = $(BUILD_DIR)/Mono.Rocks.dll.mdb
JABBER_NET_DLL_MDB = $(BUILD_DIR)/jabber-net.dll.mdb
HYENA_GUI_DLL = $(BUILD_DIR)/Hyena.Gui.dll
SYNAPSE = $(BUILD_DIR)/synapse
HYENA_GUI_DLL_CONFIG = $(BUILD_DIR)/Hyena.Gui.dll.config
HYENA_DLL = $(BUILD_DIR)/Hyena.dll
MONO_ROCKS_DLL = $(BUILD_DIR)/Mono.Rocks.dll
QTWEBKIT_DLL = $(BUILD_DIR)/qtwebkit.dll
NDESK_DBUS_DLL = $(BUILD_DIR)/NDesk.DBus.dll
JABBER_NET_DLL = $(BUILD_DIR)/jabber-net.dll
MONO_ADDINS_DLL = $(BUILD_DIR)/Mono.Addins.dll

$(eval $(call emit-deploy-target,SYNAPSE_DLL))
$(eval $(call emit-deploy-target,QTWEBKIT_DLL_CONFIG))
$(eval $(call emit-deploy-target,NDESK_DBUS_DLL_MDB))
$(eval $(call emit-deploy-target,MONO_ROCKS_DLL_MDB))
$(eval $(call emit-deploy-target,JABBER_NET_DLL_MDB))
$(eval $(call emit-deploy-target,HYENA_GUI_DLL))
$(eval $(call emit-deploy-wrapper,SYNAPSE,synapse,x))
$(eval $(call emit-deploy-target,HYENA_GUI_DLL_CONFIG))
$(eval $(call emit-deploy-target,HYENA_DLL))
$(eval $(call emit-deploy-target,MONO_ROCKS_DLL))
$(eval $(call emit-deploy-target,QTWEBKIT_DLL))
$(eval $(call emit-deploy-target,NDESK_DBUS_DLL))
$(eval $(call emit-deploy-target,JABBER_NET_DLL))
$(eval $(call emit-deploy-target,MONO_ROCKS_DLL))


$(eval $(call emit_resgen_targets))
$(build_xamlg_list): %.xaml.g.cs: %.xaml
	xamlg '$<'

$(ASSEMBLY) $(ASSEMBLY_MDB): $(build_sources) $(build_resources) $(build_datafiles) $(DLL_REFERENCES) $(PROJECT_REFERENCES) $(build_xamlg_list) $(build_satellite_assembly_list)
	mkdir -p $(shell dirname $(ASSEMBLY))
	$(ASSEMBLY_COMPILER_COMMAND) $(ASSEMBLY_COMPILER_FLAGS) -out:$(ASSEMBLY) -target:$(COMPILE_TARGET) $(build_sources_embed) $(build_resources_embed) $(build_references_ref)
	cp -r $(THEMES_DIR) $(BUILD_DIR)/themes

clean:
	rm -rf $(BUILD_DIR)/themes
