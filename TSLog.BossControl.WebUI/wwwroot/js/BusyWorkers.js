/*
    Страница занятых сотрудников.
    Автор - Буров Михаил.
*/

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
    sortTable () {
        function sortTableByColumn(table, column, asc = true) {
            const directionModifier = asc ? 1 : -1;
            const tBody = table.tBodies[0];
            const rows = Array.from(tBody.querySelectorAll("tr"));
            //sort each row
            const sortedRows = rows.sort((a, b) => {
                const aColText = a.querySelector(`td:nth-child(${ column + 1 })`).textContent.trim();
                const bColText = b.querySelector(`td:nth-child(${ column + 1 })`).textContent.trim();

                return aColText > bColText ? (1 * directionModifier) : (-1 * directionModifier);

            });

            //Remove all existing TRs from the table
            while (tBody.firstChild) {
                tBody.removeChild(tBody.firstChild);
            }

            //Re-add the newly sorted rows
            tBody.append(...sortedRows);

            //Remember how the column is currently sorted
            table.querySelectorAll("th").forEach(th => th.classList.remove("th-sort-asc", "th-sort-desc"));
            table.querySelector(`th:nth-child(${ column + 1})`).classList.toggle("th-sort-asc", asc);
            table.querySelector(`th:nth-child(${ column + 1})`).classList.toggle("th-sort-desc", !asc);
        }

        document.querySelectorAll(".table-sortable th").forEach(headerCell => {
            headerCell.addEventListener("click", () => {
                const tableElement = headerCell.parentElement.parentElement.parentElement;
                const headerIndex = Array.prototype.indexOf.call(headerCell.parentElement.children, headerCell);
                const currentIsAscending = headerCell.classList.contains("th-sort-asc");

                sortTableByColumn(tableElement, headerIndex, !currentIsAscending);
            });
        });
    }



    showSiteWorker(siteWorker) {
        const tr = document.createElement("tr");

        const workerFullName = siteWorker.worker.surname + " " + siteWorker.worker.firstName;
        viewUtils.createCell(tr, ["table-cell"], [workerFullName]);
        viewUtils.createCell(tr, ["table-cell"], [siteWorker.worker.majorPosition]);
        viewUtils.createCell(tr, ["table-cell"], [siteWorker.worker.companyName]);
        viewUtils.createCell(tr, ["table-cell"], [siteWorker.worker.wmsLogin]);
        viewUtils.createCell(tr, ["table-cell"], [siteWorker.site.name]);
        viewUtils.createCell(tr, ["table-cell"], [siteWorker.partner.name]);

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
        leaveButton.onclick = this.createLeave(siteWorker.worker.id, workerFullName);
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

    async show() {
        this.clearBusyWorkers();
        await this.getBusyWorkers();
        this.showBusyWorkers();
        this.sortTable ();
    }
}

const busyWorkersPage = new BusyWorkersPage();

$(document).ready(busyWorkersPage.show.bind(busyWorkersPage));