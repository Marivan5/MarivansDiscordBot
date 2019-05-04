# -*- coding: utf-8 -*-

import sys,os,time
from stat import *

fName = "/share/MarvBot/MarvBotV3.deps.json"
fInfo = os.stat(fName)
mTime = fInfo[ST_MTIME]

while True:
    fInfo = os.stat(fName)
    if mTime != fInfo[ST_MTIME]:
        print ("File modified")
        time.sleep(1)
        os.system('lxterminal -e /share/MarvBot/MarvBotV3')
        mTime = fInfo[ST_MTIME]
    time.sleep(1)