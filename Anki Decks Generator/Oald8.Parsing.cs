using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace deckgen
{
    partial class Oald8 : Parser
    {
        string PrintList(List<string> list)
        {
            string ret = "";

            for (int i = 0; i < list.Count; i++)
            {
                ret += list[i].Trim();
                if (i + 1 < list.Count) ret += " → ";
            }

            ret = ret.Replace("    ", " ").Replace("  ", " ");

            return ret;
        }

        void ParsePage(ref CardsStream stream, HtmlDocument document, string link, string labels)
        {
            var examples = document.DocumentNode.SelectNodes("//span[@class='x-g']");
   
            if (examples == null)
            {
                reportStream.Write("Failure. Examples was not found. Link: " + link + ".\n");
                return;
            }

            //Transcription
            var usaTranscription = document.DocumentNode.SelectSingleNode("//span[@class='y']");
            var gbrTranscription = document.DocumentNode.SelectSingleNode("//span[@class='i']");

            //Word "name"
            var word = getText(document.DocumentNode.SelectSingleNode("//h2[@class='h']"));

            //Label
            var wordLabel = word.Replace(' ', '-');
            labels = (labels == "") ? "oald8 " + wordLabel : "oald8 " + wordLabel + " " + labels;

            foreach (HtmlNode example in examples)
            {
                var card = new Card();
                var parentNode = example.ParentNode;

                //Getting a structure
                var structure1 = example.SelectSingleNode("span[@class='cf']");
                if (GetName(parentNode) == "pv-g")
                {
                    var structure2 = parentNode.SelectSingleNode("h4[@class='pv']");
                    if (structure2 != null) card.Structure.Add(structure2.InnerText);
                    if (structure1 != null) card.Structure.Add(structure1.InnerText);
                }
                else if (GetName(parentNode) == "n-g" && GetName(parentNode.ParentNode) == "pv-g")
                {
                    var structure2 = parentNode.SelectSingleNode("span[@class='vs-g']");
                    var structure3 = parentNode.ParentNode.SelectSingleNode("h4[@class='pv']");
                    if (structure3 != null) card.Structure.Add(structure3.InnerText);
                    if (structure2 != null) card.Structure.Add(structure2.InnerText);
                    if (structure1 != null) card.Structure.Add(structure1.InnerText);
                }
                else if (GetName(parentNode) == "n-g")
                {
                    var structure2 = parentNode.SelectSingleNode("span[@class='cf']");
                    if (structure2 != null) card.Structure.Add(structure2.InnerText);
                    if (structure1 != null) card.Structure.Add(structure1.InnerText);
                }
                else if (GetName(parentNode) == "id-g")
                {
                    var structure2 = parentNode.SelectSingleNode("h4[@class='id']");
                    if (structure2 != null) card.Structure.Add(structure2.InnerText);
                    if (structure1 != null) card.Structure.Add(structure1.InnerText);
                }
                else if (GetName(parentNode) == "h-g")
                {
                    var structure2 = parentNode.SelectSingleNode("span[@class='cf']");
                    if (structure2 != null) card.Structure.Add(structure2.InnerText);
                    if (structure1 != null) card.Structure.Add(structure1.InnerText);
                }

                //Getting a definition
                HtmlNode definition = null;

                if (GetName(parentNode) == "id-g" || GetName(parentNode) == "h-g")
                {
                    definition = parentNode.SelectSingleNode("div[@class='def_block']");
                }
                if (definition == null)
                {
                    definition = parentNode;
                }

                var temp = definition.SelectSingleNode("span[@class='ud']");
                if (temp == null)
                {
                    definition = definition.SelectSingleNode("span[@class='d']");
                }
                else {
                    definition = temp;
                }

                //An example itself
                card.Interpretation = getText(example.SelectSingleNode("span[@class='x']"));
                card.Sentence = (new Regex(" \\(=.*?\\)")).Replace(card.Interpretation, "");

                card.Definition = getText(definition);
                card.Definition = card.Definition.Replace("    ", " ");

                card.UsaTranscription = getText(usaTranscription);
                card.GbrTranscription = getText(gbrTranscription);

                if (card.Interpretation == card.Sentence)
                {
                    card.Interpretation = "";
                }
                if (card.Definition == "")
                {
                    reportStream.Write("Failure. Definition was not found. Link: " + link + ". Example: '" + card.Sentence + "'.\n");
                }
                if (card.Sentence == "")
                {
                    reportStream.Write("Failure. Example was not found. Link: " + link + ".\n");
                }

                var outStr = card.Sentence + "\t" + card.Interpretation +
                    "\t" + word + "\t" + card.GbrTranscription +
                    "\t" + card.UsaTranscription + "\t" +
                    PrintList(card.Structure) + "\t" + card.Definition +
                    "\t" + labels + "\n";
                stream.Write(outStr);
                count++;
            }
        }

        List<string> GetArticleCrossrefenceLinkList()
        {
            var articleCrossreferenceWordList = new List<string>();

            foreach (String word in pages.Keys)
            {
                var page = (HtmlDocument)pages[word];

                var links1 = page.DocumentNode.SelectNodes("//span[@class='xh']");
                var links2 = page.DocumentNode.SelectNodes("//a[@class='Ref']");

                if (links1 == null && links2 == null) continue;

                if (links2 != null)
                {

                }

                foreach (var link in links2)
                {
                    if (articleCrossreferenceWordList.Contains(link.InnerText) == false)
                    {
                        Console.WriteLine("Word: {0} Reference: {1}", word, link.InnerText);
                        articleCrossreferenceWordList.Add(link.InnerText);
                    }
                }
            }

            return articleCrossreferenceWordList;
        }
    }
}
