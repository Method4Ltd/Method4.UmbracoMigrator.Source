/*
 * This file is a part of Method4.UmbracoMigrator.Source.
 * 
 * Modified from uSync (https://github.com/KevinJump/uSync8) 
 * Original file: (https://github.com/KevinJump/uSync/blob/v8/8.10-main/uSync8.ContentEdition/Serializers/MediaSerializer.cs)
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

using System.Xml.Linq;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Method4.UmbracoMigrator.Source.Core.Serializers
{
    internal class MediaSerializer : ContentSerializerBase<IMedia>, ISerializer<IMedia>
    {
        public MediaSerializer(ILocalizationService localizationService,
            IRelationService relationService,
            IEntityService entityService,
            IContentService contentService,
            IMediaService mediaService,
            IDataTypeService dataTypeService)
            : base(localizationService, relationService, entityService, contentService, mediaService, dataTypeService)
        {
            this.relationAlias = Constants.Conventions.RelationTypes.RelateParentMediaFolderOnDeleteAlias;
        }

        public XElement Serialize(IMedia item)
        {
            var node = InitializeNode(item, "Media");

            var info = SerializeInfo(item);
            var properties = SerializeProperties(item);

            node.Add(info);
            node.Add(properties);

            return node;
        }
    }
}