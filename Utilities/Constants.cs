namespace OpenAI_WebAPI.Utilities
{
    public static class Constants
    {
        public static string LocalImagePath = "D:\\images\\881358\\Filtered"; //replace the local folder path
        public static string PublicUrlImageBasePath = "https://content.images.com.au/images"; //replace the public url path

        public static List<Inventory> AllInventory = new List<Inventory>()
        {
            new Inventory() {AssetName = "Cooktop", IsDivision40 = true },
            new Inventory() {AssetName = "Oven", IsDivision40 = true },
            new Inventory() {AssetName = "Exhaust fan", IsDivision40 = true },
            new Inventory() {AssetName = "Microwave", IsDivision40 = true },
            new Inventory() {AssetName = "Light shades", IsDivision40 = true },
            new Inventory() {AssetName = "Vanity", IsDivision40 = true },
            new Inventory() {AssetName = "Bath tub", IsDivision40 = true },
            new Inventory() {AssetName = "Water Tank", IsDivision40 = false },
            new Inventory() {AssetName = "Air conditioning unit", IsDivision40 = true }
        };

        public static string BlobName = "881358/"; //Azure blob name
        public static string Container = "siteimages"; //Azure container
        //replace the connectionstring
        public static string AzureConnectionString = "DefaultEndpointsProtocol=https;AccountName=bmtimagesmissingdemo;AccountKey=mJC8i4ratbNygpUsDBn9wFbhxeALYzAxDsKdOp+HwTbmtTslA91IrVNF;EndpointSuffix=core.windows.net";


    }
}
