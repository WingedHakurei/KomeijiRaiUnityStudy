mkdir build64 & pushd build64
cmake -G "MinGW Makefiles" ..
popd
cmake --build build64 --config Release
md plugin_lua53\Plugins\x86_64
copy /Y build64\libxlua.dll plugin_lua53\Plugins\x86_64\xlua.dll
pause