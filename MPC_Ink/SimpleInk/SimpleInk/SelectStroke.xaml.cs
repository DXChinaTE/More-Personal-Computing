using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Phone.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;


namespace SimpleInk
{
    /// <summary>
    /// this page show selectstroke
    /// </summary>
    public sealed partial class SelectStroke : Page
    {
        List<string> ListStroke = new List<string>();
        int index = -1;
        public SelectStroke()
        {
            this.InitializeComponent();

            HardwareButtons.BackPressed += HardwareButtons_BackPressed; ;
        }

        private void HardwareButtons_BackPressed(object sender, BackPressedEventArgs e)
        {
            e.Handled = true;
            if(this.Frame.CanGoBack)
                this.Frame.GoBack();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            //load stroke
            base.OnNavigatedTo(e);
            int count = (int)e.Parameter;
            for(int i = 1;i<=count;i++)
            {
                ListStroke.Add(ResourceManagerHelper.ReadValue("selectStrokeText") + i.ToString());
            }
            this.StrokeControl.ItemsSource = ListStroke;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            if(e.SourcePageType == typeof(Scenario3_phone))
            {
                MainPage.Current.selectStrokeIndex = this.index;
            }
            base.OnNavigatedFrom(e);
        }

        private void StrokeControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //select stroke index
            this.index = StrokeControl.SelectedIndex;
            if(this.Frame.CanGoBack)
                this.Frame.GoBack();
        }
    }
}
