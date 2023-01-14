using System;
using System.Threading;
using System.Windows.Media;
using System.Windows.Threading;
using GTA;
using GTA.Math;

namespace GTA.NPCTest.src.utils
{
	public class GameSound
	{
		public MediaPlayer soundStream;
		private Vector3 soundPosition;
		private Entity entitySource;
		private double maxSoundDistance;
		private bool loopSound;
		private bool HasPlayed;

		public TimeSpan Position
		{
			get
			{
				return (soundStream != null) ? soundStream.Position : new TimeSpan(0, 0, 0);
			}
			set
			{
				soundStream.Position = value;
			}
		}

		public double Volume
		{
			get
			{
				return (soundStream != null) ? soundStream.Volume : (-1.0);
			}
			set
			{
				if (soundStream != null)
				{
					soundStream.Volume = value;
				}
			}
		}

		public double MaxDistance
		{
			get
			{
				return maxSoundDistance;
			}
			set
			{
				value = maxSoundDistance;
			}
		}

        public Dispatcher dispatcher;

        public GameSound(Vector3 position, string filepath, double volume = 1.0, double maxDistance = 50.0, bool loop = false)
		{
			if (!StreamExists())
			{
				soundPosition = position;
				soundStream = new MediaPlayer();
				soundStream.Open(new Uri(filepath, UriKind.Relative));
				soundStream.Volume = volume;
				Volume = volume;
				maxSoundDistance = maxDistance;
				loopSound = loop;
				dispatcher = Dispatcher.CurrentDispatcher;

			}
		}

		public GameSound(Entity sourceEntity, string filepath, double volume = 1.0, double maxDistance = 50.0, bool loop = false)
		{
			if (!StreamExists())
			{
				entitySource = sourceEntity;
				soundPosition = sourceEntity.Position;
				soundStream = new MediaPlayer();
				soundStream.Open(new Uri(filepath, UriKind.Relative));
				soundStream.Volume = volume;
				Volume = volume;
				maxSoundDistance = maxDistance;
				loopSound = loop;
				dispatcher = Dispatcher.CurrentDispatcher;
			}
		}

		public GameSound(string filepath, double volume = 1.0, double maxDistance = 50.0, bool loop = false)
		{
			if (!StreamExists())
			{
				try
				{
					soundStream = new MediaPlayer();
					soundStream.Open(new Uri(filepath, UriKind.Relative));
					soundStream.Volume = volume;
					Volume = volume;
					maxSoundDistance = maxDistance;
					loopSound = loop;
					dispatcher = soundStream.Dispatcher;
				} catch (Exception ex)
                {
					Logger.ERROR(ex);
                }

			}
		}

		public void RefreshSourceAndPlay(string filepath)
		{
			try
			{
				//soundStream.VerifyAccess();
				Logger.INFO("RefreshSourceAndPlay " + Thread.CurrentThread.ManagedThreadId.ToString() + soundStream.Dispatcher.Thread.ManagedThreadId.ToString());

				if (soundStream.HasAudio)
                {
					soundStream.Close();
				}
				soundStream = new MediaPlayer();
				soundStream.Open(new Uri(filepath, UriKind.Relative));
				soundStream.Volume = Volume;
				soundStream.Play();
				HasPlayed = true;

			} catch (Exception ex)
            {
				Logger.ERROR(ex);
            }
		}

		public void StartPlay(bool forceRestart = true)
		{
			if (StreamExists())
			{
				Logger.INFO("StartPlay ");

				soundStream.Dispatcher.Invoke(new Action(() => {
					if (forceRestart)
					{
						Stop();
					}
					soundStream.Play();
					HasPlayed = true;
				}));
			}
		}

		public bool IsPlaying()
		{
			if (StreamExists() && HasPlayed)
			{
				soundStream.Dispatcher.Invoke(new Action(() =>
				{
					Logger.INFO("playing " + soundStream.Position.ToString());
				}));
				if (Position >= soundStream.NaturalDuration)
				{
					if (loopSound)
					{
						Position = new TimeSpan(0, 0, 0);
						StartPlay();
						return true;
					}
					Stop();
					return false;
				}
				return true;
			}
			return false;
		}

		public void Stop()
		{
			if (StreamExists())
			{
				soundStream.Stop();
			}
			HasPlayed = false;
		}

		public void Dispose()
		{
			if (StreamExists())
			{
				HasPlayed = false;
				//soundStream.Dispatcher.BeginInvoke(new Action(() =>
				//{
				//	soundStream.Stop();
				//	soundStream.Close();
				//}));
				soundStream.Stop();
				soundStream.Close();
			}
		}

		private bool StreamExists()
		{
			return soundStream != null;
		}
	}

}
