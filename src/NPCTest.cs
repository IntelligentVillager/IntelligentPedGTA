using System;
using GTA.Math;
using System.Windows.Forms;
using System.IO;
using GTA;
using GTA.UI;
using GTA.Native;
using System.Collections.Generic;
using Control = GTA.Control;
using System.Linq;
using GTA.NPCTest.src.utils;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Threading;
using System.Net.Http;
using System.Text;
using GTA.NPCTest.src.config;
using System.Diagnostics;
using System.Speech.AudioFormat;

namespace GTA.NPCTest
{
    class NPCTest : Script
    {
        private const string PathMain = "NPCTest\\";

        private Dictionary<int, SoundPlayer> externalSounds = new Dictionary<int, SoundPlayer>();

        private List<Ped> pedList;
        private Ped currentTargetPed;

        private Dictionary<int, string> pedNameDictionary;
        private bool isProcessingMessage = false;

        private bool inputWindowOpen = false;
        private string enteredText = "";

        private SpeechRecognizer speechRecognizer;
        private SpeechSynthesizer speechSynthesizer;

        private bool isMicrophoneListening = false;
        private int microPhoneNotificationHandle;

        static string speechKey = "d1dc2e4991404d29a7244d0e42565783";
        static string speechRegion = "westus";

        private readonly Dispatcher dispatcher;
        private readonly Dispatcher tickDispatcher;
        private Dispatcher keyEventDispatcher;
        private SpeechConfig speechConfig;

        private string audioFileOutputPath = ".\\scripts\\NPCTest\\";

        public NPCTest()
        {
            try
            {
                this.Tick += OnTick;
                this.KeyUp += OnKeyUp;
                this.KeyDown += OnKeyDown;
                this.Aborted += OnAbort;
                this.pedList = new List<Ped>();
                this.isProcessingMessage = false;

                Logger.ClearLog();

                //REQUEST_ANIM_DICT
                Function.Call(Hash.REQUEST_ANIM_DICT, "mp_facial");
                Function.Call(Hash.REQUEST_ANIM_SET, "mic_chatter");

                this.pedNameDictionary = new Dictionary<int, String>();
                this.currentTargetPed = null;
                string currentDirectory = Directory.GetCurrentDirectory();
                //this.audioFileOutputPath += "33.wav";

                this.audioFileOutputPath += Guid.NewGuid().ToString() + ".wav";
                this.dispatcher = Dispatcher.CurrentDispatcher;
                Function.Call(Hash.SET_TEXT_FONT,2);

                GTA.UI.Notification.Show("init at ~b~" + currentDirectory);

                speechConfig = SpeechConfig.FromSubscription(speechKey, speechRegion);
                speechConfig.SpeechRecognitionLanguage = "en-US";
                speechConfig.SpeechSynthesisLanguage = "en-US";
                speechConfig.SpeechSynthesisVoiceName = "en-US-JennyNeural";

                //speechConfig.SetSpeechSynthesisOutputFormat(SpeechSynthesisOutputFormat.Audio16Khz64KBitRateMonoMp3);

                //var audioConfig = AudioConfig.FromDefaultSpeakerOutput();
 
                this.speechRecognizer = new SpeechRecognizer(speechConfig, AudioConfig.FromDefaultMicrophoneInput());

                Logger.INFO("MAIN UI THREAD: " + Thread.CurrentThread.ManagedThreadId.ToString());

                Network.getAuthToken("hiker@rct.ai", "rctRocks2021!", OnSSOTokenUpdate);
            }
            catch (Exception ex)
            {
                Logger.ERROR(ex);
            }
        }

        void OnSSOTokenUpdate(string response)
        {
            ConfigManager.getInstance.setSSOToken(response);
            this.dispatcher.BeginInvoke(new Action(() =>
            {
                //GTA.UI.Notification.Show("sso-token " + response);
                Network.getAccessKeyToken(response, OnAccessKeyUpdate);
            }));
        }

        void OnAccessKeyUpdate(Tuple<string, string> keyandtoken)
        {
            ConfigManager.getInstance.setAccessKey(keyandtoken.Item1);
            ConfigManager.getInstance.setAccessToken(keyandtoken.Item2);
            this.dispatcher.BeginInvoke(new Action(() =>
            {
                GTA.UI.Notification.Show("~h~Socrates ~h~initialized ~h~successfully.");
            }));
        }

        void OnInteractRespond(string respondMsg)
        {
            SynthesizeAudioAsync(respondMsg);
        }

        private void OnAbort(object sender, EventArgs e)
        {
            Logger.INFO("OnAbort");
            foreach (SoundPlayer voice in this.externalSounds.Values)
            {
                if (voice != null && !voice.IsPlaying())
                {
                    voice.Dispose();
                }
            }
        }

