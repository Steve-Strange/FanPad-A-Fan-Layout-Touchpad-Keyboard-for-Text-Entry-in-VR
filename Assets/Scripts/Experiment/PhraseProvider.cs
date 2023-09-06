using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class PhraseProvider
{
    public static string PhraseSetPath = "Assets/phrases2.txt";
    public static List<string> GetPhrases(int number){
        if (PhraseSetPath == string.Empty)
        {
            return null;
        }
        var phrases = System.IO.File.ReadAllLines(PhraseSetPath); //ReadAllLines不包含换行.
        var random = new Random();
        var result = new List<string>();
        for (int i = 0; i < number; i++)
        {
            var index = random.Next(phrases.Length);
            result.Add(phrases[index]);
        }
        return result;
    }

    public static List<string> GetPhrases(int number, string path){
        var phrases = System.IO.File.ReadAllLines(path);
        var random = new Random();
        var result = new List<string>();
        for (int i = 0; i < number; i++)
        {
            var index = random.Next(phrases.Length);
            result.Add(phrases[index]);
        }
        return result;
    }
}
