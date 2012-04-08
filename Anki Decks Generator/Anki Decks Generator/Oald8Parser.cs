using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace Anki_Decks_Generator
{
    class Oald8Parser
    {
        //Сделать только геттер
        public int count;

        string searchPath = "http://oald8.oxfordlearnersdictionaries.com/dictionary/";

        public Oald8Parser()
        {
            count = 0;
        }

        string GetName(HtmlNode node)
        {
            return node.Attributes["class"].Value;
        }

        string PrintList(List<string> list)
        {
            string ret = "";

            for (int i = 0; i < list.Count; i++)
            {
                ret += list[i];
                if (i + 1 < list.Count) ret += " → ";
            }

            ret = ret.Replace("    ", " ").Replace("  ", " ");

            return ret;
        }

        public void Process(ref CardsStream stream, List<string> wordlist, string labels, bool relatedFlag)
        {
            var relatedWordlist = new Hashtable();

            foreach (string word in wordlist)
            {
                var document = (new HtmlWeb()).Load(searchPath + word);

                //Получаем список связанных слов, в т.ч. и данное
                var relatedLinksNodes = document.DocumentNode.SelectNodes("//div[@id='relatedentries']/ul/li/a");
                if (relatedLinksNodes == null) continue;

                foreach (HtmlNode link in relatedLinksNodes)
                {
                    var relatedWord = (new Regex("(\\w+?)#.*")).Replace(link.Attributes["href"].Value, "$1");
                    if (relatedWordlist.ContainsKey(relatedWord) == false)
                    {
                        relatedWordlist.Add(relatedWord, false);
                    }
                }

                //Получили, начинаем обрабатывать.
                var updatedWordList = new Hashtable();

                foreach (string relatedLink in relatedWordlist.Keys)
                {
                    if ((bool)relatedWordlist[relatedLink] == true)
                    {
                        continue;
                    }

                    updatedWordList.Add(relatedLink, true);
                    //Console.WriteLine("T: {0}, ", relatedLink);
                    if ((new Regex("^(" + word.Replace(' ', '-') + "_\\d+|" + word.Replace(' ', '-') + ")$")).Match(relatedLink).Success)
                    {
                        Console.WriteLine("{0}", relatedLink);
                        GetPage(ref stream, (new HtmlWeb()).Load(searchPath + relatedLink), relatedLink, labels);
                    }
                    else if(relatedFlag == true)
                    {
                        Console.WriteLine("    {0}", relatedLink);
                        GetPage(ref stream, (new HtmlWeb()).Load(searchPath + relatedLink), relatedLink, labels);
                    }                    
                }
                foreach (DictionaryEntry update in updatedWordList)
                {
                    relatedWordlist[update.Key] = update.Value;
                }
            }
        }

        void GetPage(ref CardsStream stream, HtmlDocument document, string word, string labels)
        {
            var examples = document.DocumentNode.SelectNodes("//span[@class='x-g']");
            if (examples == null) return;

            //Init
            word = (new Regex("(\\w+?)_.*")).Replace(word, "$1");
            labels = (labels == "") ? "oald8 " + word : "oald8 " + word + " " + labels;

            HtmlNode definition;

            var usaTranscriptionNode = document.DocumentNode.SelectSingleNode("//span[@class='y']");
            var usaTranscription = (usaTranscriptionNode != null) ? usaTranscriptionNode.InnerText : "";
            var gbrTranscriptionNode = document.DocumentNode.SelectSingleNode("//span[@class='i']");
            var gbrTranscription = (gbrTranscriptionNode != null) ? gbrTranscriptionNode.InnerText : "";
            usaTranscription = (new Regex("^ ")).Replace(usaTranscription, "");
            gbrTranscription = (new Regex("^ ")).Replace(gbrTranscription, "");

            foreach (HtmlNode example in examples)
            {
                var card = new Card();
                var parentNode = example.ParentNode;

                //A structure
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

                //An example itself
                var interpretation = example.SelectSingleNode("span[@class='x']");
                card.interpretation = (interpretation != null) ? interpretation.InnerText : "";
                card.sentence = (new Regex(" \\(=.*?\\)")).Replace(card.interpretation, "");

                //Definition
                if (GetName(parentNode) == "id-g")
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

                card.definition = (definition != null) ? definition.InnerText : "";
                card.definition.Replace("    ", " ");

                card.usaTranscription = usaTranscription;
                card.gbrTranscription = gbrTranscription;

                var outStr = card.sentence + "\t" + card.interpretation + "\t" + word + "\t" + card.gbrTranscription + "\t" + card.usaTranscription + "\t" + PrintList(card.structure) + "\t" + card.definition + "\t" + labels + "\n";
                stream.Write(outStr);
                count++;
                
                //Console.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}", card.sentence, card.interpretation, PrintList(card.structure), card.definition, card.gbrTranscription, card.usaTranscription, labels);
                //file.WriteLine("Structure: {1}\nDefinition: {2}\nExample: {0}\nFull: {5}\nBr: {3}\nAm: {4}\n\n", card.sentence, PrintList(card.structure), card.definition, card.gbrTranscription, card.usaTranscription, card.interpretation);

            }
        }

        //Недоделано::Берет общий для всей статьи регистр, который находится сразу после div#h-g и до любого определения 
        List<string> getGeneralRegister(HtmlDocument document)
        {
            var ret = new List<string>();

            var register = document.DocumentNode.SelectSingleNode("//div[@class='top-container']");
            register = register.NextSibling;
            Console.WriteLine("{0}", register.InnerHtml);
            register = register.NextSibling;
            Console.ReadKey();
            Console.WriteLine("{0}", register.InnerHtml);
            register = register.NextSibling;
            Console.ReadKey();
            Console.WriteLine("{0}", register.InnerHtml);
            register = register.NextSibling;
            Console.ReadKey();

            if (register.InnerText.Contains("(") == false)
            {
                return ret;
            }

            register = register.NextSibling;

            while (register.InnerText.Contains(")") != true)
            {
                ret.Add(register.InnerText);
                register = register.NextSibling.NextSibling;
            }

            return ret;
        }

    }
}
