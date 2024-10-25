using HtmlAgilityPack;

namespace get_npm_package_authors
{
  internal class Program
  {
    static async Task Main(string[] args)
    {
      var baseUrl = "https://www.npmjs.com";
      var client = new HttpClient();
      client.BaseAddress = new Uri(baseUrl);

      var response = await client.GetAsync(args[0]);
      var document = await response.Content.ReadAsStringAsync();
      var authors = GetAuthors(document);
      var urls = new List<string>();

      foreach (var author  in authors)
      {
        var authorName = await GetAuthorName(author, client);
        urls.Add(GetUrl(authorName, author, baseUrl));
      }

      Console.WriteLine(string.Join(", ", urls));
    }

    private static IEnumerable<string> GetAuthors(string document)
    {
      var result = new List<string>();
      var doc = new HtmlDocument();
      doc.LoadHtml(document);
      var nodes = doc.DocumentNode.SelectNodes("//ul[@aria-labelledby='collaborators']//a");
      foreach ( var item in nodes)
      {
        var author = item.GetAttributeValue("href", "");
        result.Add(author);
      }

      return result;
    }

    private static async Task<string> GetAuthorName(string url, HttpClient client)
    {
      var response = await client.GetAsync(url);
      var content = await response.Content.ReadAsStringAsync();
      var doc = new HtmlDocument();
      doc.LoadHtml(content);
      var userName = doc.DocumentNode.SelectSingleNode("//h1[@class='b219ea1a black tracked-tight fw6 mv1']").InnerText;
      if (string.IsNullOrEmpty(userName))
        userName = doc.DocumentNode.SelectSingleNode("//div[@class='eaac77a6 mv2']").InnerText;

      return userName;
    }

    private static string GetUrl(string userName, string authorUrl, string baseUrl)
    {
      return $"[{userName}]({baseUrl + authorUrl})";
    }
  }
}
