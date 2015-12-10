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
using System.Threading.Tasks;
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
    public sealed partial class Scenario3_phone : Page
    {
        private Rect boundingRect;
        private InkStroke currentSelectStroke;

        private MainPage rootPage;

        public Scenario3_phone()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
            //read language related resource file .strings/en or zh-cn/resources.resw
            Run run1 = new Run();
            run1.Text = ResourceManagerHelper.ReadValue("Description3_p1");
            Run run2 = new Run();
            run2.Text = ResourceManagerHelper.ReadValue("Description3_pp2");
            this.textDes.Inlines.Add(run1);
            this.textDes.Inlines.Add(new LineBreak());
            this.textDes.Inlines.Add(run2);

            // Initialize the InkCanvas
            inkCanvas.InkPresenter.InputDeviceTypes = Windows.UI.Core.CoreInputDeviceTypes.Mouse | Windows.UI.Core.CoreInputDeviceTypes.Pen | Windows.UI.Core.CoreInputDeviceTypes.Touch;
            inkCanvas.InkPresenter.InputProcessingConfiguration.Mode = InkInputProcessingMode.Inking;

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

        private void ClearDrawnBoundingRect()
        {
            //clear selectionCanvas children
            if (selectionCanvas.Children.Any())
            {
                selectionCanvas.Children.Clear();
                boundingRect = Rect.Empty;
            }
        }

        private void ClearSelection()
        {
            //clear select strokes
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
            //Draw select stroke Rect
            if (e.NavigationMode == NavigationMode.Back)
            {
                this.DrawSelectRect();
            }
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
            //clear all stroke
            inkCanvas.InkPresenter.StrokeContainer.Clear();
            this.currentSelectStroke = null;
            ClearDrawnBoundingRect();
        }

        void OnCut(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            //cope select Stroke and Delete selecte
            bool rev = this.copySelectStroke();
            if (rev)
            {
                inkCanvas.InkPresenter.StrokeContainer.DeleteSelected();
                ClearDrawnBoundingRect();
            }
            else
            {
                rootPage.ShowMessage("please select stroke");
            }
        }

        void OnCopy(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            this.copySelectStroke();
        }

        void OnPaste(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            if(this.currentSelectStroke != null)
            {

                this.FlyoutColor.ShowAt((FrameworkElement)sender);
            }
            else
            {
                rootPage.ShowMessage("please select stroke");
            }
        }

        void OnPenColorChanged(object sender, RoutedEventArgs e)
        {
            this.FlyoutColor.Hide();
            if (this.currentSelectStroke != null)
            {
                //clear select Stroke
                this.ClearSelection();
                //set paste color
                var borderSender = sender as Border;
                var brush = borderSender.Background as Windows.UI.Xaml.Media.SolidColorBrush;
                InkDrawingAttributes drawingAttributes = inkCanvas.InkPresenter.CopyDefaultDrawingAttributes();
                drawingAttributes.Color = brush.Color;
                this.currentSelectStroke.DrawingAttributes = drawingAttributes;
                //add stroke
                inkCanvas.InkPresenter.StrokeContainer.AddStroke(this.currentSelectStroke);
                inkCanvas.InkPresenter.StrokeContainer.MoveSelected(new Point(20, 20));
                //clear select
                this.ClearSelection();
                this.currentSelectStroke = null;
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

        private void AppBarSelect_Click(object sender, RoutedEventArgs e)
        {
            //navigate selectstroke
            int strokesCount = inkCanvas.InkPresenter.StrokeContainer.GetStrokes().Count;
            if (strokesCount > 0)
            {
                this.Frame.Navigate(typeof(SelectStroke), strokesCount);
            }
            else
            {
                rootPage.ShowMessage("Must first write something.");
            }
        }

        public void DrawSelectRect()
        {
            //get current select stroke index
            int currentIndex = MainPage.Current.selectStrokeIndex;
            if (currentIndex > -1)
            {
                //set stroke selected
                IReadOnlyList<InkStroke> listStroke = inkCanvas.InkPresenter.StrokeContainer.GetStrokes();
                for (int i = 0; i < listStroke.Count; i++)
                {
                    if (i == currentIndex)
                    {
                        listStroke[i].Selected = true;
                        boundingRect = listStroke[i].BoundingRect;
                    }
                    else
                    {
                        listStroke[i].Selected = false;
                    }
                }
                //draw select rect
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
        }

        private bool copySelectStroke()
        {
            //copy select stroke
            bool rev = false;
            IReadOnlyList<InkStroke> listStroke = inkCanvas.InkPresenter.StrokeContainer.GetStrokes();
            for(int i = 0;i<listStroke.Count;i++)
            {
                if (listStroke[i].Selected)
                {
                    this.currentSelectStroke = listStroke[i].Clone();
                    this.currentSelectStroke.Selected = true;
                    selectionCanvas.Children.Clear();
                    rev = true;
                    break;
                }
            }
            return rev;
        }
        
    }
}
