using System;

namespace MemoryGameLab2.Models
{
    public class User
    {
        public string Username { get; set; }
        public string ImagePath { get; set; }

        public User(string username, string imagePath)
        {
            Username = username;
            ImagePath = imagePath;
        }
    }
} 