function getUrlParameter(sParam) {
    var sPageURL = window.location.search.substring(1),
        sURLVariables = sPageURL.split('&'),
        sParameterName,
        i;

    for (i = 0; i < sURLVariables.length; i++) {
        sParameterName = sURLVariables[i].split('=');

        if (sParameterName[0] === sParam) {
            return sParameterName[1] === undefined ? true : decodeURIComponent(sParameterName[1]);
        }
    }
    return false;
}

$(document).ready(function() {
    $(".datepicker").datepicker({
            format: "dd.mm.yyyy",
            autoclose: true,
            startDate: "0d",
            language: "ru",
            todayBtn: true,
            todayHighlight: true,
        })
        .datepicker("setDate", getUrlParameter("date") ? getUrlParameter("date") : 'now')
        .on("changeDate", function(e) {
            LoadCardsList(e.date.toLocaleDateString());

        });
});

function PrintCard() {
    window.print();
}

const state = {
    time: 0
};
//
const time_split = 4;
//
const time_start = 2;
const time_finish = 24;
//
var trail_length = 5;

function GetTrailLength() {
    var operation_type_time = $("#operations").children(':selected').text().split("(")[1].split(")")[0];

    var date1 = new Date().setHours(0, 0, 0, 0);
    var date2 = ConvertToTime(operation_type_time);

    var diffInSeconds = Math.abs(date1 - date2) / 1000;
    var hours = Math.floor(diffInSeconds / 60 / 60 % 24);
    var minutes = Math.floor(diffInSeconds / 60 % 60);

    var koef = hours + minutes / 60;

    trail_length = Math.floor((time_split * koef) - 1);
}

$(document).ready(function() {
    $("#time_table").addClass("d-none");

    $("#operations").change(function() {
        // hide time panel if operation type is not selected
        var val = $('#operations :selected').val();

        if (val.length == 0) {
            $("#time_table").addClass("d-none");
        } else {
            $("#time_table").removeClass("d-none");
            try {
                GetTrailLength();
            } catch (e) {
                console.log(e);
            }
        }
    });
});

function SelectTime(time, cellId) {
    return function() {
        // your code that does something with param
        $(".btn-info").addClass("btn-outline-primary");
        $(".btn-info").removeClass("btn-info");

        $(".btn-outline-info").addClass("btn-outline-primary");
        $(".btn-outline-info").removeClass("btn-outline-info");

        //
        $("." + cellId).removeClass("btn-outline-primary");
        $("." + cellId).addClass("btn-info");
        // add trail
        var parsedCellId = cellId.split("-");
        var erorOccuredCount = 0;

        for (var i = 1; i <= trail_length; i++) {
            var nextCellCollumn = Number(parsedCellId[2]) + i;
            var shift = Math.floor(nextCellCollumn / time_split);

            nextCellCollumn = nextCellCollumn % time_split;
            var nextTrailBlock = parsedCellId[0] + '-' + (shift + Number(parsedCellId[1])) + '-' + nextCellCollumn;

            $("." + nextTrailBlock).removeClass("btn-outline-primary");
            $("." + nextTrailBlock).addClass("btn-info");

            if ($("." + nextTrailBlock).length == 0) {
                erorOccuredCount++;
            } else {
                if ($("." + nextTrailBlock).prop("disabled")) {
                    erorOccuredCount++;
                }
            }
        }

        if (erorOccuredCount > 0) {
            $(".btn-info").addClass("btn-outline-primary");
            $(".btn-info").removeClass("btn-info");

            $("#eror-text").removeClass("d-none");
            if (true) {
                //if (erorOccuredCount == trail_length) {
                $("#eror-text p").text("Врямя недоступно из за типа операции, выберите другой промежуток времени или тип операции");

            } else {
                $("#eror-text p").text("Врямя уже занято, выберите другой промежуток времени");
            }
            erorOccuredCount = 0;
            state.time = 0;
        } else {
            $("#eror-text").addClass("d-none");
            state.time = time;

        }
    };
}

function ConvertToTime(strTime) {
    var ret = new Date();
    var arr = strTime.split(":");
    ret.setHours(arr[0], arr[1], 0, 0);
    return ret;
}

$(document).ready(function() {
    // default
    periods = [];

    RenderTable(periods);

});

