using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using ChatLa.Client.ViewModels;
using ChatLa.Client.Helpers;

namespace ChatLa.Client.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class RoomListPage : ContentPage
	{
		public RoomListPage ()
		{
			InitializeComponent ();
		}

        private void ListView_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            ((RoomListPageViewModel)BindingContext).JoinRoom(CurrentSettings.Settings.ChatName, ((Common_Library.Room)e.Item).Id);
        }
    }
}