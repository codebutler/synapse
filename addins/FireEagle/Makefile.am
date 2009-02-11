# Begin MonoDevelop-maintained variables

FILES = AssemblyInfo.cs  \
	FireEagleAddinPreferencesDialog.cs  \
	FireEagleService.cs  \
	FireEagleWebIdentityConfigurator.cs \
	qt-gui/FireEagleAddinPreferencesDialog.cs

RESOURCES =  \
	fireeagle-48.png \
	logo.png

REFERENCES =  \
	$(QYOTO_LIBS) \
	System \
	System.Web \
	System.Xml

PROJECT_REFERENCES =  \
	../../build/Debug/Synapse.Core.dll \
	../../build/Debug/Synapse.Xmpp.dll \
	../../build/Debug/Synapse.Services.dll \
	../../build/Debug/Synapse.exe

DLL_REFERENCES =  \
	../../contrib/OAuth.dll \
	../../contrib/OAuthXmpp.dll \
	../../contrib/jabber-net.dll

# End MonoDevelop-maintained variables

pkglibdir = $(prefix)/lib/synapse/addins
BUILD_DIR = $(top_builddir)/build/Debug/addins

ASSEMBLY = $(BUILD_DIR)/FireEagle.dll
ASSEMBLY_MDB = $(ASSEMBLY).mdb

all: $(ASSEMBLY) $(ASSEMBLY_MDB)

include $(top_srcdir)/Makefile.include

$(ASSEMBLY) $(ASSEMBLY_MDB): $(build_sources) $(build_resources) $(DLL_REFERENCES) $(PROJECT_REFERENCES)
	mkdir -p $(shell dirname $(ASSEMBLY))
	$(MCS) $(MCS_FLAGS) -out:$(ASSEMBLY) -target:library $(build_sources) $(build_resources_embed) $(build_references_ref)

