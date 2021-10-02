# TrackMapGenerator
Create tropical cyclone track maps.

It is inspired by titoxd's [wptc-track](https://github.com/titoxd/wptc-track), written in C# and based on .NET core for cross-platform compatibility. It is ***not*** backwards-compatible with wptc-track as it uses a renewed arguments system built from the ground up, inspired by FFmpeg.

If you're not used to a command line interface, a much more user-friendly generator is available [here](https://trackgen.codingcactus.repl.co/).

## Beta
This is a project in development. Some old features have not been ported yet, including:
* HURDAT, TCR, MD, and TAB format support
* JMA, IMD, and MFR scales
* Scale color customization
* Wikipedia template autofill

## Differences with wptc-track
* Written in C# and uses .NET core, which allows it to run on Linux, macOS, and Windows (even without WSL or Cygwin), as long as the [.NET Core 3.1 Runtime](https://dotnet.microsoft.com/download/dotnet/3.1/runtime) has been installed.
    * .NET is open-source and released under the [MIT license](https://github.com/dotnet/runtime/blob/main/LICENSE.TXT).
* Uses the ImageSharp library instead of Cairo.
* Arguments are now parsed in order.
* `--next` is entirely removed, `--input` is now used to delimit storms.
* Support for different map projections have been added, but the only available projection as of now is the [Mercator projection](https://en.wikipedia.org/wiki/Mercator_projection).
* Maps and data are no longer bundled with code. Maps are now downloaded from the internet.
    * The default map used by the Mercator projection is now a 24,000 by 12,000 pixel image (compared to the previous 8,192 by 4,096). 
    * Maps are saved in `%AppData%/Roaming/TrackMapGenerator` on Windows, `~/.config/TrackMapGenerator` on macOS and Linux.
* The size of the dots and lines will scale depending on the map size.

## License
This program is licensed under the GNU General Public License v3.0. There are no limitations to the images created by this program — you may license them under any license you want.