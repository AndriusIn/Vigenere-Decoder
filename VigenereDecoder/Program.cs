using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace VigenereDecoder
{
    class Trees
    {
        public class Node
        {
            public char Letter { get; set; }
            public Node[] Children { get; set; }
            public Node(char letter)
            {
                Letter = letter;
            }
        }
        private Node[] TreeParents;
        private List<List<char>> Categories;
        private List<List<char>> Keys;
        private Dictionary<char, int> Alphabet;
        private List<Dictionary<char, int>> FrequencyAnalyses;
        private string Text;
        private string Letters;
        private char MostCommonLetter;
        private int KeyLength;
        public Trees(string text, string letters, char mostCommonLetter, int keyLength)
        {
            Text = text;
            Letters = letters;
            MostCommonLetter = char.ToUpper(mostCommonLetter);
            KeyLength = keyLength;
            SetAlphabet();
            SetCategories();
            SetFrequencyAnalyses();
            SetTreeParents();
            SetKeys();
        }
        private void SetAlphabet()
        {
            Alphabet = new Dictionary<char, int>();
            for (int i = 0; i < Letters.Length; i++)
            {
                if (char.IsLetter(Letters[i]))
                {
                    char key = char.ToUpper(Letters[i]);
                    if (!Alphabet.ContainsKey(key))
                    {
                        Alphabet.Add(key, Alphabet.Count);
                    }
                }
            }
        }
        private void SetCategories()
        {
            Categories = new List<List<char>>();
            for (int i = 0; i < KeyLength; i++)
            {
                Categories.Add(new List<char>());
            }
            int categoryIndex = -1;
            for (int i = 0; i < Text.Length; i++)
            {
                if (Alphabet.ContainsKey(char.ToUpper(Text[i])))
                {
                    if (categoryIndex == Categories.Count - 1)
                    {
                        categoryIndex = 0;
                    }
                    else
                    {
                        categoryIndex++;
                    }
                    Categories[categoryIndex].Add(char.ToUpper(Text[i]));
                }
            }
        }
        private List<char> GetMostFrequentLettersInCategory(int categoryIndex)
        {
            int maxValue = 0;
            List<char> mostFrequentLettersInCategory = new List<char>();
            SortedDictionary<char, int> frequencyAnalysis = new SortedDictionary<char, int>();
            for (int i = 0; i < Categories[categoryIndex].Count; i++)
            {
                if (char.IsLetter(Categories[categoryIndex][i]))
                {
                    char key = char.ToUpper(Categories[categoryIndex][i]);
                    if (frequencyAnalysis.ContainsKey(key))
                    {
                        frequencyAnalysis[key] += 1;
                    }
                    else
                    {
                        frequencyAnalysis.Add(key, 1);
                    }
                    if (frequencyAnalysis[key] > maxValue)
                    {
                        mostFrequentLettersInCategory = new List<char>
                        {
                            key
                        };
                        maxValue = frequencyAnalysis[key];
                    }
                    else if (frequencyAnalysis[key] == maxValue)
                    {
                        mostFrequentLettersInCategory.Add(key);
                    }
                }
            }
            return mostFrequentLettersInCategory;
        }
        private void SetTreeParents()
        {
            List<char> mostFrequentLettersInCategory = GetMostFrequentLettersInCategory(0);
            TreeParents = new Node[mostFrequentLettersInCategory.Count];
            for (int i = 0; i < TreeParents.Length; i++)
            {
                TreeParents[i] = new Node(mostFrequentLettersInCategory[i]);
            }
            for (int i = 0; i < TreeParents.Length; i++)
            {
                SetTreeChildren(ref TreeParents[i], 1);
            }
        }
        private void SetTreeChildren(ref Node parent, int categoryIndex)
        {
            if (categoryIndex >= Categories.Count)
            {
                return;
            }
            List<char> mostFrequentLettersInCategory = GetMostFrequentLettersInCategory(categoryIndex);
            Node[] children = new Node[mostFrequentLettersInCategory.Count];
            for (int i = 0; i < children.Length; i++)
            {
                children[i] = new Node(mostFrequentLettersInCategory[i]);
                SetTreeChildren(ref children[i], categoryIndex + 1);
            }
            parent.Children = children;
        }
        private void SetKeys()
        {
            Keys = new List<List<char>>();
            for (int i = 0; i < TreeParents.Length; i++)
            {
                Keys.Add(new List<char>());
                Keys[Keys.Count - 1].Add(TreeParents[i].Letter);
                SetChildrenKeys(TreeParents[i].Children, TreeParents[i].Letter.ToString());
            }
        }
        private void SetChildrenKeys(Node[] children, string chain)
        {
            if (children == null)
            {
                return;
            }
            for (int i = 0; i < children.Length; i++)
            {
                chain += children[i].Letter;
                Keys[Keys.Count - 1].Add(children[i].Letter);
                SetChildrenKeys(children[i].Children, chain);
                if (i < children.Length - 1)
                {
                    chain = chain.Substring(0, chain.Length - 1);
                    Keys.Add(new List<char>());
                    for (int j = 0; j < chain.Length; j++)
                    {
                        Keys[Keys.Count - 1].Add(chain[j]);
                    }
                }
            }
        }
        private void SetFrequencyAnalyses()
        {
            FrequencyAnalyses = new List<Dictionary<char, int>>();
            for (int i = 0; i < Categories.Count; i++)
            {
                FrequencyAnalyses.Add(new Dictionary<char, int>());
            }
            for (int i = 0; i < Categories.Count; i++)
            {
                for (int j = 0; j < Categories[i].Count; j++)
                {
                    if (FrequencyAnalyses[i].ContainsKey(Categories[i][j]))
                    {
                        FrequencyAnalyses[i][Categories[i][j]] += 1;
                    }
                    else
                    {
                        FrequencyAnalyses[i].Add(Categories[i][j], 1);
                    }
                }
            }
        }
        public void PrintAlphabet(string outputFile)
        {
            using (StreamWriter sw = new StreamWriter(outputFile, true))
            {
                sw.WriteLine("Alphabet:");
                int totalCount = Alphabet.Count;
                int lastIndexLength = (totalCount - 1).ToString().Length;
                string keyString = "";
                string valueString = "";
                string spaces = new string(' ', lastIndexLength);
                int count = 0;
                foreach (KeyValuePair<char, int> kvp in Alphabet)
                {
                    count++;
                    if (count == totalCount)
                    {
                        keyString += kvp.Key;
                        valueString += kvp.Value;
                    }
                    else
                    {
                        keyString += kvp.Key + new string(' ', lastIndexLength);
                        if ((kvp.Value + 1).ToString().Length > kvp.Value.ToString().Length)
                        {
                            spaces = new string(' ', lastIndexLength - kvp.Value.ToString().Length);
                        }
                        valueString += kvp.Value + spaces;
                    }
                }
                sw.WriteLine(keyString);
                sw.WriteLine(valueString);
            }
        }
        public void PrintEncodedText(string outputFile)
        {
            using (StreamWriter sw = new StreamWriter(outputFile, true))
            {
                sw.WriteLine("Encoded text:");
                sw.WriteLine(Text);
            }
        }
        public void PrintCategories(string outputFile)
        {
            using (StreamWriter sw = new StreamWriter(outputFile, true))
            {
                sw.WriteLine("Categories:");
                for (int i = 0; i < Categories.Count; i++)
                {
                    sw.WriteLine(string.Format("Category {0}:", i + 1));
                    string categoryText = "";
                    for (int j = 0; j < Categories[i].Count; j++)
                    {
                        categoryText += Categories[i][j];
                    }
                    sw.WriteLine(categoryText);
                }
            }
        }
        public void PrintFrequencyAnalyses(string outputFile)
        {
            using (StreamWriter sw = new StreamWriter(outputFile, true))
            {
                sw.WriteLine("Frequency analyses:");
                for (int i = 0; i < FrequencyAnalyses.Count; i++)
                {
                    var sortedByMaxValue = FrequencyAnalyses[i].OrderByDescending(kvp => kvp.Value);
                    sw.WriteLine(string.Format("Category {0}:", i + 1));
                    foreach (KeyValuePair<char, int> kvp in sortedByMaxValue)
                    {
                        sw.WriteLine(string.Format("{0}: {1}", kvp.Key, kvp.Value));
                    }
                }
            }
        }
        public void PrintAllKeys(string outputFile)
        {
            using (StreamWriter sw = new StreamWriter(outputFile, true))
            {
                sw.WriteLine(string.Format("All keys (key length = {0}):", KeyLength));
                for (int i = 0; i < Keys.Count; i++)
                {
                    string keyText = "";
                    for (int j = 0; j < Keys[i].Count; j++)
                    {
                        int distance = Alphabet[Keys[i][j]] - Alphabet[MostCommonLetter];
                        if (distance < 0)
                        {
                            distance += Alphabet.Count;
                        }
                        keyText += Alphabet.ElementAt(distance).Key;
                        //keyText += Keys[i][j];
                    }
                    sw.WriteLine(keyText);
                }
            }
        }
        public void PrintDecodedText(string outputFile)
        {
            using (StreamWriter sw = new StreamWriter(outputFile, true))
            {
                sw.WriteLine("Decoded text:");
                for (int i = 0; i < Keys.Count; i++)
                {
                    string keyText = "";
                    for (int j = 0; j < Keys[i].Count; j++)
                    {
                        int distance = Alphabet[Keys[i][j]] - Alphabet[MostCommonLetter];
                        if (distance < 0)
                        {
                            distance += Alphabet.Count;
                        }
                        keyText += Alphabet.ElementAt(distance).Key;
                    }
                    sw.WriteLine(string.Format("Key '{0}':", keyText));
                    int keyIndex = -1;
                    for (int j = 0; j < Text.Length; j++)
                    {
                        if (Alphabet.ContainsKey(char.ToUpper(Text[j])))
                        {
                            if (keyIndex == Keys[i].Count - 1)
                            {
                                keyIndex = 0;
                            }
                            else
                            {
                                keyIndex++;
                            }
                            int distance = Alphabet[Keys[i][keyIndex]] - Alphabet[MostCommonLetter];
                            if (distance < 0)
                            {
                                distance += Alphabet.Count;
                            }
                            int decodedLetterIndex = Alphabet[char.ToUpper(Text[j])] - distance;
                            if (decodedLetterIndex < 0)
                            {
                                decodedLetterIndex += Alphabet.Count;
                            }
                            if (char.IsUpper(Text[j]))
                            {
                                sw.Write(Alphabet.ElementAt(decodedLetterIndex).Key);
                            }
                            else
                            {
                                sw.Write(char.ToLower(Alphabet.ElementAt(decodedLetterIndex).Key));
                            }
                        }
                        else
                        {
                            sw.Write(Text[j]);
                        }
                    }
                    sw.WriteLine();
                }
            }
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            string inputFile = @"d:\cypher_input.txt";
            string outputFile = @"d:\cypher_output.txt";
            if (File.Exists(inputFile))
            {
                Console.WriteLine("Choose language:");
                List<string> languages = new List<string>();
                languages.Add("Lithuanian");
                languages.Add("English");
                languages.Sort();
                for (int i = 0; i < languages.Count; i++)
                {
                    Console.WriteLine(string.Format("{0} > {1}", i + 1, languages[i]));
                }
            LanguageInput:
                string input = Console.ReadLine().Trim();
                if (int.TryParse(input, out int languageNumber))
                {
                    if (languageNumber < 1 || languageNumber > languages.Count)
                    {
                        Console.WriteLine("Invalid language!");
                        goto LanguageInput;
                    }
                    string letters;
                    char mostCommonLetter;
                    string language = languages[languageNumber - 1];
                    switch (language)
                    {
                        case "Lithuanian":
                            letters = "AĄBCČDEĘĖFGHIĮYJKLMNOPRSŠTUŲŪVZŽ";
                            mostCommonLetter = 'I';
                            break;
                        default:
                            // English
                            letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
                            mostCommonLetter = 'E';
                            break;
                    }
                    List<int> mostCommonDivisors = GetMostCommonDivisors(File.ReadAllText(inputFile), letters.ToUpper(), 3);
                    string stringOfDivisors = mostCommonDivisors[0].ToString();
                    for (int i = 1; i < mostCommonDivisors.Count; i++)
                    {
                        stringOfDivisors += ", " + mostCommonDivisors[i].ToString();
                    }
                    Console.WriteLine(string.Format("Enter key length (suggestions: {0}):", stringOfDivisors));
                KeyLengthInput:
                    input = Console.ReadLine().Trim();
                    if (int.TryParse(input, out int keyLength))
                    {
                        if (keyLength < 1)
                        {
                            Console.WriteLine("Invalid key length!");
                            goto KeyLengthInput;
                        }
                        string text = File.ReadAllText(inputFile);
                        File.WriteAllText(outputFile, "");
                        Trees trees = new Trees(text, letters, mostCommonLetter, keyLength);
                        trees.PrintAlphabet(outputFile);
                        File.AppendAllText(outputFile, Environment.NewLine);
                        trees.PrintEncodedText(outputFile);
                        File.AppendAllText(outputFile, Environment.NewLine);
                        trees.PrintCategories(outputFile);
                        File.AppendAllText(outputFile, Environment.NewLine);
                        trees.PrintFrequencyAnalyses(outputFile);
                        File.AppendAllText(outputFile, Environment.NewLine);
                        trees.PrintAllKeys(outputFile);
                        File.AppendAllText(outputFile, Environment.NewLine);
                        trees.PrintDecodedText(outputFile);
                        Console.WriteLine(string.Format("Output file: '{0}'", Path.GetFullPath(outputFile)));
                    }
                    else
                    {
                        Console.WriteLine("Invalid key length!");
                        goto KeyLengthInput;
                    }
                }
                else
                {
                    Console.WriteLine("Invalid language!");
                    goto LanguageInput;
                }
            }
            else
            {
                Console.WriteLine(string.Format("Input file '{0}' doesn't exist!", Path.GetFullPath(inputFile)));
            }
        }

        static List<int> GetMostCommonDivisors(string text, string letters, int groupSize)
        {
            List<int> mostCommonDivisors = new List<int>
            {
                1
            };
            string onlyLetters = "";
            for (int i = 0; i < text.Length; i++)
            {
                char letter = char.ToUpper(text[i]);
                if (letters.Contains(letter))
                {
                    onlyLetters += letter;
                }
            }
            Dictionary<string, List<int>> groups = new Dictionary<string, List<int>>();
            SortedDictionary<int, int> divisors = new SortedDictionary<int, int>();
            int maxDivisorValue = 0;
            for (int i = 0; i < onlyLetters.Length - (groupSize - 1); i++)
            {
                string group = "";
                for (int j = i; j < i + groupSize; j++)
                {
                    group += onlyLetters[j];
                }
                if (i + 1 < onlyLetters.Length - (groupSize - 1) && group.Length > 0)
                {
                    int startIndex = i + 1;
                    int nextGroupIndex = onlyLetters.IndexOf(group, startIndex);
                    while (nextGroupIndex > -1)
                    {
                        int offset = nextGroupIndex - i;
                        if (groups.ContainsKey(group))
                        {
                            groups[group].Add(offset);
                        }
                        else
                        {
                            groups.Add(group, new List<int>());
                            groups[group].Add(offset);
                        }
                        for (int divisor = 2; divisor <= offset; divisor++)
                        {
                            if (offset % divisor == 0)
                            {
                                if (divisors.ContainsKey(divisor))
                                {
                                    divisors[divisor] += 1;
                                }
                                else
                                {
                                    divisors.Add(divisor, 1);
                                }
                                if (divisors[divisor] > maxDivisorValue)
                                {
                                    maxDivisorValue = divisors[divisor];
                                    mostCommonDivisors = new List<int>
                                    {
                                        divisor
                                    };
                                }
                                else if (divisors[divisor] == maxDivisorValue)
                                {
                                    mostCommonDivisors.Add(divisor);
                                }
                            }
                        }
                        startIndex = nextGroupIndex + 1;
                        if (startIndex < onlyLetters.Length - (groupSize - 1))
                        {
                            nextGroupIndex = onlyLetters.IndexOf(group, startIndex);
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }
            return mostCommonDivisors;
        }
    }
}
