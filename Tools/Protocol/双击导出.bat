@echo off
chcp 65001

echo ****************************************  
echo ===============协议导出=================
echo ****************************************  
echo .

SET OUT_PATH=%cd%\..\..\WayPoint\Assets\Shared\ProtocolDef

echo cs文件目录 %OUT_PATH%

echo .
echo 开始执行...

echo 正在导出 proto_battle.proto 
tools\ProtoGen.exe "proto_battle.proto" -output_directory=%OUT_PATH% --proto_path=.\ --include_imports=%cd%
echo .



echo .
echo 导出完成!
pause 