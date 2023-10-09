build: build_release run_assetbuilder

run: build_release run_assetbuilder run_release

build_assetbuilder:
	dotnet build AssetBuilder

run_assetbuilder: build_assetbuilder
	cd AssetBuilder/bin/Debug/net7.0; \
		dotnet AssetBuilder.dll

build_debug:
	dotnet build Voxel

build_release:
	dotnet build Voxel --configuration Release

run_debug: build_debug
	cd Voxel/bin/Debug/net7.0; \
		dotnet Voxel.dll

run_release: build_release
	cd Voxel/bin/Release/net7.0; \
		dotnet Voxel.dll
