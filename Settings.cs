using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace StrausRadio
{
    public static class Settings
    {
        public static string TempDirectory { get; private set; }
        public static List<string> AudioExtensions { get; private set; }
        public static string MusicLibraryPath { get; private set; }
        public static string APlayArgumnets { get; private set; }


        private static IConfigurationRoot ConfigFile { get; set; }

        private static readonly string DefaultDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".StrausRadio");

        /// <summary>
        /// Initializes the settings and loads any settings from
        /// the app settings files
        /// </summary>
        public static void Init()
        {
            var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            var builder = new ConfigurationBuilder()
                .AddJsonFile($"appsettings.json", true, true)
                //.AddJsonFile($"appsettings.{env}.json", true, true)
                .AddJsonFile(Path.Combine(DefaultDirectory, "settings.json"), true, true)
                .AddUserSecrets(Assembly.GetExecutingAssembly(), true);

            ConfigFile = builder.Build();

            Directory.CreateDirectory(DefaultDirectory);


            if (!string.IsNullOrEmpty(ConfigFile["TempDirectory"]))
            {
                TempDirectory = ConfigFile["TempDirectory"];

                Directory.CreateDirectory(TempDirectory);
            }

            else
            {
                TempDirectory = @"/tmp";
            }

            if (!string.IsNullOrEmpty(ConfigFile["AudioExtensions"]))
            {
                AudioExtensions = ConfigFile["CompletedDirectory"].Split(";").ToList();
            }

            else
            {
                AudioExtensions = new List<string>() { ".mp3", ".flac", ".wav" };
            }

            if (!string.IsNullOrEmpty(ConfigFile["MusicLibraryPath"]))
            {
                MusicLibraryPath = ConfigFile["MusicLibraryPath"];
            }

            if (!string.IsNullOrEmpty(ConfigFile["APlayArgumnets"]))
            {
                APlayArgumnets = ConfigFile["APlayArgumnets"];
            }
        }
    }
}
