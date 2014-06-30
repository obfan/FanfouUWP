﻿using FanfouWP2.Common;
using FanfouWP2.FanfouAPI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

// “基本页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234237 上有介绍

namespace FanfouWP2
{
    public sealed partial class TimelinePage : Page
    {

        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();
        private ObservableCollection<ObservableCollection<Status>> statuses = new ObservableCollection<ObservableCollection<Status>>();

        public enum PageType { Statuses, Favorite };
        private PageType currentType;
        private object data;
        private Status currentClick;

        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }


        public TimelinePage()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += navigationHelper_LoadState;
            this.navigationHelper.SaveState += navigationHelper_SaveState;

            FanfouAPI.FanfouAPI.Instance.UserTimelineSuccess += Instance_UserTimelineSuccess;
            FanfouAPI.FanfouAPI.Instance.UserTimelineFailed += Instance_UserTimelineFailed;

            FanfouAPI.FanfouAPI.Instance.FavoritesSuccess += Instance_FavoritesSuccess;
            FanfouAPI.FanfouAPI.Instance.FavoritesFailed += Instance_FavoritesFailed;

            this.send.StatusUpdateSuccess += send_StatusUpdateSuccess;
            this.send.StatusUpdateFailed += send_StatusUpdateFailed;

            this.status.UserButtonClick += status_UserButtonClick;
            this.status.ReplyButtonClick += status_ReplyButtonClick;
            this.status.RepostButtonClick += status_RepostButtonClick;
            this.status.FavButtonClick += status_FavButtonClick;
        }

        private void status_FavButtonClick(object sender, RoutedEventArgs e)
        {
        }

        void status_RepostButtonClick(object sender, RoutedEventArgs e)
        {
            this.sendPopup.IsOpen = true;
            this.send.ChangeMode(CustomControl.SendSettingsFlyout.SendMode.Repose, currentClick);
        }

        void status_ReplyButtonClick(object sender, RoutedEventArgs e)
        {
            this.sendPopup.IsOpen = true;
            this.send.ChangeMode(CustomControl.SendSettingsFlyout.SendMode.Reply, currentClick);
        }

        void status_UserButtonClick(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(UserPage), currentClick.user);
        }

        void Instance_FavoritesFailed(object sender, FailedEventArgs e)
        {
            loading.Visibility = Visibility.Collapsed;
        }

        void Instance_FavoritesSuccess(object sender, EventArgs e)
        {
            loading.Visibility = Visibility.Collapsed;
            var ss = sender as List<Status>;
            if (ss.Count != 0)
                statuses.Add(new ObservableCollection<Status>(ss));
        }

        void send_StatusUpdateFailed(object sender, FailedEventArgs e)
        {
            loading.Visibility = Visibility.Collapsed;
        }

        void send_StatusUpdateSuccess(object sender, EventArgs e)
        {
            this.sendPopup.IsOpen = false;
            loading.Visibility = Visibility.Visible;
        }
        void Instance_UserTimelineFailed(object sender, FailedEventArgs e)
        {
            loading.Visibility = Visibility.Collapsed;
        }

        void Instance_UserTimelineSuccess(object sender, EventArgs e)
        {
            loading.Visibility = Visibility.Collapsed;
            var ss = sender as List<Status>;
            if (ss.Count != 0)
                statuses.Add(new ObservableCollection<Status>(ss));
        }

        private void navigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            currentType = ((KeyValuePair<PageType, object>)e.NavigationParameter).Key;
            data = ((KeyValuePair<PageType, object>)e.NavigationParameter).Value;

            loading.Visibility = Visibility.Visible;

            this.defaultViewModel["statuses"] = statuses;

            switch (currentType)
            {
                case PageType.Statuses:
                    FanfouAPI.FanfouAPI.Instance.StatusUserTimeline((data as User).id, 60);
                    if ((data as User).id == FanfouAPI.FanfouAPI.Instance.currentUser.id)
                        this.defaultViewModel["title"] = "我的消息";
                    else
                        this.defaultViewModel["title"] = (data as User).screen_name + "的消息";
                    break;
                case PageType.Favorite:
                    FanfouAPI.FanfouAPI.Instance.FavoritesId((data as User).id, 60, 1);
                    if ((data as User).id == FanfouAPI.FanfouAPI.Instance.currentUser.id)
                        this.defaultViewModel["title"] = "我的收藏";
                    else
                        this.defaultViewModel["title"] = (data as User).screen_name + "的收藏";
                    break;
                default:
                    break;
            }

            this.defaultViewModel["page"] = "第1页";
        }

        private void navigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
        }

        #region NavigationHelper 注册

        /// 此部分中提供的方法只是用于使
        /// NavigationHelper 可响应页面的导航方法。
        /// 
        /// 应将页面特有的逻辑放入用于
        /// <see cref="GridCS.Common.NavigationHelper.LoadState"/>
        /// 和 <see cref="GridCS.Common.NavigationHelper.SaveState"/> 的事件处理程序中。
        /// 除了在会话期间保留的页面状态之外
        /// LoadState 方法中还提供导航参数。

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedFrom(e);
        }

        #endregion

        private void statusesGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            currentClick = e.ClickedItem as Status;
            this.status.setStatus(currentClick);
            this.statusPopup.IsOpen = true;
        }

        private void flipView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.defaultViewModel["page"] = "第" + (this.flipView.SelectedIndex + 1).ToString() + "页";

            if (this.flipView.SelectedIndex == this.flipView.Items.Count() - 1)
            {
                loading.Visibility = Visibility.Visible;
                switch (currentType)
                {
                    case PageType.Statuses:
                        FanfouAPI.FanfouAPI.Instance.StatusUserTimeline((data as User).id, 60, this.flipView.Items.Count() + 1);
                        break;
                    case PageType.Favorite:
                        FanfouAPI.FanfouAPI.Instance.FavoritesId(FanfouAPI.FanfouAPI.Instance.currentUser.id, 60, this.flipView.Items.Count() + 1);
                        break;
                    default:
                        break;
                }
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            this.statuses.Clear();
            loading.Visibility = Visibility.Visible;

            switch (currentType)
            {
                case PageType.Statuses:
                    FanfouAPI.FanfouAPI.Instance.StatusUserTimeline((data as User).id, 60, 1);
                    if ((data as User).id == FanfouAPI.FanfouAPI.Instance.currentUser.id)
                        this.defaultViewModel["title"] = "我的消息";
                    else
                        this.defaultViewModel["title"] = (data as User).screen_name + "的消息";
                    break;
                case PageType.Favorite:
                    FanfouAPI.FanfouAPI.Instance.FavoritesId(FanfouAPI.FanfouAPI.Instance.currentUser.id, 60, 1);
                    if ((data as User).id == FanfouAPI.FanfouAPI.Instance.currentUser.id)
                        this.defaultViewModel["title"] = "我的收藏";
                    else
                        this.defaultViewModel["title"] = (data as User).screen_name + "的收藏";
                    break;
                default:
                    break;
            }
        }

        private void LeftButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.flipView.SelectedIndex > 0)
                this.flipView.SelectedIndex--;
        }

        private void RightButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.flipView.SelectedIndex < this.flipView.Items.Count() - 1)
                this.flipView.SelectedIndex++;
        }
        private void flipView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
        }

        private void send_BackClick(object sender, BackClickEventArgs e)
        {
            this.sendPopup.IsOpen = false;
        }

        private void status_BackClick(object sender, BackClickEventArgs e)
        {
            this.statusPopup.IsOpen = false;
        }
    }
}
