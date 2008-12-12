
EXTRA_DIST =  

# Warning: This is an automatically generated file, do not edit!

if ENABLE_DEBUG
ASSEMBLY_COMPILER_COMMAND = gmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:4 -optimize- -debug "-define:DEBUG"
ASSEMBLY = ../../build/Debug/Synapse.Services.dll
ASSEMBLY_MDB = $(ASSEMBLY).mdb
COMPILE_TARGET = library
PROJECT_REFERENCES =  \
	../../build/Debug/Synapse.Core.dll
BUILD_DIR = ../../build/Debug

HYENA_DLL_SOURCE=../../contrib/Hyena.dll
SYNAPSE_SERVICES_DLL_MDB_SOURCE=../../build/Debug/Synapse.Services.dll.mdb
SYNAPSE_SERVICES_DLL_MDB=$(BUILD_DIR)/Synapse.Services.dll.mdb
SYNAPSE_CORE_DLL_SOURCE=../../build/Debug/Synapse.Core.dll
SYNAPSE_CORE_DLL=$(BUILD_DIR)/Synapse.Core.dll
DRAGON_CORE_DLL=
SYNAPSE_CORE_DLL_MDB_SOURCE=../../build/Debug/Synapse.Core.dll.mdb
SYNAPSE_CORE_DLL_MDB=$(BUILD_DIR)/Synapse.Core.dll.mdb

endif

if ENABLE_RELEASE
ASSEMBLY_COMPILER_COMMAND = gmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:4 -optimize-
ASSEMBLY = ../../build/Release/Dragon.Services.dll
ASSEMBLY_MDB = 
COMPILE_TARGET = library
PROJECT_REFERENCES =  \
	../../build/Release/Dragon.Core.dll
BUILD_DIR = ../../build/Release

HYENA_DLL_SOURCE=../../contrib/Hyena.dll
SYNAPSE_SERVICES_DLL_MDB=
SYNAPSE_CORE_DLL=
DRAGON_CORE_DLL_SOURCE=../../build/Release/Dragon.Core.dll
DRAGON_CORE_DLL=$(BUILD_DIR)/Dragon.Core.dll
SYNAPSE_CORE_DLL_MDB=

endif

AL=al2
SATELLITE_ASSEMBLY_NAME=$(notdir $(basename $(ASSEMBLY))).resources.dll

PROGRAMFILES = \
	$(HYENA_DLL) \
	$(SYNAPSE_SERVICES_DLL_MDB) \
	$(SYNAPSE_CORE_DLL) \
	$(DRAGON_CORE_DLL) \
	$(SYNAPSE_CORE_DLL_MDB)  

LINUX_PKGCONFIG = \
	$(SYNAPSE_SERVICES_PC)  


RESGEN=resgen2
	
all: $(ASSEMBLY) $(PROGRAMFILES) $(LINUX_PKGCONFIG) 

FILES = \
	Synapse.Services/NetworkService.cs \
	Synapse.Services/NotificationService.cs \
	Synapse.Services/ScreensaverService.cs \
	Synapse.Services/NowPlayingService.cs \
	Synapse.ServiceStack/IService.cs \
	Synapse.ServiceStack/ServiceManager.cs \
	Synapse.ServiceStack/IRequiredService.cs \
	Synapse.ServiceStack/IInitializeService.cs \
	Synapse.ServiceStack/IExtensionService.cs \
	Synapse.ServiceStack/ServiceStartedHandler.cs \
	Synapse.ServiceStack/IDelayedInitializeService.cs \
	Synapse.ServiceStack/DBusServiceManager.cs \
	Synapse.ServiceStack/IDBusExportable.cs \
	Synapse.ServiceStack/Application.cs \
	Synapse.ServiceStack/Client.cs \
	Synapse.ServiceStack/DBusConnection.cs \
	Synapse.ServiceStack/IDBusObjectName.cs \
	Synapse.ServiceStack/IRegisterOnDemandService.cs 

DATA_FILES = 

RESOURCES = \
	Synapse.Services.addin.xml 

EXTRAS = \
	synapse.services.pc.in 

REFERENCES =  \
	$(MONO_ADDINS_LIBS) \
	System \
	$(NDESK_DBUS_10_LIBS) \
	$(NOTIFY_SHARP_LIBS) \
	System.Xml \
	Mono.Posix

DLL_REFERENCES =  \
	../../contrib/Hyena.dll

CLEANFILES = $(PROGRAMFILES) $(LINUX_PKGCONFIG) 

include $(top_srcdir)/Makefile.include

HYENA_DLL = $(BUILD_DIR)/Hyena.dll
SYNAPSE_SERVICES_PC = $(BUILD_DIR)/synapse.services.pc

$(eval $(call emit-deploy-target,HYENA_DLL))
$(eval $(call emit-deploy-wrapper,SYNAPSE_SERVICES_PC,synapse.services.pc))


$(eval $(call emit_resgen_targets))
$(build_xamlg_list): %.xaml.g.cs: %.xaml
	xamlg '$<'

$(ASSEMBLY) $(ASSEMBLY_MDB): $(build_sources) $(build_resources) $(build_datafiles) $(DLL_REFERENCES) $(PROJECT_REFERENCES) $(build_xamlg_list) $(build_satellite_assembly_list)
	mkdir -p $(shell dirname $(ASSEMBLY))
	$(ASSEMBLY_COMPILER_COMMAND) $(ASSEMBLY_COMPILER_FLAGS) -out:$(ASSEMBLY) -target:$(COMPILE_TARGET) $(build_sources_embed) $(build_resources_embed) $(build_references_ref)
