class OperationPage {
    shortUrl;

    /*
        Выбор с начала суток по 8 часов утра след. суток.
        Всего промежуток 32 часа.
        Блоков в часе - 4.
    */
    startHour = 0;
    finishHour = 32;
    hourBlocksCount = 4;
    blockDuration = 0;

    stateCode = "";
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
    doDisplay = false;

    allWorkers = [];
    selectedWorkers = [];
    selectedWorkersIds = new Set([]);

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
        if (date != null && date != undefined && date > utils.nullDate) {
            const time = date.getHours() * 60 * 60 + date.getMinutes() * 60 + date.getSeconds();
            for (let i = 0; i < this.blocks.length; i++) {
                if (time >= this.blocks[i].startTime && time < this.blocks[i].finishTime) {
                    return this.blocks[i].id;
                }
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
            if (!this.doDisplay && this.factWorkStartDate <= utils.nullDate && (block == undefined || block.getAttribute("disabled"))) {
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
                button.textContent = (hour % 24).toString().padStart(2, "0") + ":" + (blockIndex * this.blockDuration / 60).toString().padStart(2, "0");

                const blockStartTime = hour * 60 * 60 + blockIndex * this.blockDuration;
                const blockStart = new Date(date);
                blockStart.setSeconds(date.getSeconds() + blockStartTime);
                const blockFinish = new Date(date);
                blockFinish.setSeconds(date.getSeconds() + blockStartTime + this.blockDuration);

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

    getOperationTypeHint() {
        const select = document.getElementById("operation-types-select");
        if (select.selectedIndex >= 0) {
            select.title = select.options[select.selectedIndex].getAttribute("note");
        }
    }

    async onChangeOperationTypeVehicleKind() {
        this.getOperationTypeHint();

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
            this.doDisplay = true;
            this.showSelectedBlocks(blockId);
            this.doDisplay = false;
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
                this.doDisplay = true;
                this.showSelectedBlocks(blockId);
                this.doDisplay = false;
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
            document.getElementById("partners-select").value == "" ||
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
            partnerId: document.getElementById("partners-select").value,
            partnerActionId: document.getElementById("partner-action-id-input").value
        };
        api.createOperation(data);
    }

    async createOperationByPartner() {
        if (this.planWorkStartDate <= utils.nullDate || this.planWorkFinishDate <= utils.nullDate ||
            document.getElementById("operation-types-select").value == "" ||
            document.getElementById("vehicle-kinds-select").value == "" ||
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
            document.getElementById("partner-action-id-input").value == "") {
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
            document.getElementById("partner-action-id-input").value == "") {
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

    createRemovePosition() {
        return function () {
            operationPage.removePosition(this);
        }
    }

    removePosition(rowButton) {
        const row = $(rowButton).parent().parent().parent();
        const positionCode = row.attr('class');
        console.log($("." + positionCode));

        removeOperationWorker(row.find(".input-group"));

        const positionRows = $("." + positionCode);
        console.log(positionRows);

        const curSize = positionRows.length;
        row.remove();
        recalculateAdditionalTableInfo(positionCode, row.find(".cell-info-text").text(), curSize);

        this.rerenderWorkerTableIndexes();
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

    async getAllWorkers() {
        const response = await api.getAllWorkers()
        if (response.code === 0) {
            this.allWorkers = response.data;
        }
        else {
            console.log(response.message);
        }
    }

    onChangeWorkersFilter() {
        this.showAllWorkers();
    }

    async onWorkerClick(button) {
        const operationId = document.getElementById("id-input").value;
        const workerId = button.dataset.workerId;
        const positionCode = button.dataset.positionCode;
        await api.addOperationWorker(operationId, workerId, positionCode);
                
        await this.getSelectedWorkers();
        this.showSelectedWorkers();
        this.showAllWorkers();
    }

    showWorker(worker, button) {
        button.type = "button";
        button.classList.add("list-group-item");
        button.style.cssText = "text-align: left !important";
        button.dataset.workerId = worker.id;
        button.dataset.positionCode = "EXT_LOADER";
        button.textContent = worker.fullName;
        button.onclick = () => {
            this.onWorkerClick(button);
        } 
    }

    showAllWorkers() {
        const allWorkersDiv = document.getElementById("all-workers-div");
        allWorkersDiv.replaceChildren();
        const namePattern = document.getElementById("worker-filter-input").value.trim().toLowerCase();
        for (let worker of this.allWorkers) {
            if (namePattern != "" && !worker.fullName.toLowerCase().includes(namePattern)) {
                continue;
            }
            if (this.selectedWorkersIds.has(worker.id)) {
                continue;
            }

            const button = document.createElement("button");
            this.showWorker(worker, button);
            allWorkersDiv.append(button);
        }
    }

    async getSelectedWorkers() {
        const response = await api.getOperationWorkers(this.getOperationId())
        if (response.code === 0) {
            this.selectedWorkers = response.data;
            for (let worker of this.selectedWorkers) {
                this.selectedWorkersIds.add(worker.id);
            }
        }
        else {
            this.selectedWorkers = [];
            this.selectedWorkersIds.clear();
            console.log(response.message);
        }
    }

    createRemoveWorkerFunction(operationId, workerId, positionCode) {
        return async function () {
            const response = await api.removeOperationWorker(operationId, workerId, positionCode);
            if (response.code != 0) {
                console.log(response.message);
            }
        }
    }

    async onSelectedWorkerClick(button) {
        const operationId = document.getElementById("id-input").value;
        const workerId = button.dataset.workerId;
        const positionCode = button.dataset.positionCode;
        await api.removeOperationWorker(operationId, workerId, positionCode);

        await this.getSelectedWorkers();
        this.showSelectedWorkers();
        this.showAllWorkers();
    }

    showSelectedWorker(worker, button) {
        button.type = "button";
        button.classList.add("list-group-item");
        button.cssText = "text-align: left";
        button.dataset.workerId = worker.id;
        button.dataset.positionCode = worker.positionCode;
        button.textContent = worker.fullName;
        button.onclick = () => {
            this.onSelectedWorkerClick(button);
        }
    }

    showSelectedWorkers() {
        const selectedWorkersDiv = document.getElementById("selected-workers-div");
        selectedWorkersDiv.replaceChildren();
        for (let operationWorker of this.selectedWorkers) {
            const operationWorkerButton = document.createElement("button");
            this.showSelectedWorker(operationWorker, operationWorkerButton);
            selectedWorkersDiv.append(operationWorkerButton);
        }
    }

    createSelectWorker(workerId, workerFullName) {
        return function () {
            this.selectedWorkerId = workerId;
            document.getElementById("worker-input").value = workerFullName;

            const workerNameUl = document.getElementById("workers-names-ul");
            workerNameUl.classList.add("d-none");
        }
    }

    showWorkers(workers) {
        const workerNameUl = document.getElementById("workers-names-ul");
        workerNameUl.classList.remove("d-none");

        workerNameUl.replaceChildren();
        workers.forEach(worker => {
            const workerLi = document.createElement("li");
            workerLi.classList.add("dropdown-item");

            workerLi.value = worker.Id;
            workerLi.textContent = worker.FullName;
            workerLi.onclick = this.createSelectWorker(worker.Id, worker.FullName).bind(this);
            workerNameUl.append(workerLi);
        });
    }

    async dropdownToggleHandler() {
        const positionCode = document.getElementById("position-select").value;
        const namePattern = document.getElementById("worker-input").value;
        const response = await api.getPositionWorkers(positionCode, namePattern);
        if (response.code === 0) {
            const workers = response.data;
            this.showWorkers(workers);
        } else {
            console.log(response.message);
        }
    }

    workerInputLeave() {
        const workerNameUl = document.getElementById("workers-names-ul");
        workerNameUl.classList.add("d-none");
    }

    async addOperationWorker() {
        const operationId = document.getElementById("id-input").value;
        const workerId = -1;
        const positionCode = document.getElementById("worker-input").value;
        const response = await api.addOperationWorker(operationId, workerId, positionCode);
        if (response.code != 0) {
            console.log(response.message);
        }
    }

    async allocateOperationWorkers() {
        const response = await api.allocateOperationWorkers(this.getOperationId());
        if (response.code === 0) {
            window.location.reload();
        } else {
            console.log(response.message);
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

    showOperationsPage() {
        let operationsPageUrl = sessionStorage.getItem("OPERATIONS_PAGE_URL");
        if (operationsPageUrl == null || operationsPageUrl == "")
            operationsPageUrl = "/OperationsPage"
        location.href = operationsPageUrl;
    }

    async finishOperation() {
        api.finishOperation(this.getOperationId());
        this.showOperationsPage();
    }

    async cancelOperation() {
        if (!window.confirm("Отменить операцию двора?")) {
            return;
        }
        api.cancelOperation(this.getOperationId());
        this.showOperationsPage();
    }

    async deleteOperation() {
        if (!window.confirm("Удалить операцию двора?")) {
            return;
        }
        api.deleteOperation(this.getOperationId());
        this.showOperationsPage();
    }

    closeOperation() {
        this.showOperationsPage();
    }

    printOperation() {
        const idInput = document.getElementById("id-input");
        window.open("/OperationPrintPage?id=" + idInput.value, "_blank");
    }

    setControlsState() {

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

    showOperationTypeNote() {
        if (document.getElementById("operation-types-select").value == "")
            return;
        const p = document.getElementById("operation-type-note-p");
        const select = document.getElementById("operation-types-select");
        p.innerText = select.options[select.selectedIndex].getAttribute("note");
        const noteDiv = document.getElementById("operation-type-note-div");
        noteDiv.style.display = "block";
    }

    hideOperationTypeNote() {
        const noteDiv = document.getElementById("operation-type-note-div");
        noteDiv.style.display = "none";
    }

    windowClick(event) {
        const noteDiv = document.getElementById("operation-type-note-div");
        if (event.target == noteDiv) {
            this.hideOperationTypeNote();
        }
    }

    async show() {
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
            this.stateCode = operation.stateCode;
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
                this.doDisplay = true;
                this.showSelectedBlocks(blockId);
                this.doDisplay = false;
            }
            await this.showManager();

            //const workersTable = document.getElementById("workers-table");
            const states = ["STARTED", "WORK_STARTED", "WORK_FINISHED", "FINISHED"];
            if (states.includes(this.stateCode)) {
                //workersTable.classList.remove("d-none");

                const workersFilterInput = document.getElementById("worker-filter-input");
                workersFilterInput.onchange = this.onChangeWorkersFilter.bind(this);

                await this.getSelectedWorkers();
                this.showSelectedWorkers();
                await this.getAllWorkers();
                this.showAllWorkers();                

                //const workersFieldset = document.getElementById("workers-fieldset");
                //workersFieldset.disabled = false && (this.stateCode != "STARTED");
            } else {
                //workersTable.classList.add("d-none");
            }
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
        this.getOperationTypeHint();

        // Make search on every letter change in #card-operation.dropdown-toggle
        // also on change if nearest selected-worker-id is not empty delete data from it and post to api
        // also change list of items in #card-operation.dropdown-menu

        //const workerInput = document.getElementById("worker-input");

        //workerInput.onkeyup = this.dropdownToggleHandler.bind(this);
        //workerInput.onclick = this.dropdownToggleHandler.bind(this);
        //workerInput.onblur = this.workerInputLeave.bind(this);

        const showOperationTypeNoteButton = document.getElementById("operation-type-note-button");
        const closeOperationTypeNoteSpan = document.getElementById("operation-type-note-close-span");

        showOperationTypeNoteButton.onclick = this.showOperationTypeNote.bind(this);
        closeOperationTypeNoteSpan.onclick = this.hideOperationTypeNote.bind(this);

        window.onclick = this.windowClick.bind(this);
    }

    addWorkerClick(element) {
        alert(element.textContent);
    }

    removeWorkerClick() {
    }
}

const operationPage = new OperationPage();

document.addEventListener('DOMContentLoaded', operationPage.show.bind(operationPage));