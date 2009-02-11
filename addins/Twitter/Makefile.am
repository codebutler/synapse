# Begin MonoDevelop-maintained variables

FILES =  \
	AssemblyInfo.cs \
	qt-gui/TwitterAddinPreferencesDialog.cs \
	TwitterAddinPreferencesDialog.cs \
	TwitterService.cs \
	TwitterShoutHandler.cs \
	TwitterWebIdentityConfigurator.cs \
	TwitterXmlApi.cs \
	ViewOnTwitterAction.cs 

RESOURCES =  \
	twitm-16.png \
	twitm-48.png \
	Twitter.addin.xml 

REFERENCES =  \
	$(QYOTO_LIBS) \
	Mono.Posix \
	System \
	System.Core \
	System.Web \
	System.Xml

PROJECT_REFERENCES =  \
	../../build/Debug/Synapse.Core.dll \
	../../build/Debug/Synapse.exe \
	../../build/Debug/Synapse.Services.dll \
	../../build/Debug/Synapse.UI.dll \
	../../build/Debug/Synapse.Xmpp.dll 

DLL_REFERENCES = ../../contrib/Mono.Addins.dll 

# End MonoDevelop-maintained variables

pkglibdir = $(prefix)/lib/synapse/addins
BUILD_DIR = $(top_builddir)/build/Debug/addins

ASSEMBLY = $(BUILD_DIR)/Twitter.dll
ASSEMBLY_MDB = $(ASSEMBLY).mdb

all: $(ASSEMBLY) $(ASSEMBLY_MDB)

include $(top_srcdir)/Makefile.include

$(ASSEMBLY) $(ASSEMBLY_MDB): $(build_sources) $(build_resources) $(DLL_REFERENCES) $(PROJECT_REFERENCES)
	mkdir -p $(shell dirname $(ASSEMBLY))
	$(MCS) $(MCS_FLAGS) -out:$(ASSEMBLY) -target:library $(build_sources) $(build_resources_embed) $(build_references_ref)
