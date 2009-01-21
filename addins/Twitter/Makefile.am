
EXTRA_DIST =  

ASSEMBLY_COMPILER_COMMAND = gmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:4 -optimize- -debug "-define:DEBUG"
ASSEMBLY = ../../build/Debug/addins/Twitter.dll
ASSEMBLY_MDB = $(ASSEMBLY).mdb
COMPILE_TARGET = library
PROJECT_REFERENCES =  \
	../../build/Debug/Synapse.Services.dll \
	../../build/Debug/Synapse.UI.dll \
	../../build/Debug/Synapse.Xmpp.dll 
BUILD_DIR = ../../build/Debug/addins

TWITTER_DLL_MDB_SOURCE=../../build/Debug/addins/Twitter.dll.mdb
TWITTER_DLL_MDB=$(BUILD_DIR)/Twitter.dll.mdb

AL=al2
SATELLITE_ASSEMBLY_NAME=$(notdir $(basename $(ASSEMBLY))).resources.dll

PROGRAMFILES = \
	$(TWITTER_DLL_MDB)  


RESGEN=resgen2
	
all: $(ASSEMBLY) $(PROGRAMFILES) 

FILES =  \
	gtk-gui/generated.cs \
	gtk-gui/Synapse.Addins.TwitterAddin.TwitterAddinPreferencesDialog.cs \
	TwitterAddinPreferencesDialog.cs \
	TwitterAddinPreferencesFactory.cs \
	TwitterService.cs \
	TwitterShoutHandler.cs \
	TwitterXmlApi.cs 

DATA_FILES = 

RESOURCES =  \
	gtk-gui/gui.stetic \
	twitm-16.png \
	twitm-48.png \
	Twitter.addin.xml 

EXTRAS = 

REFERENCES =  \
	$(GTK_SHARP_20_LIBS) \
	Mono.Posix \
	System \
	System.Core \
	System.Web \
	System.Xml

DLL_REFERENCES = 

CLEANFILES = $(PROGRAMFILES) 

include $(top_srcdir)/Makefile.include




$(eval $(call emit_resgen_targets))
$(build_xamlg_list): %.xaml.g.cs: %.xaml
	xamlg '$<'

$(ASSEMBLY) $(ASSEMBLY_MDB): $(build_sources) $(build_resources) $(build_datafiles) $(DLL_REFERENCES) $(PROJECT_REFERENCES) $(build_xamlg_list) $(build_satellite_assembly_list)
	mkdir -p $(shell dirname $(ASSEMBLY))
	$(ASSEMBLY_COMPILER_COMMAND) $(ASSEMBLY_COMPILER_FLAGS) -out:$(ASSEMBLY) -target:$(COMPILE_TARGET) $(build_sources_embed) $(build_resources_embed) $(build_references_ref)
