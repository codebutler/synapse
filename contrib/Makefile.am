CONTRIBS = *.dll*

EXTRA_DIST = $(CONTRIBS)

all: copycontrib

copycontrib: $(CONTRIBS)
	mkdir -p $(BUILD_DIR)
	for i in $(CONTRIBS); do cp -v $$i $(BUILD_DIR)/$$i; done

clean-local:
	for i in $(CONTRIBS); do rm -fv $(BUILD_DIR)/$$i; done

install-data-hook:
	for i in $(CONTRIBS); do $(INSTALL_DATA) -D -v $$i $(DESTDIR)$(prefix)/lib/synapse/$$i; done

uninstall-hook:
	for i in $(CONTRIBS); do rm -fv $(prefix)/lib/synapse/$$i; done
