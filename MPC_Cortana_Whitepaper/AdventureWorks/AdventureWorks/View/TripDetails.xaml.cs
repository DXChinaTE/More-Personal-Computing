//*********************************************************
//
// Copyright (c) Microsoft. All rights reserved.
// This code is licensed under the MIT License (MIT).
// THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
// ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
// IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
// PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
//
//*********************************************************

using AdventureWorks.Common;
using AdventureWorks.Model;
using AdventureWorks.ViewModel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources.Core;
using Windows.Globalization;
using Windows.Media.SpeechRecognition;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace AdventureWorks.View
{
    /// <summary>
    /// Code Behind for a list of trips, including a Save and Delete button. Associated with the 
    /// TripViewModel for most behaviors and properties.
    /// </summary>
    public sealed partial class TripDetails : Page
    {
        private bool isNote = false;
        private NavigationHelper navigationHelper;
        private TripViewModel defaultViewModel;
        // The speech recognizer used throughout this sample.
        private SpeechRecognizer speechRecognizer;
        // The speech recognizer used throughout this sample.
        private SpeechRecognizer speechRecognizerNote;
        /// <summary>
        /// the HResult 0x8004503a typically represents the case where a recognizer for a particular language cannot
        /// be found. This may occur if the language is installed, but the speech pack for that language is not.
        /// See Settings -> Time & Language -> Region & Language -> *Language* -> Options -> Speech Language Options.
        /// </summary>
        private static uint HResultRecognizerNotFound = 0x8004503a;

        private ResourceContext speechContext;
        private ResourceMap speechResourceMap;
        private Language speechLanguage;
        private string originalNote;
        private string hypothesis;
        /// <summary>
        /// The ViewModel that provides behaviors and properties associated with displaying a Trip's
        /// details.
        /// </summary>
        public TripViewModel DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        /// <summary>
        /// NavigationHelper is used on each page to aid in navigation and 
        /// process lifetime management
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }


        public TripDetails()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += navigationHelper_LoadState;
            this.navigationHelper.SaveState += navigationHelper_SaveState;
        }

        /// <summary>
        /// Populates the page with content passed during navigation. Any saved state is also
        /// provided when recreating a page from a prior session.
        /// Resets the state of the view model by passing in the parameters provided by the
        /// caller.
        /// </summary>
        /// <param name="sender">
        /// The source of the event; typically <see cref="Common.NavigationHelper"/>
        /// </param>
        /// <param name="e">Event data that provides both the navigation parameter passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested and
        /// a dictionary of state preserved by this page during an earlier
        /// session. The state will be null the first time a page is visited.</param>
        private void navigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            defaultViewModel = (TripViewModel)this.DataContext;
           
            if (e.NavigationParameter is Trip)
            {
                // Activated via selecting a trip in the main page's list of trips.
                DefaultViewModel.ShowTrip((Trip)e.NavigationParameter);
            }
            else if (e.NavigationParameter is TripVoiceCommand)
            {
                // look up destination, set trip.
                TripVoiceCommand voiceCommand = (TripVoiceCommand)e.NavigationParameter;
                DefaultViewModel.LoadTripFromStore(voiceCommand.destination);

                // artificially populate the page backstack so we have something to
                // go back to to get to the main page.
                PageStackEntry backEntry = new PageStackEntry(typeof(View.TripListView), null, null);
                this.Frame.BackStack.Add(backEntry);
            }
            else if (e.NavigationParameter is string)
            {
                // We've been URI Activated, possibly by a user clicking on a tile in a Cortana session,
                // we should see an argument like destination=<Location>. 
                // This should handle finding all of the destinations that match, but currently it only
                // finds the first one.
                string arguments = e.NavigationParameter as string;
                if (arguments != null)
                {
                    string[] args = arguments.Split('=');
                    if (args.Length == 2 && args[0].ToLowerInvariant() == "destination")
                    {
                        DefaultViewModel.LoadTripFromStore(args[1]);

                        // artificially populate the page backstack so we have something to
                        // go back to to get to the main page.
                        PageStackEntry backEntry = new PageStackEntry(typeof(View.TripListView), null, null);
                        this.Frame.BackStack.Add(backEntry);
                    }
                }
            }
            else
            {
                DefaultViewModel.NewTrip();
            }
            this.originalNote = defaultViewModel.Trip.Notes;
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="Common.SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="sender">The source of the event; typically <see cref="Common.NavigationHelper"/></param>
        /// <param name="e">Event data that provides an empty dictionary to be populated with
        /// serializable state.</param>
        private void navigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
        }

        #region NavigationHelper registration

        /// The methods provided in this section are simply used to allow
        /// NavigationHelper to respond to the page's navigation methods.
        /// 
        /// Page specific logic should be placed in event handlers for the  
        /// <see cref="Common.NavigationHelper.LoadState"/>
        /// and <see cref="Common.NavigationHelper.SaveState"/>.
        /// The navigation parameter is available in the LoadState method 
        /// in addition to page state preserved during an earlier session.

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedTo(e);

            // Prompt the user for permission to access the microphone. This request will only happen
            // once, it will not re-prompt if the user rejects the permission.
            bool permissionGained = await AudioCapturePermissions.RequestMicrophonePermission();

            speechLanguage = SpeechRecognizer.SystemSpeechLanguage;
            string langTag = speechLanguage.LanguageTag;
            // Initialize resource map to retrieve localized speech strings.
            speechContext = ResourceContext.GetForCurrentView();
            IReadOnlyList<Language> supportedLanguages = SpeechRecognizer.SupportedGrammarLanguages;
            if (supportedLanguages.Count > 1)
            {
                if (Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride == "zh-Hans-CN")
                {
                    speechContext.Languages = new string[] { "zh-Hans-CN" };
                    speechLanguage = new Windows.Globalization.Language("zh-Hans-CN");
                }
                else
                {
                    speechContext.Languages = new string[] { "en-US" };
                    speechLanguage = new Windows.Globalization.Language("en-US");
                }
            }
            else
            {
                speechContext.Languages = new string[] { langTag };
            }
            speechResourceMap = ResourceManager.Current.MainResourceMap.GetSubtree("LocalizationSpeechResources");
            //Initia Command
            await InitializeRecognizer(speechLanguage);
            //Initia RecognizerNote
            await InitializeRecognizerNote(speechLanguage);
            if (speechRecognizer.State == SpeechRecognizerState.Idle && permissionGained)
            {
                try
                {
                    await speechRecognizer.ContinuousRecognitionSession.StartAsync();
                }
                catch (Exception ex)
                {
                    var messageDialog = new Windows.UI.Popups.MessageDialog(ex.Message, "Exception");
                    await messageDialog.ShowAsync();
                }
            }
        }

        protected async override void OnNavigatedFrom(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedFrom(e);
            if (this.speechRecognizer != null)
            {
                await this.speechRecognizer.ContinuousRecognitionSession.CancelAsync();
                speechRecognizer.ContinuousRecognitionSession.ResultGenerated -= ContinuousRecognitionSession_ResultGenerated;
                this.speechRecognizer.Dispose();
                this.speechRecognizer = null;
            }
            if (this.speechRecognizerNote != null && this.speechRecognizerNote.State != SpeechRecognizerState.Idle && this.isNote)
            {
                await this.speechRecognizerNote.ContinuousRecognitionSession.CancelAsync();
                speechRecognizerNote.HypothesisGenerated -= SpeechRecognizer_HypothesisGenerated;
                speechRecognizerNote.ContinuousRecognitionSession.Completed -= ContinuousRecognitionSession_Completed;
                this.speechRecognizerNote.Dispose();
                this.speechRecognizerNote = null;
            }
        }

        #endregion

        /// <summary>
        /// Handle footer link clicks.
        /// </summary>
        /// <param name="sender">The target uri</param>
        /// <param name="e">Ignored</param>
        async void Footer_Click(object sender, RoutedEventArgs e)
        {
            await Windows.System.Launcher.LaunchUriAsync(new Uri(((HyperlinkButton)sender).Tag.ToString()));
        }

        private void SettingLink_Click(object sender, RoutedEventArgs e)
        {
            App.NavigationService.Navigate(typeof(Setting));
        }

        /// <summary>
        /// Initialize Speech Recognizer and compile constraints.
        /// </summary>
        /// <param name="recognizerLanguage">Language to use for the speech recognizer</param>
        /// <returns>Awaitable task.</returns>
        private async Task InitializeRecognizer(Language recognizerLanguage)
        {
            if (speechRecognizer != null)
            {
                // cleanup prior to re-initializing this scenario.
                speechRecognizer.ContinuousRecognitionSession.ResultGenerated -= ContinuousRecognitionSession_ResultGenerated;

                this.speechRecognizer.Dispose();
                this.speechRecognizer = null;
            }

            try
            {
                this.speechRecognizer = new SpeechRecognizer(recognizerLanguage);
                // Build a command-list grammar. Commands should ideally be drawn from a resource file for localization, and 
                // be grouped into tags for alternate forms of the same command.
                speechRecognizer.Constraints.Add(
                    new SpeechRecognitionListConstraint(
                        new List<string>()
                        {
                        speechResourceMap.GetValue("ListGrammarTakeNote", speechContext).ValueAsString
                        }, "Note"));
                speechRecognizer.Constraints.Add(
                    new SpeechRecognitionListConstraint(
                        new List<string>()
                        {
                        speechResourceMap.GetValue("ListGrammarSaveTrip", speechContext).ValueAsString
                        }, "Trip"));
                // Update the help text in the UI to show localized examples
                string uiOptionsText = string.Format(listeningTip.Text,
                    speechResourceMap.GetValue("ListGrammarTakeNote", speechContext).ValueAsString,
                    speechResourceMap.GetValue("ListGrammarSaveTrip", speechContext).ValueAsString);
                listeningTip.Text = uiOptionsText;

                SpeechRecognitionCompilationResult result = await speechRecognizer.CompileConstraintsAsync();
                if (result.Status == SpeechRecognitionResultStatus.Success)
                {
                    // Handle continuous recognition events. Completed fires when various error states occur. ResultGenerated fires when
                    // some recognized phrases occur, or the garbage rule is hit.
                    speechRecognizer.ContinuousRecognitionSession.ResultGenerated += ContinuousRecognitionSession_ResultGenerated;
                }
            }
            catch (Exception ex)
            {
                if ((uint)ex.HResult == HResultRecognizerNotFound)
                {
                    var messageDialog = new Windows.UI.Popups.MessageDialog(ex.Message, "Speech Language pack for selected language not installed.");
                    await messageDialog.ShowAsync();
                }
                else
                {
                    var messageDialog = new Windows.UI.Popups.MessageDialog(ex.Message, "Exception");
                    await messageDialog.ShowAsync();
                }
            }

        }

        /// <summary>
        /// Initialize Speech Recognizer and compile constraints.
        /// </summary>
        /// <param name="recognizerLanguage">Language to use for the speech recognizer</param>
        /// <returns>Awaitable task.</returns>
        private async Task InitializeRecognizerNote(Language recognizerLanguage)
        {
            if (speechRecognizerNote != null)
            {
                // cleanup prior to re-initializing this scenario.
                speechRecognizerNote.HypothesisGenerated -= SpeechRecognizer_HypothesisGenerated;
                speechRecognizerNote.ContinuousRecognitionSession.Completed -= ContinuousRecognitionSession_Completed;
                this.speechRecognizerNote.Dispose();
                this.speechRecognizerNote = null;
            }

            try
            {
                this.speechRecognizerNote = new SpeechRecognizer(recognizerLanguage);
                // Provide feedback to the user about the state of the recognizer. This can be used to provide visual feedback in the form
                // of an audio indicator to help the user understand whether they're being heard.
                speechRecognizerNote.ContinuousRecognitionSession.Completed += ContinuousRecognitionSession_Completed;
                SpeechRecognitionCompilationResult result = await speechRecognizerNote.CompileConstraintsAsync();
                if (result.Status == SpeechRecognitionResultStatus.Success)
                {
                    // Handle continuous recognition events. Completed fires when various error states occur. ResultGenerated fires when
                    // some recognized phrases occur, or the garbage rule is hit.
                    speechRecognizerNote.HypothesisGenerated += SpeechRecognizer_HypothesisGenerated;
                }
            }
            catch (Exception ex)
            {
                if ((uint)ex.HResult == HResultRecognizerNotFound)
                {
                    var messageDialog = new Windows.UI.Popups.MessageDialog(ex.Message, "Speech Language pack for selected language not installed.");
                    await messageDialog.ShowAsync();
                }
                else
                {
                    var messageDialog = new Windows.UI.Popups.MessageDialog(ex.Message, "Exception");
                    await messageDialog.ShowAsync();
                }
            }

        }

        /// <summary>
        /// Handle events fired when error conditions occur, such as the microphone becoming unavailable, or if
        /// some transient issues occur.
        /// </summary>
        /// <param name="sender">The continuous recognition session</param>
        /// <param name="args">The state of the recognizer</param>
        private async void ContinuousRecognitionSession_Completed(SpeechContinuousRecognitionSession sender, SpeechContinuousRecognitionCompletedEventArgs args)
        {
            if (args.Status != SpeechRecognitionResultStatus.Success)
            {
                // If TimeoutExceeded occurs, the user has been silent for too long. We can use this to 
                // cancel recognition if the user in dictation mode and walks away from their device, etc.
                // In a global-command type scenario, this timeout won't apply automatically.
                // With dictation (no grammar in place) modes, the default timeout is 20 seconds.
                if (args.Status == SpeechRecognitionResultStatus.TimeoutExceeded)
                {
                    await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => {
                        this.defaultViewModel.Trip.Notes = this.originalNote;
                    });
                }
             
            }
        }

        /// <summary>
        /// While the user is speaking, update the textbox with the partial sentence of what's being said for user feedback.
        /// </summary>
        /// <param name="sender">The recognizer that has generated the hypothesis</param>
        /// <param name="args">The hypothesis formed</param>
        private void SpeechRecognizer_HypothesisGenerated(SpeechRecognizer sender, SpeechRecognitionHypothesisGeneratedEventArgs args)
        {
            if (this.isNote)
            {
                string saveCommand = speechResourceMap.GetValue("ListGrammarSaveTrip", speechContext).ValueAsString;
                this.hypothesis = args.Hypothesis.Text;
            }
        }

        /// <summary>
        /// Handle events fired when a result is generated. This may include a garbage rule that fires when general room noise
        /// or side-talk is captured (this will have a confidence of Rejected typically, but may occasionally match a rule with
        /// low confidence).
        /// </summary>
        /// <param name="sender">The Recognition session that generated this result</param>
        /// <param name="args">Details about the recognized speech</param>
        private async void ContinuousRecognitionSession_ResultGenerated(SpeechContinuousRecognitionSession sender, SpeechContinuousRecognitionResultGeneratedEventArgs args)
        {
            // The garbage rule will not have a tag associated with it, the other rules will return a string matching the tag provided
            // when generating the grammar.
            string tag = "unknown";
            if (args.Result.Constraint != null)
            {
                tag = args.Result.Constraint.Tag;
            }

            // Developers may decide to use per-phrase confidence levels in order to tune the behavior of their 
            // grammar based on testing.
            if (args.Result.Confidence == SpeechRecognitionConfidence.Medium ||
                args.Result.Confidence == SpeechRecognitionConfidence.High ||
                args.Result.Confidence == SpeechRecognitionConfidence.Low)
            {
                if (tag == "Note")
                {
                    this.isNote = true;
                    await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                    {
                        this.defaultViewModel.Trip.Notes = speechResourceMap.GetValue("NotesTip", speechContext).ValueAsString;
                        try
                        {
                            await speechRecognizerNote.ContinuousRecognitionSession.StartAsync();
                        }
                        catch (Exception ex)
                        {
                            var messageDialog = new Windows.UI.Popups.MessageDialog(ex.Message, "Exception");
                            await messageDialog.ShowAsync();
                        }
                    });

                }
                else if (tag == "Trip")
                {
                    this.isNote = false;
                    await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                   {
                       if (defaultViewModel.Trip.Notes == speechResourceMap.GetValue("NotesTip", speechContext).ValueAsString)
                           defaultViewModel.Trip.Notes = this.originalNote;
                       defaultViewModel.SaveTrip();
                   });
                }
            }
            else
            {
                if (this.isNote && !string.IsNullOrEmpty(this.hypothesis))
                {
                    await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        this.defaultViewModel.Trip.Notes = this.hypothesis;
                    });
                }
            }
        }




    }
}
