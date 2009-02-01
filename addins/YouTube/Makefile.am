
EXTRA_DIST =  

# Warning: This is an automatically generated file, do not edit!

if ENABLE_DEBUG
ASSEMBLY_COMPILER_COMMAND = gmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:4 -optimize- -debug "-define:DEBUG"
ASSEMBLY = ../../build/Debug/addins/YouTube.dll
ASSEMBLY_MDB = $(ASSEMBLY).mdb
COMPILE_TARGET = library
PROJECT_REFERENCES =  \
	../../build/Debug/Synapse.Core.dll \
	../../build/Debug/Synapse.UI.dll \
	../../build/Debug/Synapse.Services.dll
BUILD_DIR = ../../build/Debug/addins

SYNAPSE_SERVICES_DLL_SOURCE=../../build/Debug/Synapse.Services.dll
SYNAPSE_CORE_DLL_SOURCE=../../build/Debug/Synapse.Core.dll
NDESK_DBUS_DLL_MDB_SOURCE=../../contrib/NDesk.DBus.dll.mdb
SYNAPSE_UI_DLL_SOURCE=../../build/Debug/Synapse.UI.dll
SYNAPSE_CORE_DLL_MDB_SOURCE=../../build/Debug/Synapse.Core.dll.mdb
SYNAPSE_CORE_DLL_MDB=$(BUILD_DIR)/Synapse.Core.dll.mdb
JABBER_NET_DLL_MDB_SOURCE=../../contrib/jabber-net.dll.mdb
SYNAPSE_UI_DLL_MDB_SOURCE=../../build/Debug/Synapse.UI.dll.mdb
SYNAPSE_UI_DLL_MDB=$(BUILD_DIR)/Synapse.UI.dll.mdb
SYNAPSE_XMPP_DLL_MDB_SOURCE=../../build/Debug/Synapse.Xmpp.dll.mdb
SYNAPSE_XMPP_DLL_MDB=$(BUILD_DIR)/Synapse.Xmpp.dll.mdb
YOUTUBE_DLL_MDB_SOURCE=../../build/Debug/addins/YouTube.dll.mdb
YOUTUBE_DLL_MDB=$(BUILD_DIR)/YouTube.dll.mdb
SYNAPSE_SERVICES_DLL_MDB_SOURCE=../../build/Debug/Synapse.Services.dll.mdb
SYNAPSE_SERVICES_DLL_MDB=$(BUILD_DIR)/Synapse.Services.dll.mdb
HYENA_DLL_SOURCE=../../contrib/Hyena.dll
SYNAPSE_XMPP_DLL_SOURCE=../../build/Debug/Synapse.Xmpp.dll
NDESK_DBUS_DLL_SOURCE=../../contrib/NDesk.DBus.dll
JABBER_NET_DLL_SOURCE=../../contrib/jabber-net.dll

endif

if ENABLE_RELEASE
ASSEMBLY_COMPILER_COMMAND = gmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:4 -optimize-
ASSEMBLY = bin/Release/YouTube.dll
ASSEMBLY_MDB = 
COMPILE_TARGET = library
PROJECT_REFERENCES =  \
	../../build/Release/Synapse.Core.dll \
	../../build/Release/Synapse.UI.dll \
	../../build/Release/Synapse.Services.dll
BUILD_DIR = bin/Release

SYNAPSE_SERVICES_DLL_SOURCE=../../build/Release/Synapse.Services.dll
SYNAPSE_CORE_DLL_SOURCE=../../build/Release/Synapse.Core.dll
NDESK_DBUS_DLL_MDB_SOURCE=../../contrib/NDesk.DBus.dll.mdb
SYNAPSE_UI_DLL_SOURCE=../../build/Release/Synapse.UI.dll
SYNAPSE_CORE_DLL_MDB=
JABBER_NET_DLL_MDB_SOURCE=../../contrib/jabber-net.dll.mdb
SYNAPSE_UI_DLL_MDB=
SYNAPSE_XMPP_DLL_MDB=
YOUTUBE_DLL_MDB=
SYNAPSE_SERVICES_DLL_MDB=
HYENA_DLL_SOURCE=../../contrib/Hyena.dll
SYNAPSE_XMPP_DLL_SOURCE=../../build/Release/Synapse.Xmpp.dll
NDESK_DBUS_DLL_SOURCE=../../contrib/NDesk.DBus.dll
JABBER_NET_DLL_SOURCE=../../contrib/jabber-net.dll

