function getPartnersNames(operation) {
    let result = "";
    for (let partner of operation.Partners) {
        result += ((result.Length > 0) ? ", " : "") + partner.Name;
    }
    return result;
}

function showOperationPage(id) {
    location.href = "OperationPage?id=" + id;
}

function showOperation(base, card) {
    var baseCol = document.createElement("div");
    $(baseCol).addClass("col action-card");

    var card0 = document.createElement("div");
    $(card0).addClass("container2");

    var baseCard = document.createElement("div");
    $(baseCard).addClass("m-1 card");
    
    var cardHeader = document.createElement("div");
    $(cardHeader).addClass("card-header");
    $(cardHeader).css("background-color", card.DisplayColor);
    $(cardHeader).text(utils.convertTime(card.PlanWorkStartDate) + " - " + utils.convertTime(card.PlanWorkFinishDate) + " (" + utils.convertDate(card.PlanWorkFinishDate) + ")");

    var cardBody = document.createElement("div");
    $(cardBody).addClass("card-body");
    
    var cardBodyMndnr = document.createElement("h5");
    $(cardBodyMndnr).addClass("card-title");
    $(cardBodyMndnr).text(getPartnersNames(card));

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
    $(cardBodyButton).text("Открыть");
    $(cardBodyButton).click(function () {
        var url = "OperationPage?id=" + card.Id;
        location.href = url;
    });
    
    baseCard.append(cardHeader);
    baseCard.append(cardBody);
    baseCard.append(cardBodyButton);

    card0.append(baseCard)
    baseCol.append(card0);
    base.append(baseCol);
}

function createParameters() {
    var fromDatePicker = document.getElementById("from-date-picker");
    var result = "?from_date=" + fromDatePicker.value;

    var toDatePicker = document.getElementById("to-date-picker");
    result = result.concat("&to_date=" + toDatePicker.value);

    var filters = document.getElementById("filters");
    var gatesDropdown = filters.querySelector("#gates-dropdown");
    if (gatesDropdown != undefined && gatesDropdown.value != "") {
        result = result.concat("&gate_code=" + gatesDropdown.value);
    }

    var partnersDropdown = filters.querySelector("#partners-dropdown");
    if (partnersDropdown != undefined && partnersDropdown.value != "") {
        result = result.concat("&partner_id=" + partnersDropdown.value);
    }

    var operationTypesDropdown = filters.querySelector("#operation-types-dropdown");
    if (operationTypesDropdown != undefined && operationTypesDropdown.value != "") {
        result = result.concat("&operation_type_code=" + operationTypesDropdown.value);
    }

    var vehicleKindsDropdown = filters.querySelector("#vehicle-kinds-dropdown");
    if (vehicleKindsDropdown != undefined && vehicleKindsDropdown.value != "") {
        result = result.concat("&vehicle_kind_code=" + vehicleKindsDropdown.value);
    }

    var operationStatesDropdown = filters.querySelector("#operation-states-dropdown");
    if (operationStatesDropdown != undefined && operationStatesDropdown.value != "") {
        result = result.concat("&state_code=" + operationStatesDropdown.value);
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

async function getOperations() {
    const parameters = createParameters();
    let url = "./api/v1/yard/Operations" + parameters;
    const response = await fetch(url);
    if (response.status >= 200 && response.status < 300) {
        var operations = await response.json();
        clearOperations();
        for (let operation of operations) {
            console.log(operation);
            showOperation($(".data-operation-list"), operation);
        }
    }
    else {
        const message = await response.text();
        console.log(message);
    }
    url = window.location.href.split('?')[0] + parameters;
    history.pushState({}, null, url);
}

function clearOperations() {
    $(".action-card").remove();
}

function onChangeFilter() {
    getOperations();
}

function show() {
    const filters = document.getElementById("filters");
    const fromDatePicker = $("#from-date-picker");
    if (fromDatePicker != undefined) {
        fromDatePicker.datepicker({
            format: "dd.mm.yyyy",
            autoclose: true,
            language: "ru",
            todayBtn: true,
            todayHighlight: true,
        });
        const fromDate = utils.getUrlParameter("from_date");
        if (fromDate == "")
            fromDate = utils.getNowDate();
        fromDatePicker.datepicker("setDate", fromDate);
        fromDatePicker.on("changeDate", onChangeFilter);
    }
    const toDatePicker = $("#to-date-picker");
    if (toDatePicker != undefined) {
        toDatePicker.datepicker({
            format: "dd.mm.yyyy",
            autoclose: true,
            language: "ru",
            todayBtn: true,
            todayHighlight: true,
        });
        const toDate = utils.getUrlParameter("to_date");
        if (toDate == "")
            toDate = utils.getNowDate();
        toDatePicker.datepicker("setDate", toDate);
        toDatePicker.on("changeDate", onChangeFilter);
    }

    const gatesDropdown = filters.querySelector("#gates-dropdown");
    if (gatesDropdown != undefined)
        gatesDropdown.onchange = onChangeFilter

    const partnersDropdown = filters.querySelector("#partners-dropdown");
    if (partnersDropdown != undefined)
        partnersDropdown.onchange = onChangeFilter

    const operationTypesDropdown = filters.querySelector("#operation-types-dropdown");
    if (operationTypesDropdown != undefined)
        operationTypesDropdown.onchange = onChangeFilter

    const vehicleKindsDropdown = filters.querySelector("#vehicle-kinds-dropdown");
    if (vehicleKindsDropdown != undefined)
        vehicleKindsDropdown.onchange = onChangeFilter

    const operationStatesDropdown = filters.querySelector("#operation-states-dropdown");
    if (operationStatesDropdown != undefined)
        operationStatesDropdown.onchange = onChangeFilter

    const vehicleNumberInput = filters.querySelector("#vehicle-number-input");
    if (vehicleNumberInput != undefined)
        vehicleNumberInput.onchange = onChangeFilter

    const driverNameInput = filters.querySelector("#driver-name-input");
    if (driverNameInput != undefined)
        driverNameInput.onchange = onChangeFilter

    const url = window.location.href.split('?')[0] + createParameters();
    history.pushState({}, null, url);
}

$(document).ready(show);
