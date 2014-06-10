﻿using System;
using System.Linq;
using Windows.UI.Xaml.Data;

namespace FanfouWP2.ItemControl.ValueConverter
{
    public sealed class TextToLeftCountTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {          
            return "还剩" + (140-(value as String).Count()).ToString();   
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }
}
