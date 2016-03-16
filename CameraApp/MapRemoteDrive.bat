@echo off

set dstpc=192.168.6.59
set sharedir=Nokia8310(00E04C7CBD55)
set uname=administrator
set upass=cat

net use Y: \\%dstpc%\%sharedir% "%upass%" /user:"%uname%" /persistent:no


