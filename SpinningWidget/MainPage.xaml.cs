using System;
using Windows.Media.SpeechRecognition;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace SpinningWidget
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        const string SpinStoryboardName = "SpinStoryboard";
        const string AccelerateSpeechCommand = "Accelerate";
        const string SlowDownSpeechCommand = "Slow down";
        const string ResetSpeechCommand = "Reset";
        const string StopSpeechCommand = "Stop";
        const string StartSpeechCommand = "Start";

        private readonly string[] _spinCommands =
            {AccelerateSpeechCommand, SlowDownSpeechCommand,
            ResetSpeechCommand, StopSpeechCommand, StartSpeechCommand};
        private SpeechRecognizer _speechRecognizer;
        private CoreDispatcher _speechRecognizerDispatcher;

        private float _spinSpeedRatio = 1.0f;

        public MainPage()
        {
            this.InitializeComponent();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            _speechRecognizer = new SpeechRecognizer();
            var speechConstraint = 
                new SpeechRecognitionListConstraint(_spinCommands, "spinCommands");
            _speechRecognizer.Constraints.Add(speechConstraint);
            await _speechRecognizer.CompileConstraintsAsync();
            Storyboard sb = (Storyboard)MainImage.Resources[SpinStoryboardName];
            sb.Begin();

            _speechRecognizer.Timeouts.BabbleTimeout = TimeSpan.FromSeconds(2.0);

            _speechRecognizerDispatcher = CoreWindow.GetForCurrentThread().Dispatcher;
            _speechRecognizer.ContinuousRecognitionSession.ResultGenerated +=
                ContinuousRecognitionSession_ResultGenerated;
            if (_speechRecognizer.State == SpeechRecognizerState.Idle)
            {
                await _speechRecognizer.ContinuousRecognitionSession.StartAsync();
            }
        }

        private async void ContinuousRecognitionSession_ResultGenerated( 
            SpeechContinuousRecognitionSession sender, 
            SpeechContinuousRecognitionResultGeneratedEventArgs args)
        {

            if (args.Result.Confidence == SpeechRecognitionConfidence.Medium ||
              args.Result.Confidence == SpeechRecognitionConfidence.High)
            {
                await _speechRecognizerDispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    var result = args.Result.Text;
                    switch (result)
                    {
                        case StartSpeechCommand:
                            StartSpin();
                            break;
                        case StopSpeechCommand:
                            StopSpin();
                            break;
                        case ResetSpeechCommand:
                            _spinSpeedRatio = 1.0f;
                            UpdateSpinSpeedRatio(_spinSpeedRatio);
                            break;
                        case AccelerateSpeechCommand:
                            _spinSpeedRatio *= 2.0f;
                            UpdateSpinSpeedRatio(_spinSpeedRatio);
                            break;
                        case SlowDownSpeechCommand:
                            _spinSpeedRatio /= 2.0f;
                            UpdateSpinSpeedRatio(_spinSpeedRatio);
                            break;
                        default:
                            break;
                    }
                });
            }
        }

        private void UpdateSpinSpeedRatio(float ratio)
        {
            Storyboard sb = (Storyboard)MainImage.Resources[SpinStoryboardName];
            sb.SpeedRatio = ratio;
        }

        private void StartSpin()
        {
            Storyboard sb = (Storyboard)MainImage.Resources[SpinStoryboardName];
            sb.Begin();
        }

        private void StopSpin()
        {
            Storyboard sb = (Storyboard)MainImage.Resources[SpinStoryboardName];
            sb.Stop();
        }

        private async void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            if (_speechRecognizer.State == SpeechRecognizerState.Idle)
            {
                await _speechRecognizer.ContinuousRecognitionSession.StartAsync();
            }
            await _speechRecognizer.StopRecognitionAsync();
        }
    }
}
