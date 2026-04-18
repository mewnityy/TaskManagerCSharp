using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskManagerCSharp
{
    internal class CommandParser
    {
        public static string[] Parse(string text)
        {
            List<string> arguments = new List<string>();
            string word = "";
            bool isInsideLapok = false;

            foreach (char s in text)
            {
                if (s == '"')
                {
                    isInsideLapok = !isInsideLapok;
                }
                else if (s == ' ' && !isInsideLapok)
                {
                    if (word != "")
                    {
                        arguments.Add(word);
                        word = "";
                    }
                }
                else
                {
                    word += s;
                }
            }

            if (word != "")
                arguments.Add(word);

            return arguments.ToArray();
        }
    }
}
