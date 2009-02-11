# Begin MonoDevelop-maintained variables

FILES =  \
	AssemblyInfo.cs \
	Synapse.Xmpp.Services/AccountService.cs \
	Synapse.Xmpp.Services/GeoService.cs \
	Synapse.Xmpp.Services/ShoutService.cs \
	Synapse.Xmpp.Services/XmppService.cs \
	Synapse.Xmpp/Account.cs \
	Synapse.Xmpp/AvatarManager.cs \
	Synapse.Xmpp/ChatStates.cs \
	Synapse.Xmpp/ClientStatus.cs \
	Synapse.Xmpp/Helper.cs \
	Synapse.Xmpp/IDiscoverable.cs \
	Synapse.Xmpp/Microblogging.cs \
	Synapse.Xmpp/Namespace.cs \
	Synapse.Xmpp/PersonalEventing.cs \
	Synapse.Xmpp/StreamTypeInfo.cs \
	Synapse.Xmpp/UserAvatars.cs \
	Synapse.Xmpp/UserMood.cs \
	Synapse.Xmpp/UserTune.cs \
	Synapse.Xmpp/UserWebIdentities.cs \
	Synapse.Xmpp/XmppActivityFeedItem.cs \
	Synapse.Xmpp/XmppUriQueryInfo.cs

RESOURCES = Synapse.Xmpp.addin.xml 

REFERENCES =  \
	$(NOTIFY_SHARP_LIBS) \
	System \
	System.Core \
	System.Drawing \
	System.Web \
	System.Xml

PROJECT_REFERENCES = \
	../../build/Debug/Synapse.Core.dll \
	../../build/Debug/Synapse.Services.dll

DLL_REFERENCES =  \
	../../contrib/Hyena.dll \
	../../contrib/jabber-net.dll \
	../../contrib/Mono.Addins.dll \
	../../contrib/OAuth.dll \
	../../contrib/OAuthXmpp.dll

# End MonoDevelop-maintained variables

ASSEMBLY = $(BUILD_DIR)/Synapse.Xmpp.dll
ASSEMBLY_MDB = $(ASSEMBLY).mdb

SYNAPSE_XMPP_PC = $(BUILD_DIR)/synapse.xmpp.pc

LINUX_PKGCONFIG = \
	$(SYNAPSE_XMPP_PC)

EXTRA_DIST = $(SYNAPSE_XMPP_PC)

all: $(ASSEMBLY) $(ASSEMBLY_MDB) $(LINUX_PKGCONFIG)

include $(top_srcdir)/Makefile.include

$(eval $(call emit-deploy-wrapper,SYNAPSE_XMPP_PC,synapse.xmpp.pc))

$(ASSEMBLY) $(ASSEMBLY_MDB): $(build_sources) $(build_resources) $(DLL_REFERENCES) $(PROJECT_REFERENCES)
	mkdir -p $(shell dirname $(ASSEMBLY))
	$(MCS) $(MCS_FLAGS) -out:$(ASSEMBLY) -target:library $(build_sources) $(build_resources_embed) $(build_references_ref)
