using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CombinedSolution
{
    public class Message
    {
        private string input;
        private int encryptionKey;

        public Message(string inp, int key)
        {
            input = inp;
            encryptionKey = key;
        }

        public void Decrypt()
        {
            string output = "";
            int len = input.Length;
            for (int i = 0; i < len; ++i)
            {
                output = output + nextChar(input[i]);
            }
            Console.WriteLine(input + " " + output);
        }
        private char nextChar(char inp)
        {
            int inpVal = (int)inp;
            int diff = inpVal - 32;
            if ((diff - encryptionKey) < 0)
            {
                inpVal = 127 + diff;
            }
            else
            {
                inpVal = inpVal - encryptionKey;
            }
            return (char)inpVal;
        }
    }
}
