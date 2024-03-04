function fetchBikeRoutes(params)
{
    console.log("entering fetchBikeRoutes. ", params );
    $.ajax({
        type: "GET",
        url: "/BikeRoutes/Index?handler=FetchBikeRoutes",
        // dataType: json
    }).done(function (data) {
        console.log(data);
        params.success(data);
        return data;        
    }).fail(function(jqXHR, textStatus, errorThrown) {
        console.log("Fetch Bike Routes Failed. ", { textStatus, errorThrown} );
        return {};
    })
    console.log("exiting fetchBikeRoutes");
}


function fetchAllUsers(params)
{
    console.log("entering fetchAllUsers. ", params );
    $.ajax({
        type: "GET",
        url: "?handler=FetchAllUsers",
        // dataType: json
    }).done(function (data) {
        console.log(data);
        params.success(data);
        return data;        
    }).fail(function(jqXHR, textStatus, errorThrown) {
        console.log("Fetch All Users Failed. ", { textStatus, errorThrown} );
        return {};
    })
    console.log("exiting fetchAllUsers");

}