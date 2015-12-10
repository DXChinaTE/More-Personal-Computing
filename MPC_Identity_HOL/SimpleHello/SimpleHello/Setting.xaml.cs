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
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace SimpleHello
{
    /// <summary>
    /// This page displays code for Language setting
    /// </summary>
    public sealed partial class Setting : Page
    {
        private MainPage rootPage;
        private string currentLanguage = string.Empty;
        public Setting()
        {
            this.InitializeComponent();
            rootPage = MainPage.Current;
            //set Language list
            this.cmbLanguage.Items.Add("中文(中华人民共和国)");
            this.cmbLanguage.Items.Add("English(United States)");
            string strLanguage = Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride;
            if (strLanguage == string.Empty)
                Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride = Windows.Globalization.ApplicationLanguages.Languages[0];
            this.currentLanguage = strLanguage;
            if (strLanguage == "zh-Hans-CN")
            {
                this.cmbLanguage.SelectedIndex = 0;
            }
            else
            {
                this.cmbLanguage.SelectedIndex = 1;
            }
        }

        private void cmbLanguage_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //set current Language
            string selectedValue = (string)cmbLanguage.SelectedValue;
            if (selectedValue == "中文(中华人民共和国)")
            {
                if (Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride != "zh-Hans-CN")
                {
                    this.currentLanguage = "zh-Hans-CN";
                    this.txtLanguageTip.Visibility = Visibility.Visible;
                    this.btnClose.Visibility = Visibility.Visible;
                }
                else
                {
                    this.txtLanguageTip.Visibility = Visibility.Collapsed;
                    this.btnClose.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                if (Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride != "en-US")
                {
                    this.currentLanguage = "en-US";
                    this.txtLanguageTip.Visibility = Visibility.Visible;
                    this.btnClose.Visibility = Visibility.Visible;
                }
                else
                {
                    this.txtLanguageTip.Visibility = Visibility.Collapsed;
                    this.btnClose.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //set Language and close App
            Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride = this.currentLanguage;
            Application.Current.Exit();
        }

        private void btnClearAccount_Click(object sender, RoutedEventArgs e)
        {
            UserSelect.accountList.Clear();
            AccountsHelper.SaveAccountList(UserSelect.accountList);
            rootPage.ShowMessage("Clear Account Success.");
        }
    }
}
