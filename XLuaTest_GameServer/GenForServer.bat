cd ./Protoc/protoc-3.19.0-win64/bin/
protoc ^
--csharp_out=./../../../GameServer/GateServer/PB/ ^
--proto_path=./../../../ProtoFiles/ ^
LaunchPB.proto
pause