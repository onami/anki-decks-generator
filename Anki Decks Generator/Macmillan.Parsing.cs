using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace deckgen
{
    public partial class Macmillan : Parser
    {
        void ParsePage(ref CardsStream stream, HtmlDocument document, string link, string userLabels)
        {
            var examples = document.DocumentNode.SelectNodes("//div[@class='EXAMPLES']");

            if (examples == null)
            {
                reportStream.Write("Failure. Examples was not found. Link: " + link + "\n");
                return;
            }

            var gbrTranscription = document.DocumentNode.SelectSingleNode("//span[@class='PRON']");
            var word = getText(document.DocumentNode.SelectSingleNode("//span[@class='BASE']"));

            var wordLabel = word.Replace(' ', '-');
            userLabels = (userLabels == "") ? "macmillan " + wordLabel : "macmillan " + wordLabel + " " + userLabels;

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
                
                card.Sentence = getText(example.SelectSingleNode("p[@id='EXAMPLE']"));
                card.Definition = getText(parentNode.SelectSingleNode("span[@class='DEFINITION']"));
                card.GbrTranscription = getText(gbrTranscription).Replace("/", "");
                card.SimpleStructure = getText(structure).Replace(":", "");               
                

                if (card.Definition == "")
                {
                    reportStream.Write("Failure. Definition was not found. Link: " + link + " Example: '" + card.Sentence + "'\n");
                }
                if (card.Sentence == "")
                {
                    reportStream.Write("Failure. Example was not found. Link: " + link + "\n");
                }

                var outStr = card.Sentence + "\t" + card.Interpretation +
                   "\t" + word + "\t" + card.GbrTranscription +
                   "\t" + card.UsaTranscription + "\t" +
                   card.SimpleStructure + "\t" + card.Definition +
                   "\t" + userLabels + "\n";
                stream.Write(outStr);
                count++;
            }
        }
    }
}
