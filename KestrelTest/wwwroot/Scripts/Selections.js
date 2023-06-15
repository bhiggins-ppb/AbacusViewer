$(document).ready(function () {

    var selectedRows = {};

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
            selectedRows[rows[i].selection_id] = rows[i].unpacked_outcomes;
        }

        var result = calcJointProbability(selectedRows);
        updateLabels(result);
    })
    .on("deselected.rs.jquery.bootgrid", function (e, rows) {
        for (var i = 0; i < rows.length; i++) {
            delete selectedRows[rows[i].selection_id];
        }

        var result = calcJointProbability(selectedRows);
        updateLabels(result);
    });

    //add pull-left class to search bar
    $('#grid-data-header .search').addClass('pull-left');
})

function updateLabels(jointProbability) {
    $("#joint span").text(jointProbability.toFixed(3));

    if (jointProbability != 0) {
        $("#joint span").text(jointProbability.toFixed(3));
        $("#price span").text((1 / jointProbability).toFixed(3));
    }
    else {
        $("#joint span").text(0);
        $("#price span").text(0);
    }
}

function calcJointProbability(selectedRows) {
    
    var outcomes = new Array();
    var count = 0;

    jQuery.each(selectedRows, function (i, val) {
        outcomes.push(selectedRows[i]);
    });
    if (outcomes.length > 0) {
        var simulationsNumber = outcomes[0].length;

        for (var i = 0; i < simulationsNumber; i++) {
            var simulationResult = 1;
            for (var j = 0; j < outcomes.length; j++) {
                simulationResult *= outcomes[j][i];
            }
            count += simulationResult;
        }
        
        return count / simulationsNumber;
    }
    else return 0;
    
    /*var selectedRows = $("#grid-data").bootgrid("getSelectedRows");
    for (var i = 0; i < selectedRows.length; i++) {

        rowData = $("#grid-data").data('.rs.jquery.bootgrid').rows[1];
        alert(rowData.unpacked_outcomes);
    }*/
}

function DoReload() {
    //if the id exists in the div; likewise you could get the value from the window.location.search value.
    const id = $('#CurrentEventId').val();
    if (id && id > 0) {
        location.href = "/Home/Selections?eventId=" + id;
    } else {
        location.reload();
    }
}
