SUBDIRS  = contrib src addins

desktopdir = $(datadir)/applications
desktop_DATA = synapse.desktop

# for application icon
pixmapdir = $(datadir)/pixmaps
pixmap_DATA = synapse.png

EXTRA_DIST = synapse.png synapse.desktop

run:
	cd $(BUILD_DIR) && mono --debug Synapse.exe --debug --uninstalled

