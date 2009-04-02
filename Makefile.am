SUBDIRS  = contrib src addins

desktopdir = $(datadir)/applications
desktop_DATA = synapse.desktop

# for application icon
pixmapdir = $(datadir)/pixmaps
pixmap_DATA = synapse.png

EXTRA_DIST = synapse.png synapse.desktop debian/*

run:
	cd $(BUILD_DIR) && mono --debug Synapse.exe --debug --uninstalled

# Update debian version but don't stop if fails
# (command will likely fail on other distros)
dist-hook:
	debian/rules update-version || true
