using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.IO;

namespace StrausRadio
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private static Random rng = new Random();
        // TODO: Add Extension filter to settings
        private List<string> AudioExtensions = new List<string>() { ".mp3", ".flac" };
        // TODO: Add Path to music to settings
        private const string MUSIC_PATH = @"\\cerebrum\music";

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        private void Init()
        {

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
                        // track.FullPath;
                    }
                }
            }
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

                    album.Artist = artistDir.Name;
                    album.Name = albumDir.Name;

                    var tracks = albumDir.EnumerateFiles();

                    album.Tracks = tracks.Where(t => AudioExtensions.Contains(t.Extension))
                        .Select(t => new Track() { Name = t.Name, Extension = t.Extension, FullPath = t.FullName })
                        .ToList();

                    results.Add(album);
                }                
            }

            return results.OrderBy(r => rng.Next()).ToList();
        }
    }

    public struct Album
    {
        public string Name;
        public string Artist;
        public List<Track> Tracks;
    }

    public struct Track
    {
        public string Name;
        public string Extension;
        public string FullPath;
    }
}
