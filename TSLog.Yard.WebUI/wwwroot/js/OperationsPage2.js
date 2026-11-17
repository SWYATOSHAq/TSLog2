/*
    Страница списка операций двора.
    Автор - Буров Михаил (m.burov@tslog.ru).
*/
class OperationsPage {
    shortUrl;
    operations = [];
    sortField = "driverName";
    sortDirection = "forward";
    header = document.getElementById("header3");
    onParkingHeader = document.getElementById("onParkingHeader");
    onGatesCarsHeader = document.getElementById("onGatesCarsHeader");
    filterOptions = document.getElementById("operation-filter-options");
    filterTags = document.getElementById("operation-filter-tags");
    resetFiltersButton = document.getElementById("operation-reset-filters");
    filterStatus = document.getElementById("operation-filter-status");
    tableBody = document.getElementById("operations-table-body");
    headers = new Map(Array.from(this.tableBody.closest("table").tHead.rows[0].cells,
        header => [header.dataset.column, header]));
    isPartnerUser = document.querySelector(".yard-operations-tools").dataset.partnerUser === "true";
    appliedFilters = new URLSearchParams();
    filterRequest = 0;
    refreshTime = 180000; // In milliseconds.

    constructor() {
        this.shortUrl = location.href.split('?')[0];
        window.onbeforeunload = this.saveScrollPosition.bind(this);
        this.setAutoRefresh(this.refreshTime); 
    }

    setAutoRefresh(interval) {
        setTimeout(() => {
            location.reload();
        }, interval);
    }

    saveScrollPosition() {
        try { sessionStorage.setItem("scrollpos", window.scrollY); } catch (_) { }
    }

    restoreScrollPosition() {
        try {
            const scrollpos = sessionStorage.getItem("scrollpos");
            if (scrollpos) window.scrollTo(0, Number(scrollpos));
        } catch (_) { }
    }

    createOpenOperationFunction(id, isPartnerUser) {
        return function () {
            try { sessionStorage.setItem("OPERATIONS_PAGE_URL", location.href); } catch (_) { }
            location.href = "/OperationPage?id=" + id + "&action=" + (!isPartnerUser ? "VIEW" : "VIEW_BY_PARTNER");
        }
    }

