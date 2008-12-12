
EXTRA_DIST =  

# Warning: This is an automatically generated file, do not edit!

if ENABLE_DEBUG
ASSEMBLY_COMPILER_COMMAND = gmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:4 -optimize- -debug "-define:DEBUG"
ASSEMBLY = ../../build/Debug/Synapse.Core.dll
ASSEMBLY_MDB = $(ASSEMBLY).mdb
COMPILE_TARGET = library
PROJECT_REFERENCES = 
BUILD_DIR = ../../build/Debug

HYENA_DLL_MDB_SOURCE=../../contrib/Hyena.dll.mdb
HYENA_DLL_SOURCE=../../contrib/Hyena.dll
SYNAPSE_CORE_DLL_MDB_SOURCE=../../build/Debug/Synapse.Core.dll.mdb
SYNAPSE_CORE_DLL_MDB=$(BUILD_DIR)/Synapse.Core.dll.mdb

endif

if ENABLE_RELEASE
ASSEMBLY_COMPILER_COMMAND = gmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:4 -optimize-
ASSEMBLY = ../../build/Release/Dragon.Core.dll
ASSEMBLY_MDB = 
COMPILE_TARGET = library
PROJECT_REFERENCES = 
BUILD_DIR = ../../build/Release

HYENA_DLL_MDB_SOURCE=../../contrib/Hyena.dll.mdb
HYENA_DLL_SOURCE=../../contrib/Hyena.dll
SYNAPSE_CORE_DLL_MDB=

endif

AL=al2
SATELLITE_ASSEMBLY_NAME=$(notdir $(basename $(ASSEMBLY))).resources.dll

PROGRAMFILES = \
	$(HYENA_DLL_MDB) \
	$(HYENA_DLL) \
	$(SYNAPSE_CORE_DLL_MDB)  

LINUX_PKGCONFIG = \
	$(SYNAPSE_CORE_PC)  


RESGEN=resgen2
	
all: $(ASSEMBLY) $(PROGRAMFILES) $(LINUX_PKGCONFIG) 

FILES = \
	Synapse.Core/Paths.cs \
	Synapse.Core/PropertyCollection.cs \
	Synapse.Core/ReadOnlyDictionary.cs \
	Synapse.Core/Util.cs \
	Synapse.Core/Serialization.cs \
	Synapse.Core/StringWriterWithEncoding.cs \
	Synapse.Core/ApplicationContext.cs \
	Synapse.Core/PlatformHacks.cs \
	Synapse.Core/XdgBaseDirectorySpec.cs \
	IEnumerable.cs \
	Synapse.Core/UserException.cs \
	Synapse.Core/Pair.cs 

DATA_FILES = 

RESOURCES = \
	Synapse.Core.addin.xml 

EXTRAS = \
	synapse.core.pc.in 

REFERENCES =  \
	System \
	System.Xml \
	$(MONO_ADDINS_LIBS) \
	System.Core \
	Mono.Posix

DLL_REFERENCES =  \
	../../contrib/Hyena.dll

CLEANFILES = $(PROGRAMFILES) $(LINUX_PKGCONFIG) 

include $(top_srcdir)/Makefile.include

HYENA_DLL_MDB = $(BUILD_DIR)/Hyena.dll.mdb
HYENA_DLL = $(BUILD_DIR)/Hyena.dll
SYNAPSE_CORE_PC = $(BUILD_DIR)/synapse.core.pc

$(eval $(call emit-deploy-target,HYENA_DLL_MDB))
$(eval $(call emit-deploy-target,HYENA_DLL))
$(eval $(call emit-deploy-wrapper,SYNAPSE_CORE_PC,synapse.core.pc))


$(eval $(call emit_resgen_targets))
$(build_xamlg_list): %.xaml.g.cs: %.xaml
	xamlg '$<'

$(ASSEMBLY) $(ASSEMBLY_MDB): $(build_sources) $(build_resources) $(build_datafiles) $(DLL_REFERENCES) $(PROJECT_REFERENCES) $(build_xamlg_list) $(build_satellite_assembly_list)
	mkdir -p $(shell dirname $(ASSEMBLY))
	$(ASSEMBLY_COMPILER_COMMAND) $(ASSEMBLY_COMPILER_FLAGS) -out:$(ASSEMBLY) -target:$(COMPILE_TARGET) $(build_sources_embed) $(build_resources_embed) $(build_references_ref)
