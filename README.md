# cmdr

**Important: CMDR has moved to a new home since Jan 2020.**\
.\
The new site is: https://github.com/cmdr-editor/cmdr/blob/master/README.md \
The latest download is: https://github.com/cmdr-editor/cmdr/releases/latest/download/cmdr_tsi_editor_latest.zip

[<img src="docs/pics/icon/download.png?raw=true" width="400">](https://github.com/cmdr-editor/cmdr/releases/latest/download/cmdr_tsi_editor_latest.zip)

## Description

CMDR is a TSI mappings editor for NI Traktor Pro. It supports TP3.3 and specific controllers like S4MK3.

Use CMDR to edit TSI files with basic features missing in Traktor's Controller Manager:
* Copy-Paste
* Mass-Edit
* MAss-Automation
* plus much more ([Full Changelog](docs/development/Change_Log.md))

CMDR is a Windows application, but there are several ways to [run it in you macOS](docs/running_on_macos.md).

## Download and Installation

The latest download is ALWAYS available in this link: 
[![download](docs/pics/icon/download.png?raw=true)](https://github.com/cmdr-editor/cmdr/releases/latest/download/cmdr_tsi_editor_latest.zip)

There is no installer yet. After download just unzip the file and start cmdr.exe.

## AntiVirus issues

If the EXE file disapears, this is because your anti-virus quarantined the file. CMDR doesn't have a digital signature yet.\
In this case make an exception for CMDR.exe; CMDR has a perfect VirusTotal check: [0 out of 72](https://www.virustotal.com/gui/file/ab1db0d627782cfd55867addbac61d55be9debed9c3483ee527733eecf7b6610/detection)


## Requirements

* Microsoft Windows
* [.NET Framework V4.5](https://www.microsoft.com/en-US/download/details.aspx?id=30653)

## Usage 

Please see [the docs folder](docs) to learn how to use CMDR:
* [User Guide](docs/user_guide.md)
* [Midi Mapping](docs/midi_mapping.md) 
* [Using Effects](docs/Effects.md)
* [FAQ](docs/faq.md)
* [macOS usage](docs/running_on_macos.md).

If you are already familiar with CMDR, check [this folder](docs/development) as well
* [2020 improvements](#2020-improvements)
* [Full Change Log](docs/development/Change_Log.md)
* [Development Info](docs/development/Development_Info.md)

## Screenshot

![cmdr_summary](docs/pics/cmdr_summary.png?raw=true)

Be sure to see the [2020 updates as well](#2020-improvements)

## Disclaimer

This software is under active development, comes with ABSOLUTELY NO WARRANTY and may contain bugs. 
If you find a bug please [create an issue](https://github.com/cmdr-editor/cmdr/issues).
cmdr is NOT RELATED TO Native Instruments, but an unofficial software.
Until the software has reached a stable state, it is recommended to work on copies of your TSI files.

## Licence

* cmdr was developed and released in 2018 by Michael Rahier under the GPL v3 licence.
* As of Jan 2020 this is being maintained and improved by Pedro Estrela, same license.
* Used libraries:
  * [Xceed AvalonDock](http://avalondock.codeplex.com), Copyright (c) 2007-2013, Xceed Software Inc. 
  * [Pure Midi](https://puremidi.codeplex.com), Copyright (c) 2004 Slawomir Cichon
  * [Newtonsoft Json](http://www.newtonsoft.com/json), Copyright (c) 2007 James Newton-King

## Credits

* Michael Rahier for creating this Program
* Pedro Estrela for maitaining it since 2020
* Ivan Zlatev, for [reverse engineering large parts of the TSI structure](https://github.com/ivanz/Traktor.Mapping).
* [Native Instruments](http://gitwww.native-instruments.com/) for providing the powerful DJ software Traktor Pro.
* [Acid Buddha](http://www.acidbuddha.com/) for testing and support. 

## Donation

If you like this piece of software, please make a donation to the original author: 
[![Donate button](https://www.paypalobjects.com/en_US/i/btn/btn_donate_SM.gif)](https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=9BNNTDQF4X782 "Donate")


## 2020 improvements

Consult the [changelog](docs/development/Change_Log.md) for complete details.

![cmdr_improvements1](docs/pics/cmdr_improvements1.png?raw=true "")
![cmdr_improvements2](docs/pics/cmdr_improvements2.png?raw=true "")
![cmdr_reports](docs/pics/cmdr_reports.png?raw=true "")
![cmdr_csv_export](docs/pics/cmdr_csv_export.png?raw=true "")
![cmdr_keyboard_shortcuts](docs/pics/cmdr_keyboard_shortcuts.png?raw=true "")
![cmdr_settings](docs/pics/cmdr_settings.png?raw=true "dede")
