#!/bin/sh
# Post-remove script for BabySmash
# Updates the icon cache and desktop database after removal

set -e

# Update icon cache if gtk-update-icon-cache is available
if command -v gtk-update-icon-cache >/dev/null 2>&1; then
    gtk-update-icon-cache -q -t -f /usr/share/icons/hicolor 2>/dev/null || true
fi

# Update desktop database if update-desktop-database is available
if command -v update-desktop-database >/dev/null 2>&1; then
    update-desktop-database -q /usr/share/applications 2>/dev/null || true
fi

exit 0
