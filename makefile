build: build_release run_assetbuilder

run: build_release run_assetbuilder run_release

build_assetbuilder:
	dotnet build AssetBuilder

run_assetbuilder: build_assetbuilder
	cd AssetBuilder/bin/Debug/net7.0 && dotnet AssetBuilder.dll

build_debug:
	dotnet build Client

build_release:
	dotnet build Client --configuration Release

run_debug: build_debug
	cd Client/bin/Debug/net7.0 && dotnet Client.dll

run_release: build_release
	cd Client/bin/Release/net7.0 && dotnet Client.dll
