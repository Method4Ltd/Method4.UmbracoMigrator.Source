# Method4.UmbracoMigrator.Source
[![Mozilla Public License](https://img.shields.io/badge/MPL--2.0-orange?label=license)](https://opensource.org/licenses/MPL-2) 
[![Latest version](https://img.shields.io/nuget/v/Method4.UmbracoMigrator.Source?label=version)](https://marketplace.umbraco.com/package/method4.umbracomigrator.source) 
[![NuGet download count](https://img.shields.io/nuget/dt/Method4.UmbracoMigrator.Source?label=downloads)](https://www.nuget.org/packages/Method4.UmbracoMigrator.Source)
[![Umbraco Marketplace](https://img.shields.io/badge/umbraco-marketplace-%233544B1)](https://marketplace.umbraco.com/package/method4.umbracomigrator.source)

Import Migration Snapshots created using the Method4.UmbracoMigrator.Source package.

![A screenshot of the backoffice dashboard](https://raw.githubusercontent.com/Method4Ltd/Method4.UmbracoMigrator.Source/v8/main/docs/images/backofficedashboard.png)

## Features
Generates the Migration Snapshot (.zip) files that will be imported into your new Umbraco v10+ site using the Target package.

### [Method4.UmbracoMigrator.Target](https://github.com/Method4Ltd/Method4.UmbracoMigrator.Target)
Imports the migration snapshots and runs mappers to transform the data.

Please view the Method4.UmbracoMigrator.Target repo to find out about all of the available features, which include:
- Repeatable imports
    - Subsequent migration imports will simply update the previously migrated nodes, rather than creating new nodes, when imported.
- Custom Mappings
    - Define custom mapping logic by implementing our `IDocTypeMapping`/`IMediaTypeMapping` interfaces.
- Default Mappings
    - The package ships with built-in default mappers that perform "lazy" mappings, e.g. if an old Node's DocType matches one of our new DocTypes it will attempt to map it, and if any of its properties have identical aliases, then their raw values be copied across.
- Automatically convert Media Picker formats
    - MediaPicker (legacy) can be converted to the new MediPicker 3's format automatically

## Installation & Umbraco Version Support
> The Package's major versions will match the minimum compatible Umbraco version.<br>
> Whilst this does go against semantic versioning, it should make it easier to figure out which version will work for you.<br>
> We'll try to keep breaking changes out of minor versions, but they may happen if we need them.

<table>
  <tr>
    <th><strong>Umbraco Version</strong></th>
    <th><string>Package Version</strong></th>
  </tr>
  <tr>
    <td>v7</td>
    <td>v7.x</td>
  </tr>
  <tr>
    <td>v8</td>
    <td>v8.x</td>
  </tr>
</table>

## Links
- Repo: https://github.com/Method4Ltd/Method4.UmbracoMigrator.Source
- Docs: https://github.com/Method4Ltd/Method4.UmbracoMigrator.Source/blob/v8/main/docs/README.md

## Roadmap
Please see the [roadmap](https://github.com/Method4Ltd/Method4.UmbracoMigrator.Source/blob/v8/main/docs/ROADMAP.md) for a list of outstanding features and TODOs.

## License
Copyright &copy; [Method4](https://www.method4.co.uk/).

All source code is licensed under the [Mozilla Public License](https://github.com/Method4Ltd/Method4.UmbracoMigrator.Source/blob/v8/main/LICENSE).
