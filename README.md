# Convert2Dsk #

![CI Build](https://github.com/jonthysell/Convert2Dsk/workflows/CI%20Build/badge.svg)

Convert2Dsk is an application for converting Disk Copy 4.2 images into raw disk images.

Convert2Dsk was written in C# and should run anywhere that supports [.NET Core 3.1](https://github.com/dotnet/core/blob/master/release-notes/3.1/3.1-supported-os.md). It has been officially tested on:

* Windows 10
* Ubuntu 20.04
* macOS 10.15

## Installation ##

### Windows ###

The Windows release is a self-contained x86 binary and runs on Windows 7 SP1+, 8.1, and 10.

1. Download the latest Windows zip file (Convert2Dsk.Windows.zip) from https://github.com/jonthysell/Convert2Dsk/releases/latest
2. Extract the zip file

### MacOS ###

The MacOS release is a self-contained x64 binary and runs on OSX >= 10.13.

1. Download the latest MacOS tar.gz file (Convert2Dsk.MacOS.tar.gz) from https://github.com/jonthysell/Convert2Dsk/releases/latest
2. Extract the tar.gz file

### Linux ###

The Linux release is a self-contained x64 binary and runs on many Linux distributions.

1. Download the latest Linux tar.gz file (Convert2Dsk.Linux.tar.gz) from https://github.com/jonthysell/Convert2Dsk/releases/latest
2. Extract the tar.gz file

### Usage ###

```none
Usage: convert2dsk [--version] [--help]
                   [options...] <paths...>

Paths can be files or directories of files.

Options:
-v, --verbose  Show verbose output
```

## Errata ##

Convert2Dsk is open-source under the MIT license.

Convert2Dsk Copyright (c) 2020 Jon Thysell
