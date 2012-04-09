using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace deckgen
{
    public partial class Oald8
    {
        //Сделать только геттер
        public int count;
        Hashtable pages;
        string searchPath = "http://oald8.oxfordlearnersdictionaries.com/dictionary/";

        public Oald8()
        {
            count = 0;
            pages = new Hashtable();
        }

        public void ProcessWordlist(ref CardsStream stream, List<string> wordlist, string labels, bool crossreferenceFlag)
        {
            //Разбираем исходные страницы. Возможно, удастся ускорить работу засчёт распараллеливания.
            downloadPages(ref stream, wordlist, labels);
            //Собираем ссылки в теле статьи и добавляем в список.
            //downloadPages(ref stream, GetArticleCrossrefenceLinkList(), labels);

            //Работаем с ссылками, находящиеся в блоке Search Results, в т.ч. и на само word)
            var searchCrossreferenceUrlList = new Hashtable();
            var firstLinkLists = new List<String>();

            foreach(String word in pages.Keys)
            {
                var page = (HtmlDocument) pages[word];
  
                var updatedWordList = new Hashtable();
                                
                var currentLink = page.DocumentNode.SelectSingleNode("//li[@class='currentpage']/a");
                if (currentLink != null)
                {
                    var url = getCleanUrl(currentLink);
                    if (firstLinkLists.Contains(url) == false)
                    {
                        firstLinkLists.Add(url);
                    }
                    else
                    {
                        Console.WriteLine("Bug");
                    }
                }

                var linksNodes = page.DocumentNode.SelectNodes("//div[@id='relatedentries']/ul/li/a");
                if (linksNodes != null)
                {
                    foreach (HtmlNode link in linksNodes)
                    {
                        var url = getCleanUrl(link);
                        if (searchCrossreferenceUrlList.ContainsKey(url) == false)
                        {
                            searchCrossreferenceUrlList.Add(url, false);
                        }
                    }
                }

                //Всё нужное получили, начинаем обрабатывать searchCrossreferenceLinkList
                foreach (string url in searchCrossreferenceUrlList.Keys)
                {
                    if ((bool)searchCrossreferenceUrlList[url] == true)
                    {
                        continue;
                    }

                    //Console.WriteLine("T: {0}, ", relatedLink);
                    var tranformedWord = (new Regex("^(" + word.Replace(' ', '-') + "_\\d+)$", RegexOptions.IgnoreCase));

                    //Первую страницу мы уже скачали.
                    if (firstLinkLists.Contains(url))
                    {
                        Console.WriteLine("{0}", url);
                        ParsePage(ref stream, page, word, labels);
                        updatedWordList.Add(url, true);
                    }
                    else if (tranformedWord.Match(url).Success)
                    {
                        Console.WriteLine("{0}", url);
                        ParsePage(ref stream, (new HtmlWeb()).Load(searchPath + url), url, labels);
                        updatedWordList.Add(url, true);
                    }
                    else if (crossreferenceFlag == true)
                    {
                        Console.WriteLine("    {0}", url);
                        ParsePage(ref stream, (new HtmlWeb()).Load(searchPath + url), url, labels);
                        updatedWordList.Add(url, true);
                    }                    
                }

                foreach (DictionaryEntry update in updatedWordList)
                {
                    searchCrossreferenceUrlList[update.Key] = update.Value;
                }
            }
        } 
    }
}
