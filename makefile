build_assetbuilder:
	dotnet build AssetBuilder

run_assetbuilder: build_assetbuilder
	cd AssetBuilder/bin/Debug/net7.0; \
		dotnet AssetBuilder.dll

build: run_assetbuilder
	dotnet build Voxel

build_release: run_assetbuilder
	dotnet build Voxel --configuration Release

run: build
	cd Voxel/bin/Debug/net7.0; \
		dotnet Voxel.dll

run_release: build_release
	cd Voxel/bin/Release/net7.0; \
    	dotnet Voxel.dll
