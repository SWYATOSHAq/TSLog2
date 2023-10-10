/*
    Страница - карточка операции двора.
    Автор - Буров Михаил (m.burov@tslog.ru).
*/
class OperationPage {
    shortUrl;
    action = "";

    /*
        Выбор с начала суток по 8 часов утра след. суток.
        Всего промежуток 32 часа.
        Блоков в часе - 4.
    */
    startHour = 0; // час начала работ
    finishHour = 32; // час окончания работ
    hourBlocksCount = 4; // кол-во временных блоков в одной строке
    blockDuration = 0;

    defaultConstantOperationDuration = (8 * 60 + 15) + (6 * 60 + 15); // Время операции двора, не зависящая от состава
                                                                      // груза(время на постановку и снятие машины) в сек.

    id = -1; // идентификатор операции двора
    stateCode = ""; // код статуса заявки операции двора
    stateName = ""; // название статуса заявки
    startDate = utils.nullDate; // дата и время заезда машины
    planWorkStartDate = utils.nullDate; // плановая дата начала работы
    factWorkStartDate = utils.nullDate; // фактическая дата начала работы
    planWorkFinishDate = utils.nullDate; // плановая дата окончания работы (когда машина убыла за территорию)
    factWorkFinishDate = utils.nullDate; // фактическая дата окончания работы (когда машина убыла за территорию)
    planDuration = 0; // плановая продолжительность(сколько времени машина будет находится на воротах)
    factPlanDuration = 0; // фактическая плановая продолжительность операция двора
    factDuration = 0; // фактическое время на ворота
    fillingPlanLaborCost = 0; // плановые трудозатраты (одни из...)
    hoodmanPlanLaborCost = 0; // плановые трудозатраты, рассчитанные по алгоритму Башлыкова
    //planLinearLaborCost = 0;
    factLaborCost = 0; // фактические трудозатраты
    factBcLaborCost = 0; // фактические трудозатраты по Boss Control
    finishDate = utils.nullDate; // дата и время отъезда машины
    availableIntervals = []; // доступные интервалы времени для постановки машины на ворота
    operationBlocksCount = 0; // кол-во временных блоков на конкретную операцию
    blocks = []; // пустой массив для нужд заполнения
    doDisplay = false;

    kind = "";
    warehouseActions = {}; // заказы или приходы (складские действа), привязанные к заявкам
    selectedWarehouseActions = {}; // выбранные складские действа, привязанные к заявкам

    linearWorkers = []; //линенйные сотрудники
    selectedWorkers = []; // назначенные сотрудники
    selectedWorkersIds = new Set([]); // ID назначенных сотруников
    bcWorks = []; // работы по Boss Control

    constructor() {
        this.shortUrl = location.href.split('?')[0];
        this.blockDuration = 60 * 60 / this.hourBlocksCount;
    }

    getSelectedDate() {
        return $("#work-start-date-picker").datepicker('getDate');
    }

    /* Связана с отображением временных блоков */
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

    /* Связана с отображением временных блоков */
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
        const block = document.getElementById(blockId);
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
            $("#error-p").text("Время недоступно из-за типа операции, типа машины (и других параметров), выберите другой промежуток времени или тип операции");
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

    async getPlanDuration() {
        const operationTypeCode = document.getElementById("operation-types-select").value;
        const vehicleKindCode = document.getElementById("vehicle-kinds-select").value;
        this.planDuration = 0;

        if (operationTypeCode === "" || vehicleKindCode === "") {
            return;
        }

        const result = await api.getStandardPlanOperationDuration(operationTypeCode, vehicleKindCode);

        if (result.code === 0) {
            this.planDuration = result.planDuration;
            if (this.planWorkStartDate > utils.nullDate) {
                this.planWorkFinishDate = new Date(this.planWorkStartDate);
                this.planWorkFinishDate = new Date(this.planWorkFinishDate.setSeconds(this.planDuration));
            }
            this.countOperationBlocks();
        } else {
            viewUtils.showError(result.message);
        }
    }

    // 2022.09.21 Буров Михаил - чепуха, переделано по указанию Алексея Охотникова.
    // Вычисляем "План фактический"
    async getFactPlanDuration() {
        this.factPlanDuration = 0;
    }

    showDuration() {
        const durationInput = document.getElementById("duration-input");
        if (this.planDuration <= 0)
            durationInput.value = "<Не определено>";
        else {
            durationInput.value = "План: " + utils.secondsToString(this.planDuration);
            if (this.factDuration > 0) {
                durationInput.value += ", факт: " + utils.secondsToString(this.factDuration);
            }
        }
    }

    showStartFinish() {
        const startFinishInput = document.getElementById("start-finish-input");
        startFinishInput.value = "<Не указано>";
        if (this.startDate > utils.nullDate) {
            startFinishInput.value = "Прибыла - " + utils.dateTimeToString(this.startDate);
            if (this.finishDate > utils.nullDate) {
                startFinishInput.value = startFinishInput.value + " Убыла - " + utils.dateTimeToString(this.finishDate);
            }
        }        
    }

    async getPlanLaborCosts() {
        const operationTypeCode = document.getElementById("operation-types-select").value;
        const vehicleKindCode = document.getElementById("vehicle-kinds-select").value;
        const partnersSelect = document.getElementById("partners-select");
        this.fillingPlanLaborCost = 0;
        this.hoodmanPlanLaborCost = 0;

        if (operationTypeCode === "" || vehicleKindCode === "") {
            return;
        }

        let actions = [];
        for (let warehouseActionId in this.selectedWarehouseActions) {
            actions.push({ kind: this.kind, id: warehouseActionId });
        }
        let result = await api.getFillingPlanOperationLaborCost(operationTypeCode, vehicleKindCode, actions);
        if (result.code === 0) {
            this.fillingPlanLaborCost = result.data.laborCost;
        } else {
            viewUtils.showError(result.message);
            return;
        }

        const partnerId = partnersSelect.value;
        if (partnerId != "") {
            result = await api.getHoodManPlanOperationLaborCost(operationTypeCode, vehicleKindCode, partnerId, actions);
            if (result.code === 0) {
                this.hoodmanPlanLaborCost = result.laborCost;
            } else {
                viewUtils.showError(result.message);
            }
        }
    }

    async showLaborCost() {
        const laborCostInput = document.getElementById("labor-cost-input");
        if (laborCostInput != undefined && laborCostInput != null) {
            if (this.fillingPlanLaborCost <= 0)
                laborCostInput.value = "<Не определено>";
            else {
                laborCostInput.value = "План (рассч.): " + (this.fillingPlanLaborCost / 60).toFixed();
                laborCostInput.value += ", план (уточн.): " + (this.hoodmanPlanLaborCost / 60).toFixed();              
                const bcWorksDiv = document.getElementById("bc-works-div");
                if (this.factWorkStartDate > utils.nullDate
                    && this.factWorkFinishDate > utils.nullDate
                    && this.stateCode != "CANCELED"
                    && bcWorksDiv != undefined
                    && bcWorksDiv != null)
                {
                    const result = await api.getBossControlWorks(this.id)
                    if (result.code === 0) {
                        this.bcWorks = result.works;
                        this.showBCWorks();

                        const showBCWorksButton = document.getElementById("show-bc-works-button");
                        const closeBCWorksSpan = document.getElementById("close-bc-works-span");

                        showBCWorksButton.onclick = this.displayBCWorks.bind(this);
                        closeBCWorksSpan.onclick = this.hideBCWorks.bind(this);
                    } else {
                        viewUtils.showError(result.message);
                        return;
                    }
                }
                if (this.factBcLaborCost > 0) {
                    laborCostInput.value = laborCostInput.value + ", факт по БК: " + (this.factBcLaborCost / 60).toFixed();
                }
            }
        }
    }

