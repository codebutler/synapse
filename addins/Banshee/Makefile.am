
EXTRA_DIST =  

# Warning: This is an automatically generated file, do not edit!

if ENABLE_DEBUG
ASSEMBLY_COMPILER_COMMAND = gmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:4 -optimize- -debug "-define:DEBUG"
ASSEMBLY = ../../build/Debug/BansheeNowPlayingProvider.dll
ASSEMBLY_MDB = $(ASSEMBLY).mdb
COMPILE_TARGET = library
PROJECT_REFERENCES =  \
	../../build/Debug/Synapse.Core.dll \
	../../build/Debug/Synapse.Services.dll
BUILD_DIR = ../../build/Debug

HYENA_DLL_SOURCE=../../contrib/Hyena.dll
SYNAPSE_SERVICES_DLL_SOURCE=../../build/Debug/Synapse.Services.dll
SYNAPSE_SERVICES_DLL=$(BUILD_DIR)/Synapse.Services.dll
BANSHEENOWPLAYINGPROVIDER_DLL_MDB_SOURCE=../../build/Debug/BansheeNowPlayingProvider.dll.mdb
BANSHEENOWPLAYINGPROVIDER_DLL_MDB=$(BUILD_DIR)/BansheeNowPlayingProvider.dll.mdb
SYNAPSE_SERVICES_DLL_MDB_SOURCE=../../build/Debug/Synapse.Services.dll.mdb
SYNAPSE_SERVICES_DLL_MDB=$(BUILD_DIR)/Synapse.Services.dll.mdb
SYNAPSE_CORE_DLL_SOURCE=../../build/Debug/Synapse.Core.dll
SYNAPSE_CORE_DLL=$(BUILD_DIR)/Synapse.Core.dll
DRAGON_SERVICES_DLL=
DRAGON_CORE_DLL=
SYNAPSE_CORE_DLL_MDB_SOURCE=../../build/Debug/Synapse.Core.dll.mdb
SYNAPSE_CORE_DLL_MDB=$(BUILD_DIR)/Synapse.Core.dll.mdb

endif

if ENABLE_RELEASE
ASSEMBLY_COMPILER_COMMAND = gmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:4 -optimize-
ASSEMBLY = bin/Release/BansheeNowPlayingProvider.dll
ASSEMBLY_MDB = 
COMPILE_TARGET = library
PROJECT_REFERENCES =  \
	../../build/Release/Dragon.Core.dll \
	../../build/Release/Dragon.Services.dll
BUILD_DIR = bin/Release

HYENA_DLL_SOURCE=../../contrib/Hyena.dll
SYNAPSE_SERVICES_DLL=
BANSHEENOWPLAYINGPROVIDER_DLL_MDB=
SYNAPSE_SERVICES_DLL_MDB=
SYNAPSE_CORE_DLL=
DRAGON_SERVICES_DLL_SOURCE=../../build/Release/Dragon.Services.dll
DRAGON_SERVICES_DLL=$(BUILD_DIR)/Dragon.Services.dll
DRAGON_CORE_DLL_SOURCE=../../build/Release/Dragon.Core.dll
DRAGON_CORE_DLL=$(BUILD_DIR)/Dragon.Core.dll
SYNAPSE_CORE_DLL_MDB=

endif

AL=al2
SATELLITE_ASSEMBLY_NAME=$(notdir $(basename $(ASSEMBLY))).resources.dll

PROGRAMFILES = \
	$(HYENA_DLL) \
	$(SYNAPSE_SERVICES_DLL) \
	$(BANSHEENOWPLAYINGPROVIDER_DLL_MDB) \
	$(SYNAPSE_SERVICES_DLL_MDB) \
	$(SYNAPSE_CORE_DLL) \
	$(DRAGON_SERVICES_DLL) \
	$(DRAGON_CORE_DLL) \
	$(SYNAPSE_CORE_DLL_MDB)  


RESGEN=resgen2
	
all: $(ASSEMBLY) $(PROGRAMFILES) 

FILES =  \
	AssemblyInfo.cs \
	BansheeNowPlayingProvider.cs \
	IPlayerEngineService.cs \
	PlayerEvent.cs 

DATA_FILES = 

RESOURCES = Banshee.addin.xml 

EXTRAS = 

REFERENCES =  \
	../../build/Debug/Synapse.Core.dll \
	../../build/Debug/Synapse.Services.dll \
	-pkg:ndesk-dbus-1.0 \
	System

DLL_REFERENCES = 

CLEANFILES = $(PROGRAMFILES) 

include $(top_srcdir)/Makefile.include

HYENA_DLL = $(BUILD_DIR)/Hyena.dll

$(eval $(call emit-deploy-target,HYENA_DLL))
$(eval $(call emit-deploy-target,DRAGON_SERVICES_DLL))
$(eval $(call emit-deploy-target,DRAGON_CORE_DLL))


$(eval $(call emit_resgen_targets))
$(build_xamlg_list): %.xaml.g.cs: %.xaml
	xamlg '$<'

$(ASSEMBLY) $(ASSEMBLY_MDB): $(build_sources) $(build_resources) $(build_datafiles) $(DLL_REFERENCES) $(PROJECT_REFERENCES) $(build_xamlg_list) $(build_satellite_assembly_list)
	mkdir -p $(shell dirname $(ASSEMBLY))
	$(ASSEMBLY_COMPILER_COMMAND) $(ASSEMBLY_COMPILER_FLAGS) -out:$(ASSEMBLY) -target:$(COMPILE_TARGET) $(build_sources_embed) $(build_resources_embed) $(build_references_ref)
