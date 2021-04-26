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
        bool isGettingLocation; // Allows or denies Xamarin Geolocation Usage
        bool latLngStored; // Check for previous Lat and Long Values
        double distanceTravelled; // Distance currently travelled
        double tripDistance = 5; // Distance for complete journey

        public MainPage()
        {
            InitializeComponent();
        }

        // Triggered by the Get Location button on the main screen
        async private void Get_Location_Clicked(object sender, EventArgs e)
        {
            isGettingLocation = true; // Turn on GPS
            latLngStored = false; // 

            // Assigning an initial random value out of Lat/Lng range
            // TODO: Fix logic to not need hardcoded previous values. Use latLngStored boolean above.
            double prevLat = 300.0;
            double prevLng = 300.0;

            //resultLocation.Text = $"Getting Location... {Environment.NewLine}";

            while (isGettingLocation)
            {
                // GeolocationAccuracy adjusts granularity of GPS lookup (ATTN: Be aware, higher accuracy uses more battery)
                // TimeSpan.FromMinutes(1) causes a timeout after 1 minute
                var result = await Geolocation.GetLocationAsync(new GeolocationRequest(GeolocationAccuracy.Default, TimeSpan.FromMinutes(1)));

                // Check that location recieved is not null and isn't from a fake location
                if (result != null && !result.IsFromMockProvider)
                {
                    // Check and fill the last Lat and Long coordinates seen
                    if (!latLngStored)
                    {
                        prevLat = result.Latitude;
                        prevLng = result.Longitude;
                        latLngStored = true;
                    }

                    // Display most recent resulting Lat and Long coordinates on the main screen in the resultLocation label
                    resultLocation.Text = $"Current Lat: {Math.Round(result.Latitude, 4)}, Current Lng: {Math.Round(result.Longitude, 4)} {Environment.NewLine}";

                    // Add recent distance travelled to current trip distance total
                    // Location.CalculateDistance is using the Haversine formula through Xamarin.Essentials
                    distanceTravelled += Location.CalculateDistance(result.Latitude, result.Longitude, prevLat, prevLng, DistanceUnits.Kilometers);

                    // Display most recent total distance travelled on the main screen in the distanceTravelledLabel label
                    distanceTravelledLabel.Text = $"Distance travelled: {Math.Round(distanceTravelled, 4)}km";

                    // Get percentage completion from current distance travelled and the trips total distance           
                    double newComp = distanceTravelled / tripDistance;
                    // Print percentage for progress bar comparison
                    Console.WriteLine(((newComp) * 100) + "% completed");
                    // Updates Xamarin ProgressBar with new completion values
                    await progressBar.ProgressTo(newComp, 250, Easing.Linear);

                    // Update previous Lat and Long coordinate values
                    prevLat = result.Latitude;
                    prevLng = result.Longitude;
                }

                // Delay of 1 second to not over-request geolocation
                await Task.Delay(1000);
            }

        }

        // Triggered by the Stop Getting Location button on the main screen
        private void Halt_GPS(object sender, EventArgs e)
        {
            // Turn off GPS
            isGettingLocation = false;
            // Ignore previous Lat and Long coordinate values
            latLngStored = false; 

            // Reset current distance travelled
            // TODO: Add a reset distance button
            distanceTravelled = 0.0; 

            // Hide current coordinates with halting message
            //resultLocation.Text = $"Halting GPS usage. {Environment.NewLine}";
        }

    }
}
