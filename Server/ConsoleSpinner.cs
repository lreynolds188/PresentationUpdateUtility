using System;

namespace Server
{
    class ConsoleSpinner
    {
        int counter;
        string[] sequence;

        public ConsoleSpinner()
        {
            Console.CursorVisible = false;
            counter = 0;
            sequence = new string[] { ".   ", "..  ", "... ", "...." };
        }

        /// <summary>
        /// 
        /// </summary>
        public void Run()
        {
            counter++;

            if (counter >= sequence.Length)
            {
                counter = 0;
            }

            Console.Write(sequence[counter]);
            Console.SetCursorPosition(Console.CursorLeft - sequence[counter].Length, Console.CursorTop);
            System.Threading.Thread.Sleep(1000);
        }
    }
}
