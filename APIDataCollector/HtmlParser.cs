using HtmlAgilityPack;

namespace DataCollector
{
    public class HtmlParser
    {
        public HtmlNodeCollection getData(string url, string nodePattern)
        {
            HtmlWeb web = new HtmlWeb();
            HtmlDocument htmlDoc = web.Load(url);

            HtmlNodeCollection nodeCollection = htmlDoc.DocumentNode.SelectNodes(nodePattern);

            return nodeCollection;
        }

    }
}