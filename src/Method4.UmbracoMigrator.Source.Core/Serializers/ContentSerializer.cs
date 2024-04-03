/*
 * This file is a part of Method4.UmbracoMigrator.Source.
 * 
 * Modified from uSync (https://github.com/KevinJump/uSync8) 
 * Original file: (https://github.com/KevinJump/uSync/blob/v8/8.10-main/uSync8.ContentEdition/Serializers/ContentSerializer.cs)
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

using System.Linq;
using System.Xml.Linq;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Services;

namespace Method4.UmbracoMigrator.Source.Core.Serializers
{
    internal class ContentSerializer : ContentSerializerBase<IContent>, ISerializer<IContent>
    {
        private readonly IFileService _fileService;
        private readonly IPublicAccessService _publicAccessService;

        public ContentSerializer(ILocalizationService localizationService,
            IRelationService relationService,
            IEntityService entityService,
            IContentService contentService,
            IMediaService mediaService,
            IFileService fileService,
            IDataTypeService dataTypeService, IPublicAccessService publicAccessService)
            : base(localizationService, relationService, entityService, contentService, mediaService, dataTypeService)
        {
            _fileService = fileService;
            _publicAccessService = publicAccessService;
            this.relationAlias = Constants.Conventions.RelationTypes.RelateParentDocumentOnDeleteAlias;
        }

        public XElement Serialize(IContent item)
        {
            var node = InitializeNode(item, "Content");
            var info = SerializeInfo(item);
            var properties = SerializeProperties(item);

            node.Add(info);
            node.Add(properties);

            return node;
        }

        protected override XElement SerializeInfo(IContent item)
        {
            var info = base.SerializeInfo(item);

            info.Add(SerailizePublishedStatus(item));
            info.Add(SerializeSchedule(item));
            info.Add(SerializeTemplate(item));
            info.Add(SerializePublicAccess(item));

            return info;
        }

        protected virtual XElement SerializeTemplate(IContent item)
        {
            if (item.TemplateId != null && item.TemplateId.HasValue)
            {
                var template = _fileService.GetTemplate(item.TemplateId.Value);
                if (template != null)
                {
                    return new XElement("Template", new XAttribute("Key", template.Key), template.Alias);
                }
            }
            return new XElement("Template");
        }

        private XElement SerailizePublishedStatus(IContent item)
        {
            var published = new XElement("Published");

            // to make this a non-breaking change, we say default = item.published, but when 
            // dealing with cultures it isn't used. 
            published.Add(new XAttribute("Default", item.Published));

            foreach (var culture in item.AvailableCultures.OrderBy(x => x))
            {
                // Umbraco issue - if all cultures are unpublished at once, 
                // IsCulturePublblished can still return true 
                // so we need to check item.Published things 'something' is published
                published.Add(new XElement("Published", item.Published && item.IsCulturePublished(culture), new XAttribute("Culture", culture)));
            }
            return published;
        }

        protected virtual XElement SerializeSchedule(IContent item)
        {
            var node = new XElement("Schedule");
            var schedules = item.ContentSchedule.FullSchedule;

            if (schedules != null)
            {
                foreach (var schedule in schedules
                    .OrderBy(x => x.Action.ToString())
                    .ThenBy(x => x.Culture))
                {
                    node.Add(new XElement("ContentSchedule",
                        // new XAttribute("Key", schedule.Id),
                        new XElement("Culture", schedule.Culture),
                        new XElement("Action", schedule.Action),
                        new XElement("Date", schedule.Date.ToString("s"))));
                }
            }

            return node;
        }

        protected XElement SerializePublicAccess(IContent item)
        {
            var node = new XElement("PublicAccess");
            var paEntry = _publicAccessService.GetEntryForContent(item);
            if (paEntry == null) { return node; }

            var loginNode = _contentService.GetById(paEntry.LoginNodeId);
            var noAccessNode = _contentService.GetById(paEntry.NoAccessNodeId);
            var protectedNode = _contentService.GetById(paEntry.ProtectedNodeId);
            node.Add(new XAttribute("LoginNode", loginNode?.Key.ToString() ?? ""));
            node.Add(new XAttribute("NoAccessNode", noAccessNode?.Key.ToString() ?? ""));

            foreach (var publicAccessRule in paEntry.Rules)
            {
                node.Add(new XElement("Rule", publicAccessRule.RuleValue, 
                    new XAttribute("Type", publicAccessRule.RuleType)));
            }

            return node;
        }
    }
}
