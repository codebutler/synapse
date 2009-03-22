# Begin MonoDevelop-maintained variables
EXTRA_DIST = $(FILES) $(RESOURCES)
FILES =  \
	AssemblyInfo.cs \
	WikipediaMessageDisplayFormatter.cs 

RESOURCES = Wikipedia.addin.xml

REFERENCES =  \
	System \
	System.Web \
	System.Xml

PROJECT_REFERENCES =  \
	../../build/Debug/Synapse.Core.dll \
	../../build/Debug/Synapse.Services.dll \
	../../build/Debug/Synapse.UI.dll 

DLL_REFERENCES =  \
	../../contrib/jabber-net.dll \
	../../contrib/Mono.Addins.dll 

# End MonoDevelop-maintained variables

pkglibdir = $(prefix)/lib/synapse/addins
BUILD_DIR = $(top_builddir)/build/Debug/addins

ASSEMBLY = $(BUILD_DIR)/Wikipedia.dll
ASSEMBLY_MDB = $(ASSEMBLY).mdb

all: $(ASSEMBLY) $(ASSEMBLY_MDB) 

include $(top_srcdir)/Makefile.include

$(ASSEMBLY) $(ASSEMBLY_MDB): $(build_sources) $(build_resources) $(DLL_REFERENCES) $(PROJECT_REFERENCES)
	mkdir -p $(shell dirname $(ASSEMBLY))
	$(MCS) $(MCS_FLAGS) -out:$(ASSEMBLY) -target:library $(build_sources) $(build_resources_embed) $(build_references_ref)
