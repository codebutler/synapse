
EXTRA_DIST =  

# Warning: This is an automatically generated file, do not edit!

if ENABLE_DEBUG
ASSEMBLY_COMPILER_COMMAND = gmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:4 -optimize- -debug "-define:DEBUG"
ASSEMBLY = ../../build/Debug/Synapse.UI.dll
ASSEMBLY_MDB = $(ASSEMBLY).mdb
COMPILE_TARGET = library
PROJECT_REFERENCES =  \
	../../build/Debug/Synapse.Services.dll \
	../../build/Debug/Synapse.Xmpp.dll \
	../../build/Debug/Synapse.Core.dll
BUILD_DIR = ../../build/Debug

SYNAPSE_SERVICES_DLL_SOURCE=../../build/Debug/Synapse.Services.dll
SYNAPSE_SERVICES_DLL=$(BUILD_DIR)/Synapse.Services.dll
DRAGON_CORE_DLL=
SYNAPSE_CORE_DLL_SOURCE=../../build/Debug/Synapse.Core.dll
SYNAPSE_CORE_DLL=$(BUILD_DIR)/Synapse.Core.dll
SYNAPSE_CORE_DLL_MDB_SOURCE=../../build/Debug/Synapse.Core.dll.mdb
SYNAPSE_CORE_DLL_MDB=$(BUILD_DIR)/Synapse.Core.dll.mdb
JABBER_NET_DLL_MDB_SOURCE=../../contrib/jabber-net.dll.mdb
SYNAPSE_UI_DLL_MDB_SOURCE=../../build/Debug/Synapse.UI.dll.mdb
SYNAPSE_UI_DLL_MDB=$(BUILD_DIR)/Synapse.UI.dll.mdb
HYENA_DLL_MDB_SOURCE=../../contrib/Hyena.dll.mdb
SYNAPSE_XMPP_DLL_MDB_SOURCE=../../build/Debug/Synapse.Xmpp.dll.mdb
SYNAPSE_XMPP_DLL_MDB=$(BUILD_DIR)/Synapse.Xmpp.dll.mdb
DRAGON_SERVICES_DLL=
SYNAPSE_SERVICES_DLL_MDB_SOURCE=../../build/Debug/Synapse.Services.dll.mdb
SYNAPSE_SERVICES_DLL_MDB=$(BUILD_DIR)/Synapse.Services.dll.mdb
HYENA_DLL_SOURCE=../../contrib/Hyena.dll
DRAGON_CORE_XMPP_DLL=
SYNAPSE_XMPP_DLL_SOURCE=../../build/Debug/Synapse.Xmpp.dll
SYNAPSE_XMPP_DLL=$(BUILD_DIR)/Synapse.Xmpp.dll
JABBER_NET_DLL_SOURCE=../../contrib/jabber-net.dll

endif

if ENABLE_RELEASE
ASSEMBLY_COMPILER_COMMAND = gmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:4 -optimize-
ASSEMBLY = bin/Release/Synapse.dll
ASSEMBLY_MDB = 
COMPILE_TARGET = library
PROJECT_REFERENCES =  \
	../../build/Release/Dragon.Services.dll \
	../Synapse.Xmpp/bin/Release/Dragon.Core.Xmpp.dll \
	../../build/Release/Dragon.Core.dll
BUILD_DIR = bin/Release

SYNAPSE_SERVICES_DLL=
DRAGON_CORE_DLL_SOURCE=../../build/Release/Dragon.Core.dll
DRAGON_CORE_DLL=$(BUILD_DIR)/Dragon.Core.dll
SYNAPSE_CORE_DLL=
SYNAPSE_CORE_DLL_MDB=
JABBER_NET_DLL_MDB_SOURCE=../../contrib/jabber-net.dll.mdb
SYNAPSE_UI_DLL_MDB=
HYENA_DLL_MDB_SOURCE=../../contrib/Hyena.dll.mdb
SYNAPSE_XMPP_DLL_MDB=
DRAGON_SERVICES_DLL_SOURCE=../../build/Release/Dragon.Services.dll
DRAGON_SERVICES_DLL=$(BUILD_DIR)/Dragon.Services.dll
SYNAPSE_SERVICES_DLL_MDB=
HYENA_DLL_SOURCE=../../contrib/Hyena.dll
DRAGON_CORE_XMPP_DLL_SOURCE=../Synapse.Xmpp/bin/Release/Dragon.Core.Xmpp.dll
DRAGON_CORE_XMPP_DLL=$(BUILD_DIR)/Dragon.Core.Xmpp.dll
SYNAPSE_XMPP_DLL=
JABBER_NET_DLL_SOURCE=../../contrib/jabber-net.dll

endif

AL=al2
SATELLITE_ASSEMBLY_NAME=$(notdir $(basename $(ASSEMBLY))).resources.dll

