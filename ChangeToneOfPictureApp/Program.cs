using ChangeToneOfPictureApp;

try
{
    if (args.Length == 0)
    {
        Console.WriteLine("No argument value. Terminate..");
        return;
    }

    var imagesWithData = FileHandling.ReadAllImagesFromArguments(args);

    var processedImages = ImageDataHandling.ProcessImages(imagesWithData);

    foreach (var processedImage in processedImages)
    {
        Console.WriteLine($"{processedImage} has been finished sucessfully!");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Fatal Exception: {ex}");
}