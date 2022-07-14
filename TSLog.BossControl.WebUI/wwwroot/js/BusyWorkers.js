/*
    Страница занятых сотрудников.
    Автор - Буров Михаил.
*/

// Страница занятых сотрудников
class BusyWorkersPage {
    shortUrl;

    sitesWorkers = [];

    constructor() {
        this.shortUrl = location.href.split('?')[0];
    }

    clearBusyWorkers() {
        const tbody = document.getElementById("sites-workers-tbody");
        tbody.replaceChildren();
    }

    createLeave(workerId, workerFullName) {
        return async function () {
            if (!window.confirm("Снять сотрудника (" + workerFullName + ") с участка?")) {
                return;
            }

            const result = await api.leaveSite(workerId);
            if (result.code === 0) {
                busyWorkersPage.clearBusyWorkers();
                await busyWorkersPage.getBusyWorkers();
                busyWorkersPage.showBusyWorkers();
            } else {
                viewUtils.showError(result.message);
            }
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

        const startTimeB = document.createElement("b");
        startTimeB.textContent = siteWorker.startTime;
        viewUtils.createCell(tr, ["table-cell"], [startTimeB, " (" + siteWorker.startDay + ")"]);

        const leaveButton = document.createElement("button");
        leaveButton.classList.add("btn");
        leaveButton.classList.add("btn-outline-info");
        leaveButton.classList.add("m-3");
        leaveButton.classList.add("table-cell-button");
        leaveButton.style.cssText = "width: 160px";
        leaveButton.textContent = "Снять с участка";
        leaveButton.onclick = this.createLeave(siteWorker.workerId, siteWorker.workerFullName);
        viewUtils.createCell(tr, ["table-cell"], [leaveButton]);

        const tbody = document.getElementById("sites-workers-tbody");
        tbody.appendChild(tr);
    }

    showBusyWorkers() {
        for (let siteWorker of this.sitesWorkers) {
            this.showSiteWorker(siteWorker);
        }
    }

    async getBusyWorkers() {
        const result = await api.getBusyWorkers();
        if (result.code === 0) {
            this.sitesWorkers = result.sitesWorkers;
        } else {
            viewUtils.showError(result.message);
        }
    }

    onChangeFilter() {
        const filters = [];
        const workerNameInput = document.getElementById("workerInput");
        if (workerNameInput != undefined && workerNameInput != null && workerNameInput.value.trim() != "") {
            const workerNameFilterElement = viewUtils.getFilterElementByStringInput("workerName", workerNameInput);
            filters.push(workerNameFilterElement);
        }

        const wmsUserInput = document.getElementById("wmsUserInput");
        if (wmsUserInput != undefined && wmsUserInput != null && wmsUserInput.value.trim() != "") {
            const wmsUserFilterElement = viewUtils.getFilterElementByStringInput("wmsUser", wmsUserInput);
            filters.push(wmsUserFilterElement);
        }

        const positionsSelect = document.getElementById("positions-select");
        const positionsFilterElement = viewUtils.getFilterElementBySelect("position", positionsSelect);
        filters.push(positionsFilterElement);

        let filteredData = utils.filterData(this.sitesWorkers, filters);
        this.clearBusyWorkers();
        this.showData(filteredData);

    }

    async show() {
        this.clearBusyWorkers();
        await this.getBusyWorkers();
        this.showBusyWorkers();
        viewUtils.sortTable();
        //viewUtils.filterTable(".table-div");
    }
}

const busyWorkersPage = new BusyWorkersPage();

$(document).ready(busyWorkersPage.show.bind(busyWorkersPage));