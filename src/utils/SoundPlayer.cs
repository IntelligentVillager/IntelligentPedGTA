using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NAudio.Wave;

namespace GTA.NPCTest.src.utils
{
    class SoundPlayer
    {
        private WaveOutEvent waveOut;
        public void SetVolume(float volume)
        {
            if (OutExisted())
            {
                waveOut.Volume = volume;
            }
        }

        public SoundPlayer()
        {
            waveOut = new WaveOutEvent();
            waveOut.PlaybackStopped += OnPlaybackStopped;
        }

        private void OnPlaybackStopped(object sender, StoppedEventArgs e)
        {
            Logger.INFO("OnPlaybackStopped" + e.ToString());
            if (e.Exception!= null)
            {
                Logger.ERROR(e.Exception);
            }
        }

        public SoundPlayer(string audioFilePath)
        {
            using (var audioFile = new AudioFileReader(audioFilePath))
            {
                waveOut = new WaveOutEvent();
                waveOut.PlaybackStopped += OnPlaybackStopped;
                waveOut.Init(audioFile);
            }
        }

        public bool IsPlaying()
        {
            if (OutExisted())
            {
                if (waveOut.PlaybackState == PlaybackState.Playing)
                {
                    return true;
                }
                return false;
            }
            return false;
        }

        public void StartPlay()
        {
            if (OutExisted())
            {
                waveOut.Stop();
                waveOut.Play();
            }
        }

        public void Dispose()
        {
            if (OutExisted())
            {
                waveOut.Stop();
                waveOut.Dispose();
            }
        }

        public void StartPlay(string audioFilePath)
        {
            if (!OutExisted())
            {
                waveOut = new WaveOutEvent();
                waveOut.PlaybackStopped += OnPlaybackStopped;
            }

            using (var audioFile = new AudioFileReader(audioFilePath))
            {
                waveOut.Init(audioFile);
                waveOut.Play();
                while (waveOut.PlaybackState == PlaybackState.Playing)
                {
                    audioFile.Volume = waveOut.Volume;
                    Thread.Sleep(1000);
                }
            }
        }

        public Boolean OutExisted()
        {
            return waveOut != null;
        }
    }
}
