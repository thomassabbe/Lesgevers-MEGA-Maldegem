using MvvmHelpers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tractivity.Models;
using Command = MvvmHelpers.Commands.Command;
using System.Net.Http;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using Newtonsoft.Json.Linq;
using System.Globalization;
using System.Linq;
using MvvmHelpers.Commands;

namespace Tractivity.ViewModels.Vervangingen
{
    class MijnVervangingenViewModel : BaseViewModel
    {
        public ObservableRangeCollection<Vervanging> MijnVervangingen { get; }
        public ObservableRangeCollection<Vervanging> MijnVervangingen_tijdelijk { get; }
        public string UserName_ { get; set; }
        public AsyncCommand RefreshCommand { get; }
        public string datum_aangevraagd_string { get; set; }
        public string datum_ingevuld_string { get; set; }

        public Command ClearCommand { get; }

        public MijnVervangingenViewModel(string gebruiker)
        {
            //Title = "Zwemmers";
            var result = Task.Run(async () => await LoadMore());
            //new ObservableRangeCollection<
            UserName_ = gebruiker;
            RefreshCommand = new AsyncCommand(Refresh);
            MijnVervangingen = new ObservableRangeCollection<Vervanging>();
            MijnVervangingen_tijdelijk = new ObservableRangeCollection<Vervanging>();
            ClearCommand = new Command(Clear);
        }


        async Task Refresh()
        {
            IsBusy = true;
            await Task.Delay(2000);
            MijnVervangingen.Clear();
            MijnVervangingen_tijdelijk.Clear();
            LoadMore();
            IsBusy = false;
        }

        async Task LoadMore()
        {
            try
            {
                string URL = "https://lesgeversmega.azurewebsites.net/";
                bool Response = false;
                var client = new HttpClient();
                var client2 = new HttpClient();
                string format = "d";
                //.ParseExact(vervanging_temp.datum_aangevraagd.ToString(), format, provider),
                CultureInfo provider = CultureInfo.InvariantCulture;
                int aantalvervangingen = 0;
                HttpResponseMessage response_count = await client.GetAsync(URL + "Vervangings/GetVervangingenCount");
                HttpResponseMessage response_Cleanup = await client2.GetAsync(URL + "Vervangings/Cleapup");
                string input = String.Empty;
                input = await response_count.Content.ReadAsStringAsync();
                char chara = '"';
                input = input.Replace(chara.ToString(), "");
                try
                {
                    aantalvervangingen = Int32.Parse(input);
                }
                catch (FormatException)
                {
                    Console.WriteLine($"Unable to parse '{input}'");
                }
                for (int i = 0; i < aantalvervangingen; i++)
                {

                    //Aim: Gather all vervangingen beloning to a person.
                    Vervanging vervanging = new Vervanging
                    {
                        Id = i,
                    };

                    var json = JsonConvert.SerializeObject(vervanging);
                    HttpContent httpContent = new StringContent(json);
                    httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                    //HttpResponseMessage response_vervanging = await client.PostAsync(URL + "Vervangings/GetVervanging", httpContent);
                    HttpResponseMessage response_vervanging = await client.PostAsync(URL + "Vervangings/GetVervanging", httpContent);
                    string json_temp = await response_vervanging.Content.ReadAsStringAsync();

                    if (json_temp == "")
                    {
                        aantalvervangingen++;
                    }
                    else
                    {
                        Vervanging vervanging_temp = JsonConvert.DeserializeObject<Vervanging>(json_temp);
                        string[] Datetimes = new string[8] { "dd/MM/yyyy HH:mm:ss", "M/dd/yyyy h:mm:ss tt", "MM/dd/yyyy hh:mm:ss tt", "dd/MM/yyyy hh:mm:ss tt", "MM/dd/yyyy HH:mm", "MM/dd/yyyy hh:mm tt", "MM/dd/yyyy H:mm", "MM/dd/yyyy h:mm tt" };
                        foreach (string str in Datetimes)
                        {
                            try
                            {
                                //Probeer elke format uit jammer genoeg!!
                                string format_database = str;
                                string aangevraagd = vervanging_temp.datum_aangevraagd.ToString();
                                string ingevuld = vervanging_temp.datum_ingevuld.ToString();
                                DateTime? date_aangevraagd = new DateTime?();
                                DateTime? date_ingevuld = new DateTime?();
                                TimeSpan? Tijd = new TimeSpan?();
                                if (ingevuld == "")
                                {
                                    date_aangevraagd = DateTime.ParseExact(aangevraagd, format_database, provider);
                                }
                                else
                                {
                                    date_aangevraagd = DateTime.ParseExact(aangevraagd, format_database, provider);
                                    date_ingevuld = DateTime.ParseExact(ingevuld, format_database, provider);
                                }
                                date_aangevraagd_second = (DateTime)date_aangevraagd;
                                Tijd = date_aangevraagd_second.TimeOfDay;
                                Tijd_second = (TimeSpan)Tijd;
                                MijnVervangingen_tijdelijk.Add(new Vervanging
                                {
                                    Id = vervanging_temp.Id,
                                    ingevuld = vervanging_temp.ingevuld,
                                    datum_aangevraagd = date_aangevraagd,
                                    datum_ingevuld = date_ingevuld,
                                    eigenaar = vervanging_temp.eigenaar,
                                    groep = vervanging_temp.groep,
                                    uur = Tijd,
                                    vervanger = vervanging_temp.vervanger,
                                    opmerking = vervanging_temp.opmerking
                                });
                                break;
                            }
                            catch (Exception e)
                            {
                                continue;
                            }
                        }
                    }

                }
                //Equalize both collections
                foreach (Vervanging item in MijnVervangingen_tijdelijk)
                {
                    MijnVervangingen.Add(item);
                }
                MijnVervangingen_tijdelijk.Clear();
                //Remove if not owner (only for 'MijnVervangingen')
                foreach (Vervanging item in MijnVervangingen)
                {
                    MijnVervangingen_tijdelijk.Add(item);
                    if (item.eigenaar != UserName_)
                    {
                        MijnVervangingen_tijdelijk.Remove(item);
                    }
                }
                MijnVervangingen.Clear();
                //Sort by date
                MijnVervangingen_tijdelijk.OrderByDescending(i => i.datum_aangevraagd); foreach (var item in MijnVervangingen_tijdelijk) { MijnVervangingen.Add(item); }
            }catch(Exception outOfRange)
            {

            }
            
        }
        void Clear()
        {
            MijnVervangingen.Clear();
        }
        private DateTime Date_aangevraagd_second;
        private TimeSpan tijd_second;

        public DateTime date_aangevraagd_second
        {
            get { return Date_aangevraagd_second; }
            set
            {
                Date_aangevraagd_second = value;
                OnPropertyChanged(nameof(Date_aangevraagd_second));
            }
        }
        public TimeSpan Tijd_second
        {
            get { return tijd_second; }
            set
            {
                tijd_second = value;
                OnPropertyChanged(nameof(tijd_second));
            }
        }

    }
}
