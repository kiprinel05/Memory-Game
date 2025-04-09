using System.Collections.Generic;

namespace MemoryGameLab2.Models
{
    public class GameCategory
    {
        public string Name { get; set; }
        public List<string> ImagePaths { get; set; }

        public GameCategory(string name, List<string> imagePaths)
        {
            Name = name;
            ImagePaths = imagePaths;
        }
    }
} 