﻿using UpWork.Abstracts;

namespace UpWork.Entities
{
    public class Social:Id
    {
        public string Name { get; set; }
        public string Link { get; set; }

        public override string ToString()
        {
            return $@"{base.ToString()}
Name: {Name}
Link: {Link}";
        }
    }
}