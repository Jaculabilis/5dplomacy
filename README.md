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

Now let's start creating the project. dotnet has a lot of template options. We'll eventually want to have a web client and server, for true multiplayer, but first we want to build the core infrastructure that we can slap a server on top of. So, we're just going to make a console app for now.

```
5dplomacy$ dotnet new console -n MultiversalDiplomacy -o MultiversalDiplomacy
```

.NET 6 makes the Main() method implicit, but this program is going to become more complicated than a single Main(), so let's put the whole boilerplate back.

```
using System;

namespace MultiversalDiplomacy
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("stab");
        }
    }
}
```

And when we run it through `dotnet`:

```
5dplomacy$ dotnet run --project MultiversalDiplomacy/
stab
```

Neat. VS Code doesn't seamlessly work with nix, so to get the extensions working we'll need to restart it from an existing `nix develop` shell so `dotnet` is on the path. For the integrated terminal, we can creating a profile for `nix develop` and set it as the default profile.

```
"terminal.integrated.profiles.linux": {
    "nix develop": {
        "path": "nix",
        "args": ["develop"]
    }
}
```

## Comprehending all of time and space

Now for the data model. The state of the world can be described in three layers, so to speak.

1. The board is the same across time and multiverse. Since it consists of (i) provinces and (ii) borders connecting two provinces, we're going  to model it like a graph. However, a simple graph won't work because provinces are differently accessible to armies and fleets, and coasts are part of the same province while having distinct connectivity. Following the data model described in [godip](https://github.com/zond/godip), we will model this by subdividing provinces and making connections between those subdivisions. This will effectively create multiple distinct graphs for fleets and armies within the map, with some nodes grouped for control purposes into provinces. Since the map itself does not change across time, we can make this "layer" completely immutable and shared between turns and timelines.
2. Since we need to preserve the state of the past in order to time travel effectively, we won't be mutating a board state. Instead, we'll use orders submitted for each turn to append a new copy of the board state. This can't be represented by a simple list, since we can have more than one timeline branch off of a particular turn, so instead we'll use something like a directed graph and create turns pointing to their immediate past.
3. Given the map of the board and the state of all timelines, all units have a spatial location on the board and a temporal location in one of the turns. As the game progresses and timelines are extended, we'll create copies of the unit in each turn it appears in.

This is going to get complicated, and we're looking forward to implementing the [Diplomacy Adjudicator Test Cases](http://web.inter.nl.net/users/L.B.Kruijswijk/), so let's also create a test project:

```
5dplomacy:5dplomacy$ dotnet new nunit --name MultiversalDiplomacyTests --output MultiversalDiplomacyTests
```

I think dotnet will fetch NUnit when it needs it, but to get it into our environment so VS Code recognizes it, we add it to the nix shell:

```
packages = [ pkgs.dotnet-sdk pkgs.dotnetPackages.NUnit3 ];
```

After writing some basic tests, we can run them with:

```
5dplomacy:5dplomacy$ dotnet test MultiversalDiplomacyTests/
[...]
Starting test execution, please wait...
A total of 1 test files matched the specified pattern.

Passed!  - Failed:     0, Passed:     2, Skipped:     0, Total:     2, Duration: 29 ms - 5dplomacy/MultiversalDiplomacyTests/bin/Debug/net6.0/MultiversalDiplomacyTests.dll (net6.0)
```

Neat.
