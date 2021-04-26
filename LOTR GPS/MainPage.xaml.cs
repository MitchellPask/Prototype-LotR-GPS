using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Essentials;

namespace LOTR_GPS
{
    public partial class MainPage : ContentPage
    {
        bool isGettingLocation;
        bool latLngStored;
        double tripDistance;

        public MainPage()
        {
            InitializeComponent();
        }

        async private void Button_Clicked(object sender, EventArgs e)
        {
            isGettingLocation = true;
            latLngStored = false;

            // Assigning a random value out of Lat/Lng range
            double prevLat = 300.0;
            double prevLng = 300.0;

            resultLocation.Text = $"Getting Location... {Environment.NewLine}";

            while (isGettingLocation)
            {
                // GeolocationAccuracy adjusts granularity of GPS lookup (ATTN: Be aware, higher accuracy uses more battery)
                // TimeSpan.FromMinutes(1) causes a timeout after 1 minute
                var result = await Geolocation.GetLocationAsync(new GeolocationRequest(GeolocationAccuracy.Default, TimeSpan.FromMinutes(1)));

                if(!latLngStored)
                {
                    prevLat = result.Latitude;
                    prevLng = result.Longitude;
                    latLngStored = true;
                }

                resultLocation.Text = $"Current Lat: {Math.Round(result.Latitude, 4)}, Current Lng: {Math.Round(result.Longitude, 4)} {Environment.NewLine}";

                tripDistance += Location.CalculateDistance(result.Latitude, result.Longitude, prevLat, prevLng, 0);

                distanceTravelled.Text = $"Distance travelled: {Math.Round(tripDistance, 4)}km"; // 0 for KM 1 for Miles

                prevLat = result.Latitude;
                prevLng = result.Longitude;

                // Delay of 1 second
                await Task.Delay(1000);
            }
        }

        private void Button_Clicked_1(object sender, EventArgs e)
        {
            isGettingLocation = false;
            latLngStored = false;

            tripDistance = 0.0;

            resultLocation.Text = $"Halting GPS usage. {Environment.NewLine}";
        }
    }
}
