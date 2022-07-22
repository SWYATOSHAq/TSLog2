/*
    Страница свободных сотрудников (тех, которые в данный момент находятся в текущей смене).
    Автор - Буров Михаил.
*/

// Страница свободных сотрудников (тех, которые в данный момент находятся в текущей смене)
class FreeShiftWorkersPage {
    shortUrl;

    workers = [];

    constructor() {
        this.shortUrl = location.href.split('?')[0];
    }

    createParameters() {
        let result = "";

        result = utils.addParameter(result, "worker", "workerInput");
        result = utils.addParameter(result, "wmsUser", "wmsUserInput");
        result = utils.addParameter(result, "position", "positions-select");
        result = utils.addParameter(result, "company", "companies-select");
        result = utils.addParameter(result, "site", "sites-select");
        result = utils.addParameter(result, "partner", "partners-select");
        //result = utils.addParameterValue(result, "sort_direction", this.sortDirection);

        return result != "" ? "?" + result : "";
    }

    clearFreeShiftWorkers() {
        const tbody = document.getElementById("sites-workers-tbody");
        tbody.replaceChildren();
    }

    createEnter(workerId) {
        return async function () {
            alert("Не реализовано!");
        }
    }

    showSiteWorker(siteWorker) {
        const tr = document.createElement("tr");

        viewUtils.createCell(tr, ["table-cell"], [siteWorker.workerFullName]);
        viewUtils.createCell(tr, ["table-cell"], [siteWorker.workerMajorPositionName]);
        viewUtils.createCell(tr, ["table-cell"], [siteWorker.workerCompanyName]);
        viewUtils.createCell(tr, ["table-cell"], [siteWorker.workerWmsLogin]);
        viewUtils.createCell(tr, ["table-cell"], [siteWorker.siteName]);
        viewUtils.createCell(tr, ["table-cell"], [siteWorker.partnerName]);

        const startDateTdBold = document.createElement("b");
        startDateTdBold.textContent = siteWorker.startTime;
        viewUtils.createCell(tr, ["table-cell"], [startDateTdBold, " (" + siteWorker.startDay + ")"]);

        const enterButton = document.createElement("button");
        enterButton.classList.add("btn");
        enterButton.classList.add("btn-outline-info");
        enterButton.classList.add("m-3");
        enterButton.classList.add("table-cell-button");
        enterButton.style.cssText = "width: 160px";
        enterButton.textContent = "Назначить на уч.";
        enterButton.onclick = this.createEnter(siteWorker.workerId);
        viewUtils.createCell(tr, ["table-cell", "not-for-print"], [enterButton]);

        const tbody = document.getElementById("sites-workers-tbody");
        tbody.appendChild(tr);
    }

    showFreeShiftWorkers(filteredData) {
        for (let siteWorker of filteredData) {
            this.showSiteWorker(siteWorker);
        }
    }

    async getFreehiftWorkers() {
        const result = await api.getFreeShiftWorkers();
        if (result.code === 0) {
            this.workers = result.sitesWorkers;
        } else {
            viewUtils.showError(result.message);
        }
    }

    onChangeFilter() {
        const filters = [];
        const workerNameInput = document.getElementById("workerInput");
        if (workerNameInput != undefined && workerNameInput != null && workerNameInput.value.trim() != "") {
            const workerNameFilterElement = viewUtils.getFilterElementByStringInput("workerFullName", workerNameInput);
            filters.push(workerNameFilterElement);
        }

        const wmsUserInput = document.getElementById("wmsUserInput");
        if (wmsUserInput != undefined && wmsUserInput != null && wmsUserInput.value.trim() != "") {
            const wmsUserFilterElement = viewUtils.getFilterElementByStringInput("workerWmsLogin", wmsUserInput);
            filters.push(wmsUserFilterElement);
        }

        const positionsSelect = document.getElementById("positions-select");
        const positionsFilterElement = viewUtils.getFilterElementBySelect("workerMajorPositionCode", positionsSelect);
        if (positionsFilterElement.value != undefined && positionsFilterElement.value != null && positionsFilterElement.value.trim() != "") {
            filters.push(positionsFilterElement);
        }

        const companiesSelect = document.getElementById("companies-select");
        const companiesFilterElement = viewUtils.getFilterElementBySelect("workerCompanyId", companiesSelect);
        if (companiesFilterElement.value != undefined && companiesFilterElement.value != null && companiesFilterElement.value.trim() != "") {
            filters.push(companiesFilterElement);
        }

        const sitesSelect = document.getElementById("sites-select");
        const sitesFilterElement = viewUtils.getFilterElementBySelect("siteCode", sitesSelect);
        if (sitesFilterElement.value != undefined && sitesFilterElement.value != null && sitesFilterElement.value.trim() != "") {
            filters.push(sitesFilterElement);
        }

        const partnersSelect = document.getElementById("partners-select");
        const partnersFilterElement = viewUtils.getFilterElementBySelect("partnerId", partnersSelect);
        if (partnersFilterElement.value != undefined && partnersFilterElement.value != null && partnersFilterElement.value.trim() != "") {
            filters.push(partnersFilterElement);
        }


        let filteredData = utils.filterData(this.workers, filters);
        this.clearFreeShiftWorkers();
        this.showFreeShiftWorkers(filteredData);
        const url = this.shortUrl + this.createParameters();
        history.pushState({}, null, url);

    }

    printPage () {
        const printButton = document.querySelector(".print-button");
        printButton.onclick = () => {
            window.print();
        }
    }

    async show() {
        await this.getFreehiftWorkers();
        this.onChangeFilter();
        viewUtils.initInput("workerInput", utils.getUrlParameter("worker"), this.onChangeFilter.bind(this));
        viewUtils.initInput("wmsUserInput", utils.getUrlParameter("wmsUser"), this.onChangeFilter.bind(this));
        viewUtils.initSelect("positions-select", utils.getUrlParameter("position"), this.onChangeFilter.bind(this));
        viewUtils.initSelect("companies-select", utils.getUrlParameter("company"), this.onChangeFilter.bind(this));
        viewUtils.initSelect("sites-select", utils.getUrlParameter("site"), this.onChangeFilter.bind(this));
        viewUtils.initSelect("partners-select", utils.getUrlParameter("partner"), this.onChangeFilter.bind(this));
        viewUtils.sortTable ();
        this.printPage();
    }
}

const freeShiftWorkersPage = new FreeShiftWorkersPage();

$(document).ready(freeShiftWorkersPage.show.bind(freeShiftWorkersPage));