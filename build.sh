#!/usr/bin/env bash

# Uses msbuild to compile the release version of the plugin. A distributable zip will be placed in your working dir.
VERSION="2.0.0"
workingDir=$(pwd)
msbuild Tebex-SpaceEngineers.sln -p:Configuration=Release
cd ./Tebex-SpaceEngineers/bin/x64/Release
mv Tebex-SpaceEngineers.dll Tebex-SpaceEngineers-${VERSION}.dll
zip "Tebex-SpaceEngineers-${VERSION}.zip" Tebex-SpaceEngineers-${VERSION}.dll
mv "Tebex-SpaceEngineers-${VERSION}.zip" "${workingDir}"
cd $workingDir