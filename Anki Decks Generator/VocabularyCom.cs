using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using HtmlAgilityPack;

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

        public void ProcessWordlist(ref CardsStream stream, List<string> wordlist, string userLabels, int limit)
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

                Console.WriteLine("Word: {0}", word);

                foreach (var p in (new HtmlWeb()).Load(definitionsPath + word).DocumentNode.SelectNodes("//div[@class='def selected']"))
                {
                    var cell = p.ParentNode.ParentNode.ChildNodes;
                    primaryDefinitions += "<i><b>" + getText(cell[3]) + "</i></b> " + p.InnerText + "<br/>";
                }

                foreach (var f in (new HtmlWeb()).Load(definitionsPath + word).DocumentNode.SelectNodes("//h3[@class='definition']"))
                {
                    var partOfSpeech = f.SelectSingleNode("a");
                    var currentDefinition = new Regex("[\t\r\n]").Replace(getText(f), "");
                    currentDefinition = (new Regex(@"^\w\s+(.*?)$")).Replace(currentDefinition, "$1");
                    fullDefinitions += "<i><b>" + getText(partOfSpeech) + "</i></b> " + currentDefinition + "<br/>";
                }

                fullDefinitions = fullDefinitions.Substring(0, fullDefinitions.Length - 5);

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

                    var json_ = System.Text.UTF8Encoding.ASCII.GetString(client.DownloadData(examplesPath + word + "&maxResults=" + step + "&startOffset=" + offset + "&filter=0"));
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
                        var example =
                            (string)_.SelectToken("sentence") + "%%!!%%" +
                            (string)_.SelectToken("volume.author") + "%%!!%%" +
                            (string)_.SelectToken("volume.title") + "%%!!%%" +
                            word + "%%!!%%" +
                            primaryDefinitions + "%%!!%%" +
                            fullDefinitions + "%%!!%%" +
                            wordLabel + " " + userLabels;

                        stream.Write((new Regex("[\t\n\r]").Replace(example, "")).Replace("%%!!%%", "\t") + "\n");
                    }
                }
                while (step > 0);              

            }
            reportStream.Write("Total: " + count + "\n");
            reportStream.Save();
        } 
    }
}
