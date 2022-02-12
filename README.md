# 5D Diplomacy With Multiversal Time Travel

## So you want to conquer Europe with a declarative build system

Let's start out by initializing the project. I always hate this part of projects; it's much easier to pick up something with an established codebase and ecosystem and figure out how to modify it to be slightly different than it is to strain genius from the empty space of possibility _de novo_. The ultimate goal of this project is summoning military aid from beyond space and time, though, so we're going to have to get used to it.

A `nix flake init` gives us a fairly useless flake template:

```
{
  description = "A very basic flake";

  outputs = { self, nixpkgs }: {

    packages.x86_64-linux.hello = nixpkgs.legacyPackages.x86_64-linux.hello;

    defaultPackage.x86_64-linux = self.packages.x86_64-linux.hello;

  };
}
```

We're going to replace every line in this file, but at least we got a start. Let's also `git init` and set that part up.

```
$ git init
$ git config --add user.name Jaculabilis
$ git config --add user.email jaculabilis@git.alogoulogoi.com
$ git add flake.nix README.md
$ git commit -m "Initial commit"
$ git remote add origin gitea@git.alogoulogoi.com:Jaculabilis/5dplomacy.git
$ git push -u origin master
```

We're doing this in .NET, so we need the .NET SDK. To do that, we're going to delcare a development environment in the flake config.

```
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
```

Declaring `inputs.flake-utils` adds the `flake-utils` package as a dependency, which just gives us Nix helper functions. What's important here is that the `packages.x86_64-linux.hello` above has been abstracted away behind the `eachDefaultSystem` function: now we define our outputs with the `system` input as context, and flake-utils will define our outputs for each default system.

Basically, stripping the boilerplate, we're just doing this:

```
rec {
  devShell = pkgs.mkShell {
    packages = [ pkgs.dotnet-sdk ];
  };
}
```

`pkgs.mkShell` is the derivation builder that creates shell environments. It takes `packages` as a list of input packages that will be made available in the shell environment it creates. We add `dotnet-sdk` to this, commit the changes to git, and enter our new shell with `nix develop`:

```
$ which dotnet
/nix/store/87s452c8wj2zmy21q8q394f6rzf5y1br-dotnet-sdk-6.0.100/bin/dotnet
```

So now we have our development tools (well, tool). The `dotnet --help` text tells us a few things about telemetry, so let's define a prompt so we know when we're in the nix shell and then set the telemetry opt-out.

```
shellHook = ''
  PS1="5dplomacy:\W$ "
'';
DOTNET_CLI_TELEMETRY_OPTOUT = 1;
```
