using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using TagLib;

namespace StrausRadio
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private static Random rng = new Random();
        // TODO: Add Extension filter to settings
        private List<string> AudioExtensions = new List<string>() { ".mp3", ".flac" };
        // TODO: Add Path to music to settings
        private const string MUSIC_PATH = @"/mnt/music";
        // TODO: Add Temp Path to settings
        private const string TEMP_PATH = @"/tmp";

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        private void Init()
        {
            ClearTemp(TEMP_PATH);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Init();

            _logger.LogInformation($"Worker started at: {DateTime.Now}");

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation($"Beginning to play Albums at: {DateTime.Now}");

                var randomAlbums = GetAlbums(MUSIC_PATH);

                _logger.LogInformation($"Album listings have been retreived and randomized at: {DateTime.Now}");

                foreach (var album in randomAlbums)
                {
                    foreach (var track in album.Tracks)
                    {
                        _logger.LogInformation($"Now Playing {track.Name} from {album.Title} by {album.Artist} at: {DateTime.Now}");
                        // track.FullPath;
                        var tempWav = await ConvertToWav(track);

                        var process = new ProcessStartInfo("aplay", $"-Dhw:1,0 {tempWav}")
                        {
                            CreateNoWindow = true,
                            UseShellExecute = false,
                            RedirectStandardError = true,
                            RedirectStandardOutput = true
                        };

                        await ProcessAsync.RunAsync(process);
                    }
                }

                ClearTemp(TEMP_PATH);
            }

            ClearTemp(TEMP_PATH);
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
                    var album = new Album();

                    var trackFiles = albumDir.EnumerateFiles();

                    var tracks = trackFiles.Where(t => AudioExtensions.Contains(t.Extension))
                        .OrderBy(t => t.Name);

                    var albumTracks = new List<Track>();

                    foreach (var track in tracks)
                    {
                        albumTracks.Add(new Track()
                        {
                            Title = TagLib.File.Create(track.FullName).Tag.Title,
                            Name = track.Name,
                            FullPath = track.FullName,
                            Extension = track.Extension
                        });
                    }

                    var tags = TagLib.File.Create(albumTracks.First().FullPath);

                    album.Artist = tags.Tag.FirstAlbumArtist;
                    album.Title = tags.Tag.Album;

                    results.Add(album);
                }                
            }

            return results.OrderBy(r => rng.Next()).ToList();
        }

        private async Task<string> ConvertToWav(Track track)
        {
            ProcessStartInfo process;

            var tempFile = $"{TEMP_PATH}/{Guid.NewGuid()}.wav";

            switch (track.Extension)
            {
                case ".flac":
                    process = new ProcessStartInfo("flac", $"-d \"{track.FullPath}\" -o \"{tempFile}\"")
                    {
                        CreateNoWindow = true,
                        UseShellExecute = false,
                        RedirectStandardError = true,
                        RedirectStandardOutput = true
                    };
                    await ProcessAsync.RunAsync(process);
                    break;
                case ".mp3":
                    process = new ProcessStartInfo("mpg123", $"-w \"{tempFile}\" \"{track.FullPath}\"")
                    {
                        CreateNoWindow = true,
                        UseShellExecute = false,
                        RedirectStandardError = true,
                        RedirectStandardOutput = true
                    };
                    await ProcessAsync.RunAsync(process);
                    break;
                default:
                    return null;
            }

            return tempFile;
        }

        private void ClearTemp(string tempDir)
        {
            _logger.LogInformation($"Clearing Temp folder at: {DateTime.Now}");

            DirectoryInfo temp = new DirectoryInfo(tempDir);

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
        public string Title;
        public string Name;
        public string Extension;
        public string FullPath;
    }
}
