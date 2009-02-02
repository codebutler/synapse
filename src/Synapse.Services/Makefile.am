
EXTRA_DIST =  

ASSEMBLY_COMPILER_COMMAND = gmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:4 -optimize- -debug "-define:DEBUG"
ASSEMBLY = ../../build/Debug/Synapse.Services.dll
ASSEMBLY_MDB = $(ASSEMBLY).mdb
COMPILE_TARGET = library
PROJECT_REFERENCES = ../../build/Debug/Synapse.Core.dll
BUILD_DIR = ../../build/Debug

HYENA_DLL_SOURCE=../../contrib/Hyena.dll
SYNAPSE_SERVICES_DLL_MDB_SOURCE=../../build/Debug/Synapse.Services.dll.mdb
SYNAPSE_SERVICES_DLL_MDB=$(BUILD_DIR)/Synapse.Services.dll.mdb
SYNAPSE_CORE_DLL_SOURCE=../../build/Debug/Synapse.Core.dll
SYNAPSE_CORE_DLL=$(BUILD_DIR)/Synapse.Core.dll
NDESK_DBUS_DLL_MDB_SOURCE=../../contrib/NDesk.DBus.dll.mdb
NDESK_DBUS_DLL_SOURCE=../../contrib/NDesk.DBus.dll
SYNAPSE_CORE_DLL_MDB_SOURCE=../../build/Debug/Synapse.Core.dll.mdb
SYNAPSE_CORE_DLL_MDB=$(BUILD_DIR)/Synapse.Core.dll.mdb

AL=al2
SATELLITE_ASSEMBLY_NAME=$(notdir $(basename $(ASSEMBLY))).resources.dll

PROGRAMFILES = \
	$(HYENA_DLL) \
	$(SYNAPSE_SERVICES_DLL_MDB) \
	$(SYNAPSE_CORE_DLL) \
	$(NDESK_DBUS_DLL_MDB) \
	$(NDESK_DBUS_DLL) \
	$(SYNAPSE_CORE_DLL_MDB)  

LINUX_PKGCONFIG = \
	$(SYNAPSE_SERVICES_PC)  


RESGEN=resgen2
	
all: $(ASSEMBLY) $(PROGRAMFILES) $(LINUX_PKGCONFIG) 

FILES =  \
	AssemblyInfo.cs \
	Synapse.Services/ActivityFeedService.cs \
	Synapse.Services/NetworkService.cs \
	Synapse.Services/NowPlayingService.cs \
	Synapse.Services/ScreensaverService.cs \
	Synapse.Services/SettingsService.cs \
	Synapse.ServiceStack/Application.cs \
	Synapse.ServiceStack/Client.cs \
	Synapse.ServiceStack/DBusConnection.cs \
	Synapse.ServiceStack/DBusServiceManager.cs \
	Synapse.ServiceStack/IDBusExportable.cs \
	Synapse.ServiceStack/IDBusObjectName.cs \
	Synapse.ServiceStack/IDelayedInitializeService.cs \
	Synapse.ServiceStack/IExtensionService.cs \
	Synapse.ServiceStack/IInitializeService.cs \
	Synapse.ServiceStack/IRegisterOnDemandService.cs \
	Synapse.ServiceStack/IRequiredService.cs \
	Synapse.ServiceStack/IService.cs \
	Synapse.ServiceStack/ServiceManager.cs \
	Synapse.ServiceStack/ServiceStartedHandler.cs 

DATA_FILES = 

RESOURCES = Synapse.Services.addin.xml 

EXTRAS = \
	synapse.services.pc.in 

REFERENCES =  \
	$(NOTIFY_SHARP_LIBS) \
	Mono.Posix \
	System \
	System.Core \
	System.Xml

DLL_REFERENCES =  \
	../../contrib/Hyena.dll \
	../../contrib/Mono.Addins.dll \
	../../contrib/NDesk.DBus.dll

CLEANFILES = $(PROGRAMFILES) $(LINUX_PKGCONFIG) 

include $(top_srcdir)/Makefile.include

HYENA_DLL = $(BUILD_DIR)/Hyena.dll
NDESK_DBUS_DLL_MDB = $(BUILD_DIR)/NDesk.DBus.dll.mdb
NDESK_DBUS_DLL = $(BUILD_DIR)/NDesk.DBus.dll
SYNAPSE_SERVICES_PC = $(BUILD_DIR)/synapse.services.pc

$(eval $(call emit-deploy-target,HYENA_DLL))
$(eval $(call emit-deploy-target,NDESK_DBUS_DLL_MDB))
$(eval $(call emit-deploy-target,NDESK_DBUS_DLL))
$(eval $(call emit-deploy-wrapper,SYNAPSE_SERVICES_PC,synapse.services.pc))


$(eval $(call emit_resgen_targets))
$(build_xamlg_list): %.xaml.g.cs: %.xaml
	xamlg '$<'

$(ASSEMBLY) $(ASSEMBLY_MDB): $(build_sources) $(build_resources) $(build_datafiles) $(DLL_REFERENCES) $(PROJECT_REFERENCES) $(build_xamlg_list) $(build_satellite_assembly_list)
	mkdir -p $(shell dirname $(ASSEMBLY))
	$(ASSEMBLY_COMPILER_COMMAND) $(ASSEMBLY_COMPILER_FLAGS) -out:$(ASSEMBLY) -target:$(COMPILE_TARGET) $(build_sources_embed) $(build_resources_embed) $(build_references_ref)
