SUBDIRS  = contrib src addins

run:
	cd $(BUILD_DIR) && mono --debug Synapse.exe --debug --uninstalled

REV_NUM=$(shell git rev-list HEAD | wc -l | sed "s/[ \t]//g")
REV=$(shell git rev-list HEAD^!)
DATE=$(shell date --rfc-2822)

update-debian-version:
	echo -e "\
synapse (0.1-0+git${REV_NUM}-0) `lsb_release -c -s`; urgency=low\n\
\n\
  * Automatic package for revision ${REV}. \n\
\n\
 -- Eric Butler <eric@extremeboredom.net>  ${DATE}" > debian/changelog
