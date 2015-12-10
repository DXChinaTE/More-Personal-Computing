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
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.System.Profile;
using Windows.UI.Input.Inking;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;

namespace SimpleInk
{
    /// <summary>
    /// This page shows the code to do ink selection and cut/copy/paste
    /// </summary>
    public sealed partial class Scenario3 : Page
    {
        private Polyline lasso;
        private Rect boundingRect;

        private MainPage rootPage;

        public Scenario3()
        {
            this.InitializeComponent();
            //read language related resource file .strings/en or zh-cn/resources.resw
            Run run1 = new Run();
            run1.Text = ResourceManagerHelper.ReadValue("Description3_p1");
            Run run2 = new Run();
            run2.Text = ResourceManagerHelper.ReadValue("Description3_p2");
            this.textDes.Inlines.Add(run1);
            this.textDes.Inlines.Add(new LineBreak());
            this.textDes.Inlines.Add(run2);

            // Initialize the InkCanvas
            inkCanvas.InkPresenter.InputDeviceTypes = Windows.UI.Core.CoreInputDeviceTypes.Mouse | Windows.UI.Core.CoreInputDeviceTypes.Pen | Windows.UI.Core.CoreInputDeviceTypes.Touch;

            // By default, pen barrel button or right mouse button is processed for inking
            // Set the configuration to instead allow processing these input on the UI thread
            inkCanvas.InkPresenter.InputProcessingConfiguration.RightDragAction = InkInputRightDragAction.LeaveUnprocessed;

            inkCanvas.InkPresenter.InputProcessingConfiguration.Mode = InkInputProcessingMode.Inking;

            inkCanvas.InkPresenter.UnprocessedInput.PointerPressed += UnprocessedInput_PointerPressed;
            inkCanvas.InkPresenter.UnprocessedInput.PointerMoved += UnprocessedInput_PointerMoved;
            inkCanvas.InkPresenter.UnprocessedInput.PointerReleased += UnprocessedInput_PointerReleased;

            // Handlers to clear the selection when inking or erasing is detected
            inkCanvas.InkPresenter.StrokeInput.StrokeStarted += StrokeInput_StrokeStarted;
            inkCanvas.InkPresenter.StrokesErased += InkPresenter_StrokesErased;

            SizeChanged += Scenario3_SizeChanged;
        }

        private void StrokeInput_StrokeStarted(InkStrokeInput sender, Windows.UI.Core.PointerEventArgs args)
        {
            ClearSelection();
        }

        private void InkPresenter_StrokesErased(InkPresenter sender, InkStrokesErasedEventArgs args)
        {
            ClearSelection();
        }

        private void UnprocessedInput_PointerPressed(InkUnprocessedInput sender, Windows.UI.Core.PointerEventArgs args)
        {
            //add Polyline to Canvas
            lasso = new Polyline()
            {
                Stroke = new SolidColorBrush(Windows.UI.Colors.Blue),
                StrokeThickness = 1,
                StrokeDashArray = new DoubleCollection() { 5, 2 },
            };

            lasso.Points.Add(args.CurrentPoint.RawPosition);

            selectionCanvas.Children.Add(lasso);
        }

        private void UnprocessedInput_PointerMoved(InkUnprocessedInput sender, Windows.UI.Core.PointerEventArgs args)
        {
            lasso.Points.Add(args.CurrentPoint.RawPosition);
        }

        private void UnprocessedInput_PointerReleased(InkUnprocessedInput sender, Windows.UI.Core.PointerEventArgs args)
        {
            lasso.Points.Add(args.CurrentPoint.RawPosition);
            //Select Ink
            boundingRect = inkCanvas.InkPresenter.StrokeContainer.SelectWithPolyLine(lasso.Points);

            DrawBoundingRect();
        }

        private void DrawBoundingRect()
        {
            //Draw Select Rect
            selectionCanvas.Children.Clear();

            if ((boundingRect.Width == 0) || (boundingRect.Height == 0) || boundingRect.IsEmpty)
            {
                return;
            }

            var rectangle = new Rectangle()
            {
                Stroke = new SolidColorBrush(Windows.UI.Colors.Blue),
                StrokeThickness = 1,
                StrokeDashArray = new DoubleCollection() { 5, 2 },
                Width = boundingRect.Width,
                Height = boundingRect.Height
            };

            Canvas.SetLeft(rectangle, boundingRect.X);
            Canvas.SetTop(rectangle, boundingRect.Y);

            selectionCanvas.Children.Add(rectangle);
        }

        private void ClearDrawnBoundingRect()
        {
            if (selectionCanvas.Children.Any())
            {
                selectionCanvas.Children.Clear();
                boundingRect = Rect.Empty;
            }
        }

        private void ClearSelection()
        {
            var strokes = inkCanvas.InkPresenter.StrokeContainer.GetStrokes();
            foreach (var stroke in strokes)
            {
                stroke.Selected = false;
            }
            ClearDrawnBoundingRect();
        }

        private void Scenario3_SizeChanged(object sender, SizeChangedEventArgs e)
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
            outputGrid.Height = Window.Current.Bounds.Height / 2;
            selectionCanvas.Height = Window.Current.Bounds.Height / 2;
            inkCanvas.Height = Window.Current.Bounds.Height / 2;
        }

        void OnClear(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            inkCanvas.InkPresenter.StrokeContainer.Clear();
            ClearDrawnBoundingRect();
        }

        void OnCut(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            inkCanvas.InkPresenter.StrokeContainer.CopySelectedToClipboard();
            inkCanvas.InkPresenter.StrokeContainer.DeleteSelected();
            ClearDrawnBoundingRect();
        }

        void OnCopy(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            inkCanvas.InkPresenter.StrokeContainer.CopySelectedToClipboard();
        }

        void OnPaste(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            this.FlyoutColor.ShowAt((FrameworkElement)sender);
        }

        void OnPenColorChanged(object sender, RoutedEventArgs e)
        {
            this.FlyoutColor.Hide();
            if (inkCanvas.InkPresenter.StrokeContainer.CanPasteFromClipboard())
            {
                //get current inkstrokers count
                int currentCount = inkCanvas.InkPresenter.StrokeContainer.GetStrokes().Count;
                inkCanvas.InkPresenter.StrokeContainer.PasteFromClipboard(new Point(20,20));

                InkDrawingAttributes drawingAttributes = inkCanvas.InkPresenter.CopyDefaultDrawingAttributes();
                // Use button's background to set new pen's color
                var borderSender = sender as Border;
                var brush = borderSender.Background as Windows.UI.Xaml.Media.SolidColorBrush;

                drawingAttributes.Color = brush.Color;

                IReadOnlyList<InkStroke> inkStrokes = inkCanvas.InkPresenter.StrokeContainer.GetStrokes();
                for (int i = currentCount; i < inkStrokes.Count; i++)
                {
                    inkStrokes[i].DrawingAttributes = drawingAttributes;
                }
            }
            else
            {
                rootPage.ShowMessage("Cannot paste from clipboard.");
            }
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

    }
}
