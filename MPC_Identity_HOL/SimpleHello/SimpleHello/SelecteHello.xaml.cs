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
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace SimpleHello
{
    /// <summary>
    /// this page Show How to use Windows Hello
    /// </summary>
    public sealed partial class SelecteHello : Page
    {
        private MainPage rootPage;
        private Account activeAccount;

        public SelecteHello()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            rootPage = MainPage.Current;
            this.activeAccount = (Account)e.Parameter;
            //read language related resource file .strings/en or zh-cn/resources.resw
            Run run1 = new Run();
            run1.Text = string.Format(ResourceManagerHelper.ReadValue("SelecteHelloDs1"),activeAccount.Name);
            Run run2 = new Run();
            run2.Text = ResourceManagerHelper.ReadValue("SelecteHelloDs2");
            this.textHelloDes.Inlines.Add(run1);
            this.textHelloDes.Inlines.Add(new LineBreak());
            this.textHelloDes.Inlines.Add(run2);
        }

        async void Footer_Click(object sender, RoutedEventArgs e)
        {
            await Windows.System.Launcher.LaunchUriAsync(new Uri(((HyperlinkButton)sender).Tag.ToString()));
        }

        private void BtnLater_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(AccountDetails),this.activeAccount);
        }

        private async void BtnStart_Click(object sender, RoutedEventArgs e)
        {
            //Create Passport
            LoginHelp loginHelp = new LoginHelp(this.activeAccount);
            bool rev = await loginHelp.CreatePassportKey(this.activeAccount.Name);
            if(rev)
            {
                //add possport to server.
                bool serverAddedPassportToAccount = await loginHelp.AddPassportToAccountOnServer();

                if (serverAddedPassportToAccount == true)
                {
                    //update userPassport state
                    this.activeAccount.UsesPassport = true;
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
                this.Frame.Navigate(typeof(AccountDetails), this.activeAccount);
            }
        }
    }
}
