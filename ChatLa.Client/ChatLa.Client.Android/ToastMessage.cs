using Android.Widget;
using Xamarin.Forms;
[assembly: Dependency(typeof(ChatLa.Client.Droid.ToastMessage))]
namespace ChatLa.Client.Droid
{
    class ToastMessage : IToastMessage
    {
        public void LongTime(string message)
        {
            Toast.MakeText(Android.App.Application.Context, message, ToastLength.Long).Show();
        }
        public void ShortTime(string message)
        {
            Toast.MakeText(Android.App.Application.Context, message, ToastLength.Short).Show();
        }
    }
}