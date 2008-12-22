
EXTRA_DIST =  

# Warning: This is an automatically generated file, do not edit!

if ENABLE_DEBUG
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
DRAGON_CORE_DLL=
SYNAPSE_CORE_DLL_SOURCE=../../build/Debug/Synapse.Core.dll
SYNAPSE_CORE_DLL=$(BUILD_DIR)/Synapse.Core.dll
SYNAPSE_CORE_DLL_MDB_SOURCE=../../build/Debug/Synapse.Core.dll.mdb
SYNAPSE_CORE_DLL_MDB=$(BUILD_DIR)/Synapse.Core.dll.mdb
JABBER_NET_DLL_MDB_SOURCE=../../contrib/jabber-net.dll.mdb
SYNAPSE_XMPP_DLL_MDB_SOURCE=../../build/Debug/Synapse.Xmpp.dll.mdb
SYNAPSE_XMPP_DLL_MDB=$(BUILD_DIR)/Synapse.Xmpp.dll.mdb
DRAGON_SERVICES_DLL=
SYNAPSE_SERVICES_DLL_MDB_SOURCE=../../build/Debug/Synapse.Services.dll.mdb
SYNAPSE_SERVICES_DLL_MDB=$(BUILD_DIR)/Synapse.Services.dll.mdb
HYENA_DLL_SOURCE=../../contrib/Hyena.dll
JABBER_NET_DLL_SOURCE=../../contrib/jabber-net.dll

endif

if ENABLE_RELEASE
ASSEMBLY_COMPILER_COMMAND = gmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:4 -optimize-
ASSEMBLY = bin/Release/Dragon.Core.Xmpp.dll
ASSEMBLY_MDB = 
COMPILE_TARGET = library
PROJECT_REFERENCES =  \
	../../build/Release/Dragon.Core.dll \
	../../build/Release/Dragon.Services.dll
BUILD_DIR = bin/Release

SYNAPSE_SERVICES_DLL=
DRAGON_CORE_DLL_SOURCE=../../build/Release/Dragon.Core.dll
DRAGON_CORE_DLL=$(BUILD_DIR)/Dragon.Core.dll
SYNAPSE_CORE_DLL=
SYNAPSE_CORE_DLL_MDB=
JABBER_NET_DLL_MDB_SOURCE=../../contrib/jabber-net.dll.mdb
SYNAPSE_XMPP_DLL_MDB=
DRAGON_SERVICES_DLL_SOURCE=../../build/Release/Dragon.Services.dll
DRAGON_SERVICES_DLL=$(BUILD_DIR)/Dragon.Services.dll
SYNAPSE_SERVICES_DLL_MDB=
HYENA_DLL_SOURCE=../../contrib/Hyena.dll
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
	$(SYNAPSE_XMPP_DLL_MDB) \
	$(DRAGON_SERVICES_DLL) \
	$(SYNAPSE_SERVICES_DLL_MDB) \
	$(HYENA_DLL) \
	$(JABBER_NET_DLL)  

LINUX_PKGCONFIG = \
	$(SYNAPSE_XMPP_PC)  


RESGEN=resgen2
	
all: $(ASSEMBLY) $(PROGRAMFILES) $(LINUX_PKGCONFIG) 

FILES = \
	Synapse.Xmpp/Account.cs \
	Synapse.Xmpp/ActivityFeed.cs \
	Synapse.Xmpp/IDiscoverable.cs \
	Synapse.Xmpp/Namespace.cs \
	Synapse.Xmpp/PersonalEventing.cs \
	Synapse.Xmpp/Microblogging.cs \
	Synapse.Xmpp/UserMood.cs \
	Synapse.Xmpp/UserTune.cs \
	Synapse.Xmpp/ClientStatus.cs \
	Synapse.Xmpp/IOperation.cs \
	Synapse.Xmpp/AccountService.cs \
	Synapse.Xmpp/OperationService.cs \
	Synapse.Xmpp/AbstractOperation.cs \
	Synapse.Xmpp/UserAvatars.cs \
	Synapse.Xmpp/AvatarManager.cs \
	Synapse.Xmpp/ChatStates.cs \
	Synapse.Xmpp/AbstractIqOperation.cs \
	Synapse.Xmpp/XmppUriQueryInfo.cs \
	Synapse.Xmpp/Helper.cs 

DATA_FILES = 

RESOURCES = 

EXTRAS = \
	synapse.xmpp.pc.in 

REFERENCES =  \
	System \
	System.Xml \
	$(MONO_ADDINS_LIBS) \
	System.Drawing \
	System.Web \
	System.Core

DLL_REFERENCES =  \
	../../contrib/jabber-net.dll

CLEANFILES = $(PROGRAMFILES) $(LINUX_PKGCONFIG) 

include $(top_srcdir)/Makefile.include

JABBER_NET_DLL_MDB = $(BUILD_DIR)/jabber-net.dll.mdb
HYENA_DLL = $(BUILD_DIR)/Hyena.dll
SYNAPSE_XMPP_PC = $(BUILD_DIR)/synapse.xmpp.pc
JABBER_NET_DLL = $(BUILD_DIR)/jabber-net.dll

$(eval $(call emit-deploy-target,DRAGON_CORE_DLL))
$(eval $(call emit-deploy-target,JABBER_NET_DLL_MDB))
$(eval $(call emit-deploy-target,DRAGON_SERVICES_DLL))
$(eval $(call emit-deploy-target,HYENA_DLL))
$(eval $(call emit-deploy-wrapper,SYNAPSE_XMPP_PC,synapse.xmpp.pc))
$(eval $(call emit-deploy-target,JABBER_NET_DLL))


$(eval $(call emit_resgen_targets))
$(build_xamlg_list): %.xaml.g.cs: %.xaml
	xamlg '$<'

$(ASSEMBLY) $(ASSEMBLY_MDB): $(build_sources) $(build_resources) $(build_datafiles) $(DLL_REFERENCES) $(PROJECT_REFERENCES) $(build_xamlg_list) $(build_satellite_assembly_list)
	mkdir -p $(shell dirname $(ASSEMBLY))
	$(ASSEMBLY_COMPILER_COMMAND) $(ASSEMBLY_COMPILER_FLAGS) -out:$(ASSEMBLY) -target:$(COMPILE_TARGET) $(build_sources_embed) $(build_resources_embed) $(build_references_ref)
