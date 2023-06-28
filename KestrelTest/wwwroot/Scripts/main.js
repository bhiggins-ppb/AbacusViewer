function searchEvent(inPlay) {
    if (validateEventId()) {
        document.getElementById("selections-list").style.visibility = "visible";

        var selectedRows = {};

        $("#grid-data").bootgrid("destroy");
        $("#grid-data").bootgrid({

            selection: true,
            multiSelect: true,
            rowSelect: true,
            keepSelection: true,

            ajax: true,
            url: "/Home/Selections", //this is the same as doing a jquery Ajax POST and passing the 3 values expected in the home controller
            requestHandler: function (request) {
                request.eventId = $('#event-id-input').val();
                request.filter = "";
                request.inPlay = inPlay;
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
    }
    else {
        document.getElementById("selections-list").style.visibility = "hidden";
    }
}

const validation = {
    isNumber: function (str) {
        const pattern = /^\d+$/;
        return pattern.test(str);  // returns a boolean
    }
};

function validateEventId() {
    const eventId = document.getElementById('event-id-input').value;
    const formDomNode = document.getElementById('search-form');
    const isValidEventId = validation.isNumber(eventId);
    if (isValidEventId) {
        formDomNode.classList.add('has-success');
        formDomNode.classList.remove('has-error');
    } else {
        formDomNode.classList.add('has-error');
        formDomNode.classList.remove('has-success');
    }
    return isValidEventId;
}

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