﻿using System;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;

namespace FanfouUWP.Utils
{
    public static class ToastShow
    {
        public static void ShowToast(string title, string content)
        {
            XmlDocument toastXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastText02);
            XmlNodeList toastNodeList = toastXml.GetElementsByTagName("text");
            toastNodeList.Item(0).AppendChild(toastXml.CreateTextNode(title));
            toastNodeList.Item(1).AppendChild(toastXml.CreateTextNode(content));

            ToastNotification toast = new ToastNotification(toastXml);
            toast.SuppressPopup = false;
            ToastNotificationManager.CreateToastNotifier().Show(toast);
        }

        public static void ShowInformation(string content, Action end)
        {
            ShowToast("饭窗UWP", content);
            end();
        }

        public static void ShowInformation(string content)
        {
            ShowInformation(content, () => { });
        }

    }
}

