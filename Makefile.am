
EXTRA_DIST =  expansions.m4 contrib/Hyena.Gui.dll contrib/Mono.Rocks.dll contrib/NDesk.DBus.dll contrib/Hyena.dll contrib/jabber-net.dll ../../../../usr/lib/cli/kdebindings-2.2/qtwebkit.dll

#Warning: This is an automatically generated file, do not edit!
if ENABLE_DEBUG
 SUBDIRS =  src/Synapse.Core src/Synapse.Services src/Synapse.Xmpp src/Synapse.UI src/Synapse.QtClient addins
 BUILDDIR = build/Debug
endif
if ENABLE_RELEASE
 SUBDIRS =  src/Synapse.Core src/Synapse.Services src/Synapse.Xmpp src/Synapse.UI src/Synapse.QtClient addins
 BUILDDIR = build/Release
endif

run:
	cd $(BUILDDIR) && mono --debug Synapse.exe --debug --uninstalled
