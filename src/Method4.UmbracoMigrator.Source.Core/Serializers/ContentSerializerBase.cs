/*
 * This file is a part of Method4.UmbracoMigrator.Source.
 * 
 * Modified from uSync (https://github.com/KevinJump/uSync8) 
 * Original file: (https://github.com/KevinJump/uSync/blob/v8/8.10-main/uSync8.ContentEdition/Serializers/ContentSerializerBase.cs)
 * Original authors: Kevin Jump
 *
 * This file has been modified by Owain Jones (Method4) for use in (https://github.com/Method4Ltd/Method4.UmbracoMigrator.Source).
 * 
 * This file is distributed under the Mozilla Public License Version 2.0.
 * The original file from uSync was licensed under the same license.
 * 
 * You can obtain a copy of the Mozilla Public License Version 2.0 at 
 * http://mozilla.org/MPL/2.0/.
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.Linq;
using System.Xml.Linq;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Entities;
using Umbraco.Core.Services;
using Umbraco.Core.Services.Implement;

namespace Method4.UmbracoMigrator.Source.Core.Serializers
{
    public abstract class ContentSerializerBase<TObject> where TObject : IContentBase
    {
        protected ILocalizationService _localizationService;
        protected IRelationService _relationService;
        protected IEntityService _entityService;
        protected IContentService _contentService;
        protected IMediaService _mediaService;
        protected IDataTypeService _dataTypeService;
        protected string relationAlias;
        protected readonly string defaultCulture;

        public ContentSerializerBase(ILocalizationService localizationService, IRelationService relationService, IEntityService entityService, IContentService contentService, IMediaService mediaService, IDataTypeService dataTypeService)
        {
            _localizationService = localizationService;
            _relationService = relationService;
            _entityService = entityService;
            _contentService = contentService;
            _mediaService = mediaService;
            _dataTypeService = dataTypeService;

            defaultCulture = _localizationService.GetDefaultLanguageIsoCode();
        }

        /// <summary>
        ///  Initialize the XElement with the core Key, Name, Level values
        /// </summary>
        protected virtual XElement InitializeNode(TObject item, string typeName)
        {
            var node = new XElement(typeName,
                new XAttribute("Key", item.Key),
                new XAttribute("Id", item.Id),
                new XAttribute("Name", item.Name),
                new XAttribute("Level", GetLevel(item)));

            if (item.ContentType.VariesByCulture())
            {
                var cultures = string.Join(",", item.AvailableCultures);
                node.Add(new XAttribute(MigratorConstants.AvailableCulturesKey, cultures));
                //node.Add(new XAttribute(MigratorConstants.AvailableCulturesKey, item.CultureInfos.Values));
            }
            else
            {
                node.Add(new XAttribute(MigratorConstants.AvailableCulturesKey, defaultCulture));
            }

            return node;
        }

        /// <summary>
        ///  Calculate the level for this item
        /// </summary>
        /// <remarks>
        ///  Trashed items get a level + 100, so they get processed last
        /// </remarks>
        protected virtual int GetLevel(TObject item) => item.Trashed ? 100 + item.Level : item.Level;

        private IEntitySlim GetTrashedParent(TObject item)
        {
            if (!item.Trashed || string.IsNullOrWhiteSpace(relationAlias)) return null;

            var parents = _relationService.GetByChild(item, relationAlias);

            if (parents != null && parents.Any())
            {
                return _entityService.Get(parents.FirstOrDefault().ParentId);
            }

            return null;
        }

        /// <summary>
        ///  Serialize the Info - (Item Attributes) Node
        /// </summary>
        protected virtual XElement SerializeInfo(TObject item)
        {
            var info = new XElement("Info");

            // find parent. 
            var parentKey = Guid.Empty;
            var parentName = "";
            if (item.ParentId != -1)
            {
                dynamic parentItem = null;

                if (item is IContent)
                {
                    parentItem = _contentService.GetById(item.ParentId);
                }
                else if (item is IMedia)
                {
                    parentItem = _mediaService.GetById(item.ParentId);
                }

                if (parentItem != null)
                {
                    parentKey = parentItem.Key;
                    parentName = parentItem.Name;
                }
            }

            info.Add(new XElement("Parent", new XAttribute("Key", parentKey), parentName));
            //info.Add(new XElement("Path", GetItemPath(item)));
            info.Add(GetTrashedInfo(item));
            info.Add(new XElement("ContentType", item.ContentType.Alias));
            info.Add(new XElement("CreateDate", item.CreateDate.ToString("s")));

            var title = new XElement("NodeName", new XAttribute("Default", item.Name));
            foreach (var culture in item.AvailableCultures.OrderBy(x => x))
            {
                title.Add(new XElement("Name", item.GetCultureName(culture), new XAttribute("Culture", culture)));
            }
            info.Add(title);

            info.Add(new XElement("SortOrder", item.SortOrder));

            return info;
        }

        /// <summary>
        ///  get the trash information (including non-trashed parent)
        /// </summary>
        private XElement GetTrashedInfo(TObject item)
        {
            var trashed = new XElement("Trashed", item.Trashed);
            if (item.Trashed)
            {
                var trashedParent = GetTrashedParent(item);
                if (trashedParent != null)
                {
                    trashed.Add(new XAttribute("Parent", trashedParent.Key));
                }
            }
            return trashed;
        }

        /// <summary>
        ///  serialize all the properties for the item
        /// </summary>
        protected virtual XElement SerializeProperties(TObject item)
        {
            var node = new XElement("Properties");

            var contentTypeAlias = item.ContentType?.Alias;

            foreach (var property in item.Properties.OrderBy(x => x.Alias))
            {
                var dataType = _dataTypeService.GetDataType(property.PropertyType.DataTypeKey); // Get the property's DataType
                var propertyEditorAlias = dataType.Editor.Alias; // Get the DataType's underlying Property Editor alias

                var propertyNode = new XElement(property.Alias, new XAttribute("PropertyEditorAlias", propertyEditorAlias));

                // Add values
                foreach (var value in property.Values.OrderBy(x => x.Culture))
                {
                    var valueNode = new XElement("Value");

                    if (!string.IsNullOrWhiteSpace(value.Culture))
                    {
                        valueNode.Add(new XAttribute("Culture", value.Culture ?? string.Empty));
                    }

                    if (!string.IsNullOrWhiteSpace(value.Segment))
                    {
                        valueNode.Add(new XAttribute("Segment", value.Segment ?? string.Empty));
                    }

                    if (propertyEditorAlias == "Umbraco.DateTime" && DateTime.TryParse(value?.EditedValue?.ToString(), out var dateValue))
                    {
                        // Format value if it is a DateTime
                        valueNode.Add(new XCData(dateValue.ToString("s")));
                    }
                    else
                    {
                        // Save RAW value
                        valueNode.Add(new XCData(value?.EditedValue?.ToString() ?? string.Empty));
                    }

                    propertyNode.Add(valueNode);
                }

                // Add blank value if no values found
                if (property.Values == null || property.Values.Count == 0)
                {
                    var emptyValue = new XElement("Value");
                    emptyValue.Add(new XCData(string.Empty));
                    propertyNode.Add(emptyValue);
                }

                if (propertyNode.HasElements)
                {
                    node.Add(propertyNode);
                }
            }

            return node;
        }
    }
}