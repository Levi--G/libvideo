# libvideo

![icon](icons/icon_200.png)

[![license](https://img.shields.io/github/license/Levi--G/libvideo.svg)](LICENSE)

libvideo (aka VideoLibrary) is a modern .NET library for downloading YouTube videos. It is portable to most platforms and is very lightweight.
This is a fork of [i3arnon/libvideo](https://github.com/i3arnon/libvideo), it includes a few fixes to address some issues with youtube decryption.

## Installation

This fork has no nuget package but can be downloaded from releases instead.
Alternatively, you can try building the repo if you like your assemblies extra-fresh.

## Supported Platforms

- .NET Standard 2.0

## Getting Started

Here's a small sample to help you get familiar with libvideo:

```csharp
using VideoLibrary;

void SaveVideoToDisk(string link)
{
    var youTube = YouTube.Default; // starting point for YouTube actions
    var video = youTube.GetVideo(link); // gets a Video object with info about the video
    File.WriteAllBytes(@"C:\" + video.FullName, video.GetBytes());
}
```

If you'd like to check out some more of our features, take a look at our [docs](docs/README.md). You can also refer to our [example application](samples/Valks/Valks/Program.cs) (named Valks, yes, I know, it's a silly name) if you're looking for a more comprehensive sample.

## License

libvideo is licensed under the [BSD 2-clause license](LICENSE).

## FAQ

### What is the difference in this fork?

Since i have a gui based on this, i try to keep it up-to-date as much as possible
however other forks are also active and at times may be ahead of this one so feel free to swap alot!
If you need the specifics you can check my badly-commented commits

### Where do i submit any issues?

If its an issue on this fork you can do that right [here](https://github.com/Levi--G/libvideo/issues).
If it is for the main repo/other forks, please don't use this issue tracker.

### What do i submit in  the issue?

At the very least the youtube id that doesn't work, any errors or extra info is always handy.
Make sure however the video is not region-locked nor age-restricted as that will never work.

### Do you accept pull-requests?

Try and see ;)