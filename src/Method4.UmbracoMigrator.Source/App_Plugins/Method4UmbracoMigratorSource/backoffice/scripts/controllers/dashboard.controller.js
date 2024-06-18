(function () {
    "use strict";

    function dashboardController($controller, $scope, $timeout, notificationsService, migratorService) {
        const vm = this;

        const errorMessage = "Uh oh, something went wrong! Check the log for more information."

        // setup default properties
        vm.rootNodes = null;
        vm.rootMediaNodes = null;
        vm.currentSnapshots = [];
        vm.settings = {    
            SelectedRootNodes: [],
            SelectedRootMediaNodes: [],
            IncludeMediaFiles: true,
            IncludeOnlyPublished: false
        };

        initialise();

        //// Public Functions ////
        vm.toggleSetting = (settingName) => {
            vm.settings[settingName] = !vm.settings[settingName];
        }

        vm.toggleNode = (id) => {
            var index = vm.settings.selectedRootNodes.indexOf(id);
            if (index > -1) {
                vm.settings.selectedRootNodes.splice(index, 1);
            }
            else {
                vm.settings.selectedRootNodes.push(id);
            }
            console.log("Selected Root Nodes: " + vm.settings.selectedRootNodes.join());
        }

        vm.toggleMediaNode = (id) => {
            var index = vm.settings.selectedRootMediaNodes.indexOf(id);
            if (index > -1) {
                vm.settings.selectedRootMediaNodes.splice(index, 1);
            }
            else {
                vm.settings.selectedRootMediaNodes.push(id);
            }
            console.log("Selected Root Media Nodes: " + vm.settings.selectedRootMediaNodes.join());
        }

        vm.downloadSnapshot = (fileName) => {
            migratorService.downloadMigrationSnapshot(fileName);
        }

        vm.deleteSnapshot = (fileName) => {
            migratorService.deleteMigrationSnapshot(fileName)
                .then(function (data) {
                    notificationsService.success("😍 ", fileName + " deleted!");
                    initialise();
                }).catch(function (error) {
                    console.error(error);
                    notificationsService.error("😵 ", errorMessage);
                    initialise();
                });
        }

        vm.deleteAllSnapshots = () => {
            vm.deleteAllButtonState = 'busy';
            migratorService.deleteAllMigrationSnapshots()
                .then(function (data) {
                    notificationsService.success("🗑️ ", "All migration snapshots deleted!");
                    vm.deleteAllButtonState = 'success';
                    initialise();
                }).catch(function (error) {
                    console.error(error);
                    notificationsService.error("😵 ", errorMessage);
                    vm.deleteAllButtonState = 'error';
                    initialise();
                });
        }

        vm.createMigrationSnapshot = () => {
            vm.createButtonState = 'busy';

            if (vm.settings.selectedRootNodes.length === 0 && vm.settings.selectedRootMediaNodes.length === 0) {
                notificationsService.warning("🤔 ", "You haven't chosen any root nodes!");
                vm.createButtonState = 'error';
                return;
            }

            //var loader = document.querySelector("#migrator-loader");
            //loader.hidden = false;

            migratorService.createMigrationSnapshot(vm.settings)
                .then(function (data) {
                    console.log("Migration created!")
                    notificationsService.success("😍 ", "Migration created!");
                    vm.createButtonState = 'success';
                    initialise();
                    //loader.hidden = true;
                }).catch(function (error) {
                    console.error(error);
                    notificationsService.error("😵 ", errorMessage);
                    vm.createButtonState = 'error';
                    initialise();
                    //loader.hidden = true;
                });
        }

        vm.selectAllMedia = () => {
            if (vm.rootMediaNodes == null) { return; }
            vm.settings.selectedRootMediaNodes = [];
            vm.rootMediaNodes.forEach(item => {
                vm.settings.selectedRootMediaNodes.push(item.Id);
            });
        }

        vm.selectAllContent = () => {
            if (vm.rootNodes == null) { return; }
            vm.settings.selectedRootNodes = [];
            vm.rootNodes.forEach(item => {
                vm.settings.selectedRootNodes.push(item.Id);
            });
        }

        vm.deselectAllMedia = () => {
            vm.settings.selectedRootMediaNodes = [];
        }

        vm.deselectAllContent = () => {
            vm.settings.selectedRootNodes = [];
        }

        //// Private Functions ////
        function initialise() {
            vm.rootNodes = null;
            vm.rootMediaNodes = null;
            vm.settings.selectedRootNodes = [];
            vm.settings.selectedRootMediaNodes = [];
            vm.currentSnapshots = [];

            getRootNodes();
            getRootMedia();
            getSnapshots();
        }

        function getRootNodes() {
            migratorService.getRootContent()
                .then(function (data) {
                    vm.rootNodes = data;
                    data.forEach(item => {
                        vm.settings.selectedRootNodes.push(item.Id);
                    });
                    console.log("Selected Root Nodes: " + vm.settings.selectedRootNodes.join())
                }).catch(function (error) {
                    console.error(error);
                    notificationsService.error("😵 ", "Failed to retrieve root nodes");
                });
        }

        function getRootMedia() {
            migratorService.getRootMedia()
                .then(function (data) {
                    vm.rootMediaNodes = data;
                    data.forEach(item => {
                        vm.settings.selectedRootMediaNodes.push(item.Id);
                    });
                    console.log("Selected Root Media Nodes: " + vm.settings.selectedRootMediaNodes.join())
                }).catch(function (error) {
                    console.error(error);
                    notificationsService.error("😵 ", "Failed to retrieve root media nodes");
                });
        }

        function getSnapshots() {
            migratorService.getAllMigrationSnapshots()
                .then(function (data) {
                    if (data !== null && data !== undefined) {
                        for (var i in data) {
                            data[i].CreateDate = new Date(data[i].CreateDate);
                            data[i].SizeBytes = formatBytes(data[i].SizeBytes, 2);
                        }
                    }
                    vm.currentSnapshots = data;
                }).catch(function (error) {
                    console.error(error);
                    notificationsService.error("😵 ", "Failed to retrieve current snapshots");
                });
        }

        /// formatBytes source: https://gist.github.com/zentala/1e6f72438796d74531803cc3833c039c
        function formatBytes(bytes, decimals) {
            if (bytes == 0) return '0 Bytes';
            var k = 1024,
                dm = decimals || 2,
                sizes = ['Bytes', 'KB', 'MB', 'GB', 'TB', 'PB', 'EB', 'ZB', 'YB'],
                i = Math.floor(Math.log(bytes) / Math.log(k));
            return parseFloat((bytes / Math.pow(k, i)).toFixed(dm)) + ' ' + sizes[i];
        }
    }

    angular.module("migratorSource").controller("migratorSource.dashboardController", dashboardController);
})();