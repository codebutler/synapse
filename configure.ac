dnl Process this file with autoconf to produce a configure script.

AC_PREREQ([2.54])
AC_INIT([Synapse], [0.1])
AM_INIT_AUTOMAKE([foreign])

dnl pkg-config
AC_PATH_PROG(PKG_CONFIG, pkg-config, no)
if test "x$PKG_CONFIG" = "xno"; then
        AC_MSG_ERROR([You need to install pkg-config])
fi

AC_PROG_INSTALL

dnl Check for C# compiler
AC_PATH_PROG(MCS, gmcs, no)
if test "x$MCS" = "xno"; then
        AC_MSG_ERROR([gmcs Not found])
fi
MCS_FLAGS="-noconfig -codepage:utf8 -warn:4 -optimize- -debug \"-define:DEBUG\""
AC_SUBST(MCS)
AC_SUBST(MCS_FLAGS)

dnl package checks
PKG_CHECK_MODULES([QYOTO], qyoto >= 4.4)
AC_SUBST(QYOTO_LIBS)

PKG_CHECK_MODULES([QTWEBKIT_SHARP], qtwebkit-sharp >= 2.2)
AC_SUBST(QTWEBKIT_SHARP_LIBS)

PKG_CHECK_MODULES([NOTIFY_SHARP], [notify-sharp])
AC_SUBST(NOTIFY_SHARP_LIBS)

PKG_CHECK_MODULES([GLIB_SHARP_20], [glib-sharp-2.0])
AC_SUBST(GLIB_SHARP_20_LIBS)

PKG_CHECK_MODULES([NDESK_DBUS_GLIB_10], [ndesk-dbus-glib-1.0])
AC_SUBST(NDESK_DBUS_GLIB_10_LIBS)

PKG_CHECK_MODULES([GTK_SHARP_20], [gtk-sharp-2.0])
AC_SUBST(GTK_SHARP_20_LIBS)

PKG_CHECK_MODULES([MONO_ADDINS], [mono-addins])
AC_SUBST(MONO_ADDINS_LIBS)

BUILD_DIR="\$(top_builddir)/build/Debug"
AC_SUBST(BUILD_DIR)

AC_CONFIG_FILES([
Makefile
contrib/Makefile
src/Makefile
src/Synapse.Core/Makefile
src/Synapse.Core/synapse.core.pc
src/Synapse.Services/Makefile
src/Synapse.Services/synapse.services.pc
src/Synapse.Xmpp/synapse.xmpp.pc
src/Synapse.Xmpp/Makefile
src/Synapse.UI/Makefile
src/Synapse.UI/synapse.ui.pc
src/Synapse.QtClient/Makefile
src/Synapse.QtClient/synapse
src/Synapse.QtClient/synapse.qtclient.pc
addins/Makefile
addins/Twitter/Makefile
addins/CodeSnippets/Makefile
addins/Flickr/Makefile
addins/Banshee/Makefile
addins/YouTube/Makefile
])

AC_OUTPUT
