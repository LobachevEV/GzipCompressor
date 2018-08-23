using System;
using System.Collections.Generic;

namespace GzipCompressor
{
    internal class AnimatedBar
    {
        private readonly List<string> animation;
        private int counter;

        public AnimatedBar()
        {
            animation = new List<string> {"/", "-", @"\", "|"};
            counter = 0;
        }

        public void Step()
        {
            Console.Write(animation[counter] + "\b");
            counter++;
            if (counter == animation.Count)
                counter = 0;
        }
    }
}