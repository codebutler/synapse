
EXTRA_DIST =  

# Warning: This is an automatically generated file, do not edit!

if ENABLE_DEBUG
ASSEMBLY_COMPILER_COMMAND = gmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:4 -optimize- -debug "-define:DEBUG"
ASSEMBLY = ../../build/Debug/addins/PasteBox.dll
ASSEMBLY_MDB = $(ASSEMBLY).mdb
COMPILE_TARGET = library
PROJECT_REFERENCES = \
	../../build/Debug/Synapse.UI.dll \
	../../build/Debug/Synapse.Services.dll
BUILD_DIR = ../../build/Debug/addins

PASTEBOX_DLL_MDB_SOURCE=../../build/Debug/addins/PasteBox.dll.mdb
PASTEBOX_DLL_MDB=$(BUILD_DIR)/PasteBox.dll.mdb

endif

if ENABLE_RELEASE
ASSEMBLY_COMPILER_COMMAND = gmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:4 -optimize-
ASSEMBLY = bin/Release/PasteBox.dll
ASSEMBLY_MDB = 
COMPILE_TARGET = library
PROJECT_REFERENCES =  \
	../../build/Release/Synapse.UI.dll \
	../../build/Release/Synapse.Services.dll
BUILD_DIR = bin/Release

PASTEBOX_DLL_MDB=

endif

AL=al2
SATELLITE_ASSEMBLY_NAME=$(notdir $(basename $(ASSEMBLY))).resources.dll

PROGRAMFILES = \
	$(PASTEBOX_DLL_MDB)  


RESGEN=resgen2
	
all: $(ASSEMBLY) $(PROGRAMFILES) 

FILES =  \
	ActionHandler.cs \
	CodePasteFormatter.cs \
	IPasteFormatter.cs \
	PasteBoxDialog.cs \
	qt-gui/PasteBoxDialog.cs 

DATA_FILES = 

RESOURCES =  \
	PasteBox.addin.xml \
	SyntaxHighlighter.css \
	SyntaxHighlighter.js 

EXTRAS = 

REFERENCES = $(QYOTO_LIBS)

DLL_REFERENCES =  \
	../../../../../../usr/lib/cli/kdebindings-2.2/qtwebkit.dll 

CLEANFILES = $(PROGRAMFILES) 

include $(top_srcdir)/Makefile.include




$(eval $(call emit_resgen_targets))
$(build_xamlg_list): %.xaml.g.cs: %.xaml
	xamlg '$<'

$(ASSEMBLY) $(ASSEMBLY_MDB): $(build_sources) $(build_resources) $(build_datafiles) $(DLL_REFERENCES) $(PROJECT_REFERENCES) $(build_xamlg_list) $(build_satellite_assembly_list)
	mkdir -p $(shell dirname $(ASSEMBLY))
	$(ASSEMBLY_COMPILER_COMMAND) $(ASSEMBLY_COMPILER_FLAGS) -out:$(ASSEMBLY) -target:$(COMPILE_TARGET) $(build_sources_embed) $(build_resources_embed) $(build_references_ref)
