# Begin MonoDevelop-maintained variables

FILES = YouTubeMessageDisplayFormatter.cs 

RESOURCES =  \
	ConversationHeader.html \
	YouTube.addin.xml 

REFERENCES =  \
	../../contrib/jabber-net.dll \
	System \
	System.Web \
	System.Xml

PROJECT_REFERENCES =  \
	../../build/Debug/Synapse.Core.dll \
	../../build/Debug/Synapse.UI.dll \
	../../build/Debug/Synapse.Services.dll

DLL_REFERENCES =  \
	../../contrib/jabber-net.dll

# End MonoDevelop-maintained variables

pkglibdir = $(prefix)/lib/synapse/addins
BUILD_DIR = $(top_builddir)/build/Debug/addins

ASSEMBLY = $(BUILD_DIR)/YouTube.dll
ASSEMBLY_MDB = $(ASSEMBLY).mdb

all: $(ASSEMBLY) $(ASSEMBLY_MDB)

include $(top_srcdir)/Makefile.include

$(ASSEMBLY) $(ASSEMBLY_MDB): $(build_sources) $(build_resources) $(DLL_REFERENCES) $(PROJECT_REFERENCES)
	mkdir -p $(shell dirname $(ASSEMBLY))
	$(MCS) $(MCS_FLAGS) -out:$(ASSEMBLY) -target:library $(build_sources) $(build_resources_embed) $(build_references_ref)

