using System;
using System.Collections.Generic;
using System.IO;

namespace deckgen
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var oaldFlag = false;
            var inputPath = "";
            var outputPath = "./deck " + DateTime.Now.ToString("yyyy.MM.dd HH-mm-ss") + ".txt";
            var wordlist = new List<String>();
            var labels = "";
            var relatedFlag = false;

            CardsStream output;
            StreamReader input;

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-oald8")
                {
                    oaldFlag = true;
                }
                else if (args[i] == "-l" && i + 1 < args.Length)
                {
                    labels = (args[i+1]).Trim();
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
                Console.Write("Where's a listname, uh? You should type -p <path_to_you_wordlist>");
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
                        var newWord = input.ReadLine().Trim();
                        if (wordlist.Contains(newWord) == false)
                        {
                            wordlist.Add(newWord);
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
            
            if(oaldFlag == true)
            {
                var parser = new Oald8();
                output = new CardsStream(outputPath, 100000);
                parser.ProcessWordlist(ref output, wordlist, labels, relatedFlag);
                output.Save();
                Console.WriteLine("\nCount: {0}", parser.count);
            }

           //Console.ReadKey();
        }
    }
}
