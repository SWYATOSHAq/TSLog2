function GetUrlParameter(sParam) {
    var sPageURL = window.location.search.substring(1),
        sURLVariables = sPageURL.split('&'),
        sParameterName,
        i;

    for (i = 0; i < sURLVariables.length; i++) {
        sParameterName = sURLVariables[i].split("=");
        if (sParameterName[0] === sParam) {
            return sParameterName[1] === undefined ? true : decodeURIComponent(sParameterName[1]);
        }
    }
    return false;
}

function PrintCard() {
    window.open("/PrintDispersal?id=" + $("#identeficator").text(), "_blank");
}

function PrintStatisticsTables() {
    $(".interval-print").addClass("d-none");
    window.print();
    $(".interval-print").removeClass("d-none");
}

function PrintBlock(cellId) {
    // hide tables
    $(".statistics-table").addClass("d-none");
    // hide all intervals but selected
    $(".interval-print").addClass("d-none");
    $(".interval-print-" + cellId).removeClass("d-none");
    
    window.print();
    // unhide all
    $(".statistics-table").removeClass("d-none");
    $(".interval-print").removeClass("d-none");
}

const state = {
    time: 0
};

const timeSplit = 4;
const timeStart = 2;
const timeFinish = 23;

var trailLength = 0;

function GetTrailLength() {
    var operation_type_time = $("#text_operation_time").text().split(" ")[0];

    var date1 = new Date(new Date().setHours(0, 0, 0, 0));
    var date2 = ConvertToTime(operation_type_time);

    var diffInSeconds = Math.abs(date1 - date2) / 1000;
    var hours = Math.floor(diffInSeconds / 60 / 60 % 24);
    var minutes = Math.floor(diffInSeconds / 60 % 60);
    var koef = hours + minutes / 60;

    trailLength = Math.floor((timeSplit * koef) - 1);

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

        $("." + cellId).removeClass("btn-outline-primary");
        $("." + cellId).addClass("btn-info");
        var parsedCellId = cellId.split("-");
        var erorOccuredCount = 0;

        for (var i = 1; i <= trailLength; i++) {
            var nextCellCollumn = Number(parsedCellId[2]) + i;
            var shift = Math.floor(nextCellCollumn / timeSplit);

            nextCellCollumn = nextCellCollumn % timeSplit;
            var nextTrailBlock = parsedCellId[0] + "-" + (shift + Number(parsedCellId[1])) + "-" + nextCellCollumn;

            $("." + nextTrailBlock).removeClass("btn-outline-primary");
            $("." + nextTrailBlock).addClass("btn-info");

            if ($("." + nextTrailBlock).length == 0) {
                erorOccuredCount++;
            }
            else {
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
                $("#eror-text p").text("Время недоступно из за типа операции, выберите другой промежуток времени или тип операции");
            }
            else {
                $("#eror-text p").text("Время уже занято, выберите другой промежуток времени");
            }
            erorOccuredCount = 0;
            state.time = 0;
        }
        else {
            $("#eror-text").addClass("d-none");
            state.time = time;
            $("#text_operation_time").text($("#text_operation_time").text().split(" (")[0] + " (выбранное время: " + time + ")");
        }
    };
}

function ConvertToTime(stringTime) {
    var workStartDatePicker = document.getElementById("work-start-date-picker");
    var stringDate = workStartDatePicker.value.replaceAll(".", "/");
    stringDate = stringDate.replace(/(\d+[/])(\d+[/])/, '$2$1');

    var result = new Date(stringDate);
    var times = stringTime.split(":");
    result.setHours(times[0], times[1], 0, 0);
    return result;
}

function MultiplyTextTimeSpan(stringTime, multiplier) {
    var timeArr = stringTime.split(':'); // split it at the colons

    // Minutes are worth 60 seconds. Hours are worth 60 minutes.
    var seconds = (+timeArr[0]) * 60 * 60 + (+timeArr[1]) * 60 + (+timeArr[2]);
    var newSeconds = seconds * multiplier;

    var t = new Date();
    t.setHours(0, 0, newSeconds, 0);
    return t;
}

