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
using Windows.Security.Cryptography;
using Windows.Storage.Streams;
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
    /// The sign in form that contains username and password fields and the option to sign in
    /// with either Microsoft Passport or traditional username/password authentication.
    /// 
    /// Right now there is no authentication of username/password except for that they are not empty -
    /// so any username/password combo will be accepted. However if Passport is used, it will use the
    /// actual Passport PIN that is set up on the machine running the sample.
    ///
    /// If an account was passed from the user select we will do one of two things
    /// 
    ///     1. If the selected account used Microsoft Passport as the last sign in method, it will attempt Microsoft Passport
    ///         sign in first. If that is cancelled or fails, it will fall back on username/password form for sign in.
    /// 
    ///     2. Else - show the username/password sign in form with username filled in based on chosen account.
    /// </summary>
    public sealed partial class SignIn : Page
    {
        private MainPage rootPage;
        private Account m_account;

        public SignIn()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// The function that will be called when this frame is navigated to.
        ///
        /// Checks to see if Microsoft Passport is available and checks to see if an 
        /// account was passed in.
        ///
        /// If an account was passed, check if it uses Microsoft Passport.
        /// and set the "adding user" flag so we don't add a new account
        /// to the list of users.
        /// </summary>
        /// <param name="e"></param>
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            rootPage = MainPage.Current;

            if (e.Parameter != null)
            {
                m_account = (Account)e.Parameter;
                textbox_Username.Text = m_account.Email;
                textbox_Username.IsEnabled = false;
                if (this.m_account.UsesPassport)
                {
                    LoginHelp loginHelp = new LoginHelp(this.m_account);
                    bool rev = await loginHelp.SignInPassport();
                    if (rev)
                    {
                        this.Frame.Navigate(typeof(AccountDetails), this.m_account);
                    }
                }
            }
            var accountList = await AccountsHelper.LoadAccountList();
            if (accountList.Count > 1)
            {
                this.btnSelectUser.Visibility = Visibility.Visible;
            }
            else
            {
                this.btnSelectUser.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Function called when regular username/password sign in button is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Button_SignIn_Click(object sender, RoutedEventArgs e)
        {
            if (this.textbox_Username.Text == string.Empty || this.passwordbox_Password.Password == string.Empty)
            {
                this.textblock_ErrorField.Visibility = Visibility.Visible;
                return;
            }
            else
            {
                this.textblock_ErrorField.Visibility = Visibility.Collapsed;
            }
            bool isNewAccount = false;
            if (this.m_account == null)
            {
                this.m_account = new Account() { head = "Assets/head.png", Name = this.textbox_Username.Text, Email = this.textbox_Username.Text, loginCount = 0, UsesPassport = false, isAdd = false };
                isNewAccount = true;
            }
            LoginHelp loginHelp = new LoginHelp(this.m_account);
            bool rev = await loginHelp.SignInPassword(isNewAccount);
            if (rev)
            {
                //Checks to see if Passport is ready to be used.
                //1. Having a connected MSA Account
                //2. Having a Windows PIN set up for that account on the local machine
                var keyCredentialAvailable = await KeyCredentialManager.IsSupportedAsync();
                if (!this.m_account.UsesPassport && keyCredentialAvailable)
                {
                    this.Frame.Navigate(typeof(SelecteHello), this.m_account);
                    return;
                }
                this.Frame.Navigate(typeof(AccountDetails), this.m_account);
            }
            else
            {
                rootPage.ShowMessage("SignIn Failure");
            }
        }

        private void Button_Select_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(UserSelect));
        }
    }
}
