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
    planLinearLaborCost = 0;
    factLaborCost = 0;
    finishDate = utils.nullDate;
    avalibleIntervals = [];
    operationBlocksCount = 0;
    blocks = [];
    doDisplay = false;

    liearWorkers = [];
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

        if (operationTypeCode === "" || vehicleKindCode === "") {
            return;
        }

        const result = await api.getPlanDuration(operationTypeCode, vehicleKindCode);
        if (result.code === 0) {
            this.planDuration = result.planDuration;
            this.countOperationBlocks();
        } else {
            viewUtils.showError(result.message);
        }
    }

    showLaborCost() {
        const laborCostInput = document.getElementById("labor-cost-input");
        if (this.planLaborCost <= 0)
            laborCostInput.value = "<Не определено>";
        else {
            laborCostInput.value = "План - " + (this.planLaborCost / 60).toFixed();
            if (this.planLinearLaborCost > 0) {
                laborCostInput.value = laborCostInput.value + ", только линейные - " + (this.planLinearLaborCost / 60).toFixed();
            }
            if (this.factLaborCost > 0) {
                laborCostInput.value = laborCostInput.value + ", факт - " + (this.factLaborCost / 60).toFixed();
            }
        }
    }

    async getPlanLaborCost() {
        const operationTypeCode = document.getElementById("operation-types-select").value;
        const vehicleKindCode = document.getElementById("vehicle-kinds-select").value;
        this.planLaborCost = 0;
        this.planLinearLaborCos = 0;

        if (operationTypeCode === "" || vehicleKindCode === "") {
            return;
        }

        const result = await api.getPlanLaborCost(operationTypeCode, vehicleKindCode);
        if (result.code === 0) {
            this.planLaborCost = result.data.laborCost;
            this.planLinearLaborCost = result.data.linearLaborCost;
            this.countOperationBlocks();
        } else {
            viewUtils.showError(result.message);
        }
    }

    async getAvalibleIntervals() {
        this.avalibleIntervals = [];
        const workStartDatePicker = document.getElementById("work-start-date-picker");
        const workStartDate = workStartDatePicker.value;
        const operationTypeCode = document.getElementById("operation-types-select").value;
        const vehicleKindCode = document.getElementById("vehicle-kinds-select").value;

        const result = await api.getAvalibleIntervals(workStartDate, operationTypeCode, vehicleKindCode);
        if (result.code === 0) {
            this.avalibleIntervals = result.avalibleIntervals;
        } else {
            viewUtils.showError(result.message);
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
            row.style.cssText = "padding: 0; margin: 0";
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
        const managerInput = document.getElementById("manager-input");        
        managerInput.value = "<Не определено>"
        const partnersSelect = document.getElementById("partners-select");
        const partnerId = partnersSelect.value;
        if (partnerId === "")
            return;

        const result = await api.getPartnerManager(partnerId);
        if (result.code === 0) {
            managerInput.value = result.manager.FullName;
        } else {
            viewUtils.showError(result.message);
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

        const result = await api.createOperation(data);
        if (result.code === 0) {
            const operation = result.operation;
            location.href = this.shortUrl + "?id=" + operation.id;
        } else {
            viewUtils.showError(result.message);
        }
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

        const result = await api.createOperationByPartner(data);
        if (result.code === 0) {
            const operation = result.operation;
            location.href = shortUrl + "?id=" + operation.id;
        } else {
            viewUtils.showError(result.message);
        }
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

        const result = await api.editOperation(data);
        if (result.code === 0) {
            location.reload();
        } else {
            viewUtils.showError(result.message);
        }
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

        const result = await api.editOperationByPartner(data);
        if (result.code === 0) {
            location.reload();
        } else {
            viewUtils.showError(result.message);
        }
    }

    async editOperationNote() {
        const id = this.getOperationId();
        const note = document.getElementById("note-textarea").value;
        const result = await api.editOperationNote(id, note);
        if (result.code === 0) {
            location.reload();
        } else {
            viewUtils.showError(result.message);
        }
    }

    async setOperationGate() {
        const result = await api.setOperationGate(this.getOperationId(), "");
        if (result.code === 0) {
            location.reload();
        } else {
            viewUtils.showError(result.message);
        }
    }

    async startOperation() {
        const id = this.getOperationId();
        const result = await api.startOperation(id);
        if (result.code == 0) {
            location.reload();
        } else {
            viewUtils.showError(result.message);
        }
    }

    // #region Workers
    showWorkersDivs() {
        let workersDiv = document.getElementById("linear-workers-div");
        if (workersDiv.childElementCount < 8) {
            workersDiv.style.cssText = "width: 49%; height: 320px; padding: 0; margin: 0; margin-right: 2%";
        } else {
            workersDiv.style.cssText = "width: 49%; height: 320px; overflow-y: scroll; padding: 0; margin: 0; margin-right: 2%";
        }

        workersDiv = document.getElementById("selected-workers-div");
        if (workersDiv.childElementCount < 8) {
            workersDiv.style.cssText = "width: 49%; height: 320px; padding: 0; margin: 0";
        } else {
            workersDiv.style.cssText = "width: 49%; height: 320px; overflow-y: scroll; padding: 0; margin: 0";
        }
    }

    async getLinearFreeWorkers() {
        const result = await api.getLinearFreeWorkers();
        if (result.code === 0) {
            this.liearWorkers = result.data;
        }
        else {
            viewUtils.showError(result.message);
        }
    }

    onChangeWorkersFilter() {
        this.showLinearFreeWorkers();
    }

    async onLinearWorkerClick(button) {
        const operationId = document.getElementById("id-input").value;
        const workerId = button.dataset.workerId;
        const positionCode = button.dataset.positionCode;

        const result = await api.addOperationWorker(operationId, workerId, positionCode);
        if (result.code === 0) {
            await this.getSelectedWorkers();
            this.showSelectedWorkers();
            this.showLinearFreeWorkers();
            this.showWorkersDivs();
        } else {
            viewUtils.showError(result.message);
        }
    }

    showLinearFreeWorker(worker, button) {
        button.type = "button";
        button.classList.add("list-group-item");
        button.style.cssText = "text-align: left !important";
        button.dataset.workerId = worker.id;
        button.dataset.positionCode = worker.positionCode;
        button.dataset.positionName = worker.positionName;
        button.dataset.positionShortName = worker.positionShortName;
        button.append("[" + worker.positionShortName + "] ");

        const workerFullNameB = document.createElement("b");
        workerFullNameB.textContent = worker.fullName;
        button.append(workerFullNameB);
        button.onclick = () => {
            this.onLinearWorkerClick(button);
        }
    }

    showLinearFreeWorkers() {
        const allWorkersDiv = document.getElementById("linear-workers-div");
        allWorkersDiv.replaceChildren();
        const namePattern = document.getElementById("worker-filter-input").value.trim().toLowerCase();
        for (let worker of this.liearWorkers) {
            if (namePattern != ""
                && !worker.fullName.toLowerCase().includes(namePattern)
                && !worker.positionName.toLowerCase().includes(namePattern)
                && !worker.positionShortName.toLowerCase().includes(namePattern)) {
                continue;
            }
            if (this.selectedWorkersIds.has(worker.id)) {
                continue;
            }

            const button = document.createElement("button");
            this.showLinearFreeWorker(worker, button);
            allWorkersDiv.append(button);
        }
    }

    async getSelectedWorkers() {
        const result = await api.getOperationWorkers(this.getOperationId());
        this.selectedWorkersIds.clear();
        if (result.code === 0) {
            this.selectedWorkers = result.data;
            for (let worker of this.selectedWorkers) {
                this.selectedWorkersIds.add(worker.id);
            }
        }
        else {            
            this.selectedWorkers = [];
            viewUtils.showError(result.message);
        }
    }

    async onSelectedWorkerClick(button) {
        const operationId = document.getElementById("id-input").value;
        const workerId = button.dataset.workerId;
        const positionCode = button.dataset.positionCode;

        const result = await api.removeOperationWorker(operationId, workerId, positionCode);
        if (result.code === 0) {
            await this.getSelectedWorkers();
            this.showSelectedWorkers();
            this.showLinearFreeWorkers();
            this.showWorkersDivs();
        } else {
            viewUtils.showError(result.message);
        }
    }

    showSelectedWorker(worker, button) {
        button.type = "button";
        button.classList.add("list-group-item");
        button.style.cssText = "text-align: left";
        button.dataset.workerId = worker.id;
        button.dataset.positionCode = worker.positionCode;
        button.append("[" + worker.positionShortName + "] ");
        const workerFullNameB = document.createElement("b");
        workerFullNameB.textContent = worker.fullName;
        button.append(workerFullNameB);
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

    async allocateWorkers() {
        const result = await api.allocateOperationWorkers(this.getOperationId());
        if (result.code === 0) {
            location.reload();
        } else {
            viewUtils.showError(result.message);
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
        const id = this.getOperationId();
        const result = await api.startOperationWork(id);
        if (result.code === 0) {
            location.reload();
        } else {
            viewUtils.showError(result.message);
        }
    }

    async finishOperationWork() {
        const id = this.getOperationId();
        const result = await api.finishOperationWork(id);
        if (result.code === 0) {
            location.reload();
        } else {
            viewUtils.showError(result.message);
        }
    }

    showOperationsPage() {
        let operationsPageUrl = sessionStorage.getItem("OPERATIONS_PAGE_URL");
        if (operationsPageUrl == null || operationsPageUrl == "")
            operationsPageUrl = "/OperationsPage2"
        location.href = operationsPageUrl;
    }

    async finishOperation() {
        const id = this.getOperationId();
        const result = await api.finishOperation(id);
        if (result.code === 0) {
            this.showOperationsPage();
        } else {
            viewUtils.showError(result.message);
        }        
    }

    async cancelOperation() {
        if (!window.confirm("Отменить операцию двора?")) {
            return;
        }
        const id = this.getOperationId();
        const result = await api.cancelOperation(id);
        if (result.code === 0) {
            this.showOperationsPage();
        } else {
            viewUtils.showError(result.message);
        }   
    }

    async deleteOperation() {
        if (!window.confirm("Удалить операцию двора?")) {
            return;
        }
        const id = this.getOperationId();
        const result = await api.deleteOperation(id);
        if (result.code === 0) {
            this.showOperationsPage();
        } else {
            viewUtils.showError(result.message);
        }
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
            const result = await api.getOperation(Number(id));
            if (result.code === 0) {
                operation = result.operation;
            } else {
                viewUtils.showError(result.message);
            }
        }

        const workersDiv = document.getElementById("workers-div");
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
            this.planLinearLaborCost = Number(operation.planLinearLaborCost);
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

            if (workersDiv != null && workersDiv != undefined) {
                const states = ["STARTED", "WORK_STARTED", "WORK_FINISHED", "FINISHED"];
                if (states.includes(this.stateCode)) {
                    workersDiv.classList.remove("d-none");

                    const workersFilterInput = document.getElementById("worker-filter-input");
                    workersFilterInput.onchange = this.onChangeWorkersFilter.bind(this);

                    await this.getSelectedWorkers();
                    this.showSelectedWorkers();
                    const workersFieldset = document.getElementById("workers-fieldset");
                    const workersFilterDiv = document.getElementById("workers-filter-div");
                    const allWorkersDiv = document.getElementById("linear-workers-div");
                    if (this.stateCode === "STARTED") {
                        await this.getLinearFreeWorkers();
                        this.showLinearFreeWorkers();
                        workersFieldset.disabled = false;
                        workersFilterDiv.classList.remove("d-none");
                        allWorkersDiv.classList.remove("d-none")
                    } else {
                        workersFieldset.disabled = true;
                        workersFilterDiv.classList.add("d-none");
                        allWorkersDiv.classList.add("d-none")
                    }
                    this.showWorkersDivs();
                } else {
                    workersDiv.classList.add("d-none");
                }
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
            if (workersDiv != null && workersDiv != undefined) {
                workersDiv.classList.add("d-none");
            }
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

        const showOperationTypeNoteButton = document.getElementById("operation-type-note-button");
        const closeOperationTypeNoteSpan = document.getElementById("operation-type-note-close-span");

        showOperationTypeNoteButton.onclick = this.showOperationTypeNote.bind(this);
        closeOperationTypeNoteSpan.onclick = this.hideOperationTypeNote.bind(this);

        window.onclick = this.windowClick.bind(this);
    }
}

const operationPage = new OperationPage();

document.addEventListener('DOMContentLoaded', operationPage.show.bind(operationPage));