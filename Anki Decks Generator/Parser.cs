using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HtmlAgilityPack;

namespace deckgen
{
    public class Parser
    {
        protected string GetName(HtmlNode node)
        {
            return node.Attributes["class"].Value;
        }

        protected static string getText(HtmlNode node)
        {
            if (node != null) return node.InnerText.Trim();

            return String.Empty;
        }
    }
}
