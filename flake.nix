{
  description = "5D Diplomacy With Multiversal Time Travel";

  inputs.flake-utils.url = "github:numtide/flake-utils";

  outputs = { self, nixpkgs, flake-utils }:
    flake-utils.lib.eachDefaultSystem (system:
      let pkgs = nixpkgs.legacyPackages.${system};
      in rec {
        devShell = pkgs.mkShell {
          DOTNET_CLI_TELEMETRY_OPTOUT = 1;
          packages = [
            pkgs.bashInteractive
            pkgs.dotnet-sdk
            pkgs.dotnetPackages.NUnit3
          ];
        };
      }
    );
}
