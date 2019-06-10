using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;
namespace ChatLa.Client
{
    public class MyNavigationPage : NavigationPage
    {
        public MyNavigationPage(Page root) : base(root)
        {
            Pushed += MyNavigationPage_Pushed;
            Popped += MyNavigationPage_Popped;
        }

        private void MyNavigationPage_Popped(object sender, NavigationEventArgs e)
        {
            if (CurrentPage == RootPage)
            {
                var mainPage = App.Current.MainPage as MyTabbedPage;
                if (mainPage != null)
                    mainPage.IsHidden = false;
            }
        }

        private void MyNavigationPage_Pushed(object sender, NavigationEventArgs e)
        {
            var mainPage = App.Current.MainPage as MyTabbedPage;
            if(mainPage != null)
                mainPage.IsHidden = true;
        }
    }
}
