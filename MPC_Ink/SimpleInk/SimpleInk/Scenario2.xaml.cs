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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Globalization;
using Windows.UI.Input.Inking;
using Windows.UI.Text.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;


namespace SimpleInk
{
    /// <summary>
    /// This page shows the code to do ink recognition
    /// </summary>
    public sealed partial class Scenario2 : Page
    {
        //variable definition
        private MainPage rootPage;
        InkRecognizerContainer inkRecognizerContainer = null;
        private IReadOnlyList<InkRecognizer> recoView = null;
        private Language previousInputLanguage = null;
        private CoreTextServicesManager textServiceManager = null;

        public Scenario2()
        {
            this.InitializeComponent();
            //read language related resource file .strings/en or zh-cn/resources.resw
            Run run1 = new Run();
            run1.Text = ResourceManagerHelper.ReadValue("Description2_p1");
            this.textDes.Inlines.Add(run1);
            this.textDes.Inlines.Add(new LineBreak());

            // Initialize drawing attributes. These are used in inking mode.
            InkDrawingAttributes drawingAttributes = new InkDrawingAttributes();
            drawingAttributes.Color = Windows.UI.Colors.Red;
            double penSize = 4;
            drawingAttributes.Size = new Windows.Foundation.Size(penSize, penSize);
            drawingAttributes.IgnorePressure = false;
            drawingAttributes.FitToCurve = true;

            // Show the available recognizers
            inkRecognizerContainer = new InkRecognizerContainer();
            recoView = inkRecognizerContainer.GetRecognizers();
            if (recoView.Count > 0)
            {
                foreach (InkRecognizer recognizer in recoView)
                {
                    RecoName.Items.Add(recognizer.Name);
                }
            }
            else
            {
                RecoName.IsEnabled = false;
                RecoName.Items.Add("No Recognizer Available");
            }
            RecoName.SelectedIndex = 0;

            // Set the text services so we can query when language changes
            textServiceManager = CoreTextServicesManager.GetForCurrentView();
            textServiceManager.InputLanguageChanged += TextServiceManager_InputLanguageChanged;

            SetDefaultRecognizerByCurrentInputMethodLanguageTag();

            // Initialize the InkCanvas
            inkCanvas.InkPresenter.UpdateDefaultDrawingAttributes(drawingAttributes);
            inkCanvas.InkPresenter.InputDeviceTypes = Windows.UI.Core.CoreInputDeviceTypes.Mouse | Windows.UI.Core.CoreInputDeviceTypes.Pen | Windows.UI.Core.CoreInputDeviceTypes.Touch;

            this.SizeChanged += Scenario2_SizeChanged;
        }

        private void Scenario2_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            SetCanvasSize();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            rootPage = MainPage.Current;
            SetCanvasSize();
        }

        private void SetCanvasSize()
        {
            //set inkCanvas size
            Output.Height = Window.Current.Bounds.Height / 3;
            inkCanvas.Height = Window.Current.Bounds.Height / 3;
        }

        void OnRecognizerChanged(object sender, RoutedEventArgs e)
        {
            string selectedValue = (string)RecoName.SelectedValue;
            SetRecognizerByName(selectedValue);
        }

        async void OnRecognizeAsync(object sender, RoutedEventArgs e)
        {
            //Recognize Text
            IReadOnlyList<InkStroke> currentStrokes = inkCanvas.InkPresenter.StrokeContainer.GetStrokes();
            if (currentStrokes.Count > 0)
            {
                RecognizeBtn.IsEnabled = false;
                ClearBtn.IsEnabled = false;
                RecoName.IsEnabled = false;

                var recognitionResults = await inkRecognizerContainer.RecognizeAsync(inkCanvas.InkPresenter.StrokeContainer, InkRecognitionTarget.All);

                if (recognitionResults.Count > 0)
                {
                    // Display recognition result
                    string str = "";
                    foreach (var r in recognitionResults)
                    {
                        str += " " + r.GetTextCandidates()[0];
                    }
                    this.textShow.Text = str;
                }
                else
                {
                    rootPage.ShowMessage("No text recognized.");
                }

                RecognizeBtn.IsEnabled = true;
                ClearBtn.IsEnabled = true;
                RecoName.IsEnabled = true;
            }
            else
            {
                rootPage.ShowMessage("Must first write something.");
            }
        }

        void OnClear(object sender, RoutedEventArgs e)
        {
            //clear ink
            inkCanvas.InkPresenter.StrokeContainer.Clear();
            this.textShow.Text = "";
        }

        bool SetRecognizerByName(string recognizerName)
        {
            bool recognizerFound = false;
            //Find name and SetDefaultRecognoizer.
            foreach (InkRecognizer reco in recoView)
            {
                if (recognizerName == reco.Name)
                {
                    inkRecognizerContainer.SetDefaultRecognizer(reco);
                    recognizerFound = true;
                    break;
                }
            }

            if (!recognizerFound && rootPage != null)
            {
                rootPage.ShowMessage("Could not find target recognizer.");
            }

            return recognizerFound;
        }

        private void TextServiceManager_InputLanguageChanged(CoreTextServicesManager sender, object args)
        {
            SetDefaultRecognizerByCurrentInputMethodLanguageTag();
        }

        private void SetDefaultRecognizerByCurrentInputMethodLanguageTag()
        {
            // Query recognizer name based on current input method language tag (bcp47 tag)
            Language currentInputLanguage = textServiceManager.InputLanguage;

            if (currentInputLanguage != previousInputLanguage)
            {
                // try query with the full BCP47 name
                string recognizerName = RecognizerHelper.LanguageTagToRecognizerName(currentInputLanguage.LanguageTag);

                if (recognizerName != string.Empty)
                {
                    for (int index = 0; index < recoView.Count; index++)
                    {
                        if (recoView[index].Name == recognizerName)
                        {
                            inkRecognizerContainer.SetDefaultRecognizer(recoView[index]);
                            RecoName.SelectedIndex = index;
                            previousInputLanguage = currentInputLanguage;
                            break;
                        }
                    }
                }
            }
        }

    }
}