        private void OnTick(Object sender, EventArgs e)
        {
            try
            {
                Vector3 referencePosition = Game.Player.Character.Position;

                Ped closestPed = null;
                String pedName = "Ped";
                float closestDistance = float.MaxValue;

                foreach (Ped ped in pedList)
                {
                    if (!ped.IsAlive)
                    {
                        continue;
                    }

                    if (ped.IsAmbientSpeechPlaying)
                    {
                        StopCurrentPlayingSpeech(ped);
                    }

                    float distance = Vector3.Distance(referencePosition, ped.Position);
                    float v0 = Utils.CalculateDecayedVolume(distance, initialVolume: 0.8f);
                    try
                    {
                        if (this.externalSounds.ContainsKey(ped.GetHashCode()))
                        {
                            SoundPlayer g0  = this.externalSounds[ped.GetHashCode()];
                            if (!g0.IsPlaying())
                            {
                                ped.Task.ClearAnimation("mp_facial", "mic_chatter");
                            }
                            else
                            {
                                bool flag = Function.Call<bool>(Hash.IS_ENTITY_PLAYING_ANIM, ped.GetHashCode(), "mp_facial", "mic_chatter", 3);
                                if (!flag)
                                {
                                    ped.Task.PlayAnimation("mp_facial", "mic_chatter", 2f, -1, AnimationFlags.None);
                                }

                                g0.SetVolume(v0);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.ERROR(ex);
                    }

                    // If the distance is smaller than the current closest distance, update the closestPed and closestDistance variables
                    if (distance < closestDistance)
                    {
                        closestPed = ped;
                        closestDistance = distance;

                        try
                        {
                            pedName = this.pedNameDictionary[ped.GetHashCode()];
                        }
                        catch (Exception ex)
                        {
                            Logger.ERROR(ex);
                        }
                    }
                }

                if (closestDistance < 3 && closestPed != null && this.isProcessingMessage == false)
                {
                    closestPed.Task.LookAt(Game.Player.Character);
                    this.currentTargetPed = closestPed;
                    GTA.UI.Screen.ShowHelpText("Press T to talk \nto ~p~Abigail", 2000);

                    //GTA.UI.Screen.ShowHelpText("Press T to talk \nto " + pedName, 2000);
                } 
            }
            catch (Exception ex)
            {
                Logger.ERROR(ex);
            }
        }

        private void OnKeyUp(Object sender, KeyEventArgs e) { }

        private void OnKeyDown(Object sender, KeyEventArgs e)
        {
            try
            {
                if (this.keyEventDispatcher == null)
                {
                    this.keyEventDispatcher = Dispatcher.CurrentDispatcher;
                }

                if (e.KeyCode == Keys.F5)
                {
                    CreateIntelligentPed();
                }
                else if (e.KeyCode == Keys.U)
                {
                }
                else if (e.KeyCode == Keys.T)
                {

                    if (isMicrophoneListening)
                    {
                        return;
                    }

                    if (!ConfigManager.getInstance.isAccessTokenAndKeySetup())
                    {
                        GTA.UI.Notification.Show("Socrates not initialized, need authorization.");
                        return;
                    }

                    isMicrophoneListening = true;
                    PlaySound();
                    this.currentTargetPed.Task.ChatTo(Game.Player.Character);
                    Game.Player.Character.Task.ChatTo(this.currentTargetPed);

                    microPhoneNotificationHandle = GTA.UI.Notification.Show("Microphone starts listening");
                    ListenToMicrophone();
                }
            }
            catch (Exception ex)
            {
                Logger.ERROR(ex);
            }
        }

        private async void ListenToMicrophone()
        {
            var speechRecognitionResult = await this.speechRecognizer.RecognizeOnceAsync();
            this.isMicrophoneListening = false;
            switch (speechRecognitionResult.Reason)
            {
                case ResultReason.RecognizedSpeech:
                    Logger.INFO(String.Format("RECOGNIZED: Text={0}", speechRecognitionResult.Text));

                    _ = dispatcher.BeginInvoke(new Action(() =>
                      {
                          UI.Notification.Hide(microPhoneNotificationHandle);
                          GTA.UI.Screen.ShowSubtitle(speechRecognitionResult.Text, 2000);
                      }));

                    Network.interactWithNode(speechRecognitionResult.Text, Utils.TEST_NODE_ID, OnInteractRespond);
                    break;
                case ResultReason.NoMatch:
                    Logger.INFO(String.Format("NOMATCH: Speech could not be recognized."));
                    break;
                case ResultReason.Canceled:
                    var cancellation = CancellationDetails.FromResult(speechRecognitionResult);
                    Logger.INFO(String.Format("CANCELED: Reason={0}", cancellation.Reason));

                    if (cancellation.Reason == CancellationReason.Error)
                    {
                        Logger.INFO(String.Format("CANCELED: ErrorDetails={0}", cancellation.ErrorDetails));
                    }
                    break;
            }
        }

        private async void SynthesizeAudioAsync(string speechText)
        {
            try
            {
                Logger.INFO("1 " + Thread.CurrentThread.ManagedThreadId.ToString());

                this.isProcessingMessage = true;

                var speechConfig0 = SpeechConfig.FromSubscription(speechKey, speechRegion);
                speechConfig0.SpeechSynthesisLanguage = "en-US";
                speechConfig0.SpeechSynthesisVoiceName = "en-US-JennyNeural";

                var audioConfig0 = AudioConfig.FromWavFileOutput(this.audioFileOutputPath);
                var synthesizer0 = new SpeechSynthesizer(speechConfig0, audioConfig0);

                synthesizer0.SynthesisStarted += (s, e) =>
                {
                    Logger.INFO("Synthesis started.");
                };

                synthesizer0.Synthesizing += (s, e) =>
                {
                    Logger.INFO($"Synthesizing, received an audio chunk of {e.Result.AudioData.Length} bytes.");
                };

                synthesizer0.WordBoundary += (s, e) =>
                {
                    Logger.INFO($"Word \"{e.Text}\" | ");
                    Logger.INFO($"Text offset {e.TextOffset} | ");
                    // Unit of AudioOffset is tick (1 tick = 100 nanoseconds).
                    Logger.INFO($"Audio offset {(e.AudioOffset + 5000) / 10000}ms");
                };

                synthesizer0.SynthesisCompleted += (s, e) =>
                {
                    Logger.INFO($"Completed \"{e.Result.Reason}\" | ");
                    synthesizer0.Dispose();
                    if (currentTargetPed != null)
                    {
                        ProcessMessage(speechText, e.Result.AudioDuration.TotalMilliseconds);
                    }
                };

                synthesizer0.SynthesisCanceled += (s, e) =>
                {
                    Logger.INFO($"Cancels \"{e.Result.Reason}\" | ");
                };

                var result = await synthesizer0.SpeakTextAsync(speechText);
            }
            catch (Exception ex)
            {
                Logger.ERROR(ex);
            }
        }

        private void CreateIntelligentPed()
        {
            Logger.INFO("createIntelligentPed() " + Thread.CurrentThread.ManagedThreadId.ToString());

            Ped npc = World.CreatePed(Utils.randomPedHash(), Game.Player.Character.GetOffsetPosition(new Vector3(0, 5, 0)));
            npc.Voice = null;
            npc.IsPainAudioEnabled = false;
            this.pedNameDictionary.Add(npc.GetHashCode(), Utils.randomName());
            this.pedList.Add(npc);

            SoundPlayer g0 = new SoundPlayer();
            this.externalSounds.Add(npc.GetHashCode(), g0);

            npc.Task.GoStraightTo(Game.Player.Character.Position);
        }

        private void ProcessMessage(string text = "", double duration = 3000)
        {
            try
            {
                _ = dispatcher.BeginInvoke(new Action(() =>
                  {
                      this.isProcessingMessage = false;
                      GTA.UI.Screen.ShowSubtitle(text, (int)duration);
                  }), DispatcherPriority.Render);

                if (this.currentTargetPed != null)
                {
                    SoundPlayer g0 = this.externalSounds[this.currentTargetPed.GetHashCode()];
                    g0.StartPlay(this.audioFileOutputPath);
                }
                Logger.INFO("10 " + Thread.CurrentThread.ManagedThreadId.ToString());
            }
            catch (Exception ex)
            {
                Logger.ERROR(ex);
            }
        }

        private bool IsSpeaking_Ambient(Ped ped)
        {
            return ped.IsAmbientSpeechPlaying;
        }

        private bool IsSpeaking_Any(Ped ped)
        {

            return ped.IsAnySpeechPlaying;
        }

        private bool IsSpeaking_NonAmbient(Ped ped)
        {
            return IsSpeaking_Any(ped) && !IsSpeaking_Ambient(ped);
        }


        private void PlaySound()
        {
            Function.Call(Hash.PLAY_SOUND, -1, "SELECT", "HUD_FRONTEND_DEFAULT_SOUNDSET", 0, 0, 1);
        }

        private void StopAmbientSpeech(Ped ped)
        {
            Function.Call((Hash)(13312289524232936207L), (InputArgument[])(object)new InputArgument[1] { new InputArgument(ped) });
        }

        private void StopCurrentPlayingSpeech(Ped ped)
        {
            Function.Call((Hash)(8823625181532992711), (InputArgument[])(object)new InputArgument[1] { new InputArgument(ped) });
        }

        private void PlaySpeechAnim(Ped ped)
        {
            ped.Task.ClearAll();
            float heading = (float)(System.Math.Atan2(Game.Player.Character.Position.Y - ped.Position.Y, Game.Player.Character.Position.X - ped.Position.X) * 180 / System.Math.PI);
            ped.Heading = heading;

            ped.Task.PlayAnimation("mp_facial", "mic_chatter", 2f, -1, AnimationFlags.Loop);

            Function.Call(Hash.TASK_PLAY_ANIM, ped, "mp_facial", "mic_chatter", 8.0F, 8.0F, -1, 1, 0.0f, 0, 0);

        }

        private void StopSpeechAnim(Ped ped)
        {
            ped.Task.ClearAnimation("mp_facial", "mic_chatter");
        }

    }
}
