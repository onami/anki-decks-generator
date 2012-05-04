using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace deckgen
{
    public class Program
    {
        [FlagsAttribute]
        enum ParserMask
        {
            None = 0,
            Oald8 = 1,
            Macmillan = 2,
            VocabularyCom = 4
        }

        public static void Main(string[] args)
        {
            ParserMask useParser = ParserMask.None;
            var inputPath = "";
            var outputPath = String.Empty;
            var wordlist = new List<String>();
            var labels = "";
            var relatedFlag = false;
            var vocabularyComExamplesLimit = Int32.MaxValue;
            var vocabularyComDomain = String.Empty;

            CardsStream output;
            StreamReader input;

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-oald8")
                {
                    outputPath = "oald8";
                    useParser |= ParserMask.Oald8;
                }
                else if (args[i] == "-macmillan")
                {
                    outputPath = "macmillan";
                    useParser |= ParserMask.Macmillan;
                }
                else if (args[i] == "-vocabcom")
                {
                    outputPath = "vocabulary.com";
                    useParser |= ParserMask.VocabularyCom;
                }
                    
                else if (args[i] == "-l" && i + 1 < args.Length)
                {
                    labels = (args[i + 1]).Trim();
                    i++;
                }
                else if (args[i] == "-vlimit" && i + 1 < args.Length)
                {
                    vocabularyComExamplesLimit = Convert.ToInt32((args[i + 1]).Trim());
                    i++;
                }

                else if (args[i] == "-vdomain" && i + 1 < args.Length)
                {
                    vocabularyComDomain = (args[i + 1]).Trim();
                    i++;
                }

                else if (args[i] == "-p" && i + 1 < args.Length)
                {
                    inputPath = (args[i + 1]).Trim();
                    i++;
                }

                else if (args[i] == "-related")
                {
                    relatedFlag = true;
                }
            }

            if (inputPath == "")
            {
                Console.Write("Where's a listname, uh? You should enter -p <path_to_you_wordlist>");
                Console.ReadKey();
                return;
            }
            else
            {
                try
                {
                    input = new StreamReader(inputPath);
                    while (input.EndOfStream == false)
                    {
                        var word = (new Regex("[^- 0-9a-zA-Z']+")).Replace(input.ReadLine(), "").Trim();
                        if (wordlist.Contains(word) == false)
                        {
                            wordlist.Add(word);
                        }
                    }
                    input.Close();
                }
                catch (FileNotFoundException e)
                {
                    Console.Write("Wrong path: {0}", inputPath);
                }
                catch (DirectoryNotFoundException e)
                {
                    Console.Write("Wrong directory: {0}", inputPath);
                }
            }


            if (labels.Length != 0)
            {
                outputPath = "./" + outputPath + " " + labels;
            }
            else
            {
                outputPath = "./" + outputPath;
            }

            outputPath += " " + DateTime.Now.ToString("yyyy.MM.dd HH-mm-ss") + ".txt";

            output = new CardsStream(outputPath, 100000);

            if ((useParser & ParserMask.Oald8) != ParserMask.None)
            {
                var parser = new Oald8();
                parser.ProcessWordlist(ref output, wordlist, labels, relatedFlag);
                Console.WriteLine("\nCount: {0}\n", parser.count);
            }
            else if ((useParser & ParserMask.Macmillan) != ParserMask.None)
            {
                var parser = new Macmillan();
                parser.ProcessWordlist(ref output, wordlist, labels, relatedFlag);
                Console.WriteLine("\nCount: {0}\n", parser.count);
            }
            else if ((useParser & ParserMask.VocabularyCom) != ParserMask.None)
            {
                var parser = new VocabularyCom();
                parser.ProcessWordlist(ref output, wordlist, labels, vocabularyComDomain, vocabularyComExamplesLimit);
                Console.WriteLine("\nCount: {0}\n", parser.count);
            }

            if (useParser != ParserMask.None)
            {
                output.Save();
            }

           //Console.ReadKey();
        }
    }
}
