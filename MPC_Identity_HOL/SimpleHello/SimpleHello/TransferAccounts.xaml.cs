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

    public sealed partial class TransferAccounts : Page
    {
        private MainPage rootPage;
        private Account activeAccount;

        public TransferAccounts()
        {
            this.InitializeComponent();
            this.Loaded += TransferAccounts_Loaded;
            this.Unloaded += TransferAccounts_Unloaded;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            rootPage = MainPage.Current;
            base.OnNavigatedTo(e);
            this.activeAccount = (Account)e.Parameter;
            string param = this.activeAccount.accountNO;
            if (param != string.Empty)
            {
                this.textAccountLabelInfo.Text = param;
                this.textAccountLabelInfo.IsReadOnly = true;
            }
            else
            {
                this.textAccountLabelInfo.IsReadOnly = false;
            }
        }

        private void TransferAccounts_Unloaded(object sender, RoutedEventArgs e)
        {
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
            SystemNavigationManager.GetForCurrentView().BackRequested -= TransferAccounts_BackRequested;
        }

        private void TransferAccounts_Loaded(object sender, RoutedEventArgs e)
        {
            if (Frame.CanGoBack)
            {
                SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
                SystemNavigationManager.GetForCurrentView().BackRequested += TransferAccounts_BackRequested;
            }
            else
                SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
        }

        private void TransferAccounts_BackRequested(object sender, BackRequestedEventArgs e)
        {
            if (this.Frame.CanGoBack)
                this.Frame.GoBack();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            if (this.textTonameInfo.Text == "" || this.textToaccountInfo.Text == "" || this.textMoneyInfo.Text == "")
            {
                rootPage.ShowMessage("Please fill out the information");
                return;
            }
            TransferInfoData data = new TransferInfoData();
            data.payAccount = this.activeAccount.accountNO;
            data.receiveAccount = this.textToaccountInfo.Text;
            data.receiveAccountName = this.textTonameInfo.Text;
            data.payMoney = this.textMoneyInfo.Text;
            LoginHelp loginHelp = new LoginHelp(this.activeAccount);
            if (this.activeAccount.UsesPassport)
            {
               
                bool rev = await loginHelp.SignInPassport();
                if (rev)
                {
                    this.Frame.Navigate(typeof(TransferInfo),data);
                    this.clearInfo();
                }
            }
            else
            {
                bool rev = await loginHelp.CreatePassportKey(this.activeAccount.Name);
                if (rev)
                {
                    this.Frame.Navigate(typeof(TransferInfo), data);
                    this.clearInfo();
                }
            }
        }

        private void clearInfo()
        {
            this.textTonameInfo.Text = string.Empty;
            this.textToaccountInfo.Text = string.Empty;
            this.textMoneyInfo.Text = string.Empty;
        }
    }
}
