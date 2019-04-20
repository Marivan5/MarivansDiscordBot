# -*- coding: utf-8 -*-

# Import relevent modules.
import sys,os,time
from stat import *

# Name of the file to check.
fName = "/share/MarvBot/MarvBotV3.deps.json"

# Get the time the file was last modified.
fInfo = os.stat(fName)
mTime = fInfo[ST_MTIME]

# Start an 'infinite' loop.
while True:
    # Get the modification time again and compare
    # to the one we stored previously.
    # If it's changed act accordingly.
    fInfo = os.stat(fName)
    if mTime != fInfo[ST_MTIME]:
        print ("File modified")
        time.sleep(1)
        os.system('lxterminal -e /share/MarvBot/MarvBotV3')
        mTime = fInfo[ST_MTIME]

    # Delay of 1 second so we don't hog the processor.
    time.sleep(1)