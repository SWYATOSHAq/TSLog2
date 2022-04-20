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

    showSiteWorker(siteWorker) {
        const tr = document.createElement("tr");

        const workerTd = document.createElement("td");
        workerTd.classList.add("table-cell");
        workerTd.textContent = siteWorker.worker.surname + " " + siteWorker.worker.firstName;
        tr.appendChild(workerTd);

        const siteTd = document.createElement("td");
        siteTd.classList.add("table-cell");
        siteTd.textContent = siteWorker.site.name;
        tr.appendChild(siteTd);

        const partnerTd = document.createElement("td");
        partnerTd.classList.add("table-cell");
        partnerTd.textContent = siteWorker.partner.name;
        tr.appendChild(partnerTd);

        const startDateTd = document.createElement("td");
        startDateTd.classList.add("table-cell");
        const startDateTdBold = document.createElement("b");
        startDateTdBold.textContent = siteWorker.startTime;
        startDateTd.appendChild(startDateTdBold);
        const text = document.createTextNode(" (" + siteWorker.startDay + ")");
        startDateTd.appendChild(text);
        tr.appendChild(startDateTd);

        const tbody = document.getElementById("sites-workers-tbody");
        tbody.appendChild(tr);
    }

    showBusyWorkers() {
        for (let siteWorker of this.sitesWorkers) {
            this.showSiteWorker(siteWorker);
        }
    }

    async getBusyWorkers() {
        this.sitesWorkers = await api.getBusyWorkers();
    }

    async show() {
        this.clearBusyWorkers();
        await this.getBusyWorkers();
        this.showBusyWorkers();
    }
}

const busyWorkersPage = new BusyWorkersPage();

$(document).ready(busyWorkersPage.show.bind(busyWorkersPage));