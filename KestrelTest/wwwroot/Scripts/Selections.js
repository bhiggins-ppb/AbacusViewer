$(document).ready(function () {

    var jointProbability = 1;

    $("#grid-data").bootgrid({

        selection: true,
        multiSelect: true,
        rowSelect: true,
        keepSelection: true,

        ajax: true,
        url: "/Home/Selections", //this is the same as doing a jquery Ajax POST and passing the 3 values expected in the home controller
        requestHandler: function (request) {
            request.eventId = $('#CurrentEventId').val();
            request.filter = "";
            return request;
        },

        rowCount: [100, -1],
        navigation: 3, //show both pagination at bottom and search at top. 
        caseSensitive: false //search sensitivity.
    })
    .on("selected.rs.jquery.bootgrid", function (e, rows) {
        for (var i = 0; i < rows.length; i++) {
            jointProbability *= rows[i].probability; // TODO: replace dummy calculation
        }

        var selectedRows = $("#grid-data").bootgrid("getSelectedRows");

        $("#joint span").text(jointProbability.toFixed(3));

        if (jointProbability != 0) {
            $("#price span").text((1 / jointProbability).toFixed(3));
        }
        
    })
    .on("deselected.rs.jquery.bootgrid", function (e, rows) {
        for (var i = 0; i < rows.length; i++) {
            jointProbability /= rows[i].probability;
        }

        var selectedRows = $("#grid-data").bootgrid("getSelectedRows");

        $("#joint span").text(jointProbability.toFixed(3));

        if (jointProbability != 0) {
            $("#price span").text((1 / jointProbability).toFixed(3));
        }
    });

    //add pull-left class to search bar
    $('#grid-data-header .search').addClass('pull-left');
})

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
    const probability = tokens[3];

    // TODO: add actual implementation
}