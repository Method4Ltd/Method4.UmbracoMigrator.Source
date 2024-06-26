﻿using System.Xml.Linq;
using Umbraco.Core.Models;

namespace Method4.UmbracoMigrator.Source.Core.Serializers
{
    public interface ISerializer<TObject> where TObject : IContentBase
    {
        XElement Serialize(TObject item);
    }
}