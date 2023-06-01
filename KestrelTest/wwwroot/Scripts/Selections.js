$(document).ready(function () {

    //reload the page every 30 seconds.
    //setInterval(function () {
    //    DoReload();
    //}, 30000);

    //initialize jquery bootgrid. 
    //first we need to destroy the original instance as it was created in the markets.js also.
    $("#grid-data").bootgrid("destroy").bootgrid({
        ajax: true,
        url: "/Home/Selections", //this is the same as doing a jquery Ajax POST and passing the 3 values expected in the home controller
        requestHandler: function(request) {
            request.eventId = $('#CurrentEventId').val();
            request.filter = "";
            return request;
        },
        formatters:
        {
                "commands": function (column, row) {

                    const checkboxInputString = "<input type=\"checkbox\" id=\"" +
                        row.market_type_id + "_" + row.selection_id +
                        "\" onclick=\"CalcJointProbability(this)\"/>";

                    // row.commands represents the boolean value of the select input being selected.
                    if (row.commands) {
                        //on page load - 
                        //call the calculate joint probability function.
                        //Note that the callback(cb) needs to be recreated because of its context.
                        //This is because When we click on the checkbox input on the ui, 
                        //the`this` context contains the id and checked value already.
                        CalcJointProbability({
                            id: row.market_type_id + "_" + row.selection_id,
                            checked: true
                        });

                        //append the `checked` attribute to the input string and return it.

                        //create array of input attributes
                        const splitInputArray = checkboxInputString.split(" ");
                        //append the checked attribute to index 2 of the array
                        splitInputArray.splice(2, 0, 'checked');
                        //join array back as string and return it
                        return splitInputArray.join(" ");
                    }
                    return checkboxInputString;
                }
		},
        rowCount: [-1],
        navigation: 3, //show both pagination at bottom and search at top. 
        caseSensitive: false //search sensitivity.
    });

    //add pull-left class to search bar
    $('#grid-data-header .search').addClass('pull-left');
});

function DoReload() {
    //if the id exists in the div; likewise you could get the value from the window.location.search value.
    const id = $('#CurrentEventId').val();
    if (id && id > 0) {
        location.href = "/Home/Selections?eventId=" + id;
    } else {
        location.reload();
    }
}

function CalcJointProbability(cb) {
    const tokens = cb.id.split("_");
    const marketTypeId = tokens[0];
    const selectionId = tokens[1];
    const url = "/Home/UpdateJointSelection?forMarketTypeId=" + marketTypeId + "&forSelectionId=" + selectionId + "&include=" + cb.checked;
    $.getJSON(url, null,
        function (data) {
            //append the values to their divs.
            console.log(data);
            $("#joint span").text(data.Probability.toFixed(3));
            $("#price span").text(data.Price.toFixed(3));
        }
    );
}