using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace SimpleHello
{
    /// <summary>
    /// this page show account Detils
    /// </summary>
    public sealed partial class AccountDetails : Page
    {
        private MainPage rootPage;
        private Account activeAccount;

        public AccountDetails()
        {
            this.InitializeComponent();
            this.SizeChanged += AccountDetails_SizeChanged;
            this.Loaded += AccountDetails_Loaded;
        }

        private void AccountDetails_Loaded(object sender, RoutedEventArgs e)
        {
            this.textWelcome.Text = activeAccount.Name;
            this.activeAccount.loginCount += 1;
            this.updateAccount();
        }

        private void AccountDetails_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.SetGridSize();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            activeAccount = (Account)e.Parameter;
            //set inkCanvas size
            rootPage = MainPage.Current;
            this.SetGridSize();
        }

        private void SetGridSize()
        {
            //set grid size
            this.leftGrid.Height = Window.Current.Bounds.Height;
        }

        private void updateAccount()
        {
            //find current account and update
            foreach (Account a in UserSelect.accountList)
            {
                if (a.Email == this.activeAccount.Email)
                {
                    UserSelect.accountList.Remove(a);
                    break;
                }
            }
            UserSelect.accountList.Add(this.activeAccount);
            AccountsHelper.SaveAccountList(UserSelect.accountList);
        }

        private void textSignout_Tapped(object sender, TappedRoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(UserSelect));
        }

        private void transfer_Tapped(object sender, TappedRoutedEventArgs e)
        {
            string tag = (sender as TextBlock).Tag.ToString();
            string account = string.Empty;
            string parm = string.Empty;
            if (tag == "1")
            {
                account = "6235 2897 3123 0982  ";
            }
            else if (tag == "2")
            {
                account = "6225 9677 4020 8749  ";
            }
            else
            {
                account = "6532 7282 0980 1097  ";
            }
            this.activeAccount.accountNO = account;
            this.Frame.Navigate(typeof(TransferAccounts),this.activeAccount);
        }

        private void bill_Tapped(object sender, TappedRoutedEventArgs e)
        {
            string tag = (sender as TextBlock).Tag.ToString();
            string account = string.Empty;
            string parm = string.Empty;
            if (tag == "1")
            {
                account = ResourceManagerHelper.ReadValue("AccountType1") + "  6235 2897 3123 0982";
            }
            else if (tag == "2")
            {
                account = ResourceManagerHelper.ReadValue("AccountType2") + "  6225 9677 4020 8749";
            }
            else
            {
                account = ResourceManagerHelper.ReadValue("AccountType3") + "  6532 7282 0980 1097" ;
            }
            this.activeAccount.accountNO = account;
            this.Frame.Navigate(typeof(BillQuery), this.activeAccount);

        }

        private void TextBlock_Tapped(object sender, TappedRoutedEventArgs e)
        {
            this.activeAccount.accountNO = string.Empty;
            this.Frame.Navigate(typeof(TransferAccounts),this.activeAccount);
        }
    }
}
