const state = {
    time: 0
};

function resetVisibility() {
    $(".btn-info").addClass("btn-outline-primary");
    $(".btn-info").removeClass("btn-info");

    $(".btn-outline-info").addClass("btn-outline-primary");
    $(".btn-outline-info").removeClass("btn-outline-info");
    state.time = 0;
}

let trailLength = 0;
const timeSplit = 4;
const timeStart = 2;
const timeFinish = 23;
let operationDuration = 0;

function selectTime(time, cellId) {
    return function () {
        getTrailLength();

        $("." + cellId).removeClass("btn-outline-primary");
        $("." + cellId).addClass("btn-info");
        const parsedCellId = cellId.split("-");
        let erorOccuredCount = 0;

        for (let i = 1; i <= trailLength; i++) {
            let nextCellCollumn = Number(parsedCellId[2]) + i;
            const shift = Math.floor(nextCellCollumn / timeSplit);

            nextCellCollumn = nextCellCollumn % timeSplit;
            const nextTrailBlock = parsedCellId[0] + "-" + (shift + Number(parsedCellId[1])) + "-" + nextCellCollumn;

            $("." + nextTrailBlock).removeClass("btn-outline-primary");
            $("." + nextTrailBlock).addClass("btn-info");

            if ($("." + nextTrailBlock).length == 0) {
                erorOccuredCount++;
            }
            else if ($("." + nextTrailBlock).prop("disabled")) {
                erorOccuredCount++;
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
            $("#operation-time-text").text($("#operation-time-text").text().split(" (")[0] + " (выбранное время: " + time + ")");
        }
    };
}

function stringToTime(stringTime) {
    const workStartDatePicker = document.getElementById("work-start-date-picker");
    let stringDate = workStartDatePicker.value.replaceAll(".", "/");
    stringDate = stringDate.replace(/(\d+[/])(\d+[/])/, '$2$1');

    const result = new Date(stringDate);
    const times = stringTime.split(":");
    result.setHours(times[0], times[1], times.length > 2 ? times[2] : 0, 0);
    return result;
}

function renderTimeTable(periods) {
    var temp = document.getElementsByClassName("table-1hr-row");
    var temp = $(".table-1hr-row");
    temp.remove();

    var timePart = 60 / timeSplit;
    var timeSplits = [];
    for (let i = timeStart; i <= timeFinish; i++) {
        timeSplits[i] = [];
        for (let j = 0; j < timeSplit; j++) {
            timeSplits[i][j] = "" + i % 24 + ":" + ("" + (j * timePart)).padStart(2, "0");
        }
    }

    var timeTable = document.getElementById("time-table");
    let i = 0;
    let j = 0;
    for (let row of timeSplits) {
        if (row == undefined)
            continue;

        var newRow = document.createElement("div");
        newRow.classList.add("row");
        newRow.classList.add("table-1hr-row");        
        for (let buttonText of row) {
            const button = document.createElement("button");
            button.classList.add("btn");
            button.classList.add("col");
            button.classList.add("m-1");
            button.textContent = buttonText;

            // check if time is awailable or not
            let isSuitable = false;
            const today = new Date();

            for (let period of periods) {
                const periodFrom = new Date(period.From);
                const periodTo = new Date(period.To);

                if (periodFrom <= stringToTime(buttonText) && stringToTime(buttonText) < periodTo && stringToTime(buttonText) >= today) {
                    isSuitable = true;
                }
            }
            if (isSuitable) {
                button.classList.add("btn-outline-primary");                
            } else {
                button.classList.add("btn-outline-secondary");
                button.setAttribute("disabled", true);
            }

            const cellId = "bu-" + i + "-" + j % timeSplit;
            button.classList.add(cellId);
            button.onclick = selectTime(buttonText, cellId);
            newRow.append(button);
            j++;
            timeTable.append(newRow);
        }
        i++;
    }
}

async function updateTimeTable() {
    const workStartDatePicker = document.getElementById("work-start-date-picker");
    const workStartDate = workStartDatePicker.value;

    const operationTypeCode = document.getElementById("operation-types-dropdown").value;
    const vehicleKindCode = document.getElementById("vehicle-kinds-dropdown").value;

    const timeTable = document.getElementById("time-table");
    const operationTimeText = document.getElementById("operation-time-text");
    if (operationTypeCode != "" && vehicleKindCode != "") {
        timeTable.classList.remove("d-none");
        try {
            let response = await fetch("./api/v1/yard/OperationDuration" +
                "?operation_type=" + operationTypeCode + "&vehicle_kind=" + vehicleKindCode);
            if (response.status >= 200 && response.status < 300) {
                operationDuration = await response.json();
                const time = new Date(new Date().setHours(0, 0, operationDuration, 0));
                operationTimeText.textContent = time.toLocaleTimeString();
            }
            else {
                const text = await response.text();
                console.log(text);
                timeTable.classList.add("d-none");
                operationTimeText.textContent = "<Не определено>";
            }

            response = await fetch("./api/v1/yard/BusyIntervals" +
                "?date=" + workStartDate + "&operation_type=" + operationTypeCode + "&vehicle_kind=" + vehicleKindCode);
            if (response.status >= 200 && response.status < 300) {
                const periods = await response.json();
                renderTimeTable(periods);
            }
            else {
                const text = await response.text();
                console.log(text);
                timeTable.classList.add("d-none");
                operationTimeText.textContent = "<Не определено>";
            }

        } catch (exception) {
            console.log(exception);
            timeTable.classList.add("d-none");
            operationTimeText.textContent = "<Не определено>";
        }
    } else {        
        timeTable.classList.add("d-none");
        operationTimeText.textContent = "<Не определено>";
    }
}

function getTrailLength() {
    //var operationTimeText = document.getElementById("operation-time-text").textContent.split(" ")[0];

    //var date1 = new Date(new Date().setHours(0, 0, 0, 0));
    //var date2 = ConvertToTime(operationTimeText);

    //var diffInSeconds = Math.abs(date1 - date2) / 1000;
    //var hours = Math.floor(diffInSeconds / 60 / 60 % 24);
    //var minutes = Math.floor(diffInSeconds / 60 % 60);
    //var koef = hours + minutes / 60;

    //trailLength = Math.floor((timeSplit * koef) - 1);
    trailLength = operationDuration / 900;
    resetVisibility();
}

async function createOperation() {
    // Check for required fields
    // time, operation_type, vehicle_kind, vehicle_brand, vehicle_model, vehicle_number
    if (state.time != 0 &&
        $("#operation-types-dropdown").val() == "" ||
        $("#vehicle_kinds-dropdown").val() == "" ||
        $("#vehicle_brand").val() == "" ||
        $("#vehicle_model").val() == "" ||
        $("#vehicle_number").val() == "" ||
        $("#driver_name").val() == "" ||
        $("#order_id").val() == "" ||
        $("#partner").val() == "") {
        alert("Не все обязательные поля введены");
    }

    const workStartDatePicker = document.getElementById("work-start-date-picker");                  
    const data = {
        startDate: workStartDatePicker.value,
        operationTypeCode: $("#card-operation #operation-types-dropdown").val(),
        gateCode: $("#card-operation .gates").val(),
        vehicleKindCode: $("#card-operation #vehicle-kinds-dropdown").val(),
        vehicleBrand: $("#card-operation #vehicle_brand").val(),
        vehicleModel: $("#card-operation #vehicle_model").val(),
        vehicleNumber: $("#card-operation #vehicle_number").val(),
        orderId: $("#card-operation #order_id").val(),
        driverName: $("#card-operation #driver_name").val(),
        driverPhoneNumber: $("#card-operation #driver_phone").val(),
        managerName: $("#card-operation #manager_name").val(),
        managerPhoneNumber: $("#card-operation #manager_phone").val(),
        managerEmail: $("#card-operation #manager_email").val(),
        note: $("#card-operation #note").val(),
    };
    api.createOperation(data);
}

function getOperationId() {
    const idInput = document.getElementById("id-input");
    return idInput.value;
}

async function editOperation() {
    // Check for required fields
    // time, operation_type, vehicle_kind, vehicle_brand, vehicle_model, vehicle_number
    if ($("#card-operation #operation-types-dropdown").val() == "" ||
        $("#card-operation #vehicle-kinds-dropdown").val() == "" ||
        $("#card-operation #vehicle_brand").val() == "" ||
        $("#card-operation #vehicle_model").val() == "" ||
        $("#card-operation #vehicle_number").val() == "") {
        alert("Не все обязательные поля заполнены!");
        return;
    }

    const workStartDatePicker = document.getElementById("work-start-date-picker");
    const data = {
        id: $("#card-operation #id-input").val(),
        startDate: workStartDatePicker.value,
        operationTypeCode: $("#card-operation #operation-types-dropdown").val(),
        gateCode: $("#card-operation .gates").val(),
        vehicleKindCode: $("#card-operation #vehicle-kinds-dropdown").val(),
        vehicleBrand: $("#card-operation #vehicle_brand").val(),
        vehicleModel: $("#card-operation #vehicle_model").val(),
        vehicleNumber: $("#card-operation #vehicle_number").val(),
        orderId: $("#card-operation #order_id").val(),
        driverName: $("#card-operation #driver_name").val(),
        driverPhoneNumber: $("#card-operation #driver_phone").val(),
        managerName: $("#card-operation #manager_name").val(),
        managerPhoneNumber: $("#card-operation #manager_phone").val(),
        managerEmail: $("#card-operation #manager_email").val(),
        note: $("#card-operation #note").val(),
    };
    api.editOperation(data);
}

async function setOperationGate() {
    api.setOperationGate(getOperationId(), "");    
}

async function startOperation() {
    api.startOperation(getOperationId());
}

// #region Workers
const maxRowsPerPosition = 4;

function addNewPosition(rowButton) {
    const row = $(rowButton).parent().parent().parent();
    const posCode = row.attr("class");
    console.log($("." + posCode));

    const positionRows = $("." + posCode);
    console.log(positionRows);
    if (positionRows.length < maxRowsPerPosition) {
        const newRow = row.clone();
        setRowEmpty(newRow);

        const curSize = positionRows.length;
        row.after(newRow);

        rerenderWorkerTableIndexes();
        recalculateAdditionalTableInfo(posCode, row.find(".cell-info-text").text(), curSize);
    }
}

function removePosition(rowButton) {
    const row = $(rowButton).parent().parent().parent();
    const posCode = row.attr('class');
    console.log($("." + posCode));

    removeOperationWorker(row.find(".input-group"));

    const positionRows = $("." + posCode);
    console.log(positionRows);
    if (positionRows.length > 1) {
        const curSize = positionRows.length;
        row.remove();
        recalculateAdditionalTableInfo(posCode, row.find(".cell-info-text").text(), curSize);
    } else {
        setRowEmpty(row);
    }
    rerenderWorkerTableIndexes();
}

async function dropdownToggleHandler() {
    const inputField = this;
    const inputGroup = inputField.parentElement;

    const workerId = inputGroup.querySelector(".selected-worker-id").value;
    if (workerId != "") {
        removeOperationWorker(inputGroup);
        inputGroup.querySelector(".selected-worker-id").value = "";
    }

    const url = "./api/v1/yard/GetWorkers?name_patter=" + inputField.value + "&position_code=" + inputGroup.querySelector(".position-code").value;
    const response = await fetch(url);
    if (response.status >= 200 && response.status < 300) {
        const workers = await response.json();
        const dropdown = inputGroup.querySelector(".dropdown-menu");
        showWorkers(dropdown, workers);
    }
    else {
        const message = await response.text();
        console.log(message);
        alert(message);
    }
}

function showWorkers(parent, workers) {
    parent.replaceChildren();
    workers.forEach(worker => {
        const workerItem = document.createElement("li");
        workerItem.classList.add("dropdown-item");

        workerItem.textContent = worker.FullName;
        workerItem.value = worker.Id;
        workerItem.onclick = onItemClick;
        parent.append(workerItem);
    });
}

function onItemClick() {
    const selectedWorker = this;
    const inputGroup = selectedWorker.parentElement.parentElement;
    inputGroup.querySelector(".dropdown-toggle").value = selectedWorker.textContent;
    const selectedWorkerIdInput = inputGroup.querySelector(".selected-worker-id");

    const workerId = selectedWorkerIdInput.value;
    if (workerId == "") {
        selectedWorkerIdInput.value = selectedWorker.value;
        addOperationWorker(inputGroup);

    } else if (workerId != selectedWorker.value) {
        removeOperationWorker(inputGroup);
        selectedWorkerIdInput.value = selectedWorker.value;
        addOperationWorker(inputGroup);
    }
    return false;
}

function setRowEmpty(row) {
    row.find(".selected-worker-id").val("");
    const dropdownToggle = row.find(".dropdown-toggle");

    dropdownToggle.val("");
    dropdownToggle.bind("keyup", dropdownToggleHandler);
    dropdownToggle.bind("click", dropdownToggleHandler);
}

function rerenderWorkerTableIndexes() {
    $(".table-row-index").each(function (index) {
        $(this).text(index + 1);
    });
}

function multiplyTextTimeSpan(stringTime, multiplier) {
    var timeArr = stringTime.split(':'); // split it at the colons

    // Minutes are worth 60 seconds. Hours are worth 60 minutes.
    var seconds = (+timeArr[0]) * 60 * 60 + (+timeArr[1]) * 60 + (+timeArr[2]);
    var newSeconds = seconds * multiplier;

    var t = new Date();
    t.setHours(0, 0, newSeconds, 0);
    return t;
}

function recalculateAdditionalTableInfo(rowGroupName, exampleText, previousSize) {
    const rowGroupRows = $("." + rowGroupName);
    const strTime = exampleText.split("[")[1].split(" ")[0];
    const neededPersonal = exampleText.split("/")[1].split(")")[0];
    const newTime = multiplyTextTimeSpan(strTime, (previousSize / neededPersonal) / (rowGroupRows.length / neededPersonal));

    const outStr = "[" + newTime.toLocaleTimeString() + " чел.] (" + rowGroupRows.length + "/" + neededPersonal + ")";
    console.log(outStr);
    rowGroupRows.each(function (index) {
        $(this).find(".cell-info-text").each(function (index) {
            $(this).text(outStr);
        });
    });
}

async function addOperationWorker(inputGroup) {
    api.addOperationWorker(document.getElementById("id-input").value,
        inputGroup.querySelector(".selected-worker-id").value,
        inputGroup.querySelector(".position-code").value);
}

async function removeOperationWorker(inputGroup) {
    api.removeOperationWorker(document.getElementById("id-input").value,
        inputGroup.querySelector(".selected-worker-id").value,
        inputGroup.querySelector(".position-code").value);
}

async function allocateOperationEmployees() {
    const url = "./api/v1/yard/AllocateOperationEmployees";
    const data = {
        id: getOperationId()
    }
    const options = {
        method: "PATCH",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(data)
    };
    const response = await fetch(url, options);
    if (response.status >= 200 && response.status < 300) {
        const operation = await response.json();
        console.log(operation);
        window.location.reload();
    }
    else {
        const message = await response.text();
        console.log(message);
        alert(message);
    }
}
// #endregion

async function startOperationWork() {
    let isAllHumanFieldsFull = true;
    $(".dropdown .form-control").each(function () {
        const inputGroup = $(this).parent();
        const info = inputGroup.children(".selected-worker-id").val();
        isAllHumanFieldsFull = isAllHumanFieldsFull && info != "";
    });
    if (!isAllHumanFieldsFull) {
        alert("Не все сотрудники были заполнены!");
        return;
    }
    api.startOperationWork(getOperationId());
}

async function finishOperationWork() {
    api.finishOperationWork(getOperationId());
}

async function finishOperation() {
    api.finishOperation(getOperationId());
}

async function cancelOperation() {
    if (!window.confirm("Отменить операцию двора?")) {
        return;
    }
    api.cancelOperation(getOperationId());
}

async function deleteOperation() {
    if (!window.confirm("Удалить операцию двора?")) {
        return;
    }
    api.deleteOperation(getOperationId());
}

function printOperation() {
    const idInput = document.getElementById("id-input");
    window.open("/OperationPrintPage?id=" + idInput.value, "_blank");
}

function showPage() {
    const workStartDatePicker = $("#work-start-date-picker");
    if (workStartDatePicker != undefined) {
        workStartDatePicker.datepicker({
            format: "dd.mm.yyyy",
            autoclose: true,
            language: "ru",
            todayBtn: true,
            todayHighlight: true,
        });
        workStartDatePicker.datepicker("setDate", "now");
        workStartDatePicker.on("changeDate", updateTimeTable);
    }

    const timeTable = document.getElementById("time-table");
    timeTable.classList.add("d-none");
        
    if (document.getElementById("time-table").length) {
        document.getElementById("operation-time-text").textContent = "<Не определено>";
    }

    const operationTypesDropdown = document.getElementById("operation-types-dropdown");
    operationTypesDropdown.onchange = updateTimeTable;

    const vehiclekindsDropDown = document.getElementById("vehicle-kinds-dropdown");
    vehiclekindsDropDown.onchange = updateTimeTable;

    periods = [];
    renderTimeTable(periods);

    // Make search on every letter change in #card-operation.dropdown-toggle
    // also on change if nearest selected-item-id is not empty delete data from it and post to api
    // also change list of items in #card-operation.dropdown-menu

    $('.dropdown-toggle').bind("keyup", dropdownToggleHandler);
    $('.dropdown-toggle').bind("click", dropdownToggleHandler);
}

$(document).ready(showPage);