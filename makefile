ifeq ($(XDG_SESSION_TYPE),wayland)
#export SDL_VIDEODRIVER=wayland
endif

build: build_release

run: run_release

renderdoc_run: build_release
	cd Client/bin/Release/net8.0 && renderdoccmd capture dotnet Client.dll

build_debug:
	dotnet build Client

build_release:
	dotnet build Client --configuration Release

run_debug: build_debug
	cd Client/bin/Debug/net8.0 && dotnet Client.dll

run_release: build_release
	cd Client/bin/Release/net8.0 && dotnet Client.dll

copy_assets:
	cp -r Content/builtin Client/bin/Debug/net8.0
	cp -r Content/builtin Client/bin/Release/net8.0
