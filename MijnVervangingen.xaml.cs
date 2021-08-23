using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Tractivity.Models;
using Tractivity.ViewModels.Vervangingen;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Windows.Input;
using NavigationSam;
using LesgeversMEGA.Views.Vervangingen;

namespace Tractivity.Views.Vervangingen
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MijnVervangingen : ContentPage, INavigationPopInterceptor
    {
        public string username { get; set; }
        public string owner_of_vervanging { get; set; }
        private string UserPassword { get; set; }
        public bool IsPopRequest { get; set; }
        public string URL { get; set; }
        public MijnVervangingen(string user, string userpassword)
        {
            username = user;
            URL = "https://lesgeversmega.azurewebsites.net/";
            UserPassword = userpassword;
            InitializeComponent();
            this.BindingContext = new MijnVervangingenViewModel(username);
        }
        private async Task MenuItem_ClickedAsync(object sender, EventArgs e)
        {
            //Details redirect
            var vervanging = ((ListView)sender).SelectedItem as Vervanging;
            if (vervanging == null) { 
                return;

        }

        }

        public async Task<bool> RequestPop()
        {
            bool res = false;
            await Navigation.PushAsync(new VervangingenOverzicht(username, UserPassword));
            return res;
        }

        private async void MenuItem_Clicked_Delete(object sender, EventArgs e)
        {
            var menuitem = (MenuItem)sender;
            string classId = menuitem.ClassId;
            char c = '"';
            classId = classId.Replace(c.ToString(), "");
            int ID = Int32.Parse(classId);
            var result = await this.DisplayAlert("Opgelet", "Bent u zeker dat u deze vervanging wenst te verwijderen?", "Ja", "Neen");
            if (result)
            {
                var client = new HttpClient();
                Vervanging vervanging = new Vervanging
                {
                    Id = ID,
                };
                var json = JsonConvert.SerializeObject(vervanging);
                HttpContent httpContent = new StringContent(json);
                httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                HttpResponseMessage response = await client.PostAsync(URL + "Vervangings/GetVervangingOwner", httpContent);
                HttpContent httpContent_owner = response.Content;
                string owner = await httpContent_owner.ReadAsStringAsync();
                c = '"';
                owner = owner.Replace(c.ToString(), "");
                owner_of_vervanging = owner;
                if (username != owner_of_vervanging)
                {
                    var result3 = await this.DisplayAlert("Opgelet", "U bent niet de eigenaar!", null, "Ok");
                    if (!result3)
                    {

                    }
                }
                else
                {
                    bool complete = DeleteVervanging(ID);
                    if (complete)
                    {
                        var result2 = await this.DisplayAlert("Opgelet", "Vervanging verwijderd", null, "Ok");
                        if (!result2)
                        {
                            await Navigation.PushAsync(new MijnVervangingen(username, UserPassword));
                        }
                    }
                    else
                    {

                    }
                }
                //Api request to delete vervanging;
            }
        }
        public bool DeleteVervanging(int i)
        {
            bool Response = false;
            var client = new HttpClient();
            Vervanging vervanging = new Vervanging
            {
                Id = i,
            };

            var json = JsonConvert.SerializeObject(vervanging);
            HttpContent httpContent = new StringContent(json);
            httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            var response = client.PostAsync(URL + "Vervangings/Delete", httpContent);
            var mystring = response.GetAwaiter().GetResult();
            if (response.Result.IsSuccessStatusCode)
            {
                Response = true;
            }
            return Response;
        }
        async void MenuItem_Clicked(object sender, EventArgs e)
        {
            var menuitem = (MenuItem)sender;
            string classId = menuitem.ClassId;
            char c = '"';
            classId = classId.Replace(c.ToString(), "");
            int ID = Int32.Parse(classId);
            //Push naar details
            bool Response = false;
            var client = new HttpClient();
            Vervanging vervanging = new Vervanging
            {
                Id = ID,
            };

            var json = JsonConvert.SerializeObject(vervanging);
            HttpContent httpContent = new StringContent(json);
            httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            HttpResponseMessage response = await client.PostAsync(URL + "Vervangings/GetVervangingOwner", httpContent);
            HttpContent httpContent_owner = response.Content;
            string owner = await httpContent_owner.ReadAsStringAsync();
            c = '"';
            owner = owner.Replace(c.ToString(), "");
            owner_of_vervanging = owner;
            await Navigation.PushAsync(new SpecifiekeVervanging(ID, username, owner, UserPassword));
        }

        async void Create_Vervanging_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new CreateVervangingNew(username, UserPassword));
        }
    }
}