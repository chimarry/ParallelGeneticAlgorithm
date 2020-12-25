using Microsoft.Graphics.Canvas;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI;

namespace GeneticAlgorithm.Logic
{
    public class ImageMaker
    {
        private const int width = 1600;
        private const int height = 200;
        private const int dpi = 96;
        private static readonly (int x, int y) textLocation = (100, 100);
        private static readonly string BackgroundImagesSource = @"Assets\WhiteBackground.jpg";

        private StorageFolder folder;

        public async Task SaveResultAsImage(string jobId, string jobUnitName, string expression)
        {
            // await LoadFolder();
            await DrawAndSaveImage(jobId, jobUnitName, expression);
        }

        private async Task DrawAndSaveImage(string jobId, string jobUnitName, string expression)
        {
            StorageFile inputFile = await Package.Current.InstalledLocation.GetFileAsync(BackgroundImagesSource);
            BitmapDecoder imagedecoder;
            using (IRandomAccessStream imagestream = await inputFile.OpenAsync(FileAccessMode.Read))
            {
                imagedecoder = await BitmapDecoder.CreateAsync(imagestream);
            }
            CanvasDevice device = CanvasDevice.GetSharedDevice();
            CanvasRenderTarget renderTarget = new CanvasRenderTarget(device, width, height, dpi);
            using (CanvasDrawingSession session = renderTarget.CreateDrawingSession())
            {
                session.Clear(Colors.White);
                CanvasBitmap image = await CanvasBitmap.LoadAsync(device, inputFile.Path, dpi);
                session.DrawImage(image);
                session.DrawText(expression, new System.Numerics.Vector2(textLocation.x, textLocation.y), Colors.Black);
            }
            StorageFile file = await folder.CreateFileAsync($"GeneticAlgorithm_{jobId}_{jobUnitName}.jpg");
            using (IRandomAccessStream fileStream = await file.OpenAsync(FileAccessMode.ReadWrite))
            {
                await renderTarget.SaveAsync(fileStream, CanvasBitmapFileFormat.Png, 1f);
            }
        }

        public async Task LoadFolder()
        {
            if (folder == null)
            {
                FolderPicker openFolderPicker = new FolderPicker()
                {
                    SuggestedStartLocation = PickerLocationId.Desktop
                };
                openFolderPicker.FileTypeFilter.Add("*");
                folder = await openFolderPicker.PickSingleFolderAsync();
            }
        }
    }
}
