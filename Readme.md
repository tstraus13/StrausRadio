# Straus Radio

## Overview

Straus Radio is a background service meant to run on a Raspberry Pi running Raspbian. It will play your music collection by album in random order continuously. You can then hook it up to a receiver, speakers, FM Transmitter, or whatever.

## Dependencies

1. **Raspberry Pi/Raspbian** - I wrote this using a Raspberry Pi with Raspbian as the main distrobution but in theory any Linux Distribution could work.

2. **FFMPEG** - This is used for doing temporary conversion to WAV if your collection is in another format

3. **APlay (Alsa Play)** - Used to actually play the music

## Installation

I have included a SystemD unit file in this repository. You may customize to your liking but the one included should get to you started. Once configured to start with the system, it will begin when the system is booted. I do recommend, before running it as a service, to first do a manual run to ensure it is working properly.

## Configuration

In the repository there is a file named settings.json. It has all the settings that you can change/manipulate. I will try to include a settings file will all available settings listed. More will likely be added as time goes on. This settings file should be located in the Straus Radio default directory ($HOME/.StrausRadio) which is created on first launch. If no file is found it will use the defaults.

## Library Folder Structure

This program will look for music assuming the following directory structure is true.

Music Library Root > Artist > Album > Tracks

OR 

Music Library Root > Artist > Album > Disc > Tracks

## Building

This application is written in .Net 5 as a Hosted Service. You can build an executable yourself. When inside the root directory of the solution run the follow:

```dotnet publish -r linux-x64 -c Release /p:PublishSingleFile=true /p:PublishTrimmed=true```

This will build a single executable for simplicity. You can build/publish as you please if you want to do something different.
