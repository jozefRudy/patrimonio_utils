{
  description = "Patrimonio Utils";

  inputs = {
    nixpkgs.url = "github:NixOS/nixpkgs/nixpkgs-unstable";
  };

  outputs = { self, nixpkgs }: let
    systems = [ "aarch64-darwin" "x86_64-linux" ];
    eachSystem = nixpkgs.lib.genAttrs systems;

    buildDotnetApp = pkgs: { pname, projectFile }:
      pkgs.buildDotnetModule {
        inherit pname projectFile;
        version = "1.0.0";
        src = self;
        nugetDeps = ./deps.json;
        executables = [ pname ];
        dotnet-sdk = pkgs.dotnet-sdk_9;
        selfContainedBuild = true;
        dotnetFlags = [
          "-p:Configuration=Release"
          "-p:PublishTrimmed=true"
          "-p:TrimMode=partial"
          "-p:PublishReadyToRun=false"
          "-p:PublishSingleFile=true"
          "-p:DebuggerSupport=false"
        ];
      };
  in {
    packages = eachSystem (system: let
      pkgs = nixpkgs.legacyPackages.${system};
    in {
      account = buildDotnetApp pkgs {
        pname = "Account";
        projectFile = "Account/Account.fsproj";
      };
      tracker = buildDotnetApp pkgs {
        pname = "Tracker";
        projectFile = "Tracker/Tracker.fsproj";
      };
    });
  };
}
