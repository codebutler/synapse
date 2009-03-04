# Begin MonoDevelop-maintained variables

FILES =  \
	AssemblyInfo.cs \
	IPasteFormatPreviewer.cs \
	qt-gui/InsertSnippetDialog.cs \
	qt-gui/InsertSnippetDialog.cs \
	Synapse.Addins.CodeSnippets/CodeMessageDisplayFormatter.cs \
	Synapse.Addins.CodeSnippets/CodeSnippetsService.cs \
	Synapse.Addins.CodeSnippets/InsertSnippetAction.cs \
	Synapse.Addins.CodeSnippets/InsertSnippetDialog.cs 

RESOURCES =  \
	CodeSnippets.addin.xml \
	ConversationHeader.html 
REFERENCES =  \
	$(QYOTO_LIBS) \
	$(QTWEBKIT_SHARP_LIBS) \
	System \
	System.Core \
	System.Xml

PROJECT_REFERENCES =  \
	../../build/Debug/Synapse.Core.dll \
	../../build/Debug/Synapse.exe \
	../../build/Debug/Synapse.Services.dll \
	../../build/Debug/Synapse.UI.dll 

DLL_REFERENCES =  \
	../../contrib/jabber-net.dll \
	../../contrib/Mono.Addins.dll \
	../../contrib/Wilco.SyntaxHighlighter.dll 

# End MonoDevelop-maintained variables

pkglibdir = $(prefix)/lib/synapse/addins
BUILD_DIR = $(top_builddir)/build/Debug/addins

ASSEMBLY = $(BUILD_DIR)/CodeSnippets.dll
ASSEMBLY_MDB = $(ASSEMBLY).mdb

all: $(ASSEMBLY) $(ASSEMBLY_MDB) 

include $(top_srcdir)/Makefile.include

$(ASSEMBLY) $(ASSEMBLY_MDB): $(build_sources) $(build_resources) $(DLL_REFERENCES) $(PROJECT_REFERENCES)
	mkdir -p $(shell dirname $(ASSEMBLY))
	$(MCS) $(MCS_FLAGS) -out:$(ASSEMBLY) -target:library $(build_sources) $(build_resources_embed) $(build_references_ref)
