using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NetCoreAudio;
using NetCoreAudio.Interfaces;

namespace StrausRadio
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            /*while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                await Task.Delay(1000, stoppingToken);

            }*/


            var player = new Player();

            player.PlaybackFinished += OnPlaybackFinished;

            await player.Play(@"C:\Users\Tom\Desktop\test.mp3");
        }

        private void OnPlaybackFinished(object sender, EventArgs e)
        {
            Console.WriteLine("Playback finished");
        }
    }
}
