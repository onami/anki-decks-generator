using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using HtmlAgilityPack;

namespace deckgen
{
    class LingvoRu : Parser
    {
        string examplesPath = "http://lingvopro.abbyyonline.com/ru/Search/ExamplesPage?section=Public&srcLang=en&destLang=ru&caseSensitive=False&startIndex=";
        CardsStream reportStream = new CardsStream("./_report lingvo " + DateTime.Now.ToString("yyyy.MM.dd HH-mm-ss") + ".txt", 10000);

        string DownloadExamples(string word, int interval)
        {
            var client = new WebClient();
            client.Headers["Content-type"] = "application/x-www-form-urlencoded";
            client.Headers["X-Requested-With"] = "XMLHttpRequest";
            client.Proxy = null;

            var resultData = client.DownloadData(examplesPath + interval + "&text=" + word);
            return Encoding.UTF8.GetString(resultData);
        }

        public string Parse(HtmlNodeCollection examples, string labels)
        {
            string result = String.Empty;

            foreach (var example in examples)
            {
                var en = getInnerHtml(example.SelectSingleNode("td[@class='left']/p/span"));
                var ru = getInnerHtml(example.SelectSingleNode("td[@class='right']/p/span"));
                result += en + "\t" + ru + "\t" + labels + "\n";
            }

            return result;
        }

        public void ProcessWordlist(ref CardsStream stream, List<string> wordlist, string labels, int limit)
        {
            labels = (labels == "") ? "lingvo " + labels : "lingvo " + labels + " ";

            foreach (var word in wordlist)
            {
                var offset = 0;
                while (offset < limit)
                {
                    
                    var page = new HtmlDocument();
                    page.LoadHtml(DownloadExamples(word, offset));
                    reportStream.Write("Success. Page was parsed. Link: " + word + "\n");
                    var examples = page.DocumentNode.SelectNodes("//tr[@class='item first']");
                    if (examples == null)
                    {
                        break;
                    }

                    stream.Write(Parse(examples, labels + word));
                    count_ += examples.Count;

                    examples = page.DocumentNode.SelectNodes("//tr[@class='item']");
                    if (examples == null)
                    {
                        break;
                    }

                    stream.Write(Parse(examples, labels + word));
                    count_ += examples.Count;

                    offset += 5;
                }
            }

            reportStream.Write("Total: " + count_ + "\n");
            return;
        }
    }
}