endif

AL=al2
SATELLITE_ASSEMBLY_NAME=$(notdir $(basename $(ASSEMBLY))).resources.dll

PROGRAMFILES = \
	$(SYNAPSE_SERVICES_DLL) \
	$(SYNAPSE_CORE_DLL) \
	$(NDESK_DBUS_DLL_MDB) \
	$(SYNAPSE_UI_DLL) \
	$(SYNAPSE_CORE_DLL_MDB) \
	$(JABBER_NET_DLL_MDB) \
	$(SYNAPSE_UI_DLL_MDB) \
	$(SYNAPSE_XMPP_DLL_MDB) \
	$(YOUTUBE_DLL_MDB) \
	$(SYNAPSE_SERVICES_DLL_MDB) \
	$(HYENA_DLL) \
	$(SYNAPSE_XMPP_DLL) \
	$(NDESK_DBUS_DLL) \
	$(JABBER_NET_DLL)  

RESGEN=resgen2
	
all: $(ASSEMBLY) $(PROGRAMFILES) $(LINUX_PKGCONFIG) 

FILES = YouTubeMessageDisplayFormatter.cs 

DATA_FILES = 

RESOURCES =  \
	ConversationHeader.html \
	YouTube.addin.xml 

REFERENCES =  \
	../../build/Debug/Synapse.Core.dll \
	../../build/Debug/Synapse.Services.dll \
	../../build/Debug/Synapse.UI.dll \
	../../contrib/jabber-net.dll \
	System \
	System.Web \
	System.Xml

DLL_REFERENCES =  \
	../../contrib/jabber-net.dll

CLEANFILES = $(PROGRAMFILES) $(LINUX_PKGCONFIG) 

include $(top_srcdir)/Makefile.include

SYNAPSE_SERVICES_DLL = $(BUILD_DIR)/Synapse.Services.dll
SYNAPSE_CORE_DLL = $(BUILD_DIR)/Synapse.Core.dll
NDESK_DBUS_DLL_MDB = $(BUILD_DIR)/NDesk.DBus.dll.mdb
SYNAPSE_UI_DLL = $(BUILD_DIR)/Synapse.UI.dll
JABBER_NET_DLL_MDB = $(BUILD_DIR)/jabber-net.dll.mdb
HYENA_DLL = $(BUILD_DIR)/Hyena.dll
SYNAPSE_XMPP_DLL = $(BUILD_DIR)/Synapse.Xmpp.dll
NDESK_DBUS_DLL = $(BUILD_DIR)/NDesk.DBus.dll
JABBER_NET_DLL = $(BUILD_DIR)/jabber-net.dll

$(eval $(call emit-deploy-target,SYNAPSE_SERVICES_DLL))
$(eval $(call emit-deploy-target,SYNAPSE_CORE_DLL))
$(eval $(call emit-deploy-target,NDESK_DBUS_DLL_MDB))
$(eval $(call emit-deploy-target,SYNAPSE_UI_DLL))
$(eval $(call emit-deploy-target,SYNAPSE_CORE_DLL_MDB))
$(eval $(call emit-deploy-target,JABBER_NET_DLL_MDB))
$(eval $(call emit-deploy-target,SYNAPSE_UI_DLL_MDB))
$(eval $(call emit-deploy-target,SYNAPSE_XMPP_DLL_MDB))
$(eval $(call emit-deploy-target,SYNAPSE_SERVICES_DLL_MDB))
$(eval $(call emit-deploy-target,HYENA_DLL))
$(eval $(call emit-deploy-target,SYNAPSE_XMPP_DLL))
$(eval $(call emit-deploy-target,NDESK_DBUS_DLL))
$(eval $(call emit-deploy-target,JABBER_NET_DLL))


$(eval $(call emit_resgen_targets))
$(build_xamlg_list): %.xaml.g.cs: %.xaml
	xamlg '$<'

$(ASSEMBLY) $(ASSEMBLY_MDB): $(build_sources) $(build_resources) $(build_datafiles) $(DLL_REFERENCES) $(PROJECT_REFERENCES) $(build_xamlg_list) $(build_satellite_assembly_list)
	mkdir -p $(shell dirname $(ASSEMBLY))
	$(ASSEMBLY_COMPILER_COMMAND) $(ASSEMBLY_COMPILER_FLAGS) -out:$(ASSEMBLY) -target:$(COMPILE_TARGET) $(build_sources_embed) $(build_resources_embed) $(build_references_ref)
