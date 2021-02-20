# -*- coding: utf-8 -*-

import sys,os,time
from stat import *

botFileName = "/share/MarvBot/MarvBotV3.deps.json"
botFileInfo = os.stat(botFileName)
botModifiedTime = botFileInfo[ST_MTIME]

while True:
	botFileInfo = os.stat(botFileName)
	TcpFileInfo = os.stat(TcpFileName)
	if botModifiedTime != botFileInfo[ST_MTIME]:
		print ("File modified")
		time.sleep(1)
		os.system('lxterminal -e /share/MarvBot/MarvBotV3')
		botModifiedTime = botFileInfo[ST_MTIME]
	time.sleep(1)