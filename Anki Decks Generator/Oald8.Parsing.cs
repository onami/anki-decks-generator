using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace deckgen
{
    partial class Oald8
    {
        void ParsePage(ref CardsStream stream, HtmlDocument document, string word, string labels)
        {
            var examples = document.DocumentNode.SelectNodes("//span[@class='x-g']");
            var link = word;

            if (examples == null)
            {
                reportStream.Write("Failure. Examples was not found. Link: " + link + ".\n");
                return;
            }

            HtmlNode definition;

            //Transcription
            var usaTranscriptionNode = document.DocumentNode.SelectSingleNode("//span[@class='y']");
            var usaTranscription = (usaTranscriptionNode != null) ? usaTranscriptionNode.InnerText : "";
            var gbrTranscriptionNode = document.DocumentNode.SelectSingleNode("//span[@class='i']");
            var gbrTranscription = (gbrTranscriptionNode != null) ? gbrTranscriptionNode.InnerText : "";

            var title_ = document.DocumentNode.SelectSingleNode("//h2[@class='h']");
            var title = (title_ != null) ? title_.InnerText : "";

            var wordLabel = title.Replace(' ', '-');
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
                    if (structure2 != null) card.structure.Add(structure2.InnerText);
                    if (structure1 != null) card.structure.Add(structure1.InnerText);
                }
                else if (GetName(parentNode) == "n-g" && GetName(parentNode.ParentNode) == "pv-g")
                {
                    var structure2 = parentNode.SelectSingleNode("span[@class='vs-g']");
                    var structure3 = parentNode.ParentNode.SelectSingleNode("h4[@class='pv']");
                    if (structure3 != null) card.structure.Add(structure3.InnerText);
                    if (structure2 != null) card.structure.Add(structure2.InnerText);
                    if (structure1 != null) card.structure.Add(structure1.InnerText);
                }
                else if (GetName(parentNode) == "n-g")
                {
                    var structure2 = parentNode.SelectSingleNode("span[@class='cf']");
                    if (structure2 != null) card.structure.Add(structure2.InnerText);
                    if (structure1 != null) card.structure.Add(structure1.InnerText);
                }
                else if (GetName(parentNode) == "id-g")
                {
                    var structure2 = parentNode.SelectSingleNode("h4[@class='id']");
                    if (structure2 != null) card.structure.Add(structure2.InnerText);
                    if (structure1 != null) card.structure.Add(structure1.InnerText);
                }
                else if (GetName(parentNode) == "h-g")
                {
                    var structure2 = parentNode.SelectSingleNode("span[@class='cf']");
                    if (structure2 != null) card.structure.Add(structure2.InnerText);
                    if (structure1 != null) card.structure.Add(structure1.InnerText);
                }

                //Getting a definition
                if (GetName(parentNode) == "id-g" || GetName(parentNode) == "h-g")
                {
                    var temp = parentNode.SelectSingleNode("div[@class='def_block']");

                    if (temp == null)
                    {
                        temp = parentNode.SelectSingleNode("span[@class='ud']");
                        if (temp == null)
                        {
                            definition = parentNode.SelectSingleNode("span[@class='d']");
                        }
                        else
                        {
                            definition = temp;
                        }
                    }
                    else
                    {
                        definition = temp.SelectSingleNode("span[@class='ud']");
                        if (definition == null)
                        {
                            definition = temp.SelectSingleNode("span[@class='d']");
                        }
                    }
                }
                else
                {
                    definition = parentNode.SelectSingleNode("span[@class='ud']");
                    if (definition == null)
                    {
                        definition = parentNode.SelectSingleNode("span[@class='d']");
                    }
                }

                //An example itself
                var interpretation = example.SelectSingleNode("span[@class='x']");
                card.interpretation = (interpretation != null) ? interpretation.InnerText : "";
                card.sentence = (new Regex(" \\(=.*?\\)")).Replace(card.interpretation, "");

                card.definition = (definition != null) ? definition.InnerText : "";
                card.definition = card.definition.Replace("    ", " ");

                card.usaTranscription = usaTranscription;
                card.gbrTranscription = gbrTranscription;
                
                if (card.interpretation == card.sentence)
                {
                    card.interpretation = "";
                }
                if (card.definition == "")
                {
                    reportStream.Write("Failure. Definition was not found. Link: " + link + ". Example: '" + card.sentence + "'.\n");
                }
                if (card.sentence == "")
                {
                    reportStream.Write("Failure. Example was not found. Link: " + link + ".\n");
                }

                //"Trimming"
                title = title.Trim();
                card.sentence = card.sentence.Trim();
                card.interpretation = card.interpretation.Trim();
                card.gbrTranscription = card.gbrTranscription.Trim();
                card.usaTranscription = card.usaTranscription.Trim();
                card.definition = card.definition.Trim();

                var outStr = card.sentence + "\t" + card.interpretation + "\t" + title + "\t" + card.gbrTranscription + "\t" + card.usaTranscription + "\t" + PrintList(card.structure) + "\t" + card.definition + "\t" + labels + "\n";
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
