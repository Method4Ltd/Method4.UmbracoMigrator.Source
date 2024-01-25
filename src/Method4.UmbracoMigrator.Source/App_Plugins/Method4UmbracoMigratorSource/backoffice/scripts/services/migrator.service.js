(function () {
    function migratorService($http, umbRequestHelper) {
        const service = this;
        var serviceRoot = "/umbraco/backoffice/Api/MigratorSource";

        //// Functions ////
        service.getRootContent = () => {
            return umbRequestHelper.resourcePromise($http.get(serviceRoot + `/GetRootContent`));
        }
        service.getRootMedia = () => {
            return umbRequestHelper.resourcePromise($http.get(serviceRoot + `/GetRootMedia`));
        }
        service.createMigrationSnapshot = (settings) => {
            return umbRequestHelper.resourcePromise($http.post(serviceRoot + `/CreateMigrationSnapshot`, settings));
        }
        service.getAllMigrationSnapshots = () => {
            return umbRequestHelper.resourcePromise($http.get(serviceRoot + `/GetAllMigrationSnapshots`));
        }
        service.downloadMigrationSnapshot = (fileName) => {
            var url = serviceRoot + `/DownloadMigrationSnapshot?fileName=` + fileName;
            window.open(url, "_blank");
        }
        service.deleteMigrationSnapshot = (fileName) => {
            return umbRequestHelper.resourcePromise($http.delete(serviceRoot + `/DeleteMigrationSnapshot?filename=${fileName}`));
        }
        service.deleteAllMigrationSnapshots = () => {
            return umbRequestHelper.resourcePromise($http.delete(serviceRoot + `/DeleteAllMigrationSnapshots`));
        }
    }

    angular.module("migratorSource").service("migratorService", migratorService);
})();