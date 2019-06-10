using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms.Platform.UWP;
using Windows.UI.Xaml.Media;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using ChatLa.Client;
using ChatLa.Client.UWP;
[assembly: ExportRenderer(typeof(MyTabbedPage), typeof(TabbedRendererForUWP))]
namespace ChatLa.Client.UWP
{
    public class TabbedRendererForUWP : TabbedPageRenderer
    {

        DataTemplate originalTemplate;
        protected override void OnElementChanged(VisualElementChangedEventArgs e)
        {
            base.OnElementChanged(e);

            Element.PropertyChanged += Element_PropertyChanged;

            originalTemplate = Control.HeaderTemplate;

        }

        private void Element_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsHidden")
            {
                if ((Element as MyTabbedPage).IsHidden)
                {
                    Windows.UI.Xaml.DataTemplate template = App.Current.Resources["MyDataTemplate"] as Windows.UI.Xaml.DataTemplate;
                    Control.HeaderTemplate = template;
                    Control.IsEnabled = true;
                }
                else
                {
                    Control.HeaderTemplate = originalTemplate;
                    Control.IsEnabled = true;
                }
            }
        }
    }
}