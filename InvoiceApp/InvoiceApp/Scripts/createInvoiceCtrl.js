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
	
	$scope.submitInvoice=function(){
		
		
		
		//var itemList=$scope.gridOptions1.rowData;
		
	
	 // $http.post('/Home/submitInvoice', JSON.stringify(dataObj)).
	//	then(function(response) {
	//		alert(response.data);
	    //	});

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
             // Error
        });

		
		
	}
	
	
		
}); // closing controller
	


