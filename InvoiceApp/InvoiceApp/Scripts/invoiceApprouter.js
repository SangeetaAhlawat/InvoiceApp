agGrid.initialiseAgGridWithAngular1(angular);
var app = angular.module("invoiceapp", ["ngRoute","agGrid"]);
app.config(function($routeProvider) {
    $routeProvider
    .when("/", {
        templateUrl: "PartialHTML/InvoiceLedger.html"
    })
    .when("/createinvoice", {
        templateUrl: "PartialHTML/CreateInvoice.html"
    })
    
});