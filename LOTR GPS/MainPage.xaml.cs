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
        double prevLat; // Last known latitudinal coordinates
        double prevLng; // Last known longitudinal coordinates
        double distanceTravelled; // Distance currently travelled
        const double tripDistance = 5; // Distance for current leg of the journey
        const double journeyDistance = 100; // Distance for complete journey
        int selectedAccuracyLevel = 2; // User selected GPS accuracy (starts as: Default)
        readonly string[] accuracyTitle = { "Lowest", "Low", "Default", "High", "Best" };

        public MainPage()
        {
            InitializeComponent();
        }

        // Triggered by the Get Location button on the main screen
        private void Get_Location_Clicked(object sender, EventArgs e)
        {
            // Allow GPS usage
            isGettingLocation = true;
            // Call Geolocation method to start distance gathering
            GetDistance();
        }

        // Triggered by the Stop Getting Location button on the main screen
        private void Halt_GPS(object sender, EventArgs e)
        {
            // Turn off GPS
            isGettingLocation = false;

            // Reset current distance travelled
            // TODO: Add a reset distance button
            distanceTravelled = 0.0;
        }

        private void Slider_ValueChanged(object sender, ValueChangedEventArgs e)
        {
            // Round the slider value to the nearest int
            var newStep = Math.Round(e.NewValue);
            // Snap the slider to the new rounded value
            ((Slider)sender).Value = newStep;
            // Set / update selected accuracy for out of method use
            selectedAccuracyLevel = (int)newStep;
            // Alter title to reflect new user selection (typecast newStep to an int to access array)
            sliderTitle.Text = $"GPS Accuracy: {accuracyTitle[selectedAccuracyLevel]}";

            Console.WriteLine(accuracyTitle[selectedAccuracyLevel] + " accuracy selected");
        }

        async private void GetDistance()
        {
            var selectedAccuracy = GeolocationAccuracy.Default;

            switch (selectedAccuracyLevel)
            {
                case 0:
                    selectedAccuracy = GeolocationAccuracy.Lowest;
                    break;                
                case 1:
                    selectedAccuracy = GeolocationAccuracy.Low;
                    break;
                case 2:
                    selectedAccuracy = GeolocationAccuracy.Default;
                    break;                
                case 3:
                    selectedAccuracy = GeolocationAccuracy.High;
                    break;              
                case 4:
                    selectedAccuracy = GeolocationAccuracy.Best;
                    break;
                default:
                    selectedAccuracy = GeolocationAccuracy.Default;
                    break;
            }

            // Get initial device location
            var result = await Geolocation.GetLocationAsync(new GeolocationRequest(selectedAccuracy, TimeSpan.FromMinutes(1)));

            // Set initial device location for comparison
            prevLat = result.Latitude;
            prevLng = result.Longitude;

            while (isGettingLocation)
            {
                // GeolocationAccuracy adjusts granularity of GPS lookup (ATTN: Be aware, higher accuracy uses more battery)
                // TimeSpan.FromMinutes(1) causes a timeout after 1 minute
                result = await Geolocation.GetLocationAsync(new GeolocationRequest(selectedAccuracy, TimeSpan.FromMinutes(1)));

                // Check that location recieved is not null and isn't from a fake location
                if (result != null && !result.IsFromMockProvider)
                {
                    // Display most recent resulting Lat and Long coordinates on the main screen in the resultLocation label
                    resultLocation.Text = $"Current Lat: {Math.Round(result.Latitude, 4)}, Current Lng: {Math.Round(result.Longitude, 4)}";

                    // Add recent distance travelled to current trip distance total
                    // Location.CalculateDistance is using the Haversine formula through Xamarin.Essentials
                    distanceTravelled += Location.CalculateDistance(result.Latitude, result.Longitude, prevLat, prevLng, DistanceUnits.Kilometers);

                    // Display most recent total distance travelled on the main screen in the distanceTravelledLabel label
                    distanceTravelledLabel.Text = $"Distance travelled: {Math.Round(distanceTravelled, 4)}km";

                    UpdateProgress();

                    // Update previous Lat and Long coordinate values
                    prevLat = result.Latitude;
                    prevLng = result.Longitude;
                }

                // Delay of 1 second to not over-request geolocation
                await Task.Delay(1000);
            }
            try
            {
                isGettingLocation = true; // Turn on GPS
            }
            catch (FeatureNotSupportedException fnsEx)
            {
                // Handle not supported on device exception
                Console.WriteLine(fnsEx.Message);
            }
            catch (FeatureNotEnabledException fneEx)
            {
                // Handle not enabled on device exception
                Console.WriteLine(fneEx.Message);
            }
            catch (PermissionException pEx)
            {
                // Handle permission exception
                Console.WriteLine(pEx.Message);
            }
            catch (Exception ex)
            {
                // Unable to get location
                Console.WriteLine(ex.Message);
            }
        }

        async private void UpdateProgress()
        {
            // Get percentage completion from current distance travelled and the trips total distance           
            double tripComp = distanceTravelled / tripDistance;
            // Updates Xamarin ProgressBar with new completion values
            await tripProgressBar.ProgressTo(tripComp, 250, Easing.Linear);

            tripDistanceLabel.Text = $"{Math.Round(distanceTravelled, 4)}km of {tripDistance} completed";
            tripPercentLabel.Text = $"Percent Complete: {tripComp * 100} % completed";

            // Print percentage for progress bar comparison
            //Console.WriteLine(((tripComp) * 100) + "% completed");

            // Get percentage completion from current distance travelled and the trips total distance           
            double journeyComp = distanceTravelled / journeyDistance;
            // Updates Xamarin ProgressBar with new completion values
            await journeyProgressBar.ProgressTo(journeyComp, 250, Easing.Linear);

            journeyDistanceLabel.Text = $"{Math.Round(distanceTravelled, 4)}km of {journeyDistance} completed";
            journeyPercentLabel.Text = $"Percent Complete: {journeyComp * 100} % completed";

            // Print percentage for progress bar comparison
            //Console.WriteLine(((journeyComp) * 100) + "% completed");
        }
    }
}
