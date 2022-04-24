namespace ToyShopParser.Entities
{
    public class Toy
    {
        public string Region { get; set; }
        public string[] Breadcrumbs { get; set; }
        public string Name { get; set; }
        public uint Price { get; set; }
        public uint OldPrice { get; set; }
        public bool IsAvailable { get; set; }
        public string Link { get; set; }
        public string[] PicLinks { get; set; }
    }
}
