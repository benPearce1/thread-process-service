net stop filewatcherservice
installutil /u bin/debug/filewatcherservice.exe
msbuild
installutil bin/debug/filewatcherservice.exe
net start filewatcherservice
