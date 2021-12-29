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
var trail_length = 0;

function GetTrailLength() {
    var operation_type_time = $("#text_operation_time").text().split(" ")[2];

    var date1 = new Date(new Date().setHours(0, 0, 0, 0));
    var date2 = ConvertToTime(operation_type_time);

    var diffInSeconds = Math.abs(date1 - date2) / 1000;
    var hours = Math.floor(diffInSeconds / 60 / 60 % 24);
    var minutes = Math.floor(diffInSeconds / 60 % 60);

    var koef = hours + minutes / 60;

    trail_length = Math.floor((time_split * koef) - 1);

    ResetVisibility();
}

function ResetVisibility() {
    $(".btn-info").addClass("btn-outline-primary");
    $(".btn-info").removeClass("btn-info");

    $(".btn-outline-info").addClass("btn-outline-primary");
    $(".btn-outline-info").removeClass("btn-outline-info");
    state.time = 0;
}

function SelectTime(time, cellId) {
    return function() {
        // ResetVisibility();
        GetTrailLength();

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
    var ret = $(".datepicker").datepicker('getDate');
    var arrTime = strTime.split(":");

    ret.setHours(arrTime[0], arrTime[1], 0, 0);
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
                var isSuitable = false;
                for (let period of periods) {
                    var curPeriodFrom = new Date(period.From);
                    var curPeriodTo = new Date(period.To);

                    if (curPeriodFrom <= ConvertToTime(bu_text) && ConvertToTime(bu_text) < curPeriodTo) {
                        isSuitable = true;
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

function ConvertDate(strTime) {
    return new Date(strTime).toLocaleDateString();
}

function RenderCard(base, card, date) {

    var baseCard = document.createElement("div");
    $(baseCard).addClass("card m-3 action-card");
    //
    var cardHeader = document.createElement("div");
    $(cardHeader).addClass("card-header");
    $(cardHeader).text(ConvertTime(card.PlanWorkStartDate) + " - " + ConvertTime(card.PlanWorkFinishDate) + " (" + ConvertDate(card.PlanWorkFinishDate) + ")");
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
        location.href = '?id=' + card.Id + '&date=' + date;
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

    $.post("/api/v1/yard/createUpdateOperation", postData, function(data) {
        console.log(data);
        var card = JSON.parse(data);
        // TODO пока пусть редиректит на лк TSL
        // но тут должна быть вилка в зависимости от типа пользователя
        // location.href = '/p/user/UserHome?id=' + card.Id + '&date=' + $(".datepicker").datepicker('getDate').toLocaleDateString();
        location.href = '/p/dispatcher/DispatcherHome?id=' + card.Id + '&date=' + $(".datepicker").datepicker('getDate').toLocaleDateString();
    });


}

function ChangeState(stateChangeType) {
    var postData = {
        operation_id: $("#identeficator").text(),
        action: stateChangeType,
    };

    $.post("/api/v1/yard/changeOperationState", postData, function(data) {
        var card = JSON.parse(data);
        console.log(card);
        //alert(data);
        // window.location.href = window.location.search;
    });


}

// 

$(document).ready(function() {

    function LoadCardsList(dateParam) {

        if ($("#time_table").length) {
            manageTimeTable(dateParam);
        }
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

    if ($("#time_table").length) {
        $("#text_operation_time").text("Время операции: --:--:--");
    }

    function GetOperationTimeSpan(inpDate) {

        $.get("/api/v1/yard/OperationLength", {
                operation: $('#operations :selected').val(),
                vehicle_kind: $('#vehicle_kind :selected').val(),
            },
            function(data) {
                timeSpan = JSON.parse(data);
                var time = new Date(new Date().setHours(0, timeSpan, 0, 0));
                $("#text_operation_time").text("Время операции: " + time.toLocaleTimeString());
            });

        $.get("/api/v1/yard/BusyPeriods", {
                date: inpDate,
                operation: $('#operations :selected').val(),
                vehicle_kind: $('#vehicle_kind :selected').val(),
            },
            function(data) {
                periods = JSON.parse(data);

                RenderTable(periods);

            });



    }

    $("#time_table").addClass("d-none");

    function manageTimeTable(inpDate) {
        var val1 = $('#operations :selected').val();
        var val2 = $('#vehicle_kind :selected').val();

        if (val1.length == 0 || val2.length == 0) {
            $("#time_table").addClass("d-none");
        } else {
            $("#time_table").removeClass("d-none");
            try {
                GetOperationTimeSpan(inpDate);
            } catch (e) {
                console.log(e);
            }
        }
    }

    $("#operations").change(function() {
        manageTimeTable($(".datepicker").datepicker('getDate').toLocaleDateString());
    });

    $("#vehicle_kind").change(function() {
        manageTimeTable($(".datepicker").datepicker('getDate').toLocaleDateString());
    });
});