app.controller("invoiceledgerctrl", function($scope,$http) {

    var columnDefs = [
        {headerName: "Invoice ID", field: "InvoiceId",headerCheckboxSelection: true,checkboxSelection: true},
        { headerName: "Customer Name", field: "CustomerName" },
        { headerName: "Status", field: "Status" }
    ];

 var rowData = [];


    $scope.gridOptions = {
        columnDefs: columnDefs,
		rowSelection: 'multiple'
        //rowData: rowData
    };

    getlatestInvoicelist();

    $scope.updatePaymentStatus = function () {
        var invoicelist = "";
        var invoiceCustNames = "";
        $scope.msgdiv = false;
        $scope.paidItemSelected = false;
        
        var length = $scope.gridOptions.api.getSelectedNodes().length;        
        if (length < 1) {
            $scope.msg = "Please select at least one unpaid invoice";
            $scope.msgdiv = true;
            return false;
        }

        var selectedRow = $scope.gridOptions.api.getSelectedNodes();
        angular.forEach(selectedRow, function (item, key) {
            if ("Paid" == item.data.Status) {
                $scope.paidItemSelected = true;
            }
            invoicelist = invoicelist + item.data.InvoiceId + ",";
            invoiceCustNames = invoiceCustNames + item.data.CustomerName + ",";
        });
        invoicelist = invoicelist.substring(0, invoicelist.length - 1);
        invoiceCustNames = invoiceCustNames.substring(0, invoiceCustNames.length - 1);
        //alert(invoicelist);
        if (true == $scope.paidItemSelected) {
            $scope.msgdiv = true;
            $scope.msg = "Please uncheck the paid invoice";
            return false;
         }

        var mdILObj = {
            invoiceIdList: invoicelist,
            invoiceNameList: invoiceCustNames
        };

        $http({
            method: 'POST',
            url: '/Home/updateInvoice',
            data: JSON.stringify(mdILObj),
            headers: { 'Content-Type': 'application/json' }

        })
        .then(function (result) {
            getlatestInvoicelist();
        }, function (result) {
            alert("Error while saving data into DB");
        });

    } // closing updatePaymentStatus


    function getlatestInvoicelist() {
        $http({
            method: 'GET',
            url: '/Home/GetInvoiceList',
            headers: { 'Content-Type': 'application/json' }

        }).then(function (result) {
            // alert(result.data);
            if ("" != result.data) {
                $scope.rowCount = true;
                $scope.gridOptions.api.setRowData(JSON.parse(result.data));
            } else {
                $scope.gridOptions.api.setRowData([]);
                $scope.rowCount = false;
            }
        }, function (result) {
            alert("Error while fetching data from DB");
        });
    }

});
