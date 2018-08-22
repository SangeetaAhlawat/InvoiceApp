app.controller("createinvoicectrl", function($scope,$http,$location, $httpParamSerializerJQLike) {

    var columnDefs = [
        {headerName: "Item Id", field: "item",editable: true},
		{headerName: "Description", field: "des",editable: true},
        {headerName: "Quantity", field: "qty",editable: true},
        {headerName: "Rate/Unit", field: "rate",editable: true},
		{headerName: "Cost", field: "cost",editable: true}
    ];

    var rowData = [
	    //{item: "1", des: "S3-watch", qty: "1",rate:"350",cost:"350"},
        //{item: "2", des: "phone", qty: "1",rate:"300",cost:"300"},
        //{item: "3", des: "apple", qty: "1",rate:"50",cost:"50"},
    ];

    $scope.gridOptions1 = {
        columnDefs: columnDefs,
	    rowData: rowData,
		onCellEditingStopped: postEditCaculation 
    };
	
	
	function postEditCaculation(event){
		var totalcost=0.0;
	//	var qty=event.data.qty;
	//	var rate=event.data.rate;
	//	var rowData = $scope.gridOptions1.rowData;

		var rowData = [];
		$scope.gridOptions1.api.forEachNode(function (node) {
		    rowData.push(node.data);
		});
		
		angular.forEach(rowData, function(item, key) {
			var qty=item.qty;
			var rate=item.rate;
			var cost=qty*rate;
			item.cost=cost;
			totalcost=totalcost+cost;
		});
		
		$scope.gridOptions1.api.refreshInMemoryRowModel();
	
		$scope.totalcost=totalcost;
    }	 //closing postEditCaculation
	
	
	$scope.addItemToInvoice=function(){
		$scope.gridOptions1.api.addItems([{item: "", des: "", qty: "",rate:"",cost:""}]);
		
	}


	$scope.invoicePreview = function () {
	    var tableList = [];
	    $scope.gridOptions1.api.forEachNode(function (node) {
	        tableList.push(node.data);
	    });

	    $scope.tableList = tableList;


	    $('#previewmodal').modal();
	}
	
	
	$scope.submitInvoice=function(){
	    $scope.msgdiv = false;
	    $scope.msgCustDiv = false;
		var itemList = [];
		$scope.gridOptions1.api.forEachNode(function (node) {
		    itemList.push(node.data);
		});

		var mdObj = {
		    custName: $scope.custName,
		    custAddress: $scope.custAddress,
		    compName: $scope.compName,
		    compAddress: $scope.compAddress,
		    totalcost: $scope.totalcost,
		    invoicePaid: $scope.invoicePaid,
		    gridItems: itemList
		};
        		
		if ($scope.custName == "" || $scope.custName == null) {
		    $scope.msgCust = "Please add Customer Name";
		    $scope.msgCustDiv = true;
		    return false;
		}

		if ($scope.custAddress == "" || $scope.custAddress == null) {
		    $scope.msgCust = "Please add Customer Address";
		    $scope.msgCustDiv = true;
		    return false;
		}

		if ($scope.compName == "" || $scope.compName == null) {
		    $scope.msgCust = "Please add Company Name";
		    $scope.msgCustDiv = true;
		    return false;
		}

		if ($scope.compAddress == "" || $scope.compAddress == null) {
		    $scope.msgCust = "Please add Company Address";
		    $scope.msgCustDiv = true;
		    return false;
		}
		
		var length = itemList.length;
		if (length < 1) {
		    $scope.msg = "Please add items in invoice";
		    $scope.msgdiv = true;
		    return false;
		}
		else {
		    var isBreak = false;
		    angular.forEach(itemList, function (item, key) {
		        var qty = item.qty;
		        var rate = item.rate;
		        var des = item.des;
		        if (qty == "" && des == "" && rate == "") {
		            $scope.msg = "Please add items below in the grid";
		            $scope.msgdiv = true;
		            isBreak = true;
		            return false;
		        }
		    });
            
		    if(isBreak == true)
		        return false;
		}


		$http({
		    method: 'POST',
		    url: '/Home/submitInvoice',
		    data: JSON.stringify(mdObj),
	        headers: { 'Content-Type': 'application/json' }

	    })
        .then(function (result) {            
            alert(result.data);
            $location.url('/');
        }, function (result) {
            alert("Error while saving data into DB");
        });

		
		
	}
	
	
		
}); // closing controller
	


