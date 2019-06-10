using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration.AndroidSpecific;

namespace ChatLa.Client
{
    public class MyTabbedPage : Xamarin.Forms.TabbedPage
    {
        public static readonly BindableProperty IsHiddenProperty = BindableProperty.Create("IsHidden", typeof(bool), typeof(MyTabbedPage), false);
        public MyTabbedPage() : base() { }
        public bool IsHidden
        {
            set
            {
                SetValue(IsHiddenProperty, value);
                On<Xamarin.Forms.PlatformConfiguration.Android>().SetIsSwipePagingEnabled(!value);
            }
            get { return (bool)GetValue(IsHiddenProperty); }
        }
    }
}
