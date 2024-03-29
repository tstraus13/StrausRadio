using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;

namespace StrausRadio
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private static Random rng = new Random();

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            Settings.Init();
            ClearTemp();

            if (string.IsNullOrEmpty(Settings.MusicLibraryPath))
            {
                _logger.LogError($"Music Library Path is missing. Cannot start. Add a path and restart. Stopping at: {DateTime.Now}");
                return StopAsync(cancellationToken);
            }

            return base.StartAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation($"Beginning to play Albums at: {DateTime.Now}");

                var randomAlbums = GetAlbums(Settings.MusicLibraryPath);

                _logger.LogInformation($"Album listings have been retreived and randomized at: {DateTime.Now}");

                foreach (var album in randomAlbums)
                {
                    foreach (var track in album.Tracks)
                    {
                        _logger.LogInformation($"Now Playing \"{track.FileName}\" from {album.Title} by {album.Artist} at: {DateTime.Now}");

                        var file = track.Extension != ".wav" ? await ConvertToWav(track) : track.FullPath;

                        var process = new ProcessStartInfo("aplay", $"{Settings.APlayArgumnets} {file}")
                        {
                            CreateNoWindow = true,
                            UseShellExecute = false,
                            RedirectStandardError = true,
                            RedirectStandardOutput = true
                        };

                        var result = await ProcessAsync.RunAsync(process);

                        if (result == null || result.ExitCode == null || result.ExitCode != 0)
                            _logger.LogError($"There was an issue playing the file {track.FullPath} at: {DateTime.Now}");
                        else
                            _logger.LogInformation($"Finished playback of file at: {DateTime.Now}");

                    }

                    ClearTemp();
                }
            }
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            ClearTemp();
            return base.StopAsync(cancellationToken);
        }

        public override void Dispose()
        {
            ClearTemp();
            base.Dispose();
        }

        private List<Album> GetAlbums(string musicLocation)
        {
            var results = new List<Album>();

            DirectoryInfo musicDir = new DirectoryInfo(musicLocation);

            var artistDirs = musicDir.EnumerateDirectories();

            foreach (var artistDir in artistDirs)
            {
                var albumDirs = artistDir.EnumerateDirectories();

                foreach (var albumDir in albumDirs)
                {
                    if (albumDir.EnumerateDirectories().Any())
                    {
                        var discs = albumDir.EnumerateDirectories().OrderBy(d => d.Name);
                        var album = new Album();
                        var albumTracks = new List<Track>();

                        foreach (var disc in discs)
                        {
                            var trackFiles = disc.EnumerateFiles();

                            var tracks = trackFiles.Where(t => Settings.AudioExtensions.Contains(t.Extension)).OrderBy(t => t.Name);

                            foreach (var track in tracks)
                            {
                                var num = 1;

                                albumTracks.Add(new Track()
                                {
                                    Number = num,
                                    Disc = int.Parse(disc.Name.Split(" ")[1]),
                                    FileName = track.Name,
                                    FullPath = track.FullName,
                                    Extension = track.Extension
                                });

                                num++;
                            }
                        }

                        album.Artist = artistDir.Name;
                        album.Title = albumDir.Name;
                        album.Tracks = albumTracks.OrderBy(t => t.Disc).ThenBy(t => t.Number).ToList();

                        results.Add(album);
                    }

                    else
                    {
                        var album = new Album();

                        var trackFiles = albumDir.EnumerateFiles();

                        var tracks = trackFiles.Where(t => Settings.AudioExtensions.Contains(t.Extension)).OrderBy(t => t.Name);

                        var albumTracks = new List<Track>();

                        foreach (var track in tracks)
                        {
                            var num = 1;

                            albumTracks.Add(new Track()
                            {
                                Number = num,
                                Disc = 1,
                                FileName = track.Name,
                                FullPath = track.FullName,
                                Extension = track.Extension
                            });

                            num++;
                        }

                        album.Artist = artistDir.Name;
                        album.Title = albumDir.Name;
                        album.Tracks = albumTracks.OrderBy(t => t.Disc).ThenBy(t => t.Number).ToList();

                        results.Add(album);
                    }
                }                
            }

            return results.OrderBy(r => rng.Next()).ToList();
        }

        private async Task<string> ConvertToWav(Track track)
        {
            ProcessStartInfo process;
            ProcessAsync.Result result;

            var tempFile = $"{Settings.TempDirectory}/{Guid.NewGuid()}.wav";

            process = new ProcessStartInfo("ffmpeg", $"-i \"{track.FullPath}\" \"{tempFile}\"")
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true
            };
            result = await ProcessAsync.RunAsync(process);

            if (result == null || result.ExitCode == null || result.ExitCode != 0)
                _logger.LogError($"There was an issue Converting the file {track.FullPath} to WAV at: {DateTime.Now}");

            else
                _logger.LogInformation($"Successfully converted file to WAV at: {DateTime.Now}");

            return tempFile;
        }

        private void ClearTemp()
        {
            _logger.LogInformation($"Clearing Temp folder at: {DateTime.Now}");

            DirectoryInfo temp = new DirectoryInfo(Settings.TempDirectory);

            var files = temp.EnumerateFiles().Where(f => f.Extension.Contains("wav"));

            foreach (var file in files)
            {
                file.Delete();
            }
        }
    }

    public struct Album
    {
        public string Title;
        public string Artist;
        public List<Track> Tracks;
    }

    public struct Track
    {
        public int Number;
        public int Disc;
        public string FileName;
        public string Extension;
        public string FullPath;
    }
}
