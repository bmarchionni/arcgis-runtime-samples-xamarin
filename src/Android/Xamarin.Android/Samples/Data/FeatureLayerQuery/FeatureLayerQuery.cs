//Copyright 2015 Esri.
//
//Licensed under the Apache License, Version 2.0 (the "License");
//you may not use this file except in compliance with the License.
//You may obtain a copy of the License at
//
//http://www.apache.org/licenses/LICENSE-2.0
//
//Unless required by applicable law or agreed to in writing, software
//distributed under the License is distributed on an "AS IS" BASIS,
//WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//See the License for the specific language governing permissions and
//limitations under the License.

using Android.App;
using Android.OS;
using Android.Widget;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using System;
using System.Drawing;
using System.Linq;

namespace ArcGISRuntimeXamarin.Samples.FeatureLayerQuery
{
    [Activity]
    public class FeatureLayerQuery : Activity
    {


        //Create an hold reference to variables that will be required through the app life cycle
        private string _statesUrl = "http://sampleserver6.arcgisonline.com/arcgis/rest/services/USA/MapServer/2"; 
        private MapView _myMapView = new MapView();      
        private EditText _queryTextBox;
        private ServiceFeatureTable _featureTable;
        private FeatureLayer _featureLayer;
        

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            Title = "Feature layer  query";
            
            // Create the UI, setup the control references 
            CreateLayout();
            // Execute initialization 
            Initialize();
        }


        private void Initialize()
        {

            // Create new Map with basemap
            var myMap = new Map(Basemap.CreateTopographic());

            // Create and set initial map location
            var initialLocation = new MapPoint(
                -11000000, 5000000, SpatialReferences.WebMercator);
                myMap.InitialViewpoint = new Viewpoint(initialLocation, 100000000);

            // Create feature table using a url
            _featureTable = new ServiceFeatureTable(new Uri(_statesUrl));

            // Create feature layer using this feature table
            _featureLayer = new FeatureLayer(_featureTable);

            // Set the Opactity of the Feature Layer
            _featureLayer.Opacity = 0.6;

            // Create a new renderer for the States Feature Layer
            SimpleLineSymbol lineSymbol = new SimpleLineSymbol(
                SimpleLineSymbolStyle.Solid, Color.Black, 1); 
            SimpleFillSymbol fillSymbol = new SimpleFillSymbol(
                SimpleFillSymbolStyle.Solid, Color.Yellow, lineSymbol);

            // Set States feature layer renderer
            _featureLayer.Renderer = new SimpleRenderer(fillSymbol);

            // Add feature layer to the map
            myMap.OperationalLayers.Add(_featureLayer);

            // Assign the map to the MapView
            _myMapView.Map = myMap;

        }


        private void CreateLayout()
        {

            // Create a new vertical layout for the app
            var layout = new LinearLayout(this) { Orientation = Orientation.Vertical };

            // Create new Text box that will take the query parameter
            _queryTextBox = new EditText(this);

            // Create Button that will start the Feature Query
            var queryButton = new Button(this);
            queryButton.Text = "Query";
            queryButton.Click += OnQueryClicked;

            // Add TextBox to the layout  
            layout.AddView(_queryTextBox);

            // Add Button to the layout  
            layout.AddView(queryButton);

            // Add the map view to the layout
            layout.AddView(_myMapView);

            // Show the layout in the app
            SetContentView(layout);

        }


        private void OnQueryClicked(object sender, EventArgs e)
        {

            // Remove any previous feature selections that may have been made 
            _featureLayer.ClearSelection();

            //Begin query process 
            QueryStateFeature(_queryTextBox.Text);

        }


        private async void QueryStateFeature(string stateName)
        {

            // create dialog to display alert information
            AlertDialog.Builder alert = new AlertDialog.Builder(this);

            // Create a query param that will be used to Query the feature table  
            QueryParameters queryParams = new QueryParameters();

            // Construct and assign the where clause that will be used to query the feature table 
            queryParams.WhereClause = "upper(STATE_NAME) LIKE '%" + (_queryTextBox.Text.ToUpper()) + "%'";

            try {

                // Query the feature table 
                FeatureQueryResult queryResult = await _featureTable.QueryFeaturesAsync(queryParams);

                // Cast the QueryResult to a List so the results can be interrogated
                var features = queryResult.ToList();

                if (features.Any())
                {

                    // Get the first feature returned in the Query result 
                    Feature feature = features[0];

                    // Add the returned feature to the collection of currently selected features
                    _featureLayer.SelectFeature(feature);

                    // Zoom to the extent of the newly selected feature
                    await _myMapView.SetViewpointGeometryAsync(feature.Geometry.Extent);

                }
                else
                {
                    alert.SetTitle("Alert");
                    alert.SetMessage("State Not Found! Add a valid state name");
                    alert.Show();
                }
            }
            catch (Exception ex)
            {
                alert.SetTitle("Sample Error");
                alert.SetMessage(ex.Message);
                alert.Show();
            }                   
        }


    }
}