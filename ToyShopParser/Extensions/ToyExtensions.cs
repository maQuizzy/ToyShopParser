using System.Text;
using ToyShopParser.Entities;

namespace ToyShopParser.Extensions
{
    public static class ToyExtensions
    {
        public static string ToCSV(this Toy toy)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(toy.Region + ';');

            for(int i = 0; i < toy.Breadcrumbs.Length - 1; i++)
            {
                sb.Append(toy.Breadcrumbs[i] + '/');
            }

            if(toy.Breadcrumbs.Length > 0)
                sb.Append(toy.Breadcrumbs[toy.Breadcrumbs.Length - 1] + ';');

            sb.Append(toy.Name + ';');
            sb.Append(toy.Price + ";");
            sb.Append(toy.OldPrice + ";");
            sb.Append(toy.IsAvailable.ToString() + ';');
            sb.Append(toy.Link + ';');

            for (int i = 0; i < toy.PicLinks.Length - 1; i++)
            {
                sb.Append(toy.PicLinks[i] + ',');
            }

            if(toy.PicLinks.Length > 0)
                sb.Append(toy.PicLinks[toy.PicLinks.Length - 1]);

            return sb.ToString();
        }
    }
}
