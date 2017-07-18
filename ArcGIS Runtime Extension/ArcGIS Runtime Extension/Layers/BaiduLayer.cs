using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ArcGIS.Runtime.Extension.Layers
{
    /// <summary>
    /// 百度地图扩展 瓦片图层 
    /// 未成功，图片拼接不上
    /// </summary>
    public class BaiduLayer : TiledLayer
    {
        private string templateUri = @"http://online2.map.bdimg.com/onlinelabel/?qt=tile&x={0}&y={1}&z={2}&styles=pl&udt=20170712&scaler=1&p=1";

        protected override Task<TiledLayer.ImageTileData> GetTileDataAsync(int level, int row, int column, System.Threading.CancellationToken cancellationToken)
        {
            return Task.Run(async () =>
            {
                var url = await this.GetTileUriAsync(level, row, column, cancellationToken);
                TiledLayer.ImageTileData img = new TiledLayer.ImageTileData
                {
                    Column = column,
                    Level = level,
                    Row = row
                };
                using (var client = new HttpClient())
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

        private Task<Uri> GetTileUriAsync(int level, int row, int column, System.Threading.CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                var uriString = string.Format(this.templateUri, column, row, (level + 2));
                return new Uri(uriString, UriKind.Absolute);
            });
        }

        protected override Task<TiledLayerInitializationInfo> OnInitializeTiledLayerRequestedAsync()
        {
            return Task.Run(() =>
            {
                //var extent = new Envelope(8683687.93695427, 2379976.95194823, 14945347.5840909, 5762088.12795044, SpatialReferences.WebMercator);
                Envelope extent = new Envelope(-20037508.3427892, -20037508.3427892, 20037508.3427892, 20037508.3427892, SpatialReferences.WebMercator);
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
    }
}
