#!/bin/bash

if [ -d .git ]; then
	rev_num=`git rev-list HEAD | wc -l | sed "s/[ \t]//g"`
	echo -n 0.1-0+git${rev_num}
else
	echo -n "ERROR"
fi
