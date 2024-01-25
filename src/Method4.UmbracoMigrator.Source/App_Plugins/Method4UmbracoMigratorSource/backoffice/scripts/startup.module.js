(function () {
    "use strict";
    const umbracoApp = angular.module("umbraco");
    angular.module("migratorSource", []);
    umbracoApp.requires.push("migratorSource");
})();