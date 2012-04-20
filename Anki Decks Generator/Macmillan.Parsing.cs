using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace deckgen
{
    public partial class Macmillan
    {
        public string GetName(HtmlNode node)
        {
            return node.Attributes["class"].Value;
        }

        void ParsePage(ref CardsStream stream, HtmlDocument document, string word, string labels)
        {
            var examples = document.DocumentNode.SelectNodes("//div[@class='EXAMPLES']");
            var link = word;

            if (examples == null)
            {
                reportStream.Write("Failure. Examples was not found. Link: " + link + ".\n");
                return;
            }

            //Transcription
            var gbrTranscriptionNode = document.DocumentNode.SelectSingleNode("//span[@class='PRON']");
            var gbrTranscriptione = (gbrTranscriptionNode != null) ? gbrTranscriptionNode.InnerText.Replace("/", "") : "";

            //Word name
            var title_ = document.DocumentNode.SelectSingleNode("//span[@class='BASE']");
            var title = (title_ != null) ? title_.InnerText : "";

            //Label
            var wordLabel = title.Replace(' ', '-');
            labels = (labels == "") ? "macmillan " + wordLabel : "macmillan " + wordLabel + " " + labels;

            foreach (HtmlNode example in examples)
            {
                var card = new Card();
                var parentNode = example.ParentNode;

                //Getting a structure
                var structure = example.SelectSingleNode("strong");
                if (structure == null)
                {
                    //DIV#SENSE_BODY -> DIV -> LI 
                    structure = parentNode.ParentNode.ParentNode.SelectSingleNode("div/h2/span[@class='BASE']");
                }
                
                card.simpleStructure = (structure != null) ? structure.InnerText.Replace(":", "") : "";

                //Getting a definition
                var definition = parentNode.SelectSingleNode("span[@class='DEFINITION']");
                
                card.definition = (definition != null) ? definition.InnerText : "";

                //An example itself
                var sentence = example.SelectSingleNode("p[@id='EXAMPLE']");
                card.interpretation = "";
                card.sentence = (sentence != null) ? sentence.InnerText : "";

                card.definition = (definition != null) ? definition.InnerText : "";
                card.definition = card.definition.Replace("    ", " ");

                card.gbrTranscription = gbrTranscriptione;

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
                card.definition = card.definition.Trim();

                var outStr = card.sentence + "\t" + card.interpretation + "\t" + title + "\t" + card.gbrTranscription + "\t" + card.usaTranscription + "\t" + card.simpleStructure + "\t" + card.definition + "\t" + labels + "\n";
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
