﻿//*********************************************************
//
// Copyright (c) Microsoft. All rights reserved.
// This code is licensed under the MIT License (MIT).
// THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
// ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
// IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
// PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
//
//*********************************************************

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Windows.Storage;
using Windows.Storage.Streams;

namespace AdventureWorks.Model
{
    /// <summary>
    /// Persistance layer for Trips created with Adventure Works. Writes out and retreives trips 
    /// from a simple xml format like
    /// <root>
    ///  <Trip>
    ///   <Destination></Destination>
    ///   <Description></Description>
    ///   <StartDate></StartDate>
    ///   <EndDate></EndDate>
    ///   <Notes></Notes>
    ///  </Trip>
    ///  <Trip>
    ///   ....
    ///  </Trip>
    /// </root>
    /// </summary>
    public class TripStore
    {
        /// <summary>
        /// Persist the loaded trips in memory for use in other parts of the application.
        /// </summary>
        private ObservableCollection<Trip> trips;

        public TripStore()
        {
            Trips = new ObservableCollection<Trip>();
        }

        /// <summary>
        /// Persisted trips, reloaded across executions of the application
        /// </summary>
        public ObservableCollection<Trip> Trips
        {
            get
            {
                return trips;
            }
            private set
            {
                trips = value;
            }
        }

        /// <summary>
        /// Load trips from a file on first-launch of the app. If the file does not yet exist,
        /// pre-seed it with several trips, in order to give the app demonstration data.
        /// </summary>
        public async Task LoadTrips()
        {
            StorageFolder folder = Windows.Storage.ApplicationData.Current.LocalFolder;
            this.trips.Clear();
            IStorageItem item = null;
            if (Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride != "zh-Hans-CN")
            {
                item = await folder.TryGetItemAsync("trips_en.xml");
                if (item == null)
                {
                    // Add some 'starter' trips
                    trips.Add(
                        new Trip()
                        {
                            Destination = "London",
                            Description = "Trip to London!",
                            StartDate = new DateTime(2015, 5, 5),
                            EndDate = new DateTime(2015, 5, 15)
                        });

                    trips.Add(
                        new Trip()
                        {
                            Destination = "Melbourne",
                            Description = "Trip to Australia",
                            StartDate = new DateTime(2016, 2, 2),
                            EndDate = new DateTime(2016, 5, 17),
                            Notes = "Bring Sunscreen!"
                        });
                    trips.Add(
                        new Trip()
                        {
                            Destination = "Las Vegas",
                            Description = "Trip to Las Vegas",
                            StartDate = new DateTime(2015, 7, 11),
                            EndDate = new DateTime(2015, 7, 19),
                            Notes = "Buy some new hiking boots"
                        });
                    await WriteTrips();
                    return;
                }
            }
            else
            {
                item = await folder.TryGetItemAsync("trips_zh.xml");
                if (item == null)
                {
                    // Add some 'starter' trips
                    trips.Add(
                        new Trip()
                        {
                            Destination = "伦敦",
                            Description = "伦敦之旅!",
                            StartDate = new DateTime(2015, 5, 5),
                            EndDate = new DateTime(2015, 5, 15)
                        });

                    trips.Add(
                        new Trip()
                        {
                            Destination = "墨尔本",
                            Description = "澳大利亚之旅",
                            StartDate = new DateTime(2016, 2, 2),
                            EndDate = new DateTime(2016, 5, 17),
                            Notes = "带防晒霜!"
                        });
                    trips.Add(
                        new Trip()
                        {
                            Destination = "拉斯维加斯",
                            Description = "拉斯维加斯之旅",
                            StartDate = new DateTime(2015, 7, 11),
                            EndDate = new DateTime(2015, 7, 19),
                            Notes = "买一些新的登山靴"
                        });
                    await WriteTrips();
                    return;
                }
            }

            // Load trips out of a simple XML format. For the purposes of this example, we're treating
            // parse failures as "no trips exist" which will result in the file being erased.
            if (item.IsOfType(StorageItemTypes.File))
            {
                StorageFile tripsFile = item as StorageFile;

                string tripXmlText = await FileIO.ReadTextAsync(tripsFile);

                try
                {
                    XElement xmldoc = XElement.Parse(tripXmlText);

                    var tripElements = xmldoc.Descendants("Trip");
                    foreach (var tripElement in tripElements)
                    {
                        Trip trip = new Trip();

                        var destElement = tripElement.Descendants("Destination").FirstOrDefault();
                        if (destElement != null)
                        {
                            trip.Destination = destElement.Value;
                        }

                        var descElement = tripElement.Descendants("Description").FirstOrDefault();
                        if (descElement != null)
                        {
                            trip.Description = descElement.Value;
                        }


                        var startElement = tripElement.Descendants("StartDate").FirstOrDefault();
                        if (startElement != null)
                        {
                            DateTime startDate;
                            if (DateTime.TryParse(startElement.Value, out startDate))
                            {
                                trip.StartDate = startDate;
                            }
                            else
                            {
                                trip.StartDate = null;
                            }
                        }

                        var endElement = tripElement.Descendants("EndDate").FirstOrDefault();
                        if (endElement != null)
                        {
                            DateTime endDate;
                            if (DateTime.TryParse(startElement.Value, out endDate))
                            {
                                trip.EndDate = endDate;
                            }
                            else
                            {
                                trip.EndDate = null;
                            }
                        }

                        var notesElement = tripElement.Descendants("Notes").FirstOrDefault();
                        if (notesElement != null)
                        {
                            trip.Notes = notesElement.Value;
                        }

                        Trips.Add(trip);
                    }
                }
                catch (XmlException ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.ToString());
                    return;
                }
            }
        }

