using System.Drawing;
using System.Linq;

namespace TelegramBot
{
    class ChartDrawer
    {
        public Image GetChart(double[] arr)
        {
            int n = 510, m = 300;

            double arrMin = arr.Min();
            double koef = 200 / (arr.Max() - arrMin);

            double[] arrNew = arr.Select(t => t = (int)(250 - (t - arrMin) * koef)).ToArray();


            Bitmap bmp = new Bitmap(n, m);

            for (int i = 0; i < n; i++)
                for (int j = 0; j < m; j++)
                    if (i % 50 < 10 || j <= arrNew[i / 51])
                        bmp.SetPixel(i, j, Color.FromArgb(255, 255, 255, 255));
                    else
                        bmp.SetPixel(i, j, Color.FromArgb(255, 47, 194, 164));

            //bmp.Save("D:\\RandomImage.jpg");
            return bmp;
            
        }
    }
}