$(document).ready(function() {
    periods = [];
    RenderTable(periods);
});

function RenderTable(periods) {
    $(".table-1hr-row").remove();

    time_part = 60 / timeSplit; // 1 hr per row
    var timeSplits = [];
    for (var i = timeStart; i <= timeFinish; i++) {
        timeSplits[i] = [];
        for (var j = 0; j < timeSplit; j++) {
            timeSplits[i][j] = "" + i % 24 + ":" + ("" + (j * time_part)).padStart(2, "0");
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
        
        if (row != undefined)
            for (let bu_text of row) {
                var newButton = document.createElement("button");
                $(newButton).addClass("btn col p-2 m-1 shadow").text(bu_text);
                
                // check if time is awailable or not
                var isSuitable = false;
                var today = new Date();

                for (let period of periods) {
                    var curPeriodFrom = new Date(period.From);
                    var curPeriodTo = new Date(period.To);

                    if (curPeriodFrom <= ConvertToTime(bu_text) && ConvertToTime(bu_text) < curPeriodTo && ConvertToTime(bu_text) >= today) {
                        isSuitable = true;
                    }
                }

                if (!isSuitable) {
                    $(newButton).addClass("btn-outline-secondary");
                    $(newButton).attr("disabled", true);
                } else {
                    $(newButton).addClass("btn-outline-primary");
                }
                
                var cellId = "bu-" + i + "-" + j % timeSplit;
                $(newButton).addClass(cellId);
                $(newButton).on('click', SelectTime(bu_text, cellId));
                newRow.append(newButton);
                j++;
                timeTable.append(newRow);
            }
        i++;
    }
}

function RemoveCards() {
    $(".action-card").remove();
}

function GetCompaniesNames(operation) {
    var result = "";
    for (let partner of operation.Partners) {
        result += ((result.Length > 0) ? ", " : "") + partner.Name;
    }
    return result;
}

function ConvertTime(stringTime) {
    return new Date(stringTime).toLocaleTimeString();
}

function ConvertDate(stringDate) {
    return new Date(stringDate).toLocaleDateString();
}

function RenderCard(base, card) {
    var baseCol = document.createElement("div");
    $(baseCol).addClass("col action-card");

    var card0 = document.createElement("div");
    $(card0).addClass("container2");

    var baseCard = document.createElement("div");
    $(baseCard).addClass("m-1 card");
    
    var cardHeader = document.createElement("div");
    $(cardHeader).addClass("card-header");
    $(cardHeader).css("background-color", card.DisplayColor);
    $(cardHeader).text(ConvertTime(card.PlanWorkStartDate) + " - " + ConvertTime(card.PlanWorkFinishDate) + " (" + ConvertDate(card.PlanWorkFinishDate) + ")");

    var cardBody = document.createElement("div");
    $(cardBody).addClass("card-body");
    
    var cardBodyMndnr = document.createElement("h5");
    $(cardBodyMndnr).addClass("card-title");
    $(cardBodyMndnr).text(GetCompaniesNames(card));

    var cardBodyInputType = document.createElement("p");
    $(cardBodyInputType).addClass("card-text block-size-2");
    $(cardBodyInputType).text(card.Type.DisplayName);

    var cardBodyGate = document.createElement("p");
    $(cardBodyGate).addClass("card-text  text-primary font-1-3");
    $(cardBodyGate).text((card.Gate.DisplayName == "NULL" ? "не проставлены" : card.Gate.DisplayName));

    var cardBodyState = document.createElement("p");
    $(cardBodyState).addClass("card-text");
    $(cardBodyState).text(card.State.DisplayName);

    var cardBodyVehicleKind = document.createElement("p");
    $(cardBodyVehicleKind).addClass("card-text block-size-2");
    $(cardBodyVehicleKind).text(card.VehicleKind.DisplayName);

    var cardBodyVehicleNumber = document.createElement("p");
    $(cardBodyVehicleNumber).addClass("card-text");
    $(cardBodyVehicleNumber).text(card.VehicleNumber);

    var cardBodyDriverName = document.createElement("p");
    $(cardBodyDriverName).addClass("card-text block-size-2");
    $(cardBodyDriverName).text(card.DriverName);

    cardBody.append(cardBodyMndnr);
    cardBody.append(cardBodyInputType);
    cardBody.append(cardBodyGate);
    cardBody.append(cardBodyState);
    cardBody.append(cardBodyVehicleKind);
    cardBody.append(cardBodyVehicleNumber);
    cardBody.append(cardBodyDriverName);

    var cardBodyButton = document.createElement("h5");
    $(cardBodyButton).addClass("btn btn-outline-info m-3 open-operation");
    $(cardBodyButton).text("открыть");
    $(cardBodyButton).click(function () {
        /*
        var ref = "?date=" + date;
        const params = new URLSearchParams(window.location.search)
        for (const param of params) {
            if (param[0] == "gate") {
                ref = ref + "&gate=" + param[1];
            }
            else if (param[0] == "type") {
                ref = ref + "&type=" + param[1];
            }
            else if (param[0] == "partner") {
                ref = ref + "&partner=" + param[1];
            }
            else if (param[0] == "vehicleKind") {
                ref = ref + "&vehicleKind=" + param[1];
            }
            else if (param[0] == "state") {
                ref = ref + "&state=" + param[1];
            }
        }
        */
        var ref = CreateFiltersRequest();
        ref = ref + "&id=" + card.Id;
        location.href = ref;
    });
    
    baseCard.append(cardHeader);
    baseCard.append(cardBody);
    baseCard.append(cardBodyButton);

    card0.append(baseCard)
    baseCol.append(card0);
    base.append(baseCol);
}

function Create() {
    // Check for required fields
    // time, operation_type, vehicle_kind, vehicle_brand, vehicle_model, vehicle_number
    if (state.time != 0 &&
        $("#operation-types-dropdown").val() != "" &&
        $("#vehicle_kinds-dropdown").val() != "" &&
        $("#vehicle_brand").val() != "" &&
        $("#vehicle_model").val() != "" &&
        $("#vehicle_number").val() != "" &&
        $("#driver_name").val() != "" &&
        $("#order_id").val() != "" &&
        $("#partner").val() != "") {

        var workStartDatePicker = document.getElementById("work-start-date-picker");
        var postData = {
            operation_type: $("#operation-types-dropdown").val(),
            gate: $("#card-operation .gates").val(),
            vehicle_kind: $("#vehicle-kinds-dropdown").val(),
            vehicle_brand: $("#vehicle_brand").val(),
            vehicle_model: $("#vehicle_model").val(),
            vehicle_number: $("#vehicle_number").val(),
            order_id: $("#order_id").val(),
            driver_name: $("#driver_name").val(),

            driver_phone: $("#driver_phone").val(),
            manager_name: $("#manager_name").val(),
            manager_phone: $("#manager_phone").val(),
            manager_email: $("#manager_email").val(),
            note: $("#note").val(),

            start_date: workStartDatePicker.value + " " + state.time,
            partner_id: $("#partners-dropdown ").val(),
        };

        $.post("/api/v1/yard/createUpdateOperation", postData, function(data) {
            console.log(data);

            if (data.startsWith("eror")) {
                alert(data.split("||")[1]);
            } else {
                var card = JSON.parse(data);
            }

            // TODO: пока пусть редиректит на лк TSL
            // но тут должна быть вилка в зависимости от типа пользователя

            location.href = '/DispatcherHome?id=' + card.Id +
                '&from_date=' + workStartDatePicker.value +
                '&to_date=' + workStartDatePicker.value;
        });
    } else {
        alert("Не все обязательные поля введены");
    }
}

function Update() {
    // Check for required fields
    // time, operation_type, vehicle_kind, vehicle_brand, vehicle_model, vehicle_number

    var workStartDatePicker = document.getElementById("work-start-date-picker");

    if ($("#card-operation #operation-types-dropdown").val() != "" &&
        $("#card-operation #vehicle-kinds-dropdown").val() != "" &&
        $("#card-operation #vehicle_brand").val() != "" &&
        $("#card-operation #vehicle_model").val() != "" &&
        $("#card-operation #vehicle_number").val() != "") {

        var postData = {
            operation_id: $("#card-operation #operation_id").val(),

            operation_type: $("#card-operation #operation-types-dropdown").val(),
            gate: $("#card-operation .gates").val(),
            vehicle_kind: $("#card-operation #vehicle-kinds-dropdown").val(),
            vehicle_brand: $("#card-operation #vehicle_brand").val(),
            vehicle_model: $("#card-operation #vehicle_model").val(),
            vehicle_number: $("#card-operation #vehicle_number").val(),
            order_id: $("#card-operation #order_id").val(),
            driver_name: $("#card-operation #driver_name").val(),

            driver_phone: $("#card-operation #driver_phone").val(),
            manager_name: $("#card-operation #manager_name").val(),
            manager_phone: $("#card-operation #manager_phone").val(),
            manager_email: $("#card-operation #manager_email").val(),
            note: $("#card-operation #note").val(),
        };

        $.post("/api/v1/yard/createUpdateOperation", postData, function(data) {
            console.log(data);

            if (data.startsWith("eror")) {
                alert(data.split("||")[1]);
            } else {
                var card = JSON.parse(data);
            }

            // TODO: пока пусть редиректит на лк TSL
            // но тут должна быть вилка в зависимости от типа пользователя
            location.href = "/DispatcherHome?id=" + card.Id +
                "&from_date=" + workStartDatePicker.value +
                "&to_date=" + workStartDatePicker.value;
        });
    } else {
        alert("не все обязательные поля введены");
    }
}

function ChangeState(stateChangeType) {
    if (stateChangeType == "DeleteOperation" && !window.confirm("Удалить операцию двора?")) {
        return;
    }

    var isAllHumanFieldsFull = true;
    $(".dropdown .form-control").each(function() {
        var inputGroup = $(this).parent();
        var info = inputGroup.children(".selected-item-id").val();

        isAllHumanFieldsFull = isAllHumanFieldsFull && info != "";
    });

    if (isAllHumanFieldsFull && stateChangeType == "StartOperationWork" || stateChangeType != "StartOperationWork") {
        var postData = {
            operation_id: $("#identeficator").text(),
            action: stateChangeType,
        };

        $.post("/api/v1/yard/changeOperationState", postData, function(data) {
            var card = JSON.parse(data);
            console.log(card);
            if (stateChangeType == "DeleteOperation" || stateChangeType == "CancelOperation")
                window.location.href = window.location.href.split("?", 1);
            else
                window.location.href = window.location.search;
        });
    } else {
        alert("Не все сотрудники были заполнены!");
    }
}

function ShowGuardPage() {
    location.href = "/GuardPage";
}

function PassVehicle(id, action) {
    var data = {
        operation_id: id,
        action: action,
    };
    $.post("/api/v1/yard/changeOperationState", data, ShowGuardPage);
}

function LetVehicleIn(id) {
    PassVehicle(id, "StartOperation");
}

function ReleaseVehicle(id) {
    PassVehicle(id, "FinishOperation");
}

function CreateFiltersRequest() {
    var fromDatePicker = document.getElementById("from-date-picker");
    var value = fromDatePicker.value;
    var result = "?from_date=" + value;

    var toDatePicker = document.getElementById("to-date-picker");
    value = toDatePicker.value;
    if (value != "")
        result = result.concat("&to_date=" + value);

    var filters = document.getElementById("filters");
    var gatesDropdown = filters.querySelector("#gates-dropdown");
    if (gatesDropdown != undefined && gatesDropdown.value != "") {
        result = result.concat("&gate=" + gatesDropdown.value);
    }

    var partnersDropdown = filters.querySelector("#partners-dropdown");
    if (partnersDropdown != undefined && partnersDropdown.value != "") {        
        result = result.concat("&partner=" + partnersDropdown.value);
    }

    var operationTypesDropdown = filters.querySelector("#operation-types-dropdown");
    if (operationTypesDropdown != undefined && operationTypesDropdown.value != "") {
        result = result.concat("&type=" + operationTypesDropdown.value);
    }    

    var vehicleKindsDropdown = filters.querySelector("#vehicle-kinds-dropdown");
    if (vehicleKindsDropdown != undefined && vehicleKindsDropdown.value != "") {
        result = result.concat("&vehicleKind=" + vehicleKindsDropdown.value);
    }

    var operationStatesDropdown = filters.querySelector("#operation-states-dropdown");
    if (operationStatesDropdown != undefined && operationStatesDropdown.value != "") {
        result = result.concat("&state=" + operationStatesDropdown.value);
    }

    var vehicleNumberInput = filters.querySelector("#vehicle-number-input");
    if (vehicleNumberInput != undefined && vehicleNumberInput.value != "") {
        result = result.concat("&vehicle_number=" + vehicleNumberInput.value);
    }

    var driverNameInput = filters.querySelector("#driver-name-input");
    if (driverNameInput != undefined && driverNameInput.value != "") {
        result = result.concat("&driver_name=" + driverNameInput.value);
    }   

    return result;
}

function CreateFilterRequest() {
    var data = {};

    var fromDatePicker = document.getElementById("from-date-picker");
    var stringFromDate = fromDatePicker.value;
    var toDatePicker = document.getElementById("to-date-picker");
    var stringToDate = toDatePicker.value;

    data.from_date = stringFromDate;
    data.to_date = stringToDate;
    var filters = document.getElementById("filters");

    var gatesDropdown = filters.querySelector("#gates-dropdown");
    data.gate = gatesDropdown != undefined ? gatesDropdown.value : "";

    var partnersDropdown = filters.querySelector("#partners-dropdown");
    data.partner_id = partnersDropdown != undefined ? partnersDropdown.value : "";

    var operationTypesDropdown = filters.querySelector("#operation-types-dropdown");
    data.operation_type = operationTypesDropdown != undefined ? operationTypesDropdown.value : "";

    var vehicleKindsDropdown = filters.querySelector("#vehicle-kinds-dropdown");
    data.vehicle_kind = vehicleKindsDropdown != undefined ? vehicleKindsDropdown.value : "";

    var operationStatesDropdown = filters.querySelector("#operation-states-dropdown");
    data.state = operationStatesDropdown != undefined ? operationStatesDropdown.value : "";

    var vehicleNumberInput = filters.querySelector("#vehicle-number-input");
    data.vehicle_number = vehicleNumberInput != undefined ? vehicleNumberInput.value : "";

    var driverNameInput = filters.querySelector("#driver-name-input");
    data.driver_name = driverNameInput != undefined ? driverNameInput.value : "";

    return data;
}

function ParseOperations(data) {
    var operations = JSON.parse(data);
    RemoveCards();
    for (let operation of operations) {
        console.log(operation);
        RenderCard($(".data-operation-list"), operation);
    }
}

function LoadCardsList() {
    if ($("#time_table").length) {
        manageTimeTable(dateParam);
    }

    var request = CreateFilterRequest();
    $.get("/api/v1/yard/operationsList", request, ParseOperations);
    var newUrl = window.location.href.split('?')[0] + CreateFiltersRequest();
    history.pushState({}, null, newUrl);
}

function OnChangeFilter() {
    LoadCardsList();
}

function ParseOperationDuration(data) {
    timeSpan = JSON.parse(data);
    var time = new Date(new Date().setHours(0, 0, timeSpan, 0));
    $("#text_operation_time").text(time.toLocaleTimeString());
}

function ParseBisyIntervals(data) {
    periods = JSON.parse(data);
    RenderTable(periods);
}

function UpdateTimeTable() {
    var workStartDatePicker = document.getElementById("work-start-date-picker");
    var workStartDate = workStartDatePicker.value;

    var operationTypeCode = $('#operation-types-dropdown :selected').val();
    var vehicleKindCode = $('#vehicle-kinds-dropdown :selected').val();

    if (operationTypeCode != "" && vehicleKindCode != "") {
        $("#time_table").removeClass("d-none");
        try {
            var data = {
                operation: operationTypeCode,
                vehicle_kind: vehicleKindCode,
            }
            $.get("/api/v1/yard/OperationLength", data, ParseOperationDuration);
            data = {
                date: workStartDate,
                operation: operationTypeCode,
                vehicle_kind: vehicleKindCode,
            }
            $.get("/api/v1/yard/BusyPeriods", data, ParseBisyIntervals);
        } catch (e) {
            console.log(e);
        }
    } else {
        $("#time_table").addClass("d-none");
        $("#text_operation_time").text("<Не определено>");
    }
}

$(document).ready(function () {
    // Show modal
    if (GetUrlParameter("id")) {
        $("#RequestCardModal").modal("toggle");
    }

    // Event on change
    var filters = document.getElementById("filters");
    if (filters != undefined) {
        var fromDatePicker = $("#from-date-picker");
        if (fromDatePicker != undefined) {
            fromDatePicker.datepicker({
                format: "dd.mm.yyyy",
                autoclose: true,
                language: "ru",
                todayBtn: true,
                todayHighlight: true,
            });
            fromDatePicker.val(GetUrlParameter("from_date") ? GetUrlParameter("from_date") : "now");
            fromDatePicker.on('changeDate', OnChangeFilter);
        }
        var toDatePicker = $("#to-date-picker");
        if (toDatePicker != undefined) {
            toDatePicker.datepicker({
                format: "dd.mm.yyyy",
                autoclose: true,
                language: "ru",
                todayBtn: true,
                todayHighlight: true,
            });
            toDatePicker.val(GetUrlParameter("to_date") ? GetUrlParameter("to_date") : "now");
            toDatePicker.on('changeDate', OnChangeFilter);
        }

        var gatesDropdown = filters.querySelector("#gates-dropdown");
        if (gatesDropdown != undefined)
            gatesDropdown.onchange = OnChangeFilter

        var partnersDropdown = filters.querySelector("#partners-dropdown");
        if (partnersDropdown != undefined)
            partnersDropdown.onchange = OnChangeFilter

        var operationTypesDropdown = filters.querySelector("#operation-types-dropdown");
        if (operationTypesDropdown != undefined)
            operationTypesDropdown.onchange = OnChangeFilter

        var vehicleKindsDropdown = filters.querySelector("#vehicle-kinds-dropdown");
        if (vehicleKindsDropdown != undefined)
            vehicleKindsDropdown.onchange = OnChangeFilter

        var operationStatesDropdown = filters.querySelector("#operation-states-dropdown");
        if (operationStatesDropdown != undefined)
            operationStatesDropdown.onchange = OnChangeFilter

        var vehicleNumberInput = filters.querySelector("#vehicle-number-input");
        if (vehicleNumberInput != undefined)
            vehicleNumberInput.onchange = OnChangeFilter

        var driverNameInput = filters.querySelector("#driver-name-input");
        if (driverNameInput != undefined)
            driverNameInput.onchange = OnChangeFilter
    }

    var path = window.location.pathname;
    var page = path.split("/").pop();
    if (page == "NewOperation") {
        var workStartDatePicker = $("#work-start-date-picker");
        if (workStartDatePicker != undefined) {
            workStartDatePicker.datepicker({
                format: "dd.mm.yyyy",
                autoclose: true,
                language: "ru",
                todayBtn: true,
                todayHighlight: true,
            });
            var stringDate = (new Date()).toLocaleString("ru-RU", { year: 'numeric', month: 'numeric', day: 'numeric' });
            workStartDatePicker.val(stringDate);
            workStartDatePicker.on('changeDate', UpdateTimeTable);
        }

        $("#time_table").addClass("d-none");
        if ($("#time_table").length) {
            $("#text_operation_time").text("<Не определено>");
        }

        var operationTypesDropdown = document.getElementById("operation-types-dropdown");
        if (operationTypesDropdown != undefined)
            operationTypesDropdown.onchange = UpdateTimeTable;

        var vehiclekindsDropDown = document.getElementById("vehicle-kinds-dropdown");
        if (vehiclekindsDropDown != undefined)
            vehiclekindsDropDown.onchange = UpdateTimeTable;
    }    

    var newUrl = window.location.href.split('?')[0] + CreateFiltersRequest();
    history.pushState({}, null, newUrl);
});

function OpenHumanStatisticsByDate() {
    location.href = "/HumanDispersal?date=" + $(".datepicker").datepicker("getDate").toLocaleDateString();
}

function OpenGateStatisticsByDate() {
    location.href = "/GateDispersal?date=" + $(".datepicker").datepicker("getDate").toLocaleDateString();
}

function OpenClientReportByDate() {
    if ($("#dp1").datepicker('getDate') <= $("#dp2").datepicker("getDate") && $("#partner-filter select").val() != "") {
        location.href = "/ClientReportDispersal?start_date=" +
            $("#dp1").datepicker('getDate').toLocaleDateString() +
            "&end_date=" + $("#dp2").datepicker("getDate").toLocaleDateString() +
            "&partner=" + $("#partner-filter select").val();
    } else {
        alert("параметры отчета неверно заполнены");
    }
}

$(document).ready(function() {
    // Set params of client-report
    $(".client-report #from-date-picker").datepicker("setDate", GetUrlParameter("start_date") ? GetUrlParameter("start_date") : "now").on("changeDate", function(e) {});
    $(".client-report #dp2").datepicker("setDate", GetUrlParameter("end_date") ? GetUrlParameter("end_date") : "now").on("changeDate", function(e) {});
});

$(document).ready(function() {
    // Make search on every letter change in #card-operation.dropdown-toggle
    // also on change if nearest selected-item-id is not empty delete data from it and post to api
    // also change list of items in #card-operation.dropdown-menu

    $('.dropdown-toggle').bind("keyup", DropdownToggleHandler);
    $('.dropdown-toggle').bind("click", DropdownToggleHandler);
});

function DropdownToggleHandler(e) {
    var inputField = $(this);
    var inputGroup = inputField.parent();

    var curSelectedWorkerId = inputGroup.children(".selected-item-id").val()
    if (curSelectedWorkerId != "") {
        // has selection
        UpdateWorkerInfo("del", inputGroup);
        inputGroup.children(".selected-item-id").val('');
    }

    $.get("/api/v1/yard/SearchWorkersByName", {
            search: inputField.val(),
            position_code: inputGroup.children(".position-code").val(),
        },
        function(data) {
            var people = JSON.parse(data);
            var dropdown = inputGroup.children(".dropdown-menu");

            RenderUserList(dropdown, people);
        });
}

function RenderUserList(parent, people) {
    parent.children().remove();
    people.forEach(human => {
        var newItem = document.createElement("li");
        newItem = $(newItem);
        newItem.addClass("dropdown-item");

        newItem.text(human.FullName);
        newItem.val(human.Id);
        newItem.click(OnItemClick);

        parent.append(newItem);
    });
}

// get event on click of every li element in #card-operation.dropdown-menu 
// and chenge value of nearest #card-operation.dropdown-toggle by its content and post to api
// + save info into 
function OnItemClick() {
    var selectedWorker = $(this);
    var inputGroup = selectedWorker.parent().parent();
    inputGroup.children(".dropdown-toggle").val(selectedWorker.text());

    var curSelectedWorkerID = inputGroup.children(".selected-item-id").val()

    if (curSelectedWorkerID == "") {
        inputGroup.children(".selected-item-id").val(selectedWorker.val());
        UpdateWorkerInfo("add", inputGroup);

    } else {
        if (curSelectedWorkerID != selectedWorker.val()) {
            UpdateWorkerInfo("del", inputGroup);
            inputGroup.children(".selected-item-id").val(selectedWorker.val());
            UpdateWorkerInfo("add", inputGroup);
        }
    }
    return false;
}

function WriteUpdateLog() {
    console.log("updated");
}

function UpdateWorkerInfo(actionType, inputGroup) {
    var postData = {
        operation_id: $("#identeficator").text(),
        action_type: actionType,
        worker_id: inputGroup.children(".selected-item-id").val(),
        position_code: inputGroup.children(".position-code").val(),
    };

    $.post("/api/v1/yard/UpdateOperationWorkers", postData, WriteUpdateLog);
}

$(document).ready(function() {
    if ($("#force-print").val())
        window.print();
});

var maxRowsPerPosition = 4;

function AddNewPosition(rowButton) {
    var row = $(rowButton).parent().parent().parent();
    var posCode = row.attr("class");
    console.log($("." + posCode));

    var positionRows = $("." + posCode);
    console.log(positionRows);
    if (positionRows.length < maxRowsPerPosition) {
        var newRow = row.clone();
        SetRowEmpty(newRow);

        var curSize = positionRows.length;
        row.after(newRow);

        RerenderWorkerTableIndexes();
        RecalculateAdditionalTableInfo(posCode, row.find(".cell-info-text").text(), curSize);
    }
}

function RemovePosition(rowButton) {
    var row = $(rowButton).parent().parent().parent();
    var posCode = row.attr('class');
    console.log($("." + posCode));

    UpdateWorkerInfo("del", row.find(".input-group"));

    var positionRows = $("." + posCode);
    console.log(positionRows);
    if (positionRows.length > 1) {
        var curSize = positionRows.length;
        row.remove();
        RecalculateAdditionalTableInfo(posCode, row.find(".cell-info-text").text(), curSize);
    } else {
        SetRowEmpty(row);
    }
    RerenderWorkerTableIndexes();
}

function SetRowEmpty(row) {
    row.find(".selected-item-id").val("");
    var dropdownToggle = row.find(".dropdown-toggle");

    dropdownToggle.val("");
    dropdownToggle.bind("keyup", DropdownToggleHandler);
    dropdownToggle.bind("click", DropdownToggleHandler);
}

function RerenderWorkerTableIndexes() {
    $(".table-row-index").each(function(index) {
        $(this).text(index + 1);
    });
}

function RecalculateAdditionalTableInfo(rowGroupName, exampleText, previousSize) {
    var rowGroupRows = $("." + rowGroupName);
    var strTime = exampleText.split("[")[1].split(" ")[0];
    var neededPersonal = exampleText.split("/")[1].split(")")[0];
    var newTime = MultiplyTextTimeSpan(strTime, (previousSize / neededPersonal) / (rowGroupRows.length / neededPersonal));

    var outStr = "[" + newTime.toLocaleTimeString() + " чел.] (" + rowGroupRows.length + "/" + neededPersonal + ")";
    console.log(outStr);
    rowGroupRows.each(function(index) {
        $(this).find(".cell-info-text").each(function(index) {
            $(this).text(outStr);
        });
    });
}