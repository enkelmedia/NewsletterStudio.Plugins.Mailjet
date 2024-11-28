(function () {
    'use strict';

    var controller = function ($http, $scope) {

        var vm = this;
        vm.data = undefined;

        vm.init = function (parentVm) {

            console.log('ini-vm',vm);

            this.vm = parentVm;

            var model = {
                test: 'lorem from controller',
                settings: this.vm.model.emailServiceProvider.settings,
                workspaceKey: this.vm.model.key,
                baseUrl : this.vm.model.baseUrl
            };

            $http.post('/Umbraco/backoffice/NewsletterStudioMailjet/Mailjet/GetConfiguration', model).then(function (res) {

                console.log(res);
                vm.data = res.data;

            });

            $scope.$$watch('parentVm',function() {
                console.log('plugin vm changed');
            });

        }

        vm.configureNow = function() {

            var model = {
                settings: this.vm.model.emailServiceProvider.settings,
                workspaceKey: this.vm.model.key,
                baseUrl: this.vm.model.baseUrl
            };

            $http.post('/Umbraco/backoffice/NewsletterStudioMailjet/Mailjet/ConfigureNow', model).then(function (res) {

                console.log(res);
                
                console.log('Configuration successful');

            });

        }
        
    }

    angular.module('umbraco').controller('NewsletterStudio.Plugins.Mailjet.SettingsController', ['$http','$scope',controller]);
})();
