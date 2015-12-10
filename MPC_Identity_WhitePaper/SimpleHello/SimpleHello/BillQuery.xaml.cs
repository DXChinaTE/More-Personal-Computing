using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;


namespace SimpleHello
{
    public sealed partial class BillQuery : Page
    {
        private MainPage rootPage;
        private Account activeAccount;
        private List<BillInfo> listItem = null;

        public BillQuery()
        {
            this.InitializeComponent();
            this.Loaded += BillQuery_Loaded;
            this.Unloaded += BillQuery_Unloaded;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            rootPage = MainPage.Current;
            base.OnNavigatedTo(e);
            this.activeAccount = (Account)e.Parameter;
            this.textAccountName.Text = this.activeAccount.accountNO;
        }

        private void BillQuery_Unloaded(object sender, RoutedEventArgs e)
        {
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
            SystemNavigationManager.GetForCurrentView().BackRequested -= BillQuery_BackRequested;
        }

        private void BillQuery_Loaded(object sender, RoutedEventArgs e)
        {
            if (Frame.CanGoBack)
            {
                SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
                SystemNavigationManager.GetForCurrentView().BackRequested += BillQuery_BackRequested;
            }
            else
                SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
            this.loadBillInfo();
            this.listBill.ItemsSource = this.listItem;
        }

        private void BillQuery_BackRequested(object sender, BackRequestedEventArgs e)
        {
            if (this.Frame.CanGoBack)
                this.Frame.GoBack();
        }

        private void loadBillInfo()
        {
            this.listItem = new List<BillInfo>()
            {
               new BillInfo() {Date="2015-10-10 10:30",Type="转入",Money="￥ 100,00",Balance="￥ 1000,000.00",Remarks="支付宝" },
               new BillInfo() {Date="2015-10-08 11:30",Type="转出",Money="￥ 200,00",Balance="￥ 1000,000.00",Remarks="支付宝" },
               new BillInfo() {Date="2015-10-05 09:30",Type="转入",Money="￥ 100,00",Balance="￥ 1000,000.00",Remarks="支付宝" },
               new BillInfo() {Date="2015-10-03 09:45",Type="转入",Money="￥ 100,00",Balance="￥ 1000,000.00",Remarks="支付宝" },
               new BillInfo() {Date="2015-10-01 11:00",Type="转入",Money="￥ 100,00",Balance="￥ 1000,000.00",Remarks="支付宝" }
            };
        }

        private async void btnQueue_Click(object sender, RoutedEventArgs e)
        {
            LoginHelp loginHelp = new LoginHelp(this.activeAccount);
            if (this.activeAccount.UsesPassport)
            {
                bool rev = await loginHelp.SignInPassport();
                if (rev)
                {
                    this.listBill.Visibility = Visibility.Visible;
                }
            }
            else
            {
                bool rev = await loginHelp.CreatePassportKey(this.activeAccount.Name);
                if (rev)
                {
                    this.listBill.Visibility = Visibility.Visible;
                }
            }
        }
    }

    public class BillInfo
    {
        public string Date { get; set; }
        public string Type { get; set; }
        public string Money { get; set; }
        public string Balance { get; set; }
        public string Remarks { get; set; }

    }
}
