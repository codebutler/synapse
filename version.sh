#!/bin/bash
rev_num=`git rev-list HEAD | wc -l | sed "s/[ \t]//g"`
echo -n 0.1-0+git${rev_num}
