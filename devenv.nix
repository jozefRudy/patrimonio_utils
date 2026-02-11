{pkgs, ...}: let
  buildDotnetApp = {
    pname,
    projectFile,
  }:
    pkgs.buildDotnetModule {
      inherit pname projectFile;
      version = "1.0.0";
      src = ./.;
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
  packages = [
    pkgs.git
    pkgs.nuget-to-json
  ];

  languages.dotnet = {
    enable = true;
    package = pkgs.dotnet-sdk_9;
  };
  scripts = {
    fetch-deps = {
      exec = "dotnet restore --packages ./nuget-packages && nuget-to-json ./nuget-packages > deps.json && rm -rf ./nuget-packages";
    };
    build-all = {
      exec = ''
        devenv build outputs.account
        devenv build outputs.tracker
      '';
    };
  };

  outputs = {
    account = buildDotnetApp {
      pname = "Account";
      projectFile = "Account/Account.fsproj";
    };

    tracker = buildDotnetApp {
      pname = "Tracker";
      projectFile = "Tracker/Tracker.fsproj";
    };
  };
}
