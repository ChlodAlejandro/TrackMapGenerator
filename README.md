# TrackMapGenerator
Create tropical cyclone track maps.

It is inspired by titoxd's [wptc-track](https://github.com/titoxd/wptc-track), written in C# and based on .NET core for cross-platform compatibility. It is ***not*** backwards-compatible with wptc-track as it uses a renewed arguments system built from the ground up, inspired by FFmpeg.

## Differences with wptc-track
* Written in C# and uses .NET core, which allows it to run on Linux, macOS, and Windows (even without WSL or Cygwin), as long as the [.NET Core 3.1 Runtime](https://dotnet.microsoft.com/download/dotnet/3.1/runtime) has been installed.
    * .NET is open-source and released under the [MIT license](https://github.com/dotnet/runtime/blob/main/LICENSE.TXT).
* Uses the ImageSharp library instead of Cairo.
* Arguments are now parsed in order.
* `--next` is entirely removed, `--input` is now used to delimit storms.
* Support for different map projections have been added, but the only available projection as of now is the [Mercator projection](https://en.wikipedia.org/wiki/Mercator_projection).
* Maps and data are no longer bundled with code. Maps are now downloaded from the internet.

## License
This program is licensed under the GNU General Public License v3.0. There are no limitations to the images created by this program — you may license them under any license you want.