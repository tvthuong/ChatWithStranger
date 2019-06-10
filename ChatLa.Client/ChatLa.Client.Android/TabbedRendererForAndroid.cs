using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V4.View;
using Android.Views;
using Android.Widget;
using ChatLa.Client;
using ChatLa.Client.Droid;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android.AppCompat;

[assembly: ExportRenderer(typeof(MyTabbedPage), typeof(TabbedRendererForAndroid))]
namespace ChatLa.Client.Droid
{
    public class TabbedRendererForAndroid : TabbedPageRenderer
    {
        public TabbedRendererForAndroid(Context context) : base(context)
        {

        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            if (e.PropertyName == "IsHidden")
            {
                TabLayout TabsLayout = null;
                for (int i = 0; i < ChildCount; ++i)
                {
                    Android.Views.View view = (Android.Views.View)GetChildAt(i);
                    if (view is TabLayout)
                        TabsLayout = (TabLayout)view;
                }
                if ((Element as MyTabbedPage).IsHidden)
                {
                    TabsLayout.Visibility = ViewStates.Invisible;
                }
                else
                {
                    TabsLayout.Visibility = ViewStates.Visible;
                }
            }
        }
    }
}