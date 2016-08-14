using System;
using System.Collections.Generic;
using Microsoft.SharePoint.Client;

// Originally from https://github.com/OfficeDev/PnP-Sites-Core
// This project doesn't take a dependency on the whole of PnPCore
// So instead, it uses the source and gives credit where it is due!
// This stuff is really good.

namespace FromPnPCore
{
    public class FieldCreationInformation
    {
        public Guid Id { get; set; }
        public string DisplayName { get; set; }
        public string InternalName { get; set; }
        public bool AddToDefaultView { get; set;}
        public IEnumerable<KeyValuePair<string, string>> AdditionalAttributes { get; set; }
        public string FieldType { get; protected set; }
        public string Group { get; set; }
        public bool Required { get; set; }

        public FieldCreationInformation(string fieldType)
        {
            FieldType = fieldType;
        }

        public FieldCreationInformation(FieldType fieldType)
        {
            FieldType = fieldType.ToString();
        }
    }

}
