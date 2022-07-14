/*
    Страница свободных сотрудников (всех).
    Автор - Буров Михаил.
*/

// Страница свободных сотрудников (всех)
class FreeWorkersPage {
    shortUrl;

    workers = [];

    constructor() {
        this.shortUrl = location.href.split('?')[0];
    }

    clearFreeWorkers() {
        const tbody = document.getElementById("sites-workers-tbody");
        tbody.replaceChildren();
    }

    createEnter(workerId) {
        return async function () {
            alert("Не реализовано!");
        }
    }

    showWorker(worker) {
        const tr = document.createElement("tr");

        viewUtils.createCell(tr, ["table-cell"], [worker.surname + " " + worker.firstName]);
        viewUtils.createCell(tr, ["table-cell"], [worker.majorPositionName]);
        viewUtils.createCell(tr, ["table-cell"], [worker.companyName]);
        viewUtils.createCell(tr, ["table-cell"], [worker.wmsLogin]);

        const enterButton = document.createElement("button");
        enterButton.classList.add("btn");
        enterButton.classList.add("btn-outline-info");
        enterButton.classList.add("m-3");
        enterButton.classList.add("table-cell-button");
        enterButton.textContent = "Назначить на уч.";
        enterButton.style.cssText = "width: 160px";
        enterButton.onclick = this.createEnter(worker.id);
        viewUtils.createCell(tr, ["table-cell"], [enterButton]);

        const tbody = document.getElementById("sites-workers-tbody");
        tbody.appendChild(tr);
    }

    showFreeWorkers(filteredData) {
        for (let siteWorker of filteredData) {
            this.showWorker(siteWorker);
        }
    }

    async getFreeWorkers() {
        const result = await api.getFreeWorkers();
        if (result.code === 0) {
            this.workers = result.workers;
        } else {
            viewUtils.showError(result.message);
        }
    }

    onChangeFilter() {
        const filters = [];
        const workerNameInput = document.getElementById("workerInput");
        if (workerNameInput != undefined && workerNameInput != null && workerNameInput.value.trim() != "") {
            const workerNameFilterElement = viewUtils.getFilterElementByStringInput("fullName", workerNameInput);
            filters.push(workerNameFilterElement);
        }

        const wmsUserInput = document.getElementById("wmsUserInput");
        if (wmsUserInput != undefined && wmsUserInput != null && wmsUserInput.value.trim() != "") {
            const wmsUserFilterElement = viewUtils.getFilterElementByStringInput("wmsLogin", wmsUserInput);
            filters.push(wmsUserFilterElement);
        }

        const positionsSelect = document.getElementById("positions-select");
        const positionsFilterElement = viewUtils.getFilterElementBySelect("majorPositionCode", positionsSelect);
        if (positionsFilterElement.value != undefined && positionsFilterElement.value != null && positionsFilterElement.value.trim() != "") {
            filters.push(positionsFilterElement);
        }

        const companiesSelect = document.getElementById("companies-select");
        const companiesFilterElement = viewUtils.getFilterElementBySelect("companyId", companiesSelect);
        if (companiesFilterElement.value != undefined && companiesFilterElement.value != null && companiesFilterElement.value.trim() != "") {
            filters.push(companiesFilterElement);
        }

        let filteredData = utils.filterData(this.workers, filters);
        this.clearFreeWorkers();
        this.showFreeWorkers(filteredData);

    }

    async show() {
        await this.getFreeWorkers();
        this.onChangeFilter();
        viewUtils.initInput("workerInput", "", this.onChangeFilter.bind(this));
        viewUtils.initInput("wmsUserInput", "", this.onChangeFilter.bind(this));
        viewUtils.initSelect("positions-select", "", this.onChangeFilter.bind(this));
        viewUtils.initSelect("companies-select", "", this.onChangeFilter.bind(this));
        viewUtils.sortTable ();
    }
}

const freeWorkersPage = new FreeWorkersPage();

$(document).ready(freeWorkersPage.show.bind(freeWorkersPage));