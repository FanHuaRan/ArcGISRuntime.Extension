using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ArcGIS.Runtime.Extension.Layers
{
    /// <summary>
    /// 高德地图
    /// </summary>
    public class AmapTiledLayer : TiledLayer
    {
        private string TemplateUri = @"http://wprd0{0}.is.autonavi.com/appmaptile?style=7&ltype=11&x={1}&y={2}&z={3}";
        public MapType LayerType
        {
            set
            {
                var layerType = value;
                SetupMapLayerType(layerType);
                OnPropertyChanged("LayerType");
            }
        }

        private void SetupMapLayerType(MapType layerType)
        {
            switch (layerType)
            {
                case MapType.Map:
                    TemplateUri = @"http://wprd0{0}.is.autonavi.com/appmaptile?style=7&x={1}&y={2}&z={3}";
                    break;
                case MapType.Satellite:
                    TemplateUri = @"http://webst0{0}.is.autonavi.com/appmaptile?style=6&x={1}&y={2}&z={3}";
                    break;
                case MapType.OverlayFeature:
                    TemplateUri = @"http://wprd0{0}.is.autonavi.com/appmaptile?style=8&ltype=11&x={1}&y={2}&z={3}";
                    break;
                case MapType.OverlayPOI:
                    TemplateUri = @"http://wprd0{0}.is.autonavi.com/appmaptile?style=8&ltype=12&x={1}&y={2}&z={3}";
                    break;
                default:
                    TemplateUri = @"http://wprd0{0}.is.autonavi.com/appmaptile?style=7&x={1}&y={2}&z={3}";
                    break;
            }
        }

        private Random RandomHost = new Random();

        public AmapTiledLayer()
        {

        }
        public AmapTiledLayer(MapType mapType, string templateUri = null)
        {
            if (templateUri == null)
            {
                SetupMapLayerType(mapType);
            }
            else
            {
                TemplateUri = templateUri;
            }
        }
        protected override Task<ImageTileData> GetTileDataAsync(int level, int row, int column, CancellationToken token)
        {
            return Task.Run(async () =>
            {
                Uri url = await this.GetTileUriAsync(level, row, column, token);
                TiledLayer.ImageTileData img = new TiledLayer.ImageTileData
                {
                    Column = column,
                    Level = level,
                    Row = row
                };
                using (HttpClient client = new HttpClient())
                {
                    try
                    {
                        var response = await client.GetAsync(url);
                        response.EnsureSuccessStatusCode();
                        var data = await response.Content.ReadAsByteArrayAsync();
                        img.ImageData = data;
                    }
                    catch (Exception)
                    { }
                }
                return img;
            });
        }
        protected override Task<TiledLayerInitializationInfo> OnInitializeTiledLayerRequestedAsync()
        {
            return Task.Run(() =>
            {
                var extent = new Envelope(8683687.93695427,2379976.95194823,14945347.5840909,5762088.12795044,SpatialReferences.WebMercator);
                //Envelope extent = new Envelope(-20037508.3427892, -20037508.3427892, 20037508.3427892, 20037508.3427892, SpatialReferences.WebMercator);
                IEnumerable<Lod> lods = null;
                if (lods == null)
                {
                    double resolution = 156543.03392804062;
                    double scale = (resolution * 96.0) * 39.37;
                    Lod[] lodArray = new Lod[0x13];
                    for (int i = 0; i < lodArray.Length; i++)
                    {
                        lodArray[i] = new Lod(resolution, scale);
                        resolution /= 2.0;
                        scale /= 2.0;
                    }
                    lods = lodArray;
                }
                int width = 256;
                int height = 256;
                float dpi = 96f;
                double originX = -20037508.3427892;
                double originY = 20037508.3427892;
                return new TiledLayerInitializationInfo(width, height, originX, originY, extent, dpi, lods);
            });
        }
        protected Task<Uri> GetTileUriAsync(int level, int row, int column, CancellationToken token)
        {
            return Task.Run(() =>
            {
                string uriString = string.Format(this.TemplateUri, RandomHost.Next(1, 4), column, row, level);
                return new Uri(uriString, UriKind.Absolute);
            });
        }

        public enum MapType { Map, Satellite, OverlayFeature, OverlayPOI }
    }
}
