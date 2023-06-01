$(document).ready(function () {
    const grid = $("#grid-data").bootgrid({
        ajax: true,
		url: "/Home/Markets",

        requestHandler: function (request) {
	        request.eventId = window.location.href.split("?")[1].split("=")[1];
	        return request;
        },

        rowCount: [-1],
        navigation: 0, //hide search at top and pagination at bottom;
        formatters: {
            "commands": function (column, row) { //commands is from data-formatter="commands" in the table html
            //create a new button that acts as a link to the selections page
            return "<button type=\"button\" id=\"click-button\" class=\"btn btn-sm btn-primary command-edit\" data-row-id=\"" +
                row.event_id +
                "\"><span class=\"glyphicon glyphicon-zoom-in\"></span></button> ";
            }
        }
    }).on("loaded.rs.jquery.bootgrid",
        function () /* Executes after data is loaded and rendered */ {
            grid.find(".command-edit").on("click",
                function (e) {
                    const workingId = $(e.currentTarget).attr("data-row-id");
                    //when we click on the new generated button created in the formatted, we will be brought the selections page.
                    window.location.href = '/Home/Selections?eventid=' + workingId;
                }
    )
    });

    //add pull-left class to search bar
    $('#grid-data-header .search').addClass('pull-left');

    $('#search-events').click(function () {
        window.location.reload();
    });
});