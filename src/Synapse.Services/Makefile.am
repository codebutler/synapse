# Begin MonoDevelop-maintained variables

FILES =  \
	AssemblyInfo.cs \
	gtk-gui/generated.cs \
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

RESOURCES =  \
	gtk-gui/gui.stetic \
	Synapse.Services.addin.xml 

REFERENCES =  \
	$(GTK_SHARP_20_LIBS) \
	Mono.Posix \
	System \
	System.Core \
	System.Xml

PROJECT_REFERENCES = ../../build/Debug/Synapse.Core.dll

DLL_REFERENCES =  \
	../../contrib/Hyena.dll \
	../../contrib/Mono.Addins.dll \
	../../contrib/NDesk.DBus.dll

# End MonoDevelop-maintained variables

ASSEMBLY = $(BUILD_DIR)/Synapse.Services.dll
ASSEMBLY_MDB = $(ASSEMBLY).mdb

SYNAPSE_SERVICES_PC = $(BUILD_DIR)/synapse.services.pc

LINUX_PKGCONFIG = \
	$(SYNAPSE_SERVICES_PC)

EXTRA_DIST = $(SYNAPSE_SERVICES_PC)

all: $(ASSEMBLY) $(ASSEMBLY_MDB) $(LINUX_PKGCONFIG)

include $(top_srcdir)/Makefile.include

$(eval $(call emit-deploy-wrapper,SYNAPSE_SERVICES_PC,synapse.services.pc))

$(ASSEMBLY) $(ASSEMBLY_MDB): $(build_sources) $(build_resources) $(DLL_REFERENCES) $(PROJECT_REFERENCES)
	mkdir -p $(shell dirname $(ASSEMBLY))
	$(MCS) $(MCS_FLAGS) -out:$(ASSEMBLY) -target:library $(build_sources) $(build_resources_embed) $(build_references_ref)
