<img src="./images/UmbracoMigratorSource_Logo.png" alt="Method4.UmbracoMigrator.Source Logo" title="Method4.UmbracoMigrator.Source Logo" height="130" align="right">

# Method4.UmbracoMigrator.Source - Contributing

There is a list of outstanding features in [/docs/ROADMAP.md](./ROADMAP.md), that will be actioned as and when we need them.
Feel free to submit a PR if you need them sooner.

---

## Getting Started
After you fork and clone down the project, you can run the sample site to test it locally.

This project has been developed using `Node v21.x` and `Visual Studio 2019`.

The sample site uses the [Clean Starter Kit](https://github.com/prjseal/Clean) doctypes, to match the Target package's sample site.

> Note: The frontend views are placeholders as they are not needed for testing the export functionality.

### Copying the package to the sample site
There is a webpack script in the `/Method4.UmbracoMigrator.Source/src/Method4.UmbracoMigrator.Source/` which will copy the `App_Plugins` folder to the sample site.

```
npm run build:dev
```

And the watch script will copy the folder when it detects that a file has changed.
```
npm run watch:dev
```

### Initial setup
On the initial setup, you will need to:
1. Install NPM packages
3. Copy the `App_Plugins` folder to the sample site using webpack
4. Run a uSync import to populate the sample site