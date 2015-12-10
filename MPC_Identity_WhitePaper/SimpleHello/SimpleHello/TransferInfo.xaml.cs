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
    public sealed partial class TransferInfo : Page
    {
        private MainPage rootPage;
        private TransferInfoData activeAccount;

        public TransferInfo()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            rootPage = MainPage.Current;
            base.OnNavigatedTo(e);
            this.activeAccount = (TransferInfoData)e.Parameter;
            this.textAccountLabelInfo.Text = this.activeAccount.payAccount;
            this.textTonameInfo.Text = this.activeAccount.receiveAccountName;
            this.textToaccountInfo.Text = this.activeAccount.receiveAccount;
            this.textMoneyInfo.Text = this.activeAccount.payMoney;
        }

        private void btnTransfer_Click(object sender, RoutedEventArgs e)
        {
            if (this.Frame.CanGoBack)
                this.Frame.GoBack();
        }
    }

    public class TransferInfoData
    {
        public string payAccount { get; set; }
        public string receiveAccount { get; set; }
        public string receiveAccountName { get; set; }
        public string payMoney { get; set; }
    }
}
