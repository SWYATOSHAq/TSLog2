/*
    Страница свободных сотрудников (всех).
    Автор - Буров Михаил.
*/

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
        viewUtils.createCell(tr, ["table-cell"], [worker.majorPosition]);
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

    showFreeWorkers() {
        for (let siteWorker of this.workers) {
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

    async show() {
        this.clearFreeWorkers();
        await this.getFreeWorkers();
        this.showFreeWorkers();
        viewUtils.sortTable ();
    }
}

const freeWorkersPage = new FreeWorkersPage();

$(document).ready(freeWorkersPage.show.bind(freeWorkersPage));