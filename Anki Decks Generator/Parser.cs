using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HtmlAgilityPack;

namespace deckgen
{
    public class Parser
    {
        protected int count_;
        public int count { get { return count_; } }
        
        protected string GetName(HtmlNode node)
        {
            return node.Attributes["class"].Value;
        }

        protected static string getText(HtmlNode node)
        {
            if (node != null) return node.InnerText.Trim();

            return String.Empty;
        }

        protected static string getInnerHtml(HtmlNode node)
        {
            if (node != null) return node.InnerHtml.Trim();

            return String.Empty;
        }
    }
}
