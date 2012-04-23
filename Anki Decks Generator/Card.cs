using System.Collections.Generic;
using HtmlAgilityPack;

namespace deckgen
{
    public class Card
    {
        public string Sentence;
        public string Interpretation;
        public string Definition;
        public string UsaTranscription;
        public string GbrTranscription;
        public string SimpleStructure;

        public List<string> Structure = new List<string>();
        public List<string> Register = new List<string>();

        public string Set(HtmlNode node)
        {
            if (node != null) return node.InnerText.Trim();

            return string.Empty;
        }
    }
}
