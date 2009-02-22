# Begin MonoDevelop-maintained variables

FILES =  \
	AssemblyInfo.cs \
	IAvatarGridModel.cs \
	IAvatarProvider.cs \
	MucAvatarGridModel.cs \
	ResourceCodon.cs \
	ResourceProviderCodon.cs \
	RosterAvatarGridModel.cs \
	Synapse.UI.Chat/AbstractChatContent.cs \
	Synapse.UI.Chat/AbstractChatHandler.cs \
	Synapse.UI.Chat/ChatContentContext.cs \
	Synapse.UI.Chat/ChatContentEvent.cs \
	Synapse.UI.Chat/ChatContentMessage.cs \
	Synapse.UI.Chat/ChatContentNotification.cs \
	Synapse.UI.Chat/ChatContentStatus.cs \
	Synapse.UI.Chat/ChatContentTyping.cs \
	Synapse.UI.Chat/ChatHandler.cs \
	Synapse.UI.Chat/FakeChatHandler.cs \
	Synapse.UI.Chat/IChatHandler.cs \
	Synapse.UI.Chat/IMessageDisplayFormatter.cs \
	Synapse.UI.Chat/MucHandler.cs \
	Synapse.UI.Services/GuiService.cs \
	template4dotnet.cs 

RESOURCES = Synapse.UI.addin.xml 

REFERENCES =  \
	System \
	System.Core \
	System.Web \
	System.Xml

PROJECT_REFERENCES =  \
	../../build/Debug/Synapse.Core.dll \
	../../build/Debug/Synapse.Services.dll \
	../../build/Debug/Synapse.Xmpp.dll

DLL_REFERENCES =  \
	../../contrib/Hyena.dll \
	../../contrib/jabber-net.dll \
	../../contrib/Mono.Addins.dll

# End MonoDevelop-maintained variables

ASSEMBLY = $(BUILD_DIR)/Synapse.UI.dll
ASSEMBLY_MDB = $(ASSEMBLY).mdb

SYNAPSE_UI_PC = $(BUILD_DIR)/synapse.ui.pc

LINUX_PKGCONFIG = \
	$(SYNAPSE_UI_PC)  

EXTRA_DIST = $(SYNAPSE_UI_PC)

all: $(ASSEMBLY) $(ASSEMBLY_MDB) $(LINUX_PKGCONFIG) 

include $(top_srcdir)/Makefile.include

$(eval $(call emit-deploy-wrapper,SYNAPSE_UI_PC,synapse.ui.pc))

$(ASSEMBLY) $(ASSEMBLY_MDB): $(build_sources) $(build_resources) $(DLL_REFERENCES) $(PROJECT_REFERENCES)
	mkdir -p $(shell dirname $(ASSEMBLY))
	$(MCS) $(MCS_FLAGS) -out:$(ASSEMBLY) -target:library $(build_sources) $(build_resources_embed) $(build_references_ref)