function RenderTable(periods) {
    $(".table-1hr-row").remove();
    // 

    time_part = 60 / time_split; // 1 hr per row
    var timeSplits = [];
    for (var i = time_start; i <= time_finish; i++) {
        timeSplits[i] = [];
        for (var j = 0; j < time_split; j++) {
            timeSplits[i][j] = "" + i % 24 + ":" + ("" + (j * time_part)).padStart(2, '0');
        }
    }

    var timeTable = $("#time_table");
    //table.css("display", "none");
    // render to page
    var i = 0;
    var j = 0;
    for (let row of timeSplits) {
        var newRow = document.createElement("div");
        $(newRow).addClass("row").addClass("p-2").addClass("table-1hr-row");
        //
        if (row != undefined)
            for (let bu_text of row) {
                var newButton = document.createElement("button");
                $(newButton).addClass("btn col p-2 m-1 shadow").text(bu_text);
                //
                // check if time is awailable or not
                var isSuitable = true;
                for (let period of periods) {
                    if (ConvertToTime(period.start) <= ConvertToTime(bu_text) && ConvertToTime(bu_text) < ConvertToTime(period.finish)) {
                        isSuitable = false;
                    }
                }

                if (!isSuitable) {
                    $(newButton).addClass("btn-outline-secondary");
                    $(newButton).attr("disabled", true);
                } else {
                    $(newButton).addClass("btn-outline-primary");
                    //$(newButton).attr("disabled", true);

                }
                //
                var cellId = "bu-" + i + "-" + j % time_split;
                $(newButton).addClass(cellId);
                $(newButton).on('click', SelectTime(bu_text, cellId));
                //
                newRow.append(newButton);
                j++;
                timeTable.append(newRow);
            }
        i++;
    }
}

//
function LoadCardsList(dateParam) {
    $.get("/api/v1/yard/operationsList", {
            "date": dateParam
        },
        function(data) {
            //$(".result").html(data);
            var cards = JSON.parse(data);

            RemoveCards();
            for (let card of cards) {
                RenderCard($(".cards-list"), card, dateParam);
            }

        });
}

function RemoveCards() {
    $(".action-card").remove();
}

function GetCompaniesNames(card) {
    var result = "";
    for (let partner of card.Partners) {
        result += ((result.Length > 0) ? ", " : "") + partner.Name;
    }
    return result;

}

function ConvertTime(strTime) {
    return new Date(strTime).toLocaleTimeString();
}

function RenderCard(base, card, date) {

    var baseCard = document.createElement("div");
    $(baseCard).addClass("card m-3 action-card");
    //
    var cardHeader = document.createElement("div");
    $(cardHeader).addClass("card-header");
    $(cardHeader).text(ConvertTime(card.PlanWorkStartDate) + " - " + ConvertTime(card.PlanWorkFinishDate));
    //
    var cardBody = document.createElement("div");
    $(cardBody).addClass("card-body");
    ///
    var cardBodyMndnr = document.createElement("h5");
    $(cardBodyMndnr).addClass("card-title");
    $(cardBodyMndnr).text(GetCompaniesNames(card));

    var cardBodyInputType = document.createElement("p");
    $(cardBodyInputType).addClass("card-text");
    $(cardBodyInputType).text(card.Type.DisplayName);

    var cardBodyGate = document.createElement("p");
    $(cardBodyGate).addClass("card-text");
    $(cardBodyGate).text("Ворота: " + (card.Gate.DisplayName == "NULL" ? "не проставлены" : card.Gate.DisplayName));

    var cardBodyState = document.createElement("p");
    $(cardBodyState).addClass("card-text");
    $(cardBodyState).text("Состояние: " + card.State.DisplayName);
    ///
    cardBody.append(cardBodyMndnr);
    cardBody.append(cardBodyInputType);
    cardBody.append(cardBodyGate);
    cardBody.append(cardBodyState);

    ///

    var cardBodyButton = document.createElement("h5");
    $(cardBodyButton).addClass("btn btn-outline-info m-3");
    $(cardBodyButton).text("открыть");
    $(cardBodyButton).click(function() {
        location.href = '?id=10000005&date=' + date;
    });
    //
    baseCard.append(cardHeader);
    baseCard.append(cardBody);
    baseCard.append(cardBodyButton);
    // 
    base.append(baseCard);
}

function Create() {
    var postData = {
        operation_type: $("#operations").val(),
        gate: $("#gates").val(),
        vehicle_kind: $("#vehicle_kind").val(),
        vehicle_brand: $("#vehicle_brand").val(),
        vehicle_model: $("#vehicle_model").val(),
        vehicle_number: $("#vehicle_number").val(),
        order_id: $("#order_id").val(),
        driver_name: $("#driver_name").val(),

        start_date: $(".datepicker").datepicker('getDate').toLocaleDateString() + " " + state.time,

        partner_id: "0130",
    };
    console.log(postData);

    $.post("/api/v1/yard/createUpdateOperation", postData, function(data) {
        console.log(data);
    });


}

// 

$(document).ready(function() {
    function updateIgnoredPeriods() {
        $.get("/api/v1/yard/operationsList", {
                "date": "",
            },
            function(data) {

                periods = [{
                    'start': "9:00",
                    'finish': "10:00"
                }, {
                    'start': "12:00",
                    'finish': "13:00"
                }];

                periods = [];
                RenderTable(periods);

            });



    }

    $("#operations").change(function() {
        updateIgnoredPeriods();
    });
    $("#gates").change(function() {
        updateIgnoredPeriods();
    });
    $("#vehicle_kind").change(function() {
        updateIgnoredPeriods();
    });
});