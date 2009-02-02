
EXTRA_DIST =  

ASSEMBLY_COMPILER_COMMAND = gmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:4 -optimize- -debug "-define:DEBUG"
ASSEMBLY = ../../build/Debug/Synapse.Xmpp.dll
ASSEMBLY_MDB = $(ASSEMBLY).mdb
COMPILE_TARGET = library
PROJECT_REFERENCES =  \
	../../build/Debug/Synapse.Core.dll \
	../../build/Debug/Synapse.Services.dll
BUILD_DIR = ../../build/Debug

SYNAPSE_SERVICES_DLL_SOURCE=../../build/Debug/Synapse.Services.dll
SYNAPSE_SERVICES_DLL=$(BUILD_DIR)/Synapse.Services.dll
SYNAPSE_CORE_DLL_SOURCE=../../build/Debug/Synapse.Core.dll
SYNAPSE_CORE_DLL=$(BUILD_DIR)/Synapse.Core.dll
NDESK_DBUS_DLL_MDB_SOURCE=../../contrib/NDesk.DBus.dll.mdb
SYNAPSE_CORE_DLL_MDB_SOURCE=../../build/Debug/Synapse.Core.dll.mdb
SYNAPSE_CORE_DLL_MDB=$(BUILD_DIR)/Synapse.Core.dll.mdb
JABBER_NET_DLL_MDB_SOURCE=../../contrib/jabber-net.dll.mdb
SYNAPSE_XMPP_DLL_MDB_SOURCE=../../build/Debug/Synapse.Xmpp.dll.mdb
SYNAPSE_XMPP_DLL_MDB=$(BUILD_DIR)/Synapse.Xmpp.dll.mdb
SYNAPSE_SERVICES_DLL_MDB_SOURCE=../../build/Debug/Synapse.Services.dll.mdb
SYNAPSE_SERVICES_DLL_MDB=$(BUILD_DIR)/Synapse.Services.dll.mdb
HYENA_DLL_SOURCE=../../contrib/Hyena.dll
NDESK_DBUS_DLL_SOURCE=../../contrib/NDesk.DBus.dll
JABBER_NET_DLL_SOURCE=../../contrib/jabber-net.dll

AL=al2
SATELLITE_ASSEMBLY_NAME=$(notdir $(basename $(ASSEMBLY))).resources.dll

PROGRAMFILES = \
	$(SYNAPSE_SERVICES_DLL) \
	$(SYNAPSE_CORE_DLL) \
	$(NDESK_DBUS_DLL_MDB) \
	$(SYNAPSE_CORE_DLL_MDB) \
	$(JABBER_NET_DLL_MDB) \
	$(SYNAPSE_XMPP_DLL_MDB) \
	$(SYNAPSE_SERVICES_DLL_MDB) \
	$(HYENA_DLL) \
	$(NDESK_DBUS_DLL) \
	$(JABBER_NET_DLL)  

LINUX_PKGCONFIG = \
	$(SYNAPSE_XMPP_PC)  


RESGEN=resgen2
	
all: $(ASSEMBLY) $(PROGRAMFILES) $(LINUX_PKGCONFIG) 

FILES =  \
	AssemblyInfo.cs \
	Synapse.Xmpp.Services/AccountService.cs \
	Synapse.Xmpp.Services/ShoutService.cs \
	Synapse.Xmpp.Services/XmppService.cs \
	Synapse.Xmpp/Account.cs \
	Synapse.Xmpp/AvatarManager.cs \
	Synapse.Xmpp/ChatStates.cs \
	Synapse.Xmpp/ClientStatus.cs \
	Synapse.Xmpp/Helper.cs \
	Synapse.Xmpp/IDiscoverable.cs \
	Synapse.Xmpp/Microblogging.cs \
	Synapse.Xmpp/Namespace.cs \
	Synapse.Xmpp/PersonalEventing.cs \
	Synapse.Xmpp/StreamTypeInfo.cs \
	Synapse.Xmpp/UserAvatars.cs \
	Synapse.Xmpp/UserMood.cs \
	Synapse.Xmpp/UserTune.cs \
	Synapse.Xmpp/UserWebIdentities.cs \
	Synapse.Xmpp/XmppActivityFeedItem.cs \
	Synapse.Xmpp/XmppUriQueryInfo.cs

DATA_FILES = 

RESOURCES = Synapse.Xmpp.addin.xml 

EXTRAS = \
	synapse.xmpp.pc.in 

REFERENCES =  \
	$(NOTIFY_SHARP_LIBS) \
	System \
	System.Core \
	System.Drawing \
	System.Web \
	System.Xml

DLL_REFERENCES =  \
	../../contrib/jabber-net.dll \
	../../contrib/Mono.Addins.dll

CLEANFILES = $(PROGRAMFILES) $(LINUX_PKGCONFIG) 

include $(top_srcdir)/Makefile.include

NDESK_DBUS_DLL_MDB = $(BUILD_DIR)/NDesk.DBus.dll.mdb
JABBER_NET_DLL_MDB = $(BUILD_DIR)/jabber-net.dll.mdb
HYENA_DLL = $(BUILD_DIR)/Hyena.dll
SYNAPSE_XMPP_PC = $(BUILD_DIR)/synapse.xmpp.pc
NDESK_DBUS_DLL = $(BUILD_DIR)/NDesk.DBus.dll
JABBER_NET_DLL = $(BUILD_DIR)/jabber-net.dll

$(eval $(call emit-deploy-target,NDESK_DBUS_DLL_MDB))
$(eval $(call emit-deploy-target,JABBER_NET_DLL_MDB))
$(eval $(call emit-deploy-target,HYENA_DLL))
$(eval $(call emit-deploy-wrapper,SYNAPSE_XMPP_PC,synapse.xmpp.pc))
$(eval $(call emit-deploy-target,NDESK_DBUS_DLL))
$(eval $(call emit-deploy-target,JABBER_NET_DLL))


$(eval $(call emit_resgen_targets))
$(build_xamlg_list): %.xaml.g.cs: %.xaml
	xamlg '$<'

$(ASSEMBLY) $(ASSEMBLY_MDB): $(build_sources) $(build_resources) $(build_datafiles) $(DLL_REFERENCES) $(PROJECT_REFERENCES) $(build_xamlg_list) $(build_satellite_assembly_list)
	mkdir -p $(shell dirname $(ASSEMBLY))
	$(ASSEMBLY_COMPILER_COMMAND) $(ASSEMBLY_COMPILER_FLAGS) -out:$(ASSEMBLY) -target:$(COMPILE_TARGET) $(build_sources_embed) $(build_resources_embed) $(build_references_ref)
