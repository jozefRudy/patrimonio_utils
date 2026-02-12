{
  description = "Patrimonio Utils";

  inputs = {
    nixpkgs.url = "github:NixOS/nixpkgs/nixpkgs-unstable";
  };

  outputs = {
    self,
    nixpkgs,
  }: let
    system = "aarch64-darwin";
    pkgs = nixpkgs.legacyPackages.${system};

    buildDotnetApp = {
      pname,
      projectFile,
    }:
      pkgs.buildDotnetModule {
        inherit pname projectFile;
        version = "1.0.0";
        src = self;
        nugetDeps = ./deps.json;
        executables = [pname];
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
    packages.${system} = {
      account = buildDotnetApp {
        pname = "Account";
        projectFile = "Account/Account.fsproj";
      };
      tracker = buildDotnetApp {
        pname = "Tracker";
        projectFile = "Tracker/Tracker.fsproj";
      };
    };
  };
}