        /// <summary>
        /// Delete a trip from the persistent trip store, and save the trips file.
        /// </summary>
        /// <param name="trip">The trip to delete. If the trip is not an existing trip in the store,
        /// will not have an effect.</param>
        public async Task DeleteTrip(Trip trip)
        {
            Trips.Remove(trip);
            await WriteTrips();
        }

        /// <summary>
        /// Add a trip to the persistent trip store, and saves the trips data file.
        /// </summary>
        /// <param name="trip">The trip to save or update in the data file.</param>
        public async Task SaveTrip(Trip trip)
        {
            if (!Trips.Contains(trip))
            {
                Trips.Add(trip);
            }

            await WriteTrips();
        }

        /// <summary>
        /// Write out a new XML file, overwriting the existing one if it already exists
        /// with the currently persisted trips. See class comment for basic format.
        /// </summary>
        private async Task WriteTrips()
        {
            StorageFolder folder = Windows.Storage.ApplicationData.Current.LocalFolder;

            XElement xmldoc = new XElement("Root");

            StorageFile tripsFile;
            IStorageItem item = null;

            if (Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride != "zh-Hans-CN")
            {
                item = await folder.TryGetItemAsync("trips_en.xml");
                if (item == null)
                {
                    tripsFile = await folder.CreateFileAsync("trips_en.xml");
                }
                else
                {
                    tripsFile = await folder.GetFileAsync("trips_en.xml");
                }
            }
            else
            {
                item = await folder.TryGetItemAsync("trips_zh.xml");
                if (item == null)
                {
                    tripsFile = await folder.CreateFileAsync("trips_zh.xml");
                }
                else
                {
                    tripsFile = await folder.GetFileAsync("trips_zh.xml");
                }
            }
            foreach (var trip in Trips)
            {
                xmldoc.Add(
                    new XElement("Trip",
                    new XElement("Destination", trip.Destination),
                    new XElement("Description", trip.Description),
                    new XElement("StartDate", trip.StartDate),
                    new XElement("EndDate", trip.EndDate),
                    new XElement("Notes", trip.Notes)));
            }

            await FileIO.WriteTextAsync(tripsFile, xmldoc.ToString());
        }
    }
}
