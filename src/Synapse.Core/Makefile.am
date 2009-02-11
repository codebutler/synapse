# Begin MonoDevelop-maintained variables

FILES =  \
	AssemblyInfo.cs \
	Synapse.Core.ExtensionMethods/IEnumerable.cs \
	Synapse.Core.ExtensionMethods/StringExtensions.cs \
	Synapse.Core/Magic.cs \
	Synapse.Core/Pair.cs \
	Synapse.Core/Paths.cs \
	Synapse.Core/PlatformHacks.cs \
	Synapse.Core/PList.cs \
	Synapse.Core/PropertyCollection.cs \
	Synapse.Core/ReadOnlyDictionary.cs \
	Synapse.Core/SerializableDictionary.cs \
	Synapse.Core/Serialization.cs \
	Synapse.Core/StringWriterWithEncoding.cs \
	Synapse.Core/UserException.cs \
	Synapse.Core/Util.cs \
	Synapse.Core/XdgBaseDirectorySpec.cs 

RESOURCES = Synapse.Core.addin.xml 

REFERENCES =  \
	Mono.Posix \
	System \
	System.Core \
	System.Web \
	System.Web.Extensions \
	System.Xml

PROJECT_REFERENCES = 

DLL_REFERENCES =  \
	../../contrib/Hyena.dll \
	../../contrib/Mono.Addins.dll

# End MonoDevelop-maintained variables

ASSEMBLY = $(BUILD_DIR)/Synapse.Core.dll
ASSEMBLY_MDB = $(ASSEMBLY).mdb

SYNAPSE_CORE_PC = $(BUILD_DIR)/synapse.core.pc

LINUX_PKGCONFIG = \
	$(SYNAPSE_CORE_PC)

EXTRA_DIST = $(SYNAPSE_CORE_PC)

all: $(ASSEMBLY) $(ASSEMBLY_MDB) $(LINUX_PKGCONFIG)

include $(top_srcdir)/Makefile.include

$(eval $(call emit-deploy-wrapper,SYNAPSE_CORE_PC,synapse.core.pc))

$(ASSEMBLY) $(ASSEMBLY_MDB): $(build_sources) $(build_resources) $(DLL_REFERENCES) $(PROJECT_REFERENCES)
	mkdir -p $(shell dirname $(ASSEMBLY))
	$(MCS) $(MCS_FLAGS) -out:$(ASSEMBLY) -target:library $(build_sources) $(build_resources_embed) $(build_references_ref)
