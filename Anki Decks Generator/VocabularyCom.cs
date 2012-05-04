using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using HtmlAgilityPack;
using System.Text;

namespace deckgen
{
    class VocabularyCom : Parser
    {
        public int count;
        WebClient client = new WebClient();
        Hashtable pages = new Hashtable();
        string examplesPath = "http://corpus.vocabulary.com/examples.json?query=";
        string definitionsPath = "http://www.vocabulary.com/definition/";

        CardsStream reportStream = new CardsStream("./_report vocabulary.com " + DateTime.Now.ToString("yyyy.MM.dd HH-mm-ss") + ".txt", 10000);

        string SafeTrim(string str)
        {
            if (str == null) return "";

            return str.Trim();
        }

        public void ProcessWordlist(ref CardsStream stream, List<string> wordlist, string userLabels, string domain, int limit)
        {
            var step_ = (limit < 48) ? limit : 48;
            userLabels = "vocabulary_com " + userLabels;

            foreach (String word in wordlist)
            {
                var offset = 0;
                var step = step_;
                var wordLabel = word.Replace(' ', '-');
                var primaryDefinitions = String.Empty;
                var fullDefinitions = String.Empty;
                var page = (new HtmlWeb()).Load(definitionsPath + word).DocumentNode;
                Console.WriteLine("{0}", word);

                //Primary
                var definitionsNodes = page.SelectNodes("//div[@class='def']");

                if (definitionsNodes != null)
                {
                    foreach (var p in definitionsNodes)
                    {
                        var href = "#s" + (new Regex(@"quickDef(\d+)")).Replace(p.Attributes["id"].Value, "$1");                        
                        primaryDefinitions += "<i><b>" + getText(page.SelectSingleNode("//a[@href='" + href +"']")) + "</i></b> " + p.InnerText + "<br/>";
                    }
                }

                definitionsNodes = page.SelectNodes("//div[@class='def selected']");

                if (definitionsNodes != null)
                {
                    foreach (var p in definitionsNodes)
                    {
                        var href = "#s" + (new Regex(@"quickDef(\d+)")).Replace(p.Attributes["id"].Value, "$1");
                        primaryDefinitions += "<i><b>" + getText(page.SelectSingleNode("//a[@href='" + href + "']")) + "</i></b> " + p.InnerText + "<br/>";
                    }
                }

                if (primaryDefinitions.Length == 0)
                {
                    definitionsNodes = page.SelectNodes("//h3[@class='definition']");
                    if (definitionsNodes != null)
                    {
                        foreach (var f in definitionsNodes)
                        {
                            var partOfSpeech = f.SelectSingleNode("a");
                            var currentDefinition = new Regex("[\t\r\n]").Replace(getText(f), "");
                            currentDefinition = (new Regex(@"^\w+\s+(.*?)$")).Replace(currentDefinition, "$1");
                            primaryDefinitions += "<i><b>" + getText(partOfSpeech) + "</i></b> " + currentDefinition + "<br/>";
                        }

                        primaryDefinitions = primaryDefinitions.Substring(0, primaryDefinitions.Length - 5);
                    }
                }

                do
                {
                    if (limit - offset < step)
                    {
                        step = limit - offset;
                    }

                    if (step <= 0)
                    {
                        break;
                    }

                    try
                    {
                        var json_ = System.Text.UTF8Encoding.ASCII.GetString(client.DownloadData(examplesPath + word + "&maxResults=" + step + "&startOffset=" + offset + "&filter=0&domain=" + domain));

                        json_ = (new Regex(@"\$d\((.*?)\)")).Replace(json_, @"""$1""");
                        var json = JObject.Parse(json_);

                        if (offset == 0)
                        {
                            var hits = (int)json.SelectToken("result.totalHits");

                            reportStream.Write("Success. Word: " + word + " Hits: " + hits + "\n");
                            Console.WriteLine("Hits: {0}", hits);

                            if (hits == 0)
                            {
                                reportStream.Write("Failure. Examples was not found. Word:" + word + "\n");
                            }
                        }

                        Console.WriteLine("Processed: {0}", offset);

                        offset += step;

                        foreach (var _ in json.SelectToken("result.sentences"))
                        {
                            count++;
                            var example = new StringBuilder()
                                .Append(SafeTrim((string)_.SelectToken("sentence")))
                                .Append("%%!!%%")
                                .Append(SafeTrim((string)_.SelectToken("volume.author")))
                                .Append("%%!!%%")
                                .Append(SafeTrim((string)_.SelectToken("volume.title")))
                                .Append("%%!!%%")
                                .Append(word)
                                .Append("%%!!%%")
                                .Append(primaryDefinitions)
                                .Append("%%!!%%")
                                //.Append(fullDefinitions)
                                //.Append("%%!!%%") 
                                .Append(wordLabel)
                                .Append(" ")
                                .Append(userLabels);

                            stream.Write((new Regex("[\t\n\r]").Replace(example.ToString(), "")).Replace("%%!!%%", "\t") + "\n");
                        }
                    }
                    catch
                    {
                        continue;
                    }
                }
                while (step > 0);

                Console.WriteLine("Processed: {0}\n", offset);

            }
            reportStream.Write("Total: " + count + "\n");
            reportStream.Save();
        }
    }
}
