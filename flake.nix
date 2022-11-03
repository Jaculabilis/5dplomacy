{
  description = "5D Diplomacy With Multiversal Time Travel";

  inputs.flake-utils.url = "github:numtide/flake-utils";

  outputs = { self, nixpkgs, flake-utils }:
    flake-utils.lib.eachDefaultSystem (system:
      let pkgs = nixpkgs.legacyPackages.${system};
      in rec {
        devShell = pkgs.mkShell {
          shellHook = ''
            PS1='\[\e[0;94m\]\u@\H\[\e[0;93m\] 5dplomacy \[\e[0;92m\]$(git rev-parse --short HEAD)\[\e[0m\]\n\W$ '
          '';
          DOTNET_CLI_TELEMETRY_OPTOUT = 1;
          packages = [ pkgs.dotnet-sdk pkgs.dotnetPackages.NUnit3 ];
        };
      }
    );
}
