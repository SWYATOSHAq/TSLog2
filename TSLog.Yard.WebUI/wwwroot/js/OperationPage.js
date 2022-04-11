class OperationPage {
    shortUrl;

    startHour = 2;
    finishHour = 23;
    hourBlocksCount = 4;
    blockDuration = 0;

    startDate = utils.nullDate;
    planWorkStartDate = utils.nullDate;
    factWorkStartDate = utils.nullDate;
    planWorkFinishDate = utils.nullDate;
    factWorkFinishDate = utils.nullDate;
    planDuration = 0;
    factDuration = 0;
    planLaborCost = 0;
    factLaborCost = 0;
    finishDate = utils.nullDate;
    avalibleIntervals = [];
    operationBlocksCount = 0;
    blocks = [];

    constructor() {
        this.shortUrl = location.href.split('?')[0];
        this.blockDuration = 60 * 60 / this.hourBlocksCount;
    }

    getSelectedDate() {
        return $("#work-start-date-picker").datepicker('getDate');
    }

    resetVisibility() {
        $(".btn-info").addClass("btn-outline-primary");
        $(".btn-info").removeClass("btn-info");
        $(".btn-outline-info").addClass("btn-outline-primary");
        $(".btn-outline-info").removeClass("btn-outline-info");
    }

    countOperationBlocks() {
        this.operationBlocksCount = Math.ceil(this.planDuration * this.hourBlocksCount / 3600);
        this.resetVisibility();
    }

    getBlockId(date) {
        const time = date.getHours() * 60 * 60 + date.getMinutes() * 60 + date.getSeconds();
        for (let i = 0; i < this.blocks.length; i++) {
            if (time >= this.blocks[i].startTime && time < this.blocks[i].finishTime) {
                return this.blocks[i].id;
            }
        }
        return "";
    }

    getSelectPlanStartDate(blockId) {
        return function () {
            this.selectPlanStartDate(blockId);
        }.bind(this);
    }

    setPlanStartDate(blockId) {
        let block = document.getElementById(blockId);
        const time = Number(block.dataset.starttime);
        const date = this.getSelectedDate();
        this.planWorkStartDate = new Date(date.setSeconds(time));
        this.planWorkFinishDate = new Date(this.planWorkStartDate);
        this.planWorkFinishDate = new Date(this.planWorkFinishDate.setSeconds(this.planDuration));
    }

    showDate(planDate, factDate, input) {
        if (planDate > utils.nullDate) {
            input.value = utils.dateTimeToString(planDate);
            if (factDate > utils.nullDate) {
                input.value = input.value + " (факт. " + utils.dateTimeToString(factDate) + ")";
            }
        } else {
            input.value = "<Не указано>";
        }
    }

    showWorkStartFinish() {
        this.showDate(this.planWorkStartDate, this.factWorkStartDate, document.getElementById("work-start-input"))
        this.showDate(this.planWorkFinishDate, this.factWorkFinishDate, document.getElementById("work-finish-input"))
    }

    showSelectedBlocks(blockId) {
        this.resetVisibility();
        let tempId = blockId;
        let block = null;
        let hasError = false;
        let index = parseInt(blockId.split("-")[1]);
        const prefix = blockId.split("-")[0] + "-";

        $(".selected-block-button").removeClass("selected-block-button");

        for (let i = 0; i < this.operationBlocksCount; i++) {
            tempId = prefix + index.toString();
            index++;

            block = document.getElementById(tempId);
            if (this.factWorkStartDate <= utils.nullDate && (block == undefined || block.getAttribute("disabled"))) {
                hasError = true;
                break;
            }

            block.classList.remove("btn-outline-primary");
            block.classList.add("btn-info");
            block.classList.add("selected-block-button");
        }

        if (hasError) {            
            $("#error-div").removeClass("d-none");
            $("#error-p").text("Время недоступно из за типа операции, выберите другой промежуток времени или тип операции");
            this.planWorkStartDate = utils.nullDate;
            this.planWorkFinishDate = utils.nullDate;
        }
        else {
            $("#error-p").text("");
            $("#error-div").addClass("d-none");
        }
        this.showWorkStartFinish();
    }

    selectPlanStartDate(blockId) {
        this.setPlanStartDate(blockId);
        this.showSelectedBlocks(blockId);        
    }

    showDuration() {
        const durationInput = document.getElementById("duration-input");
        if (this.planDuration <= 0)
            durationInput.value = "<Не определено>";
        else {
            durationInput.value = utils.secondsToString(this.planDuration);
            if (this.factDuration > 0) {
                durationInput.value = durationInput.value + " (факт. " + utils.secondsToString(this.factDuration) + ")"
            }
        }
    }

    async getPlanDuration() {
        const operationTypeCode = document.getElementById("operation-types-select").value;
        const vehicleKindCode = document.getElementById("vehicle-kinds-select").value;
        this.planDuration = 0;
        if (operationTypeCode != "" && vehicleKindCode != "") {
            let response = await fetch("./api/v1/yard/OperationDuration" +
                "?operation_type=" + operationTypeCode + "&vehicle_kind=" + vehicleKindCode);
            if (response.status >= 200 && response.status < 300) {
                this.planDuration = await response.json();
                this.countOperationBlocks();
            }
            else {
                const text = await response.text();
                console.log(text);
            }
        }
    }

    showLaborCost() {
        const laborCostInput = document.getElementById("labor-cost-input");
        if (this.planLaborCost <= 0)
            laborCostInput.value = "<Не определено>";
        else {
            laborCostInput.value = (this.planLaborCost / 3600).toFixed(2);
            if (this.factLaborCost > 0) {
                laborCostInput.value = laborCostInput.value + " (факт. " + (this.factLaborCost / 3600).toFixed(2) + ")"
            }
        }
    }

    async getPlanLaborCost() {
        const operationTypeCode = document.getElementById("operation-types-select").value;
        const vehicleKindCode = document.getElementById("vehicle-kinds-select").value;
        this.planLaborCost = 0;
        if (operationTypeCode != "" && vehicleKindCode != "") {
            let response = await fetch("./api/v1/yard/OperationLaborCost" +
                "?operation_type=" + operationTypeCode + "&vehicle_kind=" + vehicleKindCode);
            if (response.status >= 200 && response.status < 300) {
                this.planLaborCost = await response.json();
                this.countOperationBlocks();
            }
            else {
                const text = await response.text();
                console.log(text);
            }
        }
    }

    async getAvalibleIntervals() {
        this.avalibleIntervals = [];
        const workStartDatePicker = document.getElementById("work-start-date-picker");
        const workStartDate = workStartDatePicker.value;
        const operationTypeCode = document.getElementById("operation-types-select").value;
        const vehicleKindCode = document.getElementById("vehicle-kinds-select").value;
        this.avalibleIntervals = [];
        let response = await fetch("./api/v1/yard/AvalibleIntervals?date=" + workStartDate +
            "&operation_type=" + operationTypeCode + "&vehicle_kind=" + vehicleKindCode);
        if (response.status >= 200 && response.status < 300) {
            this.avalibleIntervals = await response.json();            
        }
        else {
            const text = await response.text();
            console.log(text);
        }
    }

    hideTimeTable() {
        const timeDiv = document.getElementById("time-div");
        let timeTable = document.getElementById("time-table-div");
        timeTable.replaceChildren();
        this.blocks = [];
        timeDiv.classList.add("d-none");
    }

    disableTimeTable() {
        const timeTable = document.getElementById("time-table-div");
        const rows = timeTable.children;
        for (let i = 0; i < rows.length; i++) {
            const buttons = rows[i].children;
            for (let j = 0; j < buttons.length; j++)
                buttons[j].setAttribute("disabled", "");
        }
    }

    showTimeTable() {
        const timeDiv = document.getElementById("time-div");
        let timeTable = document.getElementById("time-table-div");
        timeTable.replaceChildren();
        this.blocks = [];        
        timeDiv.classList.remove("d-none");

        const date = this.getSelectedDate();
        let blockNumber = 0;
        for (let hour = this.startHour; hour < this.finishHour; hour++) {
            let row = document.createElement("div");
            row.classList.add("row");
            row.classList.add("table-1hr-row");
            for (let blockIndex = 0; blockIndex < this.hourBlocksCount; blockIndex++) {
                const button = document.createElement("button");
                button.classList.add("btn");
                button.classList.add("col");
                button.classList.add("m-1");
                button.textContent = hour.toString().padStart(2, "0") + ":" + (blockIndex * this.blockDuration / 60).toString().padStart(2, "0");

                const blockStartTime = hour * 60 * 60 + blockIndex * this.blockDuration;
                const blockStart = date.setHours(0, 0, blockStartTime, 0);
                const blockFinish = blockStart + this.blockDuration;

                let isFree = false;
                for (let interval of this.avalibleIntervals) {
                    const periodFrom = new Date(interval.From);
                    const periodTo = new Date(interval.To);
                    if (periodFrom <= blockStart && periodTo >= blockFinish && blockStart >= Date.now()) {
                        isFree = true;
                        break;
                    }
                }
                if (isFree) {
                    button.classList.add("btn-outline-primary");
                } else {
                    button.classList.add("btn-outline-secondary");
                    button.setAttribute("disabled", true);
                }

                blockNumber++;
                const blockId = "timeblock-" + blockNumber;
                button.id = blockId;
                button.setAttribute("data-starttime", blockStartTime);
                button.setAttribute("data-finishtime", blockStartTime + this.blockDuration);
                button.onclick = this.getSelectPlanStartDate(blockId);
                row.append(button);
                timeTable.append(row);
                const block = {
                    id: blockId,
                    startTime: blockStartTime,
                    finishTime: blockStartTime + this.blockDuration
                };
                this.blocks.push(block);
            }
        }       
    }

    async onChangeOperationTypeVehicleKind() {
        await this.getPlanDuration();
        await this.getPlanLaborCost();
        this.showDuration();
        this.showLaborCost();
        if (this.planDuration <= 0) {
            this.planWorkStartDate = utils.nullDate;
            this.planWorkFinishDate = utils.nullDate;
            this.hideTimeTable();
            this.showWorkStartFinish();
            return;
        }
        await this.getAvalibleIntervals();
        this.showTimeTable();
        const blockId = this.getBlockId(this.planWorkStartDate);
        if (blockId != "") {
            this.showSelectedBlocks(blockId);
        }
    }

    async onChangePlanWorkStartDate() {
        await this.getAvalibleIntervals();
        this.showTimeTable();
        let tempDate = new Date(this.planWorkStartDate);
        tempDate.setHours(0, 0, 0, 0);
        if (tempDate.getTime() == this.getSelectedDate().getTime()) {
            const blockId = this.getBlockId(this.planWorkStartDate);
            if (blockId != "") {
                this.showSelectedBlocks(blockId);
            }
        }        
    }

    async showManager() {
        const partnersSelect = document.getElementById("partners-select");
        const managerInput = document.getElementById("manager-input");
        managerInput.value = "<Не определено>"
        if (partnersSelect.value == "")
            return;
        let response = await fetch("./api/v1/yard/PartnerManager?partner_id=" + partnersSelect.value);
        if (response.status >= 200 && response.status < 300) {
            const manager = await response.json();
            managerInput.value = manager.FullName;
        }
        else {
            const text = await response.text();
            console.log(text);
        }
    }

    async onChangePartner() {
        await this.showManager();
    }

    async createOperation() {
        if (this.planWorkStartDate <= utils.nullDate || this.planWorkFinishDate <= utils.nullDate ||
            document.getElementById("operation-types-select").value == "" ||
            document.getElementById("vehicle-kinds-select").value == "" ||
            document.getElementById("vehicle-brand-input").value == "" ||
            document.getElementById("vehicle-model-input").value == "" ||
            document.getElementById("vehicle-number-input").value == "" ||
            document.getElementById("driver-name-input").value == "" ||
            document.getElementById("partners-select").value == "" ||
            document.getElementById("partner-action-id-input").value == "")            
        {
            alert("Не все обязательные поля заполнены!");
            return;
        }

        const data = {
            startDate: utils.dateTimeToString(this.planWorkStartDate),
            operationTypeCode: document.getElementById("operation-types-select").value,
            vehicleKindCode: document.getElementById("vehicle-kinds-select").value,
            vehicleBrand: document.getElementById("vehicle-brand-input").value,
            vehicleModel: document.getElementById("vehicle-model-input").value,
            vehicleNumber: document.getElementById("vehicle-number-input").value,
            trailerNumber: document.getElementById("trailer-number-input").value,
            driverName: document.getElementById("driver-name-input").value,
            driverPhoneNumber: document.getElementById("driver-phone-number-input").value,
            partnerManagerName: document.getElementById("partner-manager-name-input").value,
            partnerManagerPhoneNumber: document.getElementById("partner-manager-phone-number-input").value,
            partnerManagerEmail: document.getElementById("partner-manager-email-input").value,
            note: document.getElementById("note-textarea").value,
            partnerId: document.getElementById("partners-select").value,
            partnerActionId: document.getElementById("partner-action-id-input").value
        };
        api.createOperation(data);
    }

    async createOperationByPartner() {
        if (this.planWorkStartDate <= utils.nullDate || this.planWorkFinishDate <= utils.nullDate ||
            document.getElementById("operation-types-select").value == "" ||
            document.getElementById("vehicle-kinds-select").value == "" ||
            document.getElementById("vehicle-brand-input").value == "" ||
            document.getElementById("vehicle-model-input").value == "" ||
            document.getElementById("vehicle-number-input").value == "" ||
            document.getElementById("driver-name-input").value == "" ||
            document.getElementById("partner-action-id-input").value == "") {
            alert("Не все обязательные поля заполнены!");
            return;
        }

        const data = {
            startDate: utils.dateTimeToString(this.planWorkStartDate),
            operationTypeCode: document.getElementById("operation-types-select").value,
            vehicleKindCode: document.getElementById("vehicle-kinds-select").value,
            vehicleBrand: document.getElementById("vehicle-brand-input").value,
            vehicleModel: document.getElementById("vehicle-model-input").value,
            vehicleNumber: document.getElementById("vehicle-number-input").value,
            trailerNumber: document.getElementById("trailer-number-input").value,
            driverName: document.getElementById("driver-name-input").value,
            driverPhoneNumber: document.getElementById("driver-phone-number-input").value,
            partnerManagerName: document.getElementById("partner-manager-name-input").value,
            partnerManagerPhoneNumber: document.getElementById("partner-manager-phone-number-input").value,
            partnerManagerEmail: document.getElementById("partner-manager-email-input").value,
            note: document.getElementById("note-textarea").value,
            partnerActionId: document.getElementById("partner-action-id-input").value
        };
        await api.createOperationByPartner(data);
    }

    getOperationId() {
        const idInput = document.getElementById("id-input");
        return idInput.value;
    }

    async editOperation() {
        if (this.planWorkStartDate <= utils.nullDate || this.planWorkFinishDate <= utils.nullDate ||
            document.getElementById("operation-types-select").value == "" ||
            document.getElementById("vehicle-kinds-select").value == "" ||
            document.getElementById("vehicle-brand-input").value == "" ||
            document.getElementById("vehicle-model-input").value == "" ||
            document.getElementById("vehicle-number-input").value == "" ||
            document.getElementById("driver-name-input").value == "" ||
            document.getElementById("partners-select").value == "" ||
            document.getElementById("partner-action-id-input").value == "")
        {
            alert("Не все обязательные поля заполнены!");
            return;
        }

        const data = {
            id: this.getOperationId(),
            startDate: utils.dateTimeToString(this.planWorkStartDate),
            operationTypeCode: document.getElementById("operation-types-select").value,
            vehicleKindCode: document.getElementById("vehicle-kinds-select").value,
            gateCode: document.getElementById("gates-select") != undefined ? document.getElementById("gates-select").value : "",
            vehicleBrand: document.getElementById("vehicle-brand-input").value,
            vehicleModel: document.getElementById("vehicle-model-input").value,
            vehicleNumber: document.getElementById("vehicle-number-input").value,
            trailerNumber: document.getElementById("trailer-number-input").value,
            driverName: document.getElementById("driver-name-input").value,
            driverPhoneNumber: document.getElementById("driver-phone-number-input").value,
            partnerManagerName: document.getElementById("partner-manager-name-input").value,
            partnerManagerPhoneNumber: document.getElementById("partner-manager-phone-number-input").value,
            partnerManagerEmail: document.getElementById("partner-manager-email-input").value,
            note: document.getElementById("note-textarea").value,
            partnerId: document.getElementById("partners-select").value,
            partnerActionId: document.getElementById("partner-action-id-input").value
        };
        api.editOperation(data);
    }

    async editOperationByPartner() {
        if (this.planWorkStartDate <= utils.nullDate || this.planWorkFinishDate <= utils.nullDate ||
            document.getElementById("operation-types-select").value == "" ||
            document.getElementById("vehicle-kinds-select").value == "" ||
            document.getElementById("vehicle-brand-input").value == "" ||
            document.getElementById("vehicle-model-input").value == "" ||
            document.getElementById("vehicle-number-input").value == "" ||
            document.getElementById("driver-name-input").value == "" ||
            document.getElementById("partner-action-id-input").value == "")
        {
            alert("Не все обязательные поля заполнены!");
            return;
        }

        const data = {
            id: this.getOperationId(),
            startDate: utils.dateTimeToString(this.planWorkStartDate),
            operationTypeCode: document.getElementById("operation-types-select").value,
            vehicleKindCode: document.getElementById("vehicle-kinds-select").value,
            vehicleBrand: document.getElementById("vehicle-brand-input").value,
            vehicleModel: document.getElementById("vehicle-model-input").value,
            vehicleNumber: document.getElementById("vehicle-number-input").value,
            trailerNumber: document.getElementById("trailer-number-input").value,
            driverName: document.getElementById("driver-name-input").value,
            driverPhoneNumber: document.getElementById("driver-phone-number-input").value,
            partnerManagerName: document.getElementById("partner-manager-name-input").value,
            partnerManagerPhoneNumber: document.getElementById("partner-manager-phone-number-input").value,
            partnerManagerEmail: document.getElementById("partner-manager-email-input").value,
            note: document.getElementById("note-textarea").value,
            partnerActionId: document.getElementById("partner-action-id-input").value
        };
        api.editOperationByPartner(data);
    }

    async editOperationNote() {
        const data = {
            id: this.getOperationId(),
            note: document.getElementById("note-textarea").value
        };
        api.editOperationNote(data);
    }

    async setOperationGate() {
        api.setOperationGate(this.getOperationId(), "");
    }

    async startOperation() {
        api.startOperation(this.getOperationId());
    }

    // #region Workers
    maxPositionWorkers = 4;

    addPosition(rowButton) {
        const row = $(rowButton).parent().parent().parent();
        const positionCode = row.attr("class");
        console.log($("." + positionCode));

        const positionRows = $("." + positionCode);
        console.log(positionRows);
        if (positionRows.length < maxPositionWorkers) {
            const newRow = row.clone();
            setRowEmpty(newRow);

            const curSize = positionRows.length;
            row.after(newRow);

            this.rerenderWorkerTableIndexes();
            this.recalculateAdditionalTableInfo(positionCode, row.find(".cell-info-text").text(), curSize);
        }
    }

    removePosition(rowButton) {
        const row = $(rowButton).parent().parent().parent();
        const positionCode = row.attr('class');
        console.log($("." + positionCode));

        removeOperationWorker(row.find(".input-group"));

        const positionRows = $("." + positionCode);
        console.log(positionRows);
        if (positionRows.length > 1) {
            const curSize = positionRows.length;
            row.remove();
            recalculateAdditionalTableInfo(positionCode, row.find(".cell-info-text").text(), curSize);
        } else {
            setRowEmpty(row);
        }
        this.rerenderWorkerTableIndexes();
    }

    async dropdownToggleHandler() {
        const inputField = this;
        const inputGroup = inputField.parentElement;

        const workerId = inputGroup.querySelector(".selected-worker-id").value;
        if (workerId != "") {
            removeOperationWorker(inputGroup);
            inputGroup.querySelector(".selected-worker-id").value = "";
        }

        const url = "./api/v1/yard/GetWorkers?name_pattern=" + inputField.value + "&position_code=" + inputGroup.querySelector(".position-code").value;
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

    showWorkers(parent, workers) {
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

    onItemClick() {
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

    setRowEmpty(row) {
        row.find(".selected-worker-id").val("");
        const dropdownToggle = row.find(".dropdown-toggle");

        dropdownToggle.val("");
        dropdownToggle.bind("keyup", dropdownToggleHandler);
        dropdownToggle.bind("click", dropdownToggleHandler);
    }

    rerenderWorkerTableIndexes() {
        $(".table-row-index").each(function (index) {
            $(this).text(index + 1);
        });
    }

    multiplyTextTimeSpan(stringTime, multiplier) {
        let timeArr = stringTime.split(':'); // split it at the colons

        // Minutes are worth 60 seconds. Hours are worth 60 minutes.
        let seconds = (+timeArr[0]) * 60 * 60 + (+timeArr[1]) * 60 + (+timeArr[2]);
        let newSeconds = seconds * multiplier;

        let t = new Date();
        t.setHours(0, 0, newSeconds, 0);
        return t;
    }

    recalculateAdditionalTableInfo(rowGroupName, exampleText, previousSize) {
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

    async addOperationWorker(inputGroup) {
        api.addOperationWorker(document.getElementById("id-input").value,
            inputGroup.querySelector(".selected-worker-id").value,
            inputGroup.querySelector(".position-code").value);
    }

    async removeOperationWorker(inputGroup) {
        api.removeOperationWorker(document.getElementById("id-input").value,
            inputGroup.querySelector(".selected-worker-id").value,
            inputGroup.querySelector(".position-code").value);
    }

    async allocateOperationEmployees() {
        const url = "./api/v1/yard/AllocateOperationEmployees";
        const data = {
            id: this.getOperationId()
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

    async startOperationWork() {
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
        api.startOperationWork(this.getOperationId());
    }

    async finishOperationWork() {
        api.finishOperationWork(this.getOperationId());
    }

    async finishOperation() {
        api.finishOperation(this.getOperationId());
    }

    async cancelOperation() {
        if (!window.confirm("Отменить операцию двора?")) {
            return;
        }
        api.cancelOperation(this.getOperationId());
    }

    async deleteOperation() {
        if (!window.confirm("Удалить операцию двора?")) {
            return;
        }
        api.deleteOperation(this.getOperationId());
    }

    printOperation() {
        const idInput = document.getElementById("id-input");
        window.open("/OperationPrintPage?id=" + idInput.value, "_blank");
    }

    setControlsState() {

        /*
        if (this.factWorkStartDate <= utils.nullDate) {
            document.getElementById("time-div").removeAttribute("disabled");
            document.getElementById("work-start-date-picker").removeAttribute("disabled");
            document.getElementById("operation-types-select").removeAttribute("disabled");
            document.getElementById("vehicle-kinds-select").removeAttribute("disabled");
            if (document.getElementById("gates-select") != undefined) {
                document.getElementById("gates-select").removeAttribute("disabled");
            }            
            document.getElementById("vehicle-brand-input").removeAttribute("disabled");
            document.getElementById("vehicle-model-input").removeAttribute("disabled");
            document.getElementById("vehicle-number-input").removeAttribute("disabled");
            document.getElementById("trailer-number-input").removeAttribute("disabled");
            document.getElementById("driver-name-input").removeAttribute("disabled");
            document.getElementById("driver-phone-number-input").removeAttribute("disabled");
            document.getElementById("partner-manager-name-input").removeAttribute("disabled");
            document.getElementById("partner-manager-phone-number-input").removeAttribute("disabled");
            document.getElementById("partner-manager-email-input").removeAttribute("disabled");
            document.getElementById("partners-select").removeAttribute("disabled");
            document.getElementById("partner-action-id-input").removeAttribute("disabled");
        }
        else {
            document.getElementById("time-div").setAttribute("disabled", "");
            document.getElementById("work-start-date-picker").setAttribute("disabled", "");
            document.getElementById("operation-types-select").setAttribute("disabled", "");
            document.getElementById("vehicle-kinds-select").setAttribute("disabled", "");
            if (document.getElementById("gates-select") != undefined) {
                document.getElementById("gates-select").setAttribute("disabled", "");
            }
            document.getElementById("vehicle-brand-input").setAttribute("disabled", "");
            document.getElementById("vehicle-model-input").setAttribute("disabled", "");
            document.getElementById("vehicle-number-input").setAttribute("disabled", "");
            document.getElementById("trailer-number-input").setAttribute("disabled", "");
            document.getElementById("driver-name-input").setAttribute("disabled", "");
            document.getElementById("driver-phone-number-input").setAttribute("disabled", "");
            document.getElementById("partner-manager-name-input").setAttribute("disabled", "");
            document.getElementById("partner-manager-phone-number-input").setAttribute("disabled", "");
            document.getElementById("partner-manager-email-input").setAttribute("disabled", "");
            document.getElementById("partners-select").setAttribute("disabled", "");
            document.getElementById("partner-action-id-input").setAttribute("disabled", "");

            this.disableTimeTable();
        }
        */

        if (this.factWorkStartDate > utils.nullDate) {
            document.getElementById("time-div").setAttribute("disabled", "");
            document.getElementById("work-start-date-picker").setAttribute("disabled", "");
            document.getElementById("operation-types-select").setAttribute("disabled", "");
            document.getElementById("vehicle-kinds-select").setAttribute("disabled", "");
            if (document.getElementById("gates-select") != undefined) {
                document.getElementById("gates-select").setAttribute("disabled", "");
            }
            document.getElementById("vehicle-brand-input").setAttribute("disabled", "");
            document.getElementById("vehicle-model-input").setAttribute("disabled", "");
            document.getElementById("vehicle-number-input").setAttribute("disabled", "");
            document.getElementById("trailer-number-input").setAttribute("disabled", "");
            document.getElementById("driver-name-input").setAttribute("disabled", "");
            document.getElementById("driver-phone-number-input").setAttribute("disabled", "");
            document.getElementById("partner-manager-name-input").setAttribute("disabled", "");
            document.getElementById("partner-manager-phone-number-input").setAttribute("disabled", "");
            document.getElementById("partner-manager-email-input").setAttribute("disabled", "");
            document.getElementById("partners-select").setAttribute("disabled", "");
            document.getElementById("partner-action-id-input").setAttribute("disabled", "");            
        }
        // Костыль для охраны
        if (document.getElementById("operation-types-select").hasAttribute("disabled")) {
            document.getElementById("work-start-date-picker").setAttribute("disabled", "");
            this.disableTimeTable();
        }            
    }

    async showPage() {
        const workStartDatePicker = $("#work-start-date-picker");
        workStartDatePicker.datepicker({
            format: "dd.mm.yyyy",
            autoclose: true,
            language: "ru",
            todayBtn: true,
            todayHighlight: true,
        });                

        const id = utils.getUrlParameter("id");
        let operation = null;
        if (id != "") {
            operation = await api.getOperation(Number(id));            
        }

        if (operation != null) {
            this.startDate = new Date(operation.startDate);
            this.planWorkStartDate = new Date(operation.planWorkStartDate);
            this.factWorkStartDate = new Date(operation.factWorkStartDate);
            this.planWorkFinishDate = new Date(operation.planWorkFinishDate);
            this.factWorkFinishDate = new Date(operation.factWorkFinishDate);
            this.planDuration = Number(operation.planDuration);
            this.factDuration = Number(operation.factDuration);
            this.planLaborCost = Number(operation.planLaborCost);
            this.factLaborCost = Number(operation.factLaborCost);
            this.finishDate = new Date(operation.finishDate);
            this.countOperationBlocks();

            document.getElementById("id-input").value = operation.id;
            document.getElementById("operation-types-select").value = operation.typeCode;
            document.getElementById("vehicle-kinds-select").value = operation.vehicleKindCode;
            document.getElementById("duration-input").value = utils.secondsToString(this.planDuration);
            document.getElementById("labor-cost-input").value = (this.planLaborCost / 3600).toFixed(2);

            const startFinishInput = document.getElementById("start-finish-input");
            startFinishInput.value = "";
            if (this.startDate > utils.nullDate) {
                startFinishInput.value = "Прибыла - " + utils.dateTimeToString(this.startDate);
            }
            if (this.finishDate > utils.nullDate) {
                startFinishInput.value = startFinishInput.value + " Убыла - " + utils.dateTimeToString(this.finishDate);
            }

            if (document.getElementById("gates-select") != undefined) {
                document.getElementById("gates-select").value = operation.gateCode;
            }
            document.getElementById("vehicle-brand-input").value = operation.vehicleBrand;
            document.getElementById("vehicle-model-input").value = operation.vehicleModel;
            document.getElementById("vehicle-number-input").value = operation.vehicleNumber;
            document.getElementById("trailer-number-input").value = operation.trailerNumber;
            document.getElementById("driver-name-input").value = operation.driverName;
            document.getElementById("driver-phone-number-input").value = operation.driverPhoneNumber;
            document.getElementById("partner-manager-name-input").value = operation.partnerManagerName;
            document.getElementById("partner-manager-phone-number-input").value = operation.partnerManagerPhoneNumber;
            document.getElementById("partner-manager-email-input").value = operation.partnerManagerEmail;
            document.getElementById("note-textarea").value = operation.note;
            document.getElementById("partners-select").value = operation.partnerId;
            document.getElementById("partner-action-id-input").value = operation.partnerActionId;
            document.getElementById("responsible-officer-input").value = operation.responsibleOfficer != "" ? operation.responsibleOfficer : "<Не определено>";
                                   
            let tempDate = new Date(this.planWorkStartDate);
            tempDate.setHours(0, 0, 0, 0);
            workStartDatePicker.datepicker("setDate", tempDate);
            await this.getAvalibleIntervals();           
            this.showTimeTable();
            const blockId = this.getBlockId(this.planWorkStartDate);
            if (blockId != "") {
                this.showSelectedBlocks(blockId);
            }
            await this.showManager();
        } else {
            this.planWorkStartDate = utils.nullDate;
            this.planWorkFinishDate = utils.nullDate;
            this.planDuration = 0;
            document.getElementById("responsible-officer-input").value = "<Не определено>";
            let tempDate = new Date();
            tempDate.setHours(0, 0, 0, 0);
            workStartDatePicker.datepicker("setDate", tempDate);            
            this.hideTimeTable();
            await this.showManager();
        }
        this.showDuration();
        this.showLaborCost();
        this.showWorkStartFinish();        

        const operationTypesSelect = document.getElementById("operation-types-select");
        const vehiclekindsSelect = document.getElementById("vehicle-kinds-select");
        operationTypesSelect.onchange = this.onChangeOperationTypeVehicleKind.bind(this);
        vehiclekindsSelect.onchange = this.onChangeOperationTypeVehicleKind.bind(this);
        workStartDatePicker.on("changeDate", this.onChangePlanWorkStartDate.bind(this));
        const partnersSelect = document.getElementById("partners-select");
        partnersSelect.onchange = this.onChangePartner.bind(this);               

        this.setControlsState();

        // Make search on every letter change in #card-operation.dropdown-toggle
        // also on change if nearest selected-item-id is not empty delete data from it and post to api
        // also change list of items in #card-operation.dropdown-menu

        $('.dropdown-toggle').bind("keyup", this.dropdownToggleHandler.bind(this));
        $('.dropdown-toggle').bind("click", this.dropdownToggleHandler.bind(this));
    }
}

const operationPage = new OperationPage();

$(document).ready(operationPage.showPage.bind(operationPage));