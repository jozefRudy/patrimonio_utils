{pkgs, ...}: {
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
  };
}