PROGRAMFILES = \
	$(SYNAPSE_SERVICES_DLL) \
	$(DRAGON_CORE_DLL) \
	$(SYNAPSE_CORE_DLL) \
	$(SYNAPSE_CORE_DLL_MDB) \
	$(JABBER_NET_DLL_MDB) \
	$(SYNAPSE_UI_DLL_MDB) \
	$(HYENA_DLL_MDB) \
	$(SYNAPSE_XMPP_DLL_MDB) \
	$(DRAGON_SERVICES_DLL) \
	$(SYNAPSE_SERVICES_DLL_MDB) \
	$(HYENA_DLL) \
	$(DRAGON_CORE_XMPP_DLL) \
	$(SYNAPSE_XMPP_DLL) \
	$(JABBER_NET_DLL)  

LINUX_PKGCONFIG = \
	$(SYNAPSE_UI_PC)  


RESGEN=resgen2
	
all: $(ASSEMBLY) $(PROGRAMFILES) $(LINUX_PKGCONFIG) 

FILES = \
	AssemblyInfo.cs \
	IController.cs \
	IView.cs \
	ViewFactory.cs \
	Synapse.UI.Controllers/MainWindowController.cs \
	AbstractController.cs \
	Synapse.UI.Views/IMainWindowView.cs \
	Synapse.UI.Controllers/TrayIconController.cs \
	Synapse.UI.Views/ITrayIconView.cs \
	IDialogView.cs \
	EventHandlers.cs \
	IAvatarGridModel.cs \
	RosterAvatarGridModel.cs \
	Synapse.UI.Controllers/MucWindowController.cs \
	Synapse.UI.Controllers/DebugWindowController.cs \
	Synapse.UI.Views/IDebugWindowView.cs \
	MucAvatarGridModel.cs \
	Synapse.UI.Views/IChatWindowView.cs \
	Synapse.UI.Controllers/AbstractChatWindowController.cs \
	Synapse.UI.Controllers/ChatWindowController.cs \
	Synapse.UI.Actions.ExtensionNodes/ActionCodon.cs \
	ActionHandlerAttribute.cs \
	Synapse.UI.Actions.ExtensionNodes/ActionHandlerCodon.cs \
	Synapse.UI.Actions.ExtensionNodes/ActionItemCodon.cs \
	Synapse.UI.Actions.ExtensionNodes/SeparatorItemCodon.cs \
	IActionItemCodon.cs \
	Synapse.UI.Controllers/UserProfileWindowController.cs \
	Synapse.UI.Views/IUserProfileWindowView.cs \
	Synapse.UI.Operations/JoinMucOperation.cs \
	Synapse.UI.Operations/RequestVCardOperation.cs \
	ResourceProviderCodon.cs \
	Synapse.UI.Services/ActionService.cs \
	Synapse.UI.Services/GuiService.cs \
	ChatWindowManager.cs 

DATA_FILES = 

RESOURCES = \
	Synapse.UI.addin.xml \
	resources/profile.html 

EXTRAS = \
	synapse.ui.pc.in 

REFERENCES =  \
	System \
	System.Core \
	$(MONO_ADDINS_LIBS) \
	System.Xml

DLL_REFERENCES =  \
	../../contrib/jabber-net.dll \
	../../contrib/Hyena.dll

CLEANFILES = $(PROGRAMFILES) $(LINUX_PKGCONFIG) 

include $(top_srcdir)/Makefile.include

JABBER_NET_DLL_MDB = $(BUILD_DIR)/jabber-net.dll.mdb
HYENA_DLL_MDB = $(BUILD_DIR)/Hyena.dll.mdb
SYNAPSE_UI_PC = $(BUILD_DIR)/synapse.ui.pc
HYENA_DLL = $(BUILD_DIR)/Hyena.dll
JABBER_NET_DLL = $(BUILD_DIR)/jabber-net.dll

$(eval $(call emit-deploy-target,DRAGON_CORE_DLL))
$(eval $(call emit-deploy-target,JABBER_NET_DLL_MDB))
$(eval $(call emit-deploy-target,HYENA_DLL_MDB))
$(eval $(call emit-deploy-target,DRAGON_SERVICES_DLL))
$(eval $(call emit-deploy-wrapper,SYNAPSE_UI_PC,synapse.ui.pc))
$(eval $(call emit-deploy-target,HYENA_DLL))
$(eval $(call emit-deploy-target,DRAGON_CORE_XMPP_DLL))
$(eval $(call emit-deploy-target,JABBER_NET_DLL))


$(eval $(call emit_resgen_targets))
$(build_xamlg_list): %.xaml.g.cs: %.xaml
	xamlg '$<'

$(ASSEMBLY) $(ASSEMBLY_MDB): $(build_sources) $(build_resources) $(build_datafiles) $(DLL_REFERENCES) $(PROJECT_REFERENCES) $(build_xamlg_list) $(build_satellite_assembly_list)
	mkdir -p $(shell dirname $(ASSEMBLY))
	$(ASSEMBLY_COMPILER_COMMAND) $(ASSEMBLY_COMPILER_FLAGS) -out:$(ASSEMBLY) -target:$(COMPILE_TARGET) $(build_sources_embed) $(build_resources_embed) $(build_references_ref)