    showOperation(operation, operationTr, isPartnerUser) {
        operation.startDate = new Date(operation.startDate);
        operation.planWorkStartDate = new Date(operation.planWorkStartDate);
        operation.factWorkStartDate = new Date(operation.factWorkStartDate);
        operation.planWorkFinishDate = new Date(operation.planWorkFinishDate);
        operation.factWorkFinishDate = new Date(operation.factWorkFinishDate);
        operation.finishDate = new Date(operation.finishDate);
        const cells = new Map();
        const style = "box-shadow: none; background-color: " + operation.displayColor;
        const addCell = (key, contents, html = false) => {
            const cell = html
                ? viewUtils.createHtmlCell(operationTr, ["table-cell"], contents, style)
                : viewUtils.createCell(operationTr, ["table-cell"], contents, style);
            cell.dataset.column = key;
            cell.hidden = this.headers.get(key).hidden;
            cells.set(key, cell);
        };
        let contents;

        const openP = document.createElement("p");
        openP.classList.add("text-center");
        openP.classList.add("p-margin-0");
        openP.textContent = operation.id;
        contents = [openP];

        const openButton = document.createElement("button");
        openButton.classList.add("btn");
        openButton.classList.add("btn-outline-info");
        openButton.classList.add("table-cell-button");
        openButton.style.cssText = "width: 80px !important";
        openButton.onclick = this.createOpenOperationFunction(operation.id, isPartnerUser);
        openButton.textContent = "Открыть";
        openButton.title = "Открыть карточку операции двора";
        contents.push(openButton);

        addCell("id", contents);
        addCell("type", [operation.typeShortName]);
        addCell("orders", [operation.partnerActionId]);
        addCell("client", [operation.partnersNames]);
        addCell("driver", [operation.driverName]);
        addCell("phone", [operation.driverPhoneNumber]);
        addCell("state", [operation.stateName]);

        let text = operation.startDate > utils.nullDate ?
            operation.startDate.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }) + " - " + operation.startDate.toLocaleDateString() :
            "Ожидается";
        addCell("arrival", [text]);

        contents = [operation.planWorkStartDate.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }) + " - " + operation.planWorkStartDate.toLocaleDateString()];
        if (operation.factWorkStartDate > utils.nullDate) {
            contents.push(document.createElement("br"));
            contents.push("(" + operation.factWorkStartDate.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }) + " - " + operation.factWorkStartDate.toLocaleDateString() + ")");
        }
        addCell("workStart", contents);

        contents = [operation.planWorkFinishDate.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }) + " - " + operation.planWorkFinishDate.toLocaleDateString()];
        if (operation.factWorkFinishDate > utils.nullDate) {
            contents.push(document.createElement("br"));
            contents.push("(" + operation.factWorkFinishDate.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }) + " - " + operation.factWorkFinishDate.toLocaleDateString() + ")");
        }
        addCell("workFinish", contents);

        text = operation.finishDate > utils.nullDate ?
            operation.finishDate.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }) + " - " + operation.finishDate.toLocaleDateString() : "";

        addCell("departure", [text]);
        addCell("onSiteDuration", [(operation.finishDate - operation.startDate) > 0 ? utils.millisecondsToString(operation.finishDate - operation.startDate) : ""]);
        addCell("workDuration", [operation.factDuration > 0 ? utils.secondsToString(operation.factDuration) : ""]);
        addCell("vehicle", [operation.vehicleNumber]);
        addCell("trailer", [operation.trailerNumber]);
        addCell("gate", [operation.gateShortName]);
        addCell("vehicleKind", [operation.vehicleKindShortName]);
        addCell("note", [operation.note], true);

        // Use the Razor header order for every row, including after filtering or sorting.
        operationTr.replaceChildren(...Array.from(this.headers.keys(), key => cells.get(key)));
    }

    sortCompare(operation1, operation2) {
        switch (this.sortField) {
            case "driverName":
                return operation1.driverName.localeCompare(operation2.driverName);
            case "id":
                return operation1.id - operation2.id;
            case "partnerId":
                return operation1.partnerId.localeCompare(operation2.partnerId);
            case "gateCode":
                return operation1.gateCode.localeCompare(operation2.gateCode);
            case "vehicleNumber":
                return operation1.vehicleNumber.localeCompare(operation2.vehicleNumber);
            case "startDate":
                return (new Date(operation1.startDate)) - (new Date(operation2.startDate));
            case "directionName":
                return operation1.directionName.localeCompare(operation2.directionName);
            case "planWorkStartDate":
                return (new Date(operation1.planWorkStartDate)) - (new Date(operation2.planWorkStartDate));
            case "factWorkStartDate":
                return (new Date(operation1.factWorkStartDate)) - (new Date(operation2.factWorkStartDate));
            case "finishDate":
                return (new Date(operation1.finishDate)) - (new Date(operation2.finishDate));
            case "onSiteDuration":
                let onSiteDuration1 = (new Date(operation1.finishDate) - new Date(operation1.startDate)) > 0 ? utils.millisecondsToString(new Date(operation1.finishDate) - new Date(operation1.startDate)) : "";
                let onSiteDuration2 = (new Date(operation2.finishDate) - new Date(operation2.startDate)) > 0 ? utils.millisecondsToString(new Date(operation2.finishDate) - new Date(operation2.startDate)) : "";
                return onSiteDuration1.localeCompare(onSiteDuration2);
            case "workDuration":
                let workDuration1 = operation1.factDuration > 0 ? utils.secondsToString(operation1.factDuration) : "";
                let workDuration2 = operation2.factDuration > 0 ? utils.secondsToString(operation2.factDuration) : "";
                return workDuration1.localeCompare(workDuration2);
            case "stateId":
                return operation1.stateName.localeCompare(operation2.stateName);
        }
    }

    initFilters() {
        const fields = [
            { label: "Период", controls: [["from_date", "from-date-input"], ["to_date", "to-date-input"]] },
            { label: "Тип операции", controls: [["operation_type_code", "operation-types-select"]] },
            { label: "Тип машины", controls: [["vehicle_kind_code", "vehicle-kinds-select"]] },
            { label: "Ворота", controls: [["gate_code", "gates-select"]] },
            { label: "Состояние", controls: [["state_code", "operation-states-select"]] },
            { label: "Партнёр", controls: [["partner_id", "partners-select"]] },
            { label: "№ заявки", controls: [["id", "id-number-input"]] },
            { label: "Номер машины", controls: [["vehicle_number", "vehicle-number-input"]] },
            { label: "Водитель", controls: [["driver_name", "driver-name-input"]] },
            { label: "Заказ/приход", controls: [["partner_action_id", "partner-action-id-input"]] },
            { label: "Работник", controls: [["worker", "worker-input"]] },
            { label: "Телефон водителя", controls: [["driver_phone_number", "driver-phone-number-input"]] }
        ];
        this.filters = fields.map(field => ({
            label: field.label,
            controls: field.controls.map(([parameter, id]) => ({
                parameter, input: document.getElementById(id)
            })).filter(({ input }) => input && !input.disabled)
        })).filter(field => field.controls.length > 0);

        const storageKey = "tslog-yard-hidden-filters";
        const hidden = new Set();
        try {
            const saved = JSON.parse(localStorage.getItem(storageKey));
            if (Array.isArray(saved)) saved.forEach(key => hidden.add(key));
        } catch (_) { }

        for (const field of this.filters) {
            const { parameter, input } = field.controls[0];
            field.isPeriod = parameter === "from_date";
            const container = input.closest(".filter-div, .dates-div");
            const checkbox = document.createElement("input");
            checkbox.type = "checkbox";
            checkbox.checked = !hidden.has(parameter);
            container.hidden = !checkbox.checked;
            checkbox.addEventListener("change", () => {
                container.hidden = !checkbox.checked;
                if (checkbox.checked) hidden.delete(parameter);
                else hidden.add(parameter);
                try { localStorage.setItem(storageKey, JSON.stringify([...hidden])); } catch (_) { }
            });
            const label = document.createElement("label");
            label.append(checkbox, field.label);
            this.filterOptions.append(label);

            for (const control of field.controls) {
                const { parameter: name, input: element } = control;
                const value = utils.getUrlParameter(name);
                if (field.isPeriod) {
                    const date = value || utils.getUrlParameter("from_date") || utils.getNowDate();
                    viewUtils.initDatePicker(element.id, date, () => this.onChangeFilter());
                } else {
                    element.value = value;
                    if (element.tagName === "SELECT" && element.selectedIndex < 0) element.value = "";
                    element.addEventListener("change", () => {
                        this.updateOperationTypeHint();
                        this.onChangeFilter();
                    });
                }
            }
        }
        this.resetFiltersButton.addEventListener("click", () => this.resetFilters(this.filters));
        this.updateOperationTypeHint();
    }

    resetFilters(fields) {
        for (const field of fields) {
            for (const { input } of field.controls) {
                if (field.isPeriod) $(input).datepicker("update", utils.getNowDate());
                else input.value = "";
            }
        }
        this.updateOperationTypeHint();
        this.onChangeFilter();
    }

    renderAppliedFilters() {
        const tags = [];
        for (const field of this.filters) {
            const values = field.controls.map(({ input, parameter }) => {
                const value = this.appliedFilters.get(parameter);
                if (!value || input.tagName !== "SELECT") return value;
                return Array.from(input.options).find(option => option.value === value)?.textContent.trim() ?? value;
            }).filter(Boolean);
            if (values.length === 0 || (field.isPeriod && values.every(value => value === utils.getNowDate()))) continue;

            const caption = field.label + ": " + (values.every(value => value === values[0]) ? values[0] : values.join(" — "));
            const button = document.createElement("button");
            button.type = "button";
            button.textContent = caption + " ×";
            button.setAttribute("aria-label", "Сбросить отбор «" + caption + "»");
            button.addEventListener("click", () => this.resetFilters([field]));
            tags.push(button);
        }
        const priorFocus = Array.from(this.filterTags.children).indexOf(document.activeElement);
        const resetHadFocus = document.activeElement === this.resetFiltersButton;
        this.filterTags.replaceChildren(...tags);
        this.resetFiltersButton.hidden = tags.length === 0;
        if (priorFocus >= 0 || (resetHadFocus && this.resetFiltersButton.hidden)) {
            const next = tags[Math.min(Math.max(priorFocus, 0), tags.length - 1)]
                ?? this.filterOptions.parentElement.querySelector("summary");
            next.focus();
        }
    }

    updateLocation(replace = false) {
        const parameters = new URLSearchParams(this.appliedFilters);
        parameters.set("sort_field", this.sortField);
        parameters.set("sort_direction", this.sortDirection);
        const url = this.shortUrl + "?" + parameters;
        if (url === location.href) return;
        if (replace) history.replaceState({}, "", url);
        else history.pushState({}, "", url);
    }

    renderOperations() {
        const operations = [...this.operations].sort(this.sortCompare.bind(this));
        if (this.sortDirection === "reverse") operations.reverse();
        const rows = document.createDocumentFragment();
        let parkingCount = 0;
        let gatesCount = 0;
        for (const operation of operations) {
            if (operation.stateCode === "STARTED" || operation.stateCode === "WORK_FINISHED") parkingCount++;
            else if (operation.stateCode === "WORK_STARTED") gatesCount++;
            const row = document.createElement("tr");
            this.showOperation(operation, row, this.isPartnerUser);
            rows.append(row);
        }
        this.tableBody.replaceChildren(rows);
        this.header.textContent = "Всего заявок: " + operations.length;
        this.onParkingHeader.textContent = "Машин на парковке: " + parkingCount;
        this.onGatesCarsHeader.textContent = "Машин на воротах: " + gatesCount;
    }

    async onChangeFilter(replace = false) {
        const parameters = new URLSearchParams();
        for (const field of this.filters) {
            for (const { parameter, input } of field.controls) {
                if (input.value) parameters.set(parameter, input.value);
            }
        }

        const requestId = ++this.filterRequest;
        const table = this.tableBody.closest("table");
        table.setAttribute("aria-busy", "true");
        this.filterStatus.classList.add("visually-hidden");
        this.filterStatus.textContent = "";
        try {
            const result = await api.getOperations("?" + parameters);
            if (requestId !== this.filterRequest) return;
            if (result.code !== 0) throw new Error(result.message);
            this.operations = result.operations;
            this.appliedFilters = parameters;
            this.renderOperations();
            this.renderAppliedFilters();
            this.updateLocation(replace);
            this.filterStatus.textContent = this.header.textContent;
        } catch (error) {
            if (requestId === this.filterRequest) {
                console.error(error);
                this.filterStatus.textContent = "Не удалось обновить заявки. Повторите отбор.";
                this.filterStatus.classList.remove("visually-hidden");
            }
        } finally {
            if (requestId === this.filterRequest) table.removeAttribute("aria-busy");
        }
    }

    updateOperationTypeHint() {
        const select = document.getElementById("operation-types-select");
        if (select) select.title = select.selectedOptions[0]?.getAttribute("note") ?? "";
    }

    async show() {
        this.initFilters();
        const sortSelect = document.getElementById("sort-field-select");
        const savedSortField = utils.getUrlParameter("sort_field");
        if (Array.from(sortSelect.options).some(option => option.value === savedSortField)) sortSelect.value = savedSortField;
        this.sortField = sortSelect.value;
        sortSelect.addEventListener("change", () => {
            this.sortField = sortSelect.value;
            this.renderOperations();
            this.updateLocation();
        });

        this.sortDirection = utils.getUrlParameter("sort_direction") === "reverse" ? "reverse" : "forward";
        document.getElementById(this.sortDirection + "-sort-input").checked = true;
        for (const input of document.querySelectorAll("input[name='sort-input']")) {
            input.addEventListener("change", () => {
                this.sortDirection = input.id === "reverse-sort-input" ? "reverse" : "forward";
                this.renderOperations();
                this.updateLocation();
            });
        }
        window.addEventListener("popstate", () => location.reload());

        await this.onChangeFilter(true);
        this.restoreScrollPosition();
    }
}

const operationsPage = new OperationsPage();

document.addEventListener('DOMContentLoaded', operationsPage.show.bind(operationsPage));
