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
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Security.Credentials;
using Windows.UI;
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
    /// The user select page, if users successfully log into the app they will be added to a saved set of users. 
    /// That set of users will populate a list of account tiles that can be chosen from for signing in on this page.
    /// 
    /// If there are no accounts in the history, it will skip this and go directly to the traditional username/password sign in page.
    /// 
    /// If there exists 1 or more accounts, users can select an account tile which will pass the account object to the sign in form.
    /// </summary>
    public partial class UserSelect : Page
    {
        private MainPage rootPage;
        static public List<Account> accountList;

        public UserSelect()
        {
            this.InitializeComponent();
            this.Loaded += OnLoadedActions;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            rootPage = MainPage.Current;
        }

        /// <summary>
        /// Function called when the current frame is loaded.
        /// 
        /// Calls SetUpAccounts
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnLoadedActions(object sender, RoutedEventArgs e)
        {
            SetUpAccounts();
        }

        /// <summary>
        /// Uses the AccountsHelper to load the list of accounts from a saved local app file,
        /// then checks to see if it was empty or not. If it is empty then just go to the sign in form.
        /// </summary>
        private async void SetUpAccounts()
        {
            accountList = await AccountsHelper.LoadAccountList();
            this.listUser.ItemsSource = accountList;
            if(accountList.Count > 0)
            {
                if (accountList.Count == 1)
                {
                    this.Frame.Navigate(typeof(SignIn));
                }
                else
                {
                    this.listUser.SelectedIndex = 0;
                }
            }
        }

        /// Function called when an account is selected in the list of accounts
        ///
        /// Navigates to the sign in form and passes the chosen account
        /// The sign in form will check if Microsoft Passport should be used or not
        private async void listUser_Tapped(object sender, TappedRoutedEventArgs e)
        {
            e.Handled = true;
            await Task.Delay(TimeSpan.FromSeconds(0.1));
            if (this.listUser.SelectedIndex > -1)
            {
                Account account = (Account)this.listUser.SelectedItem;
                //Just navigates to the default sign in form with nothing filled out
                if (account.isAdd)
                {
                    this.Frame.Navigate(typeof(SignIn));
                }
                else
                {
                    this.Frame.Navigate(typeof(SignIn), account);
                }
            }
        }

    }
}