    /* Возвращает доступные интревалы времения для назначения операций двора. */
    async getAvalibleIntervals() {
        this.availableIntervals = [];
        const workStartDatePicker = document.getElementById("work-start-date-picker");
        const workStartDate = workStartDatePicker.value;
        const operationTypeCode = document.getElementById("operation-types-select").value;
        const vehicleKindCode = document.getElementById("vehicle-kinds-select").value;

        const result = await api.getAvailableIntervals(workStartDate, operationTypeCode, vehicleKindCode);
        if (result.code === 0) {
            this.availableIntervals = result.availableIntervals;
        } else {
            viewUtils.showError(result.message);
        }
    }

    /* Скрывает панель с выбором временных ячеек. */
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
                button.style.cssText = "margin: 2px";
                button.textContent = (hour % 24).toString().padStart(2, "0") + ":" + (blockIndex * this.blockDuration / 60).toString().padStart(2, "0");

                const blockStartTime = hour * 60 * 60 + blockIndex * this.blockDuration;
                const blockStart = new Date(date);
                blockStart.setSeconds(date.getSeconds() + blockStartTime);
                const blockFinish = new Date(date);
                blockFinish.setSeconds(date.getSeconds() + blockStartTime + this.blockDuration);

                let isFree = false;
                for (let interval of this.availableIntervals) {
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

    /* Всплывающая подсказка по тиипу операции двора. */
    getOperationTypeHint() {
        const select = document.getElementById("operation-types-select");
        if (select.selectedIndex >= 0) {
            select.title = select.options[select.selectedIndex].getAttribute("note");
        }
    }

    /*
        Метод отрабатывает всякий раз когда меняются параметры, влияющие на подбор ворот (время, тип, вид машины, заказы, ...).
        Мы получаем с сервера вороты которые НЕ ЗАНЯТЫ (но могут быть зарезервированы) для указанного промежутка времени,
        и мы отображаем только эти ворота.
        Если вдруг машина по плану должна встать на конкретные ворота, а они заняты, то ворота сбрасываются и система по просит выбрать другие
        (или определить автоматически - пункт "Ворота не выбраны").
        При открытии карточек, в которых уже работа начата (или уже закончена) ничего не делаем, они не редактируются, какие ворота есть такие есть.
    */
    async changeGates() {
        const create = this.action === "CREATE" || this.action === "CREATE_CONFIRM" || this.action === "CREATE_BY_PARTNER";
        const copy = this.action === "COPY" || this.action === "COPY_CONFIRM" || this.action === "COPY_BY_PARTNER";
        if (create || copy) {
            return;
        }
        const operationTypesSelect = document.getElementById("operation-types-select");
        const vehiclekindsSelect = document.getElementById("vehicle-kinds-select");
        const result = await api.getAvailableGates(utils.dateTimeToString(this.planWorkStartDate),
            utils.dateTimeToString(this.planWorkFinishDate),
            operationTypesSelect.value,
            vehiclekindsSelect.value,
            this.id);
        if (result.code === 0) {
            const gates = new Set([]);
            for (let gate of result.availableGates) {
                gates.add("gate-" + gate.id.toString());
            }
            const gatesSelect = document.getElementById("gates-select");
            for (let option of gatesSelect.children) {
                if (option.value === "") {
                    continue;
                }
                const id = option.id;
                if (!gates.has(id)) {
                    option.hidden = true;
                    if (option.value === gatesSelect.value) {
                        alert("Ворота " + gatesSelect.value + " для этой заявки недоступны.\nВыберите другие.");
                        gatesSelect.value = "";
                    }
                }
            }
        }
        else {
            viewUtils.showError(result.message);
            return;
        }
    }

    // функция-обработчик события изменения временного интревала операции двора
    async onChangeDuration() {
        await this.getPlanDuration();
        await this.getPlanLaborCosts();
        await this.getFactPlanDuration();
        this.showDuration();        
        await this.showLaborCost();
        if (this.planDuration <= 0) {
            this.planWorkStartDate = utils.nullDate;
            this.planWorkFinishDate = utils.nullDate;
            this.hideTimeTable();
            this.showWorkStartFinish();
            return;
        } else {
            this.showWorkStartFinish();
        }        
        await this.getAvalibleIntervals();
        this.showTimeTable();
        const blockId = this.getBlockId(this.planWorkStartDate);
        if (blockId != "") {
            this.doDisplay = true;
            this.showSelectedBlocks(blockId);
            this.doDisplay = false;
        }
        await this.changeGates();
    }

    getKind() {
        const operationTypesSelect = document.getElementById("operation-types-select");
        return operationTypesSelect.options[operationTypesSelect.selectedIndex].getAttribute("direction");
    }

    async onChangeOperationType() {
        await this.onChangeDuration();

        this.getOperationTypeHint();
        const kind = this.getKind();
        if (this.kind != kind) {
            this.clearSelectedWarehouseActions();
            await this.displayWarehouseActions();
            this.kind = kind;            
            this.changeWarehouseActionsTitle();
            this.resizeWarehouseActionsDivs();
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
        this.clearSelectedWarehouseActions();
        await this.displayWarehouseActions();
        this.resizeWarehouseActionsDivs();
        await this.onChangeDuration();
    }

    onChangeParking() {
        const parkingCheckBox = document.getElementById("parkingCheck");
        const parkingActive = parkingCheckBox != null ? document.getElementById("parkingCheck").checked : false;
        const gatesSelect = document.getElementById("gates-select");
        const setGateButton = document.getElementById("set-gate-button");
        if (parkingActive) {
            gatesSelect.disabled = true;
            if (setGateButton !== null) {
                setGateButton.style.display = 'none';
            }                
            gatesSelect.selectedIndex = 0;
        } else {
            gatesSelect.disabled = false;
            if (setGateButton !== null) {
                setGateButton.style.display = 'inline-block';
            }            
        }
    }

    async createOperation() {
        if (this.planWorkStartDate <= utils.nullDate || this.planWorkFinishDate <= utils.nullDate ||
            document.getElementById("operation-types-select").value == "" ||
            document.getElementById("vehicle-kinds-select").value == "" ||
            document.getElementById("partners-select").value == "" ||
            Object.keys(this.selectedWarehouseActions).length <= 0)
        {
            alert("Не все обязательные поля заполнены!");
            return;
        }

        let actions = [];
        for (let warehouseActionId in this.selectedWarehouseActions) {
            actions.push({ operationId: this.id, kind: this.kind, id: warehouseActionId });
        }
        const data = {
            startDate: utils.dateTimeToString(this.planWorkStartDate),
            operationTypeCode: document.getElementById("operation-types-select").value,
            vehicleKindCode: document.getElementById("vehicle-kinds-select").value,
            vehicleBrand: document.getElementById("vehicle-brand-input").value,
            vehicleModel: document.getElementById("vehicle-model-input").value,
            vehicleNumber: document.getElementById("vehicle-number-input").value,
            trailerNumber: document.getElementById("trailer-number-input").value,
            onParking: document.getElementById("parkingCheck") !== null && document.getElementById("parkingCheck").checked,
            driverName: document.getElementById("driver-name-input").value,
            driverPhoneNumber: document.getElementById("driver-phone-number-input").value,
            partnerManagerName: document.getElementById("partner-manager-name-input").value,
            partnerManagerPhoneNumber: document.getElementById("partner-manager-phone-number-input").value,
            partnerManagerEmail: document.getElementById("partner-manager-email-input").value,
            note: document.getElementById("note-textarea").value,
            partnerId: document.getElementById("partners-select").value,
            warehouseActions: actions
        };

        let result = await api.createOperation(data);
        if (result.code === 0) {
            const operation = result.operation;
            //for (let warehouseActionId in this.selectedWarehouseActions) {
            //    result = await api.addOperationWarehouseAction(operation.id, this.kind, warehouseActionId);
            //    if (result.code != 0) {
            //        viewUtils.showError(result.message);
            //        break;
            //    }
            //}
            location.href = this.shortUrl + "?id=" + operation.id + "&action=VIEW";
        } else {
            viewUtils.showError(result.message);
        }       
    }

    async createConfirmOperation() {
        if (this.planWorkStartDate <= utils.nullDate || this.planWorkFinishDate <= utils.nullDate ||
            document.getElementById("operation-types-select").value == "" ||
            document.getElementById("vehicle-kinds-select").value == "" ||
            document.getElementById("partners-select").value == "" ||            
            Object.keys(this.selectedWarehouseActions).length <= 0)
        {
            alert("Не все обязательные поля заполнены!");
            return;
        }

        let actions = [];
        for (let warehouseActionId in this.selectedWarehouseActions) {
            actions.push({ operationId: this.id, kind: this.kind, id: warehouseActionId });
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
            onParking: document.getElementById("parkingCheck") !== null && document.getElementById("parkingCheck").checked,
            driverPhoneNumber: document.getElementById("driver-phone-number-input").value,
            partnerManagerName: document.getElementById("partner-manager-name-input").value,
            partnerManagerPhoneNumber: document.getElementById("partner-manager-phone-number-input").value,
            partnerManagerEmail: document.getElementById("partner-manager-email-input").value,
            note: document.getElementById("note-textarea").value,
            partnerId: document.getElementById("partners-select").value,
            warehouseActions: actions
        };

        let result = await api.createConfirmOperation(data);
        if (result.code === 0) {
            const operation = result.operation;
            //for (let warehouseActionId in this.selectedWarehouseActions) {
            //    const result = await api.addOperationWarehouseAction(operation.id, this.kind, warehouseActionId);
            //    if (result.code != 0) {
            //        viewUtils.showError(result.message);
            //        break;
            //    }
            //}
            location.href = this.shortUrl + "?id=" + operation.id + "&action=VIEW";
        } else {
            viewUtils.showError(result.message);
        }
    }

    async createOperationByPartner() {
        if (this.planWorkStartDate <= utils.nullDate || this.planWorkFinishDate <= utils.nullDate ||
            document.getElementById("operation-types-select").value == "" ||
            document.getElementById("vehicle-kinds-select").value == "" || 
            Object.keys(this.selectedWarehouseActions).length <= 0)
        {
            alert("Не все обязательные поля заполнены!");
            return;
        }

        let actions = [];
        for (let warehouseActionId in this.selectedWarehouseActions) {
            actions.push({ operationId: this.id, kind: this.kind, id: warehouseActionId });
        }
        const data = {
            startDate: utils.dateTimeToString(this.planWorkStartDate),
            operationTypeCode: document.getElementById("operation-types-select").value,
            vehicleKindCode: document.getElementById("vehicle-kinds-select").value,
            vehicleBrand: document.getElementById("vehicle-brand-input").value,
            vehicleModel: document.getElementById("vehicle-model-input").value,
            onParking: document.getElementById("parkingCheck") !== null && document.getElementById("parkingCheck").checked,
            vehicleNumber: document.getElementById("vehicle-number-input").value,
            trailerNumber: document.getElementById("trailer-number-input").value,
            driverName: document.getElementById("driver-name-input").value,
            driverPhoneNumber: document.getElementById("driver-phone-number-input").value,
            partnerManagerName: document.getElementById("partner-manager-name-input").value,
            partnerManagerPhoneNumber: document.getElementById("partner-manager-phone-number-input").value,
            partnerManagerEmail: document.getElementById("partner-manager-email-input").value,
            note: document.getElementById("note-textarea").value,
            warehouseActions: actions
        };

        let result = await api.createOperationByPartner(data);
        if (result.code === 0) {
            const operation = result.operation;
            //for (let warehouseActionId in this.selectedWarehouseActions) {
            //    result = await api.addOperationWarehouseAction(operation.id, this.kind, warehouseActionId);
            //    if (result.code != 0) {
            //        viewUtils.showError(result.message);
            //        break;
            //    }
            //}
            location.href = this.shortUrl + "?id=" + operation.id + "&action=VIEW_BY_PARTNER";
        } else {
            viewUtils.showError(result.message);
        }
    }

    async editOperation() {
        if (this.planWorkStartDate <= utils.nullDate || this.planWorkFinishDate <= utils.nullDate ||
            document.getElementById("operation-types-select").value == "" ||
            document.getElementById("vehicle-kinds-select").value == "" ||
            document.getElementById("vehicle-brand-input").value == "" ||
            document.getElementById("vehicle-model-input").value == "" ||
            document.getElementById("vehicle-number-input").value == "" ||
            document.getElementById("partners-select").value == "" ||
            document.getElementById("driver-name-input").value == "" ||
            Object.keys(this.selectedWarehouseActions).length <= 0)
        {            
            alert("Не все обязательные поля заполнены!");
            return;
        }

        let actions = [];
        for (let warehouseActionId in this.selectedWarehouseActions) {
            actions.push({ operationId: this.id, kind: this.kind, id: warehouseActionId });
        }
        const data = {
            id: this.id,
            startDate: utils.dateTimeToString(this.planWorkStartDate),
            operationTypeCode: document.getElementById("operation-types-select").value,
            vehicleKindCode: document.getElementById("vehicle-kinds-select").value,
            onParking: document.getElementById("parkingCheck") !== null && document.getElementById("parkingCheck").checked,
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
            warehouseActions: actions
        };
        
        let result = await api.editOperation(data);
        if (result.code != 0) {
            viewUtils.showError(result.message);
            return;
        }
        location.reload();
    }

    async editOperationByPartner() {
        if (this.planWorkStartDate <= utils.nullDate || this.planWorkFinishDate <= utils.nullDate ||
            document.getElementById("operation-types-select").value == "" ||
            document.getElementById("vehicle-kinds-select").value == "" ||
            document.getElementById("vehicle-brand-input").value == "" ||
            document.getElementById("vehicle-model-input").value == "" ||
            document.getElementById("vehicle-number-input").value == "" ||
            document.getElementById("driver-name-input").value == "" ||
            Object.keys(this.selectedWarehouseActions).length <= 0)
        {
            alert("Не все обязательные поля заполнены!");
            return;
        }

        let actions = [];
        for (let warehouseActionId in this.selectedWarehouseActions) {
            actions.push({ operationId: this.id, kind: this.kind, id: warehouseActionId });
        }
        const data = {
            id: this.id,
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
            warehouseActions: actions
        };

        let result = await api.editOperationByPartner(data);
        if (result.code != 0) {
            viewUtils.showError(result.message);
            return;
        }
        location.reload();
    }

    copyOperation() {
        location.href = "/OperationPage?id=" + this.id + "&action=COPY";
    }

    copyConfirmOperation() {
        location.href = "/OperationPage?id=" + this.id + "&action=COPY_CONFIRM";
    }

    copyOperationByPartner() {
        location.href = "/OperationPage?id=" + this.id + "&action=COPY_BY_PARTNER";
    }

    async editOperationNote() {
        const note = document.getElementById("note-textarea").value;
        const result = await api.editOperationNote(this.id, note);
        if (result.code === 0) {
            location.reload();
        } else {
            viewUtils.showError(result.message);
        }
    }

    async setOperationGate() {
        const result = await api.setOperationGate(this.id, "");
        if (result.code === 0) {
            location.reload();
        } else {
            viewUtils.showError(result.message);
        }
    }

    async startOperation() {
        const result = await api.startOperation(this.id);
        if (result.code == 0) {
            location.reload();
        } else {
            viewUtils.showError(result.message);
        }
    }

    // #region Warehouse actions
    async getAllWarehouseActions() {
        const allWarehouseActionDiv = document.getElementById("all-actions-div");
        if (allWarehouseActionDiv != undefined && allWarehouseActionDiv != null) {
            this.warehouseActions = {};

            const kind = this.getKind();
            if (kind != "O" && kind != "I") {
                return;
            }

            const partnersSelect = document.getElementById("partners-select");
            const partnerId = partnersSelect.value;
            if (partnerId === "") {
                return;
            }

            const pattern = document.getElementById("action-filter-input").value.trim().toLowerCase();
            const result = await api.findWarehouseActions(kind, pattern, partnerId);
            if (result.code === 0) {
                for (let warehouseAction of result.data) {
                    this.warehouseActions[warehouseAction.id] = warehouseAction;
                }
            }
            else {
                viewUtils.showError(result.message);
            }
        }        
    }

    async getSelectedWarehouseActions() {
        const partnersSelect = document.getElementById("partners-select");        
        partnersSelect.value = "";
        const result = await api.getSelectedWarehouseActions(this.id);
        if (result.code === 0) {
            for (let action of result.data) {
                this.selectedWarehouseActions[action.id] = action;
                if (partnersSelect.value === "") {
                    partnersSelect.value = action.partnerId;
                }
            }
        }
        else {
            this.selectedWarehouseActions = {};
            viewUtils.showError(result.message);
        }
    }

    async onWarehouseActionClick(button) {
        const id = button.dataset.actionId;
        const warehouseAction = this.warehouseActions[id];
        button.remove();
        this.selectedWarehouseActions[id] = warehouseAction;

        const selectedWarehouseActionsDiv = document.getElementById("selected-actions-content-div");
        const selectedButton = document.createElement("button");
        await this.showWarehouseAction(warehouseAction, selectedButton, "REMOVE");
        selectedWarehouseActionsDiv.append(selectedButton);

        this.resizeWarehouseActionsDivs();
        await this.onChangeDuration();
    }

    async onSelectedWarehouseActionClick(selectedButton) {
        const id = selectedButton.dataset.actionId;
        if (confirm("Подтверждаете удаление заказа из назначенных?")) {
            delete this.selectedWarehouseActions[id];
            selectedButton.remove();
            await this.showAllWarehouseActions();

            this.resizeWarehouseActionsDivs();
            await this.onChangeDuration();
        }        
    }

    async showWarehouseAction(action, button, direction) {
        button.type = "button";
        button.classList.add("list-group-item");
        button.style.cssText = "text-align: left !important; width: 100%";
        button.dataset.actionKind = action.kind;
        button.dataset.actionId = action.id;
        button.dataset.partnerActionId = action.partnerActionId;
        button.dataset.actionPartnerId = action.partnerId;
        button.dataset.actionPartnerName = action.partnerName;
        button.dataset.actionPlanActionDate = action.planActionDate;
        button.dataset.actionInsertDate = action.insertDate;

        const workerFullNameB = document.createElement("b");
        workerFullNameB.textContent = action.partnerActionId != "" ? action.partnerActionId : "Б/Н";
        button.append(workerFullNameB);

        const date = utils.stringToDate(action.insertDate);
        if (date > utils.nullDate) {
            button.append(" (" + utils.dateToString(date) + ") ");
        }        

        button.onclick = async () => {
            if (direction === "ADD") {
                await this.onWarehouseActionClick(button);
            } else if (direction === "REMOVE") {
                await this.onSelectedWarehouseActionClick(button);
            } else {
                alert("Unknown direction " + direction);
            }
        }
    }

    async showAllWarehouseActions() {
        const allWarehoiseActionsDiv = document.getElementById("all-actions-content-div");
        allWarehoiseActionsDiv.replaceChildren();
        for (let id in this.warehouseActions) {
            if (this.selectedWarehouseActions.hasOwnProperty(id)) {
                continue;
            }
            const warehouseAction = this.warehouseActions[id];
            const button = document.createElement("button");
            await this.showWarehouseAction(warehouseAction, button, "ADD");
            allWarehoiseActionsDiv.append(button);
        }
    }

    async displayWarehouseActions() {        
        const allWarehouseActionDiv = document.getElementById("all-actions-div");
        const actionFilterDiv = document.getElementById("action-filter-div");
        const actionsDiv = document.getElementById("actions-div");
        if (allWarehouseActionDiv != undefined && allWarehouseActionDiv != null) {
            const operationTypesSelect = document.getElementById("operation-types-select");
            const partnersSelect = document.getElementById("partners-select");
            if (partnersSelect.value != "" && operationTypesSelect.value != "") {
                actionFilterDiv.classList.remove("d-none");
                actionsDiv.classList.remove("d-none");
                await this.getAllWarehouseActions();
                await this.showAllWarehouseActions();
            } else {
                actionFilterDiv.classList.add("d-none");
                actionsDiv.classList.add("d-none");
            }        
        }        
    }

    clearSelectedWarehouseActions() {
        this.selectedWarehouseActions = {};
        const selectedActionsDiv = document.getElementById("selected-actions-content-div");
        selectedActionsDiv.replaceChildren();
    }

    async displaySelectedWarehouseActions() {
        await this.getSelectedWarehouseActions();
        const selectedWarehouseActionsDiv = document.getElementById("selected-actions-content-div");
        selectedWarehouseActionsDiv.replaceChildren();
        for (let id in this.selectedWarehouseActions) {
            const button = document.createElement("button");
            await this.showWarehouseAction(this.selectedWarehouseActions[id], button, "REMOVE");
            selectedWarehouseActionsDiv.append(button);
        }
    }

    async onChangeActionsFilter() {
        await this.getAllWarehouseActions();
        await this.showAllWarehouseActions();
        this.resizeWarehouseActionsDivs();
    }

    changeWarehouseActionsTitle() {
        const allWarehouseActionsTitleB = document.getElementById("all-warehouse-actions-title-b");
        const selectedWarehouseActionsTitleB = document.getElementById("selected-warehouse-actions-title-b");
        const actionsFilterSpan = document.getElementById("action-filter-span");
        //Назначенные заказы (приходы):
        if (this.kind === "O") {
            if (actionsFilterSpan != undefined && actionsFilterSpan != null) {
                actionsFilterSpan.textContent = "Поиск заказа:";
            }
            if (allWarehouseActionsTitleB != undefined && allWarehouseActionsTitleB != null) {
                allWarehouseActionsTitleB.textContent = "Свободные заказы:";
            }            
            selectedWarehouseActionsTitleB.textContent = "Назначенные заказы:";
        } else if (this.kind === "I") {
            if (actionsFilterSpan != undefined && actionsFilterSpan != null) {
                actionsFilterSpan.textContent = "Поиск прихода:";
            }
            if (allWarehouseActionsTitleB != undefined && allWarehouseActionsTitleB != null) {
                allWarehouseActionsTitleB.textContent = "Свободные приходы:";
            }
            selectedWarehouseActionsTitleB.textContent = "Назначенные приходы:";
        } else {
            if (actionsFilterSpan != undefined && actionsFilterSpan != null) {
                actionsFilterSpan.textContent = "Поиск заказа (прихода):";
            }
            if (allWarehouseActionsTitleB != undefined && allWarehouseActionsTitleB != null) {
                allWarehouseActionsTitleB.textContent = "Свободные заказы (приходы):";
            }
            selectedWarehouseActionsTitleB.textContent = "Назначенные заказы (приходы):";
        }
    }

    resizeWarehouseActionsDivs() {
        const ACTIONS_CONTENT_DIV_TEXT_STYLE_SHORT = "width: 100%; height: fit-content; max-height: 180px; padding: 0; margin: 0";
        const ACTIONS_CONTENT_DIV_TEXT_STYLE_LONG = "width: 100%; height: fit-content; max-height: 180px; overflow-y: scroll; padding: 0; margin: 0";
        let actionsContentDiv = document.getElementById("all-actions-content-div");
        if (actionsContentDiv != undefined && actionsContentDiv != null) {
            if (actionsContentDiv.childElementCount < 5) {
                actionsContentDiv.style.cssText = ACTIONS_CONTENT_DIV_TEXT_STYLE_SHORT;
            } else {
                actionsContentDiv.style.cssText = ACTIONS_CONTENT_DIV_TEXT_STYLE_LONG;
            }
        }        
        actionsContentDiv = document.getElementById("selected-actions-content-div");
        if (actionsContentDiv != undefined && actionsContentDiv != null) {
            if (actionsContentDiv.childElementCount < 5) {
                actionsContentDiv.style.cssText = ACTIONS_CONTENT_DIV_TEXT_STYLE_SHORT;
            } else {
                actionsContentDiv.style.cssText = ACTIONS_CONTENT_DIV_TEXT_STYLE_LONG;
            }
        }           
    }
    // #endregion

    // #region Workers
    showWorkersDivs() {
        const WORKERS_DIV_TEXT_STYLE_SHORT = "width: 100%; height: fit-content; max-height: 320px; padding: 0; margin: 0; border-radius: 0";
        const WORKERS_DIV_TEXT_STYLE_LONG = "width: 100%; height: fit-content; max-height: 320px; overflow-y: scroll; padding: 0; margin: 0; border-radius: 0";
        let workersDiv = document.getElementById("linear-workers-content-div");
        if (workersDiv != undefined && workersDiv != null) {
            if (workersDiv.childElementCount < 8) {
                workersDiv.style.cssText = WORKERS_DIV_TEXT_STYLE_SHORT;
            } else {
                workersDiv.style.cssText = WORKERS_DIV_TEXT_STYLE_LONG;
            }
        }
        
        workersDiv = document.getElementById("selected-workers-content-div");
        if (workersDiv != undefined && workersDiv != null) {
            if (workersDiv.childElementCount < 8) {
                workersDiv.style.cssText = WORKERS_DIV_TEXT_STYLE_SHORT;
            } else {
                workersDiv.style.cssText = WORKERS_DIV_TEXT_STYLE_LONG;
            }
        }        
    }

    async getLinearFreeWorkers() {
        const result = await api.getLinearFreeWorkers();
        if (result.code === 0) {
            this.linearWorkers = result.data;
        }
        else {
            viewUtils.showError(result.message);
        }
    }

    onChangeWorkersFilter() {
        this.showLinearFreeWorkers();
        this.showWorkersDivs();
    }

    async onLinearWorkerClick(button) {
        const operationId = this.id;
        const workerId = button.dataset.workerId;
        const positionCode = button.dataset.positionCode;

        const result = await api.addOperationWorker(operationId, workerId, positionCode);
        if (result.code === 0) {
            await this.getSelectedWorkers();
            await this.showSelectedWorkers();
            this.showLinearFreeWorkers();
            this.showWorkersDivs();
        } else {
            viewUtils.showError(result.message);
        }
    }

    showLinearFreeWorker(worker, button) {
        button.type = "button";
        button.classList.add("list-group-item");
        button.style.cssText = "text-align: left !important; width: 100%";
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
        const allWorkersContentDiv = document.getElementById("linear-workers-content-div");
        allWorkersContentDiv.replaceChildren();
        const namePattern = document.getElementById("worker-filter-input").value.trim().toLowerCase();
        for (let worker of this.linearWorkers) {
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
            allWorkersContentDiv.append(button);
        }
    }

    async getSelectedWorkers() {
        const result = await api.getOperationWorkers(this.id);
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
        const operationId = this.id;
        const workerId = button.dataset.workerId;
        const positionCode = button.dataset.positionCode;

        const result = await api.removeOperationWorker(operationId, workerId, positionCode);
        if (result.code === 0) {
            await this.getSelectedWorkers();
            await this.showSelectedWorkers();
            this.showLinearFreeWorkers();
            this.showWorkersDivs();
        } else {
            viewUtils.showError(result.message);
        }
    }

    showSelectedWorker(worker, button) {
        button.type = "button";
        button.classList.add("list-group-item");
        button.style.cssText = "text-align: left !important; width: 100%";
        button.dataset.workerId = worker.id;
        button.dataset.positionCode = worker.positionCode;
        button.append("[" + worker.positionShortName + "] ");
        const workerFullNameB = document.createElement("b");
        workerFullNameB.textContent = worker.fullName;
        button.append(workerFullNameB);
        button.onclick = async () => {
            await this.onSelectedWorkerClick(button);
        }
    }

    async showSelectedWorkers() {
        const selectedWorkersContentDiv = document.getElementById("selected-workers-content-div");
        selectedWorkersContentDiv.replaceChildren();
        for (let operationWorker of this.selectedWorkers) {
            const operationWorkerButton = document.createElement("button");
            await this.showSelectedWorker(operationWorker, operationWorkerButton);
            selectedWorkersContentDiv.append(operationWorkerButton);
        }
    }

    async allocateWorkers() {
        const result = await api.allocateOperationWorkers(this.id);
        if (result.code === 0) {
            location.reload();
        } else {
            viewUtils.showError(result.message);
        }
    }
    // #endregion

    async startOperationWork() {
        if (this.selectedWorkersIds.size <= 0) {
            alert("Работники не назначены!");
            return;
        }
        const gatesSelect = document.getElementById("gates-select");
        if (gatesSelect.selectedIndex === 0) {
            alert("Ворота не выбраны!");
            return;
        }
        const result = await api.startOperationWork(this.id);
        if (result.code === 0) {
            location.reload();
        } else {
            viewUtils.showError(result.message);
        }
    }

    async finishOperationWork() {
        const result = await api.finishOperationWork(this.id);
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
        const result = await api.finishOperation(this.id);
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
        const result = await api.cancelOperation(this.id);
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
        const result = await api.deleteOperation(this.id);
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
        window.open("/OperationPrintPage?id=" + this.id, "_blank");
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

    // #region BossControl works
    showBCWorks() {
        const bcWorksTBody = document.getElementById("bc-works-tbody");
        bcWorksTBody.replaceChildren();

        const workersIds = new Set();

        this.factBcLaborCost = 0;
        for (let work of this.bcWorks) {
            this.factBcLaborCost += work.duration;
            const workTr = document.createElement("tr");
            viewUtils.createCell(workTr, ["table-cell"], [work.workerFullName]);
            viewUtils.createCell(workTr, ["table-cell"], [work.positionName]);
            viewUtils.createCell(workTr, ["table-cell"], [utils.convertTimeHHMM(work.startDate) + " (" + utils.convertTimeHHMM(work.clippedStartDate) + ")"]);
            viewUtils.createCell(workTr, ["table-cell"], [utils.convertTimeHHMM(work.finishDate) + " (" + utils.convertTimeHHMM(work.clippedFinishDate) + ")"]);
            viewUtils.createCell(workTr, ["table-cell"], [Math.round(work.duration / 60) + " (" + Math.round(work.clippedDuration / 60) + ")"]);
            bcWorksTBody.append(workTr);

            if (!workersIds.has(work.workerId))
                workersIds.add(work.workerId);
        }
    }

    displayBCWorks() {
        const noteDiv = document.getElementById("bc-works-div");
        noteDiv.style.display = "block";
    }

    hideBCWorks() {
        const noteDiv = document.getElementById("bc-works-div");
        noteDiv.style.display = "none";
    }
    // #endregion

    showDialogBlock() {        
        const p = document.getElementById("dialog-block-p");
        p.innerText = "Работы по заявке по ПРР ПО ПЛАНУ должны были быть уже выполнены.\nАктуализируйте плановое время работ по заявке!";
        const dialogBlockDiv = document.getElementById("dialog-block-div");
        dialogBlockDiv.style.display = "block";
    }

    hideDialogBlock() {
        const dialogBlockDiv = document.getElementById("dialog-block-div");
        dialogBlockDiv.style.display = "none";
    }

    windowClick(event) {
        const noteDiv = document.getElementById("operation-type-note-div");
        const bcWorksDiv = document.getElementById("bc-works-div");
        const dialogBlockDiv = document.getElementById("dialog-block-div");
        if (event.target == noteDiv) {
            this.hideOperationTypeNote();
        } else if (event.target == bcWorksDiv) {
            this.hideBCWorks();
        } else if (event.target == dialogBlockDiv) {
            this.hideDialogBlock();
        }
    }

    async show() {
        // CREATE, CREATE_CONFIRM, CREATE_BY_PARTNER, COPY, COPY_BY_PARTNER, VIEW, VIEW_BY_PARTNER
        this.action = utils.getUrlParameter("action");
        const create = this.action === "CREATE" || this.action === "CREATE_CONFIRM" || this.action === "CREATE_BY_PARTNER";
        const copy = this.action === "COPY" || this.action === "COPY_CONFIRM" || this.action === "COPY_BY_PARTNER";

        const response2 = await api.getIsPartnerUser();
        let isPartnerUser = false;
        if (response2.code === 0) {
            isPartnerUser = response2.isPartnerUser;
        } else {
            viewUtils.showError(response2.message);
        }

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

        // Id
        this.id = operation != null ? operation.id : -1;

        // State
        this.stateCode = operation != null && !copy ? operation.stateCode : "";
        this.stateName = operation != null && !copy ? operation.stateName : "";

        // Header
        const h1 = document.getElementById("header");
        if (this.action === "CREATE") {
            h1.textContent = "Создание";
        } else if (this.action == "CREATE_CONFIRM") {
            h1.textContent = "Создание (с подтверждением)";
        } else if (this.action == "CREATE_BY_PARTNER") {
            h1.textContent = "Создание (клиентом)";
        } else if (this.action == "COPY") {
            h1.textContent = "Копирование";
        } else if (this.action == "COPY_CONFIRM") {
            h1.textContent = "Копирование (с подтверждением)";
        } else if (this.action == "COPY_BY_PARTNER") {
            h1.textContent = "Копирование (клиентом)";
        } else if (this.action == "VIEW") {
            h1.textContent = "#" + this.id + " (" + this.stateName + ")";
        } else if (this.action == "VIEW_BY_PARTNER") {
            h1.textContent = "#" + this.id + " (" + this.stateName + ")";
        }

        // Признак АКТУАЛЬНОЙ операции двора, но которая ещё не пошла в работу (на ворота).
        const isPreparing = this.stateCode === "" || this.stateCode === "CREATED" || this.stateCode === "CONFIRMED" || this.stateCode === "STARTED";

        // Operation type
        const operationTypesSelect = document.getElementById("operation-types-select");
        operationTypesSelect.value = operation != null ? operation.typeCode : "";
        if (isPreparing) {
            if (!operationTypesSelect.disabled) {
                operationTypesSelect.onchange = this.onChangeOperationType.bind(this);
            }
        } else {
            operationTypesSelect.disabled = true;
        }
        this.kind = this.getKind();
        const showOperationTypeNoteButton = document.getElementById("operation-type-note-button");
        const closeOperationTypeNoteSpan = document.getElementById("operation-type-note-close-span");
        showOperationTypeNoteButton.onclick = this.showOperationTypeNote.bind(this);
        closeOperationTypeNoteSpan.onclick = this.hideOperationTypeNote.bind(this);
        this.getOperationTypeHint();

        // Vehicle kind
        const vehiclekindsSelect = document.getElementById("vehicle-kinds-select");
        vehiclekindsSelect.value = operation != null ? operation.vehicleKindCode : "";
        if (isPreparing) {
            vehiclekindsSelect.onchange = this.onChangeDuration.bind(this);
        } else {
            vehiclekindsSelect.disabled = true;
        }

        // Work start and finish (plan and fact)        
        this.planWorkStartDate = operation != null && !copy ? new Date(operation.planWorkStartDate) : utils.nullDate;
        this.factWorkStartDate = operation != null && !copy ? new Date(operation.factWorkStartDate) : utils.nullDate;
        this.planWorkFinishDate = operation != null && !copy ? new Date(operation.planWorkFinishDate) : utils.nullDate;
        this.factWorkFinishDate = operation != null && !copy ? new Date(operation.factWorkFinishDate) : utils.nullDate;
        if (operation != null) {
            this.countOperationBlocks();
        }
        this.showWorkStartFinish();

        // Если заявка просрочена, выводим предупеждение.
        const isLate = isPreparing && !copy && !create && this.factWorkStartDate <= utils.nullDate && this.planWorkFinishDate < new Date();
        if (isLate) {
            this.showDialogBlock();
            this.planWorkStartDate = utils.nullDate;
            this.factWorkStartDate = utils.nullDate;
        }
        

        // Warehouse actions (and partner)        
        const partnersSelect = document.getElementById("partners-select");
        partnersSelect.onchange = this.onChangePartner.bind(this);
        if (this.id > 0) {
            await this.displaySelectedWarehouseActions();
            if (copy) {
                this.clearSelectedWarehouseActions();
            }
        }

        // Duration        
        await this.getPlanDuration();
        await this.getFactPlanDuration();
        this.factDuration = operation != null && !copy ? Number(operation.factDuration) : 0;
        this.showDuration();
        const timetableFieldset = document.getElementById("timetable-fieldset");
        const timeTableDisabled = this.stateCode === "WORK_STARTED" || this.stateCode === "WORK_FINISHED" || this.stateCode === "FINISHED" ||
            this.stateCode === "CANCELED" || operationTypesSelect.disabled;
        timetableFieldset.disabled = timeTableDisabled;
        let tempDate = operation != null && !copy && !isLate ? new Date(this.planWorkStartDate) : new Date();
        tempDate.setHours(0, 0, 0, 0);
        const workStartDatePicker = $("#work-start-date-picker");
        workStartDatePicker.datepicker({
            format: "dd.mm.yyyy",
            autoclose: true,
            language: "ru",
            todayBtn: true,
            todayHighlight: true,
        });
        workStartDatePicker.datepicker("setDate", tempDate);
        workStartDatePicker.on("changeDate", this.onChangePlanWorkStartDate.bind(this));
        if (operation != null) {
            if (!timeTableDisabled) {
                await this.getAvalibleIntervals();
            }
            this.countOperationBlocks();
            this.showTimeTable();
            const blockId = this.getBlockId(this.planWorkStartDate);
            if (blockId != "") {
                this.doDisplay = true;
                this.showSelectedBlocks(blockId);
                this.doDisplay = false;
            }
        } else {
            this.hideTimeTable();
        }

        // Start and finish
        this.startDate = operation != null && !copy ? new Date(operation.startDate) : utils.nullDate;
        this.finishDate = operation != null && !copy ? new Date(operation.finishDate) : utils.nullDate;
        this.showStartFinish();

        // Labor cost
        await this.getPlanLaborCosts();
        this.factLaborCost = operation != null && !copy ? Number(operation.factLaborCost) : 0;
        await this.showLaborCost();

        // Manager        
        await this.showManager();

        const warehouseActionsFieldset = document.getElementById("warehouse-actions-fieldset");
        const actionFiltersDiv = document.getElementById("action-filter-div");
        const allActionsDiv = document.getElementById("all-actions-div");
        if (isPreparing) {
            warehouseActionsFieldset.disabled = operationTypesSelect.disabled;
            const actionsFilterInput = document.getElementById("action-filter-input");
            if (actionsFilterInput != undefined && actionsFilterInput != null) {
                actionsFilterInput.onchange = this.onChangeActionsFilter.bind(this);
            }
            await this.getAllWarehouseActions();
            await this.displayWarehouseActions();
        } else {
            warehouseActionsFieldset.disabled = true;
            if (actionFiltersDiv != undefined && actionFiltersDiv != null) {
                actionFiltersDiv.remove();
            }
            if (allActionsDiv != undefined && allActionsDiv != null) {
                allActionsDiv.remove();
            }
        }
        this.changeWarehouseActionsTitle();
        this.resizeWarehouseActionsDivs();

        // Responsible officer
        document.getElementById("responsible-officer-input").value =
            operation != null && operation.responsibleOfficer != "" ? operation.responsibleOfficer : "<Не определено>";

        // Creator, editor, create date, edit date
        const creatorInput = document.getElementById("creator-input");
        const editorInput = document.getElementById("editor-input");
        if (operation != null && this.stateCode != "") {
            creatorInput.value = operation.creator + " (" + utils.dateTimeToString(new Date(operation.createDate)) + ")";
            if (operation.editor != "") {
                editorInput.value = operation.editor + " (" + utils.dateTimeToString(new Date(operation.editDate)) + ")";
            }            
        } else {
            document.getElementById("creator-div").remove();
            document.getElementById("editor-div").remove();
        }        

        // Gates and parking
        const parkingCheckBox = document.getElementById("parkingCheck");        
        let parkingActive = parkingCheckBox != null ? document.getElementById("parkingCheck").checked : false;
        if (parkingCheckBox != null) {
            if (operation.onParking) {
                parkingCheckBox.checked = true;
                this.onChangeParking();
            }                            
            parkingCheckBox.onchange = this.onChangeParking.bind(this);
        }
                    
        const gatesSelect = document.getElementById("gates-select");
        if (gatesSelect != undefined && parkingActive != undefined && !parkingActive) {
            const gateCode = operation != null && !copy && !isLate ? operation.gateCode : "";
            gatesSelect.value = gateCode;
            if (!copy && !isPreparing) {
                gatesSelect.disabled = true;
                parkingCheckBox.disabled = true;
            } else {
                await this.changeGates();
            }
        }

        // Additional attributes
        document.getElementById("vehicle-brand-input").value = operation != null ? operation.vehicleBrand : "";
        document.getElementById("vehicle-model-input").value = operation != null ? operation.vehicleModel : "";
        document.getElementById("vehicle-number-input").value = operation != null ? operation.vehicleNumber : "";
        document.getElementById("trailer-number-input").value = operation != null ? operation.trailerNumber : "";
        document.getElementById("driver-name-input").value = operation != null ? operation.driverName : "";
        document.getElementById("driver-phone-number-input").value = operation != null ? operation.driverPhoneNumber : "";
        viewUtils.normalizePhoneNumber();
        document.getElementById("partner-manager-name-input").value = operation != null ? operation.partnerManagerName : "";
        document.getElementById("partner-manager-phone-number-input").value = operation != null ? operation.partnerManagerPhoneNumber : "";
        viewUtils.normalizePhoneNumber();
        document.getElementById("partner-manager-email-input").value = operation != null ? operation.partnerManagerEmail : "";
        document.getElementById("note-textarea").value = operation != null ? operation.note : "";

        const disableAdditionalAttributes = operationTypesSelect.disabled || !isPreparing;
        document.getElementById("vehicle-brand-input").disabled = disableAdditionalAttributes;
        document.getElementById("vehicle-model-input").disabled = disableAdditionalAttributes;
        document.getElementById("vehicle-number-input").disabled = disableAdditionalAttributes;
        document.getElementById("trailer-number-input").disabled = disableAdditionalAttributes;
        document.getElementById("driver-name-input").disabled = disableAdditionalAttributes;
        document.getElementById("driver-phone-number-input").disabled = disableAdditionalAttributes;
        document.getElementById("partner-manager-name-input").disabled = disableAdditionalAttributes;
        document.getElementById("partner-manager-phone-number-input").disabled = disableAdditionalAttributes;
        document.getElementById("partner-manager-email-input").disabled = disableAdditionalAttributes;

        const saveButton = document.getElementById("save-button");
        if (saveButton != undefined && saveButton != null) {
            document.getElementById("note-textarea").disabled = this.stateCode === "FINISHED" || this.stateCode === "CANCELED";
        } else {
            document.getElementById("note-textarea").disabled = true;
        }        

        // Workers
        const workersDiv = document.getElementById("workers-div");        
        const workersFilterDiv = document.getElementById("workers-filter-div");
        if (workersDiv != null && workersDiv != undefined) {
            const states = ["STARTED", "WORK_STARTED", "WORK_FINISHED", "FINISHED"];
            if (states.includes(this.stateCode) && gatesSelect.selectedIndex != 0) {
                workersDiv.classList.remove("d-none");

                await this.getSelectedWorkers();
                await this.showSelectedWorkers();
                const workersFieldset = document.getElementById("workers-fieldset");                
                const linearWorkersDiv = document.getElementById("linear-workers-div");
                if ((this.stateCode === "STARTED" || this.stateCode === "WORK_STARTED")
                    && workersFilterDiv != undefined && workersFilterDiv != null
                    && linearWorkersDiv != undefined && linearWorkersDiv != null)
                {
                    const workersFilterInput = document.getElementById("worker-filter-input");
                    workersFilterInput.onchange = this.onChangeWorkersFilter.bind(this);

                    await this.getLinearFreeWorkers();
                    this.showLinearFreeWorkers();
                    workersFieldset.disabled = false;
                    workersFilterDiv.classList.remove("d-none");
                    linearWorkersDiv.classList.remove("d-none");
                } else {
                    workersFieldset.disabled = true;
                    if (workersFilterDiv != undefined && workersFilterDiv != null) {
                        workersFilterDiv.remove();
                    }                    
                    if (linearWorkersDiv != undefined && linearWorkersDiv != null) {
                        linearWorkersDiv.remove();
                    }                    
                }
                this.showWorkersDivs();
            } else {
                workersDiv.remove();
            }
        }

        // Show button
        viewUtils.removeElement("show-bc-works-button", this.stateCode != "WORK_FINISHED" && this.stateCode != "FINISHED");

        const createButton = document.getElementById("create-button");
        if (createButton != undefined) {
            if (this.stateCode != "") {
                createButton.remove();
            } else {
                if (this.action === "CREATE" || this.action === "COPY") {
                    createButton.onclick = this.createOperation.bind(this);
                } else if (this.action === "CREATE_CONFIRM" || this.action === "COPY_CONFIRM") {
                    createButton.onclick = this.createConfirmOperation.bind(this);
                } else if (this.action === "CREATE_BY_PARTNER" || this.action === "COPY_BY_PARTNER") {
                    createButton.onclick = this.createOperationByPartner.bind(this);
                }
            }
        }

        viewUtils.removeElement("print-button", this.id <= 0 || copy, this.printOperation.bind(this));

        if (saveButton != undefined) {
            if (this.stateCode != "CREATED" &&
                this.stateCode != "CONFIRMED" &&
                this.stateCode != "STARTED" &&
                this.stateCode != "WORK_STARTED" &&
                this.stateCode != "WORK_FINISHED") {
                saveButton.remove();
            } else {
                if (this.stateCode === "CREATED" || this.stateCode === "CONFIRMED" || this.stateCode === "STARTED") {
                    if (operationTypesSelect.disabled) {
                        saveButton.onclick = this.editOperationNote.bind(this);
                    } else {
                        if (!isPartnerUser) {
                            saveButton.onclick = this.editOperation.bind(this);
                        } else {
                            saveButton.onclick = this.editOperationByPartner.bind(this);
                        }
                    }
                }
                else if (this.stateCode === "WORK_STARTED" || this.stateCode === "WORK_FINISHED") {
                    saveButton.onclick = this.editOperationNote.bind(this);
                }
            }            
        }

        viewUtils.removeElement("copy-button", this.id <= 0 || copy,
            isPartnerUser ? this.copyOperationByPartner.bind(this) : this.copyOperation.bind(this));
        viewUtils.removeElement("copy-confirm-button", this.id <= 0 || copy, this.copyConfirmOperation.bind(this));
        viewUtils.removeElement("set-gate-button", !isPreparing);
        viewUtils.removeElement("start-button", this.stateCode != "CONFIRMED");
        viewUtils.removeElement("start-work-button", this.stateCode != "STARTED" || gatesSelect === null || gatesSelect.selectedIndex === 0);
        viewUtils.removeElement("allocate-workers-button", this.stateCode != "STARTED" || gatesSelect === null || gatesSelect.selectedIndex === 0);
        viewUtils.removeElement("finish-work-button", this.stateCode != "WORK_STARTED");
        viewUtils.removeElement("finish-button", this.stateCode != "WORK_FINISHED");
        viewUtils.removeElement("cancel-button", this.stateCode != "CREATED" && this.stateCode != "CONFIRMED");
        viewUtils.removeElement("delete-button", this.stateCode === "");

        const closeButton = document.getElementById("close-button");
        closeButton.onclick = this.closeOperation.bind(this);

        window.onclick = this.windowClick.bind(this);
    }

    async recalculateStandard() {
        const operationTypeCode = "O_PALLET";
        const vehicleKindCode = "5_TONS";
        const actions = [{ kind: "O", id: "00030000W0003N0003564" }, { kind: "O", id: "00030000W0003N0003563" }];

        const result = await api.getFillingPlanOperationDuration(operationTypeCode, vehicleKindCode, actions);
        if (result.code === 0) {
            alert(result.planDuration);
        } else {
            alert(result.message);
        }
    }
}

const operationPage = new OperationPage();

document.addEventListener('DOMContentLoaded', operationPage.show.bind(operationPage));