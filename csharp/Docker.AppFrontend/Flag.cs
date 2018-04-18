using System;
using System.Collections.Generic;
using System.Text;

namespace Docker.AppFrontend
{
    struct Flag
    {
        public string Name { get;  }
        public char ShortHand { get;  }
        public string Description { get; }
        public bool Mandatory { get; }
        public Action<string> OnValueSet { get;  }

        public bool IsSwitch { get;  }

        public Flag(string name, Action<string> onValueSet, string description = null, bool mandatory = false, char shortHand = '-', bool isSwitch = false)
        {
            Name = name;
            Description = description;
            Mandatory = mandatory;
            OnValueSet = onValueSet;
            ShortHand = shortHand;
            IsSwitch = isSwitch;
        }

        public bool HasShortHand => ShortHand != '-';

        public string NameAndShortHand => HasShortHand ? $"--{Name}, -{ShortHand}" : $"--{Name}";
    }
    
}
