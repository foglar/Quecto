namespace text_editor;

class SpellChecker
{
   // Dictionary of words
        private readonly string[] _dictionary;

        public SpellChecker(string[] dictionary)
        {
            _dictionary = dictionary;
        }

        // Load the dictionary from a file
        public static string[] LoadDictionary(string path)
        {
            return File.ReadAllLines(path);
        }

        // Check if the word is in the dictionary
        public bool Check(string wordToCheck)
        {
            foreach (string word in _dictionary)
            {
                if (word == wordToCheck)
                {
                    return true;
                }
            }

            return false;
        }

        // Calculate the Levenshtein distance between two words
        private static int LevenshteinDistance(string word1, string word2)
        {
            // https://www.youtube.com/watch?v=d-Eq6x1yssU
            int[,] distance = new int[word1.Length + 1, word2.Length + 1];

            for (int i = 0; i <= word1.Length; i++)
            {
                distance[i, 0] = i;
            }

            for (int j = 0; j <= word2.Length; j++)
            {
                distance[0, j] = j;
            }

            for (int i = 1; i <= word1.Length; i++)
            {
                for (int j = 1; j <= word2.Length; j++)
                {
                    int cost = (word1[i - 1] == word2[j - 1]) ? 0 : 1;
                    distance[i, j] =
                        Math.Min(Math.Min(distance[i - 1, j] + 1, distance[i, j - 1] + 1),
                            distance[i - 1, j - 1] + cost);
                }
            }

            return distance[word1.Length, word2.Length];
        }

        // Find the closest words in the dictionary
        public string[] FindClosestWords(int maxDistance, string wordToCheck)
        {
            List<string> closestWords = new List<string>();
            // https://www.programiz.com/csharp-programming/list
            // https://www.geeksforgeeks.org/c-sharp-list-class/
            foreach (string word in _dictionary)
            {
                int distance = LevenshteinDistance(word, wordToCheck);
                if (distance <= maxDistance)
                {
                    closestWords.Add(word);
                }
            }

            return closestWords.ToArray();
        }
}