using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Microsoft.Extensions.Configuration;

namespace Labb2ImageAnalyzerAzure
{
    // Angelica Lindström .NET23 - Labb 2 Bildtjänster i Azure AI
    // Applikation med Azure AI Services ( Vision )

    // Användaren får skicka in en bild via URL som ska analyseras, sparas i string imageUrl,
    // när den sparats får användaren välja vilken storlek en miniatyrbild sedan ska sparas i (höjd och bredd). 
    // Efter det får användaren välja var minityrbilden ska sparas
    // på Skrivbordet, i Aktuell Arbetskatalog eller hårdkodad väg i mitt repo(byt ut den mot din sökväg till ditt repo).
    // Bilden Analyseras via Azure AI services (cognitive service) som innefattar även Computer Vision.
    // Användaren får en beskrivning av vad bilden visar samt relevanta taggar.
    // Bilden sparas sedan som en Thumbnail/miniatyrbild på vald plats.
    // Programmet körs tills användaren skriver in 'exit'.

    // Exempel bild URL länkar, testa gärna en annan
    // https://cdn.pixabay.com/photo/2018/04/26/12/14/travel-3351825_1280.jpg
    // https://cdn.pixabay.com/photo/2013/02/01/18/14/url-77169_1280.jpg
    // https://cdn.pixabay.com/photo/2013/03/01/18/40/crispus-87928_1280.jpg


    internal class Program
    {
        private static ComputerVisionClient cvClient;

        static async Task Main(string[] args)
        {


            // Ladda konfigurationen från appsettings.json
            IConfigurationBuilder builder = new ConfigurationBuilder().AddJsonFile("appsettings.json");
            IConfigurationRoot configuration = builder.Build();
            string cogSvcEndpoint = configuration["CognitiveServicesEndpoint"];
            string cogSvcKey = configuration["CognitiveServiceKey"];

            // Autentisera Azure AI Vision klient
            ApiKeyServiceClientCredentials credentials = new ApiKeyServiceClientCredentials(cogSvcKey);
            cvClient = new ComputerVisionClient(credentials) { Endpoint = cogSvcEndpoint };


            Console.WriteLine("Du analyzerar bilder med Nutri! (för att avsluta skriv : 'exit')\n");


            while (true)
            {
                // hämta bild-URL från användaren
                Console.WriteLine("Ange URL för bilden som ska analyseras och sparas som miniatyrbild:");
                string imageUrl = Console.ReadLine();

                // avsluta pprogrammet
                if (imageUrl.ToLower() == "exit")
                {
                    break;
                }
                else
                {
                    // vilken storlek på Thumbnail
                    Console.WriteLine("Ange bredd för miniatyrbilden:");
                    int width = int.Parse(Console.ReadLine());

                    Console.WriteLine("Ange höjd för miniatyrbilden:");
                    int height = int.Parse(Console.ReadLine());

                    // var ska bilden sparas?
                    Console.WriteLine("Var vill du spara bilden?");
                    Console.WriteLine("1. På skrivbordet 2.Aktuell Arbetskatalog 3. Hårdkodad sökväg till mitt repo");
                    Console.Write("skriv en siffra : ");
                    int whereToSaveThumbnail = int.Parse(Console.ReadLine());

                    // analysera bilden
                    await AnalyzeImage(imageUrl);

                    // skapa thumbnail/miniatyrbild utefter användarens val av storlek
                    await GetThumbnail(imageUrl, width, height, whereToSaveThumbnail);
                }
            }
        }



        // metod för att analysera innehållet i bilden
        static async Task AnalyzeImage(string imageUrl)
        {
            Console.WriteLine($"Analyserar bild från URL: {imageUrl}");

            // Specificera vilka funktioner som ska hämtas
            List<VisualFeatureTypes?> features = new List<VisualFeatureTypes?>()
            {
                VisualFeatureTypes.Description,
                VisualFeatureTypes.Tags,
                VisualFeatureTypes.Categories,
                VisualFeatureTypes.Brands,
                VisualFeatureTypes.Objects,
                VisualFeatureTypes.Adult
            };

            // Utför bildanalysen
            var analysis = await cvClient.AnalyzeImageAsync(imageUrl, features);


            // Visa beskrivningar + tillförlitlighet
            foreach (var caption in analysis.Description.Captions)
            {
                Console.WriteLine($"Beskrivning: {caption.Text} (tillförlitlighet: {caption.Confidence:P})");
            }


            // Visa taggar + tillförlitlighet
            if (analysis.Tags.Count > 0)
            {
                Console.WriteLine("Taggar:");
                foreach (var tag in analysis.Tags)
                {
                    Console.WriteLine($" - {tag.Name} (tillförlitlighet: {tag.Confidence:P})");
                }
                Console.WriteLine("\n");
            }

        }



        // metod för att skapa thumbnail/miniatyrbild
        static async Task GetThumbnail(string imageUrl, int width, int height, int whereToSaveThumbnail)
        {
            Console.WriteLine("Gererar thumbnail miniatyrbild");

            string thumbnailFileName = null;

            // om spara på skrivbord
            if (whereToSaveThumbnail == 1)
            {
                // Sparar thumbnail på skrivbordet
                thumbnailFileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), $"thumbnail_{width}x{height}.png");
            }

            // om spara på aktuell arbetskatalog
            else if (whereToSaveThumbnail == 2)
            {
                // Hämta aktuell arbetskatalog
                string currentDirectory = Directory.GetCurrentDirectory();
                Console.WriteLine($"Nuvarande arbetskatalog: {currentDirectory}");

                // Spara miniatyrbilden i aktuell arbetskatalog
                thumbnailFileName = Path.Combine(currentDirectory, $"thumbnail_{width}x{height}.png");
            }

            // om spara på hårdkodad sökväg till mitt personliga repo
            else if (whereToSaveThumbnail == 3)
            {
                // hårdkodat sökvägen där jag vill att bilden/filen ska sparas
                string saveDirectory = @"C:\Users\nutri\source\repos\Labb2ImageAnalyzerAzure\Labb2ImageAnalyzerAzure";

                // Kontrollera om mappen finns, annars skapa den
                if (!Directory.Exists(saveDirectory))
                {
                    Console.WriteLine("Tyvärr existerar inte mappen du vill spara miniatyrbilden i, testa igen!");
                    return;
                }

                // Spara miniatyrbilden i min specifika mapp som jag valt sökväg till
                thumbnailFileName = Path.Combine(saveDirectory, $"thumbnail_{width}x{height}.png");
            }

            else
            {
                Console.WriteLine("ogiltligt alternativ var miniatyrbilden ska sparas");
                return;
            }

            // försöker ladda ner bild och spara
            try
            {
                // Ladda ner Bild ifrån URL
                using (var httpClient = new HttpClient())
                using (var imageStream = await httpClient.GetStreamAsync(imageUrl))
                {
                    // Generera thumbnail info
                    var thumbnailStream = await cvClient.GenerateThumbnailInStreamAsync(width, height, imageStream, true);

                    // spara miniatyrbilden
                    using (Stream thumbnailFile = File.Create(thumbnailFileName))
                    {
                        await thumbnailStream.CopyToAsync(thumbnailFile);  // säkerställer asynkron kopiering
                    }

                    Console.WriteLine($"Thumbnail miniatyrbild är sparad : {thumbnailFileName}");
                }
            }
            // fånga alla exeptions
            catch (Exception ex)
            {
                Console.WriteLine($"Problem vid Thumbnail generering : Felmeddelande: {ex.Message}");
            }
        }
    }
}
