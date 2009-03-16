# Begin MonoDevelop-maintained variables
EXTRA_DIST = $(FILES) $(RESOURCES)
FILES =  \
	AssemblyInfo.cs \
	BansheeNowPlayingProvider.cs \
	IPlayerEngineService.cs \
	PlayerEvent.cs 

RESOURCES = Banshee.addin.xml 

REFERENCES = System

PROJECT_REFERENCES = ../../build/Debug/Synapse.Services.dll

DLL_REFERENCES = ../../contrib/NDesk.DBus.dll

# End MonoDevelop-maintained variables

pkglibdir = $(prefix)/lib/synapse/addins
BUILD_DIR = $(top_builddir)/build/Debug/addins

ASSEMBLY = $(BUILD_DIR)/Banshee.dll
ASSEMBLY_MDB = $(ASSEMBLY).mdb

all: $(ASSEMBLY) $(ASSEMBLY_MDB)

include $(top_srcdir)/Makefile.include

$(ASSEMBLY) $(ASSEMBLY_MDB): $(build_sources) $(build_resources) $(DLL_REFERENCES) $(PROJECT_REFERENCES)
	mkdir -p $(shell dirname $(ASSEMBLY))
	$(MCS) $(MCS_FLAGS) -out:$(ASSEMBLY) -target:library $(build_sources) $(build_resources_embed) $(build_references_ref)
