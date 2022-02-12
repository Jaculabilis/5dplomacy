{
  description = "5D Diplomacy With Multiversal Time Travel";

  inputs.flake-utils.url = "github:numtide/flake-utils";

  outputs = { self, nixpkgs, flake-utils }:
    flake-utils.lib.eachDefaultSystem (system:
      let pkgs = nixpkgs.legacyPackages.${system};
      in rec {
        devShell = pkgs.mkShell {
          packages = [ pkgs.dotnet-sdk ];
        };
      }
    );
}
