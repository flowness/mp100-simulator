using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Net.Http;
using System.Net.Http.Headers;
using Windows.Data.Json;
using Windows.UI.Popups;
using System.Text;
using System.Threading.Tasks;
// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Simulator
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        HttpClient httpClient;
        public MainPage()
        {
            this.InitializeComponent();
            StartDate.Date = DateTime.Now;
            StopDate.Date = DateTime.Now;
            StopTime.Time = DateTime.Now.TimeOfDay;
            StartTime.Time = DateTime.Now.TimeOfDay;

        }

        private async Task PostHttp()
        {
            DateTime StartDateTime;
            DateTime StopDateTime;
            int burstTime = 0;
            double flow = 0;
            double volume = 0;

            statusText.Text = "";
            Random rnd = new Random();
            StartDateTime = new DateTime(StartDate.Date.Value.Year, StartDate.Date.Value.Month, StartDate.Date.Value.Day, StartTime.Time.Hours, StartTime.Time.Minutes, StartTime.Time.Seconds);
            StopDateTime = new DateTime(StopDate.Date.Value.Year, StopDate.Date.Value.Month, StopDate.Date.Value.Day, StopTime.Time.Hours, StopTime.Time.Minutes, StopTime.Time.Seconds);
            for (; StartDateTime <= StopDateTime; StartDateTime=StartDateTime.Add(new TimeSpan(0, 1,0)))
            {
                httpClient = new HttpClient
                {
                    BaseAddress = new Uri(@"https://yg8rvhiiq0.execute-api.eu-west-1.amazonaws.com")
                };
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpClient.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("utf-8"));

                string endpoint = @"/poc/measurement";

                try
                {
                    JsonObject Data = new JsonObject();
                    Data["measurementDate"] = JsonValue.CreateStringValue(StartDateTime.ToString("yyyyMMddTHHmmssZ"));
                    //Data.Add("measurementDate", DateTime.Now.ToString("yyMMddTHHmmssZ"));
                    Data.Add("measurementInterval", JsonValue.CreateNumberValue(60));
                    Data.Add("moduleSN", JsonValue.CreateStringValue(((ListBoxItem)moduleSnList.SelectedItem).Content.ToString()));
                    if (burstTime == 0)
                    {
                        flow = 0;
                        if (rnd.Next(100) > 80)
                        {
                            burstTime = rnd.Next(1, 100);
                        }
                    }
                    else
                    {
                        burstTime--;
                        switch (rnd.Next(2))
                        {
                            case 0:
                                flow += rnd.NextDouble();
                                break;
                            case 1:
                                flow -= rnd.NextDouble();
                                if (flow < 0) flow = 0;
                                break;
                            case 2:
                                flow = 0;
                                break;
                        }
                    }

                    volume += flow;
                    Data.Add("measurementAmount", JsonValue.CreateNumberValue(flow)); //RNG
                    Data.Add("totalCount", JsonValue.CreateNumberValue(volume)); //RNG

                    statusText.Text += Data.ToString()+"\r\n";
                    HttpContent content = new StringContent(((JsonObject)Data).ToString(), Encoding.UTF8, "application/json");
                    HttpResponseMessage response = await httpClient.PostAsync(endpoint, content);

                    if (response.IsSuccessStatusCode)
                    {
                        statusText.Text += await response.Content.ReadAsStringAsync()+ "\r\n";
                        //do something with json response here
                    }
                }
                catch (Exception)
                {
                    //Could not connect to server
                    //Use more specific exception handling, this is just an example
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (moduleSnList.SelectedIndex != -1)
            {
                PostHttp();
            }
            else
            {
                new MessageDialog("please select a device SN").ShowAsync();
            }
        }

        private void StartDate_DateChanged(CalendarDatePicker sender, CalendarDatePickerDateChangedEventArgs args)
        {
            if (StartDate.Date > StopDate.Date)
                StopDate.Date = StartDate.Date;
        }

        private void StopDate_DateChanged(CalendarDatePicker sender, CalendarDatePickerDateChangedEventArgs args)
        {
            if (StartDate.Date > StopDate.Date)
                StartDate.Date = StopDate.Date ;
        }

        private void StartTime_TimeChanged(object sender, TimePickerValueChangedEventArgs e)
        {
            if (StartTime.Time > StopTime.Time)
                StopTime.Time = StartTime.Time;

        }

        private void StopTime_TimeChanged(object sender, TimePickerValueChangedEventArgs e)
        {
            if (StartTime.Time > StopTime.Time)
                StartTime.Time = StopTime.Time;

        }

    }
}
