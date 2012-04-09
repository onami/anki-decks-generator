using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace deckgen
{
    public partial class Oald8
    {
        void downloadPages(ref CardsStream stream, List<string> wordlist, string labels)
        {
            foreach (string word_ in wordlist)
            {
                var word = (new Regex("[^- 0-9a-zA-Z]+")).Replace(word_, "");
                var page = (new HtmlWeb()).Load(searchPath + word);
                pages.Add(word, page);
            }
        }

        string getCleanUrl(HtmlNode link_)
        {
            return (new Regex("(.+?)#.*")).Replace(link_.Attributes["href"].Value, "$1");
        }
    }
}
