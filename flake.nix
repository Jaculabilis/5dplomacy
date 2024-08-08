{
  description = "5D Diplomacy With Multiversal Time Travel";

  inputs.nixpkgs.url = "github:NixOS/nixpkgs/24.05";
  inputs.flake-utils.url = "github:numtide/flake-utils";

  outputs = { self, nixpkgs, flake-utils }:
    flake-utils.lib.eachDefaultSystem (system:
      let pkgs = nixpkgs.legacyPackages.${system};
      in rec {
        devShell = pkgs.mkShell {
          DOTNET_CLI_TELEMETRY_OPTOUT = 1;
          NIX_LD_LIBRARY_PATH = pkgs.lib.makeLibraryPath [ pkgs.stdenv.cc.cc ];
          NIX_LD = builtins.readFile "${pkgs.stdenv.cc}/nix-support/dynamic-linker";
          packages = [
            pkgs.bashInteractive
            pkgs.dotnet-sdk_8
            pkgs.dotnetPackages.NUnit3
          ];
        };
      }
    );
}
