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
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Input.Inking;
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
    /// This page demonstrates the usage of the InkPresenter APIs. It shows the following functionality:
    /// - Load/Save ink files
    /// - Usage of drawing attributes
    /// - Input type switching to enable/disable touch
    /// - Pen tip transform, highlighter and different pen colors and sizes
    /// </summary>
    public sealed partial class Scenario1 : Page
    {
        private MainPage rootPage;
        //brush thickness definition
        const int minPenSize = 2;
        const int penSizeIncrement = 2;
        int penSize;
        int selectThickness = 1;
        string PenType = "Ballpoint";

        public Scenario1()
        {
            this.InitializeComponent();
            //read language related resource file .strings/en or zh-cn/resources.resw
            Run run1 = new Run();
            run1.Text = ResourceManagerHelper.ReadValue("Description1_p1");
            Run run2 = new Run();
            run2.Text = ResourceManagerHelper.ReadValue("Description1_p2");
            this.textDes.Inlines.Add(run1);
            this.textDes.Inlines.Add(new LineBreak());
            this.textDes.Inlines.Add(run2);

            penSize = minPenSize + penSizeIncrement * selectThickness;
            // Initialize drawing attributes. These are used in inking mode.
            InkDrawingAttributes drawingAttributes = new InkDrawingAttributes();
            drawingAttributes.Color = Windows.UI.Colors.Red;
            drawingAttributes.Size = new Size(penSize, penSize);
            drawingAttributes.IgnorePressure = false;
            drawingAttributes.FitToCurve = true;
            //set drawingAttributes
            inkCanvas.InkPresenter.UpdateDefaultDrawingAttributes(drawingAttributes);
            inkCanvas.InkPresenter.InputDeviceTypes = Windows.UI.Core.CoreInputDeviceTypes.Mouse | Windows.UI.Core.CoreInputDeviceTypes.Pen | Windows.UI.Core.CoreInputDeviceTypes.Touch;

            this.SizeChanged += Scenario2_SizeChanged;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            //set inkCanvas size
            rootPage = MainPage.Current;
            this.SetCanvasSize();
        }

        private void Scenario2_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            SetCanvasSize();
        }

        private void SetCanvasSize()
        {
            //set inkCanvas size
            outputGrid.Height = Window.Current.Bounds.Height / 2;
            inkCanvas.Height = Window.Current.Bounds.Height / 2;
        }

        void OnPenColorChanged(object sender, RoutedEventArgs e)
        {
            this.FlyoutColor.Hide();
            if (inkCanvas != null)
            {
                InkDrawingAttributes drawingAttributes = inkCanvas.InkPresenter.CopyDefaultDrawingAttributes();

                // Use button's background to set new pen's color
                var borderSender = sender as Border;
                var brush = borderSender.Background as Windows.UI.Xaml.Media.SolidColorBrush;

                drawingAttributes.Color = brush.Color;
                inkCanvas.InkPresenter.UpdateDefaultDrawingAttributes(drawingAttributes);
            }
        }

        private void AppBarBrushType_Tapped(object sender, TappedRoutedEventArgs e)
        {
            this.FlyoutPenType.ShowAt((FrameworkElement)sender);
        }

        private void AppBarThickness_Tapped(object sender, TappedRoutedEventArgs e)
        {
            this.FlyoutPenThickness.ShowAt((FrameworkElement)sender);
        }

        private void AppBarColor_Tapped(object sender, TappedRoutedEventArgs e)
        {
            this.FlyoutColor.ShowAt((FrameworkElement)sender);
        }

        private void AppBarTouch_Tapped(object sender, TappedRoutedEventArgs e)
        {
            this.FlyoutTouch.ShowAt((FrameworkElement)sender);
        }

        private void TextBlockThickness_Tapped(object sender, TappedRoutedEventArgs e)
        {
            this.FlyoutPenThickness.Hide();
            if (inkCanvas != null)
            {
                this.selectThickness = int.Parse(((TextBlock)sender).Tag.ToString());
                InkDrawingAttributes drawingAttributes = inkCanvas.InkPresenter.CopyDefaultDrawingAttributes();
                penSize = minPenSize + penSizeIncrement * this.selectThickness;
                string value = this.PenType;
                if (value == "Highlighter" || value == "Calligraphy")
                {
                    // Make the pen tip rectangular for highlighter and calligraphy pen
                    drawingAttributes.Size = new Size(penSize, penSize * 2);
                }
                else
                {
                    drawingAttributes.Size = new Size(penSize, penSize);
                }
                inkCanvas.InkPresenter.UpdateDefaultDrawingAttributes(drawingAttributes);
            }
        }

        private void PenType_Tapped(object sender, TappedRoutedEventArgs e)
        {
            this.FlyoutPenType.Hide();
            if (inkCanvas != null)
            {
                var textBlock = (TextBlock)sender;
                this.PenType = textBlock.Tag.ToString();
                InkDrawingAttributes drawingAttributes = inkCanvas.InkPresenter.CopyDefaultDrawingAttributes();
                string value = this.PenType;

                if (value == "Ballpoint")
                {
                    // Make the pen rectangular for Ballpoint
                    drawingAttributes.Size = new Size(penSize, penSize);
                    drawingAttributes.PenTip = PenTipShape.Circle;
                    drawingAttributes.DrawAsHighlighter = false;
                    drawingAttributes.PenTipTransform = System.Numerics.Matrix3x2.Identity;
                }
                else if (value == "Highlighter")
                {
                    // Make the pen rectangular for highlighter
                    drawingAttributes.Size = new Size(penSize, penSize * 2);
                    drawingAttributes.PenTip = PenTipShape.Rectangle;
                    drawingAttributes.DrawAsHighlighter = true;
                    drawingAttributes.PenTipTransform = System.Numerics.Matrix3x2.Identity;
                }
                if (value == "Calligraphy")
                {
                    // Make the pen rectangular for Calligraphy
                    drawingAttributes.Size = new Size(penSize, penSize * 2);
                    drawingAttributes.PenTip = PenTipShape.Rectangle;
                    drawingAttributes.DrawAsHighlighter = false;

                    // Set a 45 degree rotation on the pen tip
                    double radians = 45.0 * Math.PI / 180;
                    drawingAttributes.PenTipTransform = System.Numerics.Matrix3x2.CreateRotation((float)radians);
                }
                inkCanvas.InkPresenter.UpdateDefaultDrawingAttributes(drawingAttributes);
            }
        }

        private void btnClear_Tapped(object sender, TappedRoutedEventArgs e)
        {
            inkCanvas.InkPresenter.StrokeContainer.Clear();
        }

        async void OnSaveAsync(object sender, RoutedEventArgs e)
        {
            // We don't want to save an empty file
            if (inkCanvas.InkPresenter.StrokeContainer.GetStrokes().Count > 0)
            {
                var savePicker = new Windows.Storage.Pickers.FileSavePicker();
                savePicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary;
                savePicker.FileTypeChoices.Add("Gif with embedded ISF", new System.Collections.Generic.List<string> { ".gif" });

                Windows.Storage.StorageFile file = await savePicker.PickSaveFileAsync();
                if (null != file)
                {
                    try
                    {
                        using (IRandomAccessStream stream = await file.OpenAsync(FileAccessMode.ReadWrite))
                        {
                            await inkCanvas.InkPresenter.StrokeContainer.SaveAsync(stream);
                        }
                        rootPage.ShowMessage(inkCanvas.InkPresenter.StrokeContainer.GetStrokes().Count + " stroke(s) saved!");
                    }
                    catch (Exception ex)
                    {
                        rootPage.ShowMessage(ex.Message);
                    }
                }
            }
            else
            {
                rootPage.ShowMessage("There is no ink to save.");
            }
        }

        async void OnLoadAsync(object sender, RoutedEventArgs e)
        {
            //load image
            var openPicker = new Windows.Storage.Pickers.FileOpenPicker();
            openPicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary;
            openPicker.FileTypeFilter.Add(".gif");
            openPicker.FileTypeFilter.Add(".isf");
            Windows.Storage.StorageFile file = await openPicker.PickSingleFileAsync();
            if (null != file)
            {
                using (var stream = await file.OpenSequentialReadAsync())
                {
                    try
                    {
                        await inkCanvas.InkPresenter.StrokeContainer.LoadAsync(stream);
                    }
                    catch (Exception ex)
                    {
                        rootPage.ShowMessage(ex.Message);
                    }
                }
            }
        }

        private void TouchInking_Tap(object sender, RoutedEventArgs e)
        {
            this.FlyoutTouch.Hide();
            inkCanvas.InkPresenter.InputDeviceTypes = Windows.UI.Core.CoreInputDeviceTypes.Mouse | Windows.UI.Core.CoreInputDeviceTypes.Pen | Windows.UI.Core.CoreInputDeviceTypes.Touch;
        }

        private void UnTouchInking_Tap(object sender, RoutedEventArgs e)
        {
            this.FlyoutTouch.Hide();
            inkCanvas.InkPresenter.InputDeviceTypes = Windows.UI.Core.CoreInputDeviceTypes.Mouse | Windows.UI.Core.CoreInputDeviceTypes.Pen;
        }

        private void ErasingMode_Tap(object sender, RoutedEventArgs e)
        {
            this.FlyoutTouch.Hide();
            inkCanvas.InkPresenter.InputProcessingConfiguration.Mode = InkInputProcessingMode.Erasing;
        }

        private void UnErasingMode_Tap(object sender, RoutedEventArgs e)
        {
            this.FlyoutTouch.Hide();
            inkCanvas.InkPresenter.InputProcessingConfiguration.Mode = InkInputProcessingMode.Inking;
        }
    }
}
